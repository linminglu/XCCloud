using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Transactions;
using System.Web;
using XCCloudService.Base;
using XCCloudService.BLL.IBLL.XCCloud;
using XCCloudService.BLL.XCCloud;
using XCCloudService.Common;
using XCCloudService.Common.Enum;
using XCCloudService.Common.Extensions;
using XCCloudService.DAL;
using XCCloudService.DBService.BLL;
using XCCloudService.Model.CustomModel.XCCloud;
using XCCloudService.Model.XCCloud;

namespace XXCloudService.Api.XCCloud
{
    [Authorize(Roles = "MerchUser")]
    /// <summary>
    /// Promotion 的摘要说明
    /// </summary>
    public class Promotion : ApiBase
    {        
        private bool isEveryDay(string weekDays)
        {
            return !string.IsNullOrEmpty(weekDays) && weekDays.Contains("1") && weekDays.Contains("2") && weekDays.Contains("3") && weekDays.Contains("4") && weekDays.Contains("5") && weekDays.Contains("6") && weekDays.Contains("7");
        }
        private string getWeekName(string weekDays)
        {
            return isEveryDay(weekDays) ? "每天" : (!string.IsNullOrEmpty(weekDays) ? weekDays.Replace("1", "周一").Replace("2", "周二").Replace("3", "周三").Replace("4", "周四").Replace("5", "周五").Replace("6", "周六").Replace("7", "周日") : string.Empty);
        }
        private string getTimeName(TimeSpan? startTime, TimeSpan? endTime)
        {
            var StartTime = string.Format("{0:c}", startTime).Substring(0, 5);
            var EndTime = string.Format("{0:c}", endTime).Substring(0, 5);
            return (StartTime == "00:00" && EndTime == "23:59") ? "全天" : (StartTime + "~" + EndTime);
        }
        private string getPeriodTypeName(TimeType timeType)
        {
            return timeType.Equals(TimeType.Custom) ? "自定义" : timeType.Equals(TimeType.Weekend) ? "周末" : timeType.Equals(TimeType.Holiday) ? "节假日" : timeType.Equals(TimeType.Workday) ? "工作日" : string.Empty;
        }
        private bool saveFoodSale(int iFoodId, object[] foodSales, out string errMsg)
        {
            errMsg = string.Empty;
            if (foodSales != null && foodSales.Count() >= 0)
            {
                //先删除，后添加
                var data_Food_SaleService = Data_Food_SaleService.I;
                foreach (var model in data_Food_SaleService.GetModels(p => p.FoodID == iFoodId))
                {
                    data_Food_SaleService.DeleteModel(model);
                }

                foreach (IDictionary<string, object> el in foodSales)
                {
                    if (el != null)
                    {
                        var dicPara = new Dictionary<string, object>(el, StringComparer.OrdinalIgnoreCase);
                        string balanceType = dicPara.ContainsKey("balanceType") ? (dicPara["balanceType"] + "") : string.Empty;
                        string useCount = dicPara.ContainsKey("useCount") ? (dicPara["useCount"] + "") : string.Empty;

                        if (string.IsNullOrEmpty(balanceType))
                        {
                            errMsg = "余额类别不能为空";
                            return false;
                        }
                        if (string.IsNullOrEmpty(useCount))
                        {
                            errMsg = "消耗数量不能为空";
                            return false;
                        }
                        if (!Utils.isNumber(useCount))
                        {
                            errMsg = "消耗数量格式不正确";
                            return false;
                        }
                        if (Convert.ToInt32(useCount) < 0)
                        {
                            errMsg = "消耗数量不能为负数";
                            return false;
                        }

                        var data_Food_SaleModel = new Data_Food_Sale();
                        data_Food_SaleModel.FoodID = iFoodId;
                        data_Food_SaleModel.BalanceType = Convert.ToInt32(balanceType);
                        data_Food_SaleModel.UseCount = Convert.ToInt32(useCount);
                        data_Food_SaleService.AddModel(data_Food_SaleModel);
                    }
                    else
                    {
                        errMsg = "提交数据包含空对象";
                        return false;
                    }
                }

                if (!data_Food_SaleService.SaveChanges())
                {
                    errMsg = "保存套餐余额消耗信息失败";
                    return false;
                }
            }

            return true;
        }
        private bool saveFoodLevel(int iFoodId, object[] foodLevels, out string errMsg)
        {
            errMsg = string.Empty;
            if (foodLevels != null && foodLevels.Count() >= 0)
            {
                //先删除已有数据，后添加
                var dbContext = DbContextFactory.CreateByModelNamespace(typeof(Data_Food_Level).Namespace);
                var data_Food_Level = dbContext.Set<Data_Food_Level>().Where(p => p.FoodID == iFoodId).ToList();
                foreach (var model in data_Food_Level)
                {
                    dbContext.Entry(model).State = EntityState.Deleted;
                }

                var foodLevelList = new List<Data_Food_Level>();
                foreach (IDictionary<string, object> el in foodLevels)
                {
                    if (el != null)
                    {
                        var dicPara = new Dictionary<string, object>(el, StringComparer.OrdinalIgnoreCase);
                        string memberLevelIDs = dicPara.ContainsKey("memberLevelIDs") ? dicPara["memberLevelIDs"].ToString() : string.Empty;
                        string timeType = dicPara.ContainsKey("timeType") ? dicPara["timeType"].ToString() : string.Empty;
                        string week = dicPara.ContainsKey("week") ? dicPara["week"].ToString() : string.Empty;
                        string start = dicPara.ContainsKey("startTime") ? dicPara["startTime"].ToString() : string.Empty;
                        string end = dicPara.ContainsKey("endTime") ? dicPara["endTime"].ToString() : string.Empty;
                        string client = dicPara.ContainsKey("clientPrice") ? dicPara["clientPrice"].ToString() : string.Empty;
                        string vip = dicPara.ContainsKey("vipPrice") ? dicPara["vipPrice"].ToString() : string.Empty;
                        string day_sale_count = dicPara.ContainsKey("day_sale_count") ? dicPara["day_sale_count"].ToString() : string.Empty;
                        string member_day_sale_count = dicPara.ContainsKey("member_day_sale_count") ? dicPara["member_day_sale_count"].ToString() : string.Empty;
                        string updateLevelId = dicPara.ContainsKey("updateLevelId") ? (dicPara["updateLevelId"] + "") : string.Empty;

                        #region 参数验证
                        if (string.IsNullOrEmpty(start) || string.IsNullOrEmpty(end))
                        {
                            errMsg = "优惠时段不能为空";
                            return false;
                        }
                        if (TimeSpan.Compare(TimeSpan.Parse(start), TimeSpan.Parse(end)) > 0)
                        {
                            errMsg = "优惠开始时段不能晚于结束时段";
                            return false;
                        }
                        if (string.IsNullOrEmpty(memberLevelIDs))
                        {
                            errMsg = "会员级别ID列表不能为空";
                            return false;
                        }
                        if (Convert.ToInt32(day_sale_count) < 0)
                        {
                            errMsg = "每天限购数不能为负数";
                            return false;
                        }
                        if (Convert.ToInt32(member_day_sale_count) < 0)
                        {
                            errMsg = "每人每天限购数不能为负数";
                            return false;
                        }
                        if (Convert.ToDecimal(client) < 0)
                        {
                            errMsg = "散客优惠价不能为负数";
                            return false;
                        }
                        if (Convert.ToDecimal(vip) < 0)
                        {
                            errMsg = "会员优惠价不能为负数";
                            return false;
                        }
                        if (string.IsNullOrEmpty(timeType))
                        {
                            errMsg = "时段类型不能为空";
                            return false;
                        }
                        int iTimeType = Convert.ToInt32(timeType);
                        if (iTimeType == (int)TimeType.Custom && string.IsNullOrEmpty(week))
                        {
                            errMsg = "自定义模式周数不能为空";
                            return false;
                        }
                        #endregion

                        List<string> memberLevelIDList = memberLevelIDs.Split('|').ToList();
                        foreach (var memberLevelID in memberLevelIDList)
                        {
                            var data_Food_LevelModel = new Data_Food_Level();
                            data_Food_LevelModel.FoodID = iFoodId;
                            data_Food_LevelModel.MemberLevelID = Convert.ToInt32(memberLevelID);
                            data_Food_LevelModel.TimeType = iTimeType;
                            data_Food_LevelModel.Week = week;
                            data_Food_LevelModel.StartTime = TimeSpan.Parse(start);
                            data_Food_LevelModel.EndTime = TimeSpan.Parse(end);
                            data_Food_LevelModel.ClientPrice = Convert.ToDecimal(client);
                            data_Food_LevelModel.VIPPrice = Convert.ToDecimal(vip);
                            data_Food_LevelModel.day_sale_count = Convert.ToInt32(day_sale_count);
                            data_Food_LevelModel.member_day_sale_count = Convert.ToInt32(member_day_sale_count);
                            data_Food_LevelModel.UpdateLevelID = !string.IsNullOrEmpty(updateLevelId) ? Convert.ToInt32(updateLevelId) : (int?)null;
                            foodLevelList.Add(data_Food_LevelModel);
                            dbContext.Entry(data_Food_LevelModel).State = EntityState.Added;
                        }                        
                    }
                    else
                    {
                        errMsg = "提交数据包含空对象";
                        return false;
                    }
                }

                //同一会员级别，时段类型为工作模式（即0~2）与自定义模式（即3）不能共存                                    
                foreach (var memberLevelId in foodLevelList.GroupBy(g => g.MemberLevelID).Select(o => o.Key))
                {                    
                    if (foodLevelList.Any(w => w.MemberLevelID.Equals(memberLevelId) && w.TimeType.Equals((int)TimeType.Custom)) &&
                        foodLevelList.Any(w => w.MemberLevelID.Equals(memberLevelId) && !w.TimeType.Equals((int)TimeType.Custom)))
                    {
                        string memberLevelName = (from b in dbContext.Set<Data_MemberLevel>() where b.MemberLevelID == memberLevelId select b.MemberLevelName).FirstOrDefault();
                        errMsg = string.Format("同一会员级别，自定义模式与其它模式不能混合定义 会员级别:{0}", memberLevelName);
                        return false;
                    }
                }

                //同一会员级别，同一个时段类型（如果是自定义模式，即同一天），同一时段只能有一个优惠策略
                foreach (var data_Food_LevelModel in foodLevelList)
                {
                    int memberLevelId = (int)data_Food_LevelModel.MemberLevelID;                                        
                    if (data_Food_LevelModel.TimeType == (int)TimeType.Custom)
                    {
                        var weeks = data_Food_LevelModel.Week.Split('|').ToList();
                        foreach (var day in weeks)
                        {
                            if (foodLevelList.Where(w => w.MemberLevelID.Equals(data_Food_LevelModel.MemberLevelID) && w.Week.Contains(day) &&
                                 ((TimeSpan.Compare((TimeSpan)data_Food_LevelModel.StartTime, (TimeSpan)w.StartTime) >= 0 && TimeSpan.Compare((TimeSpan)data_Food_LevelModel.StartTime, (TimeSpan)w.EndTime) < 0) ||
                                 (TimeSpan.Compare((TimeSpan)data_Food_LevelModel.EndTime, (TimeSpan)w.StartTime) > 0 && TimeSpan.Compare((TimeSpan)data_Food_LevelModel.EndTime, (TimeSpan)w.EndTime) <= 0))).Count() > 1)
                            {
                                string memberLevelName = (from b in dbContext.Set<Data_MemberLevel>() where b.MemberLevelID == memberLevelId select b.MemberLevelName).FirstOrDefault();
                                string timeSpan = Utils.TimeSpanToStr(data_Food_LevelModel.StartTime.Value) + "~" + Utils.TimeSpanToStr(data_Food_LevelModel.EndTime.Value);
                                errMsg = string.Format("同一会员级别，同一时段只能有一个优惠策略 会员级别:{0} 自定义模式:{1} 优惠时段:{2}", memberLevelName, getWeekName(day), timeSpan);
                                return false;
                            }
                        }
                    }
                    else
                    {
                        if (foodLevelList.Where(w => w.MemberLevelID.Equals(data_Food_LevelModel.MemberLevelID) && w.TimeType.Equals(data_Food_LevelModel.TimeType) &&
                                 ((TimeSpan.Compare((TimeSpan)data_Food_LevelModel.StartTime, (TimeSpan)w.StartTime) >= 0 && TimeSpan.Compare((TimeSpan)data_Food_LevelModel.StartTime, (TimeSpan)w.EndTime) < 0) ||
                                 (TimeSpan.Compare((TimeSpan)data_Food_LevelModel.EndTime, (TimeSpan)w.StartTime) > 0 && TimeSpan.Compare((TimeSpan)data_Food_LevelModel.EndTime, (TimeSpan)w.EndTime) <= 0))).Count() > 1)
                        {
                            string memberLevelName = (from b in dbContext.Set<Data_MemberLevel>() where b.MemberLevelID == memberLevelId select b.MemberLevelName).FirstOrDefault();
                            string timeSpan = Utils.TimeSpanToStr(data_Food_LevelModel.StartTime.Value) + "~" + Utils.TimeSpanToStr(data_Food_LevelModel.EndTime.Value);
                            errMsg = string.Format("同一会员级别，同一时段只能有一个优惠策略 会员级别:{0} 其它模式:{1} 优惠时段:{2}", memberLevelName, getPeriodTypeName((TimeType)data_Food_LevelModel.TimeType), timeSpan);
                            return false;
                        }
                    }
                }

                if (dbContext.SaveChanges() < 0)
                {
                    errMsg = "保存套餐级别信息失败";
                    return false;
                }
            }

            return true;
        }
        private bool saveFoodDetail(int iFoodId, object[] foodDetials, out string errMsg)
        {
            errMsg = string.Empty;
            if (foodDetials != null && foodDetials.Count() >= 0)
            {
                //先删除，后添加
                var dbContext = DbContextFactory.CreateByModelNamespace(typeof(Data_Food_Detial).Namespace);
                var data_Food_Detial = dbContext.Set<Data_Food_Detial>().Where(p => p.FoodID == iFoodId).ToList();
                foreach (var model in data_Food_Detial)
                {
                    dbContext.Entry(model).State = EntityState.Deleted;
                }

                foreach (IDictionary<string, object> el in foodDetials)
                {
                    if (el != null)
                    {
                        var dicPara = new Dictionary<string, object>(el, StringComparer.OrdinalIgnoreCase);
                        var foodDetailType = dicPara.Get("foodDetailType").Toint();
                        var balanceType = dicPara.Get("balanceType").Toint();
                        var operateType = dicPara.Get("operateType").Toint();
                        var containId = dicPara.Get("containId").Toint();
                        var containCount = dicPara.Get("containCount").Toint();
                        var weightValue = dicPara.Get("weightValue").Todecimal();
                        var days = dicPara.Get("days").Toint();

                        if (!foodDetailType.Nonempty("套餐内容类别", out errMsg)) 
                            return false;

                        if (foodDetailType == (int)FoodDetailType.Coin)
                        {
                            if (!balanceType.Nonempty("会员币种种类", out errMsg))
                                return false;
                            if (!operateType.Nonempty("会员币种处理方式", out errMsg))
                                return false;
                        }
                        
                        if (!containCount.Illegalint("内容数量", out errMsg)) 
                            return false;
                        if (!weightValue.Illegaldec("权重价值", out errMsg)) 
                            return false;

                        if (foodDetailType == (int)FoodDetailType.Ticket
                            || foodDetailType == (int)FoodDetailType.Coupon)
                        {
                            if (!days.Illegalint("有效天数", out errMsg))
                                return false;
                        }

                        var data_Food_DetialModel = new Data_Food_Detial();
                        data_Food_DetialModel.FoodID = iFoodId;
                        data_Food_DetialModel.Status = 1;
                        data_Food_DetialModel.FoodType = foodDetailType;
                        data_Food_DetialModel.BalanceType = balanceType;
                        data_Food_DetialModel.ContainCount = containCount;
                        data_Food_DetialModel.ContainID = containId;
                        data_Food_DetialModel.WeightType = (int)WeightType.Money;
                        data_Food_DetialModel.WeightValue = weightValue;
                        data_Food_DetialModel.Days = days;
                        data_Food_DetialModel.OperateType = operateType;
                        dbContext.Entry(data_Food_DetialModel).State = EntityState.Added;
                    }
                    else
                    {
                        errMsg = "提交数据包含空对象";
                        return false;
                    }
                }

                if (dbContext.SaveChanges() < 0)
                {
                    errMsg = "保存套餐内容信息失败";
                    return false;
                }
            }

            return true;
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetFoodInfoList(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as MerchDataModel).MerchID;
                string errMsg = string.Empty;

                object[] conditions = dicParas.ContainsKey("conditions") ? (object[])dicParas["conditions"] : null;

                SqlParameter[] parameters = new SqlParameter[1];
                string sqlWhere = string.Empty;
                parameters[0] = new SqlParameter("@MerchId", merchId);

                if (conditions != null && conditions.Length > 0)
                {
                    if (!QueryBLL.GenDynamicSql(conditions, "a.", ref sqlWhere, ref parameters, out errMsg))
                    {
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                }

                string sql = @"select a.FoodID, a.FoodName,a.ClientPrice,a.MemberPrice, b.DictKey as FoodTypeStr, (case a.AllowInternet when 1 then '允许' when 0 then '禁止' else '' end) as AllowInternet, " +
                    " (case a.AllowPrint when 1 then '允许' when 0 then '禁止' else '' end) as AllowPrint, (case a.ForeAuthorize when 1 then '允许' when 0 then '禁止' else '' end) as ForeAuthorize, " +
                    " (case when a.StartTime is null or a.StartTime='' then '' else convert(varchar,a.StartTime,23) end) as StartTime, (case when a.EndTime is null or a.EndTime='' then '' else convert(varchar,a.EndTime,23) end) as EndTime from Data_FoodInfo a " +
                    " left join (select b.* from Dict_System a inner join Dict_System b on a.ID=b.PID where a.DictKey='套餐类别' and a.PID=0) b on convert(varchar, a.FoodType)=b.DictValue " +
                    " where a.MerchID=@MerchId and a.FoodState=1 ";
                sql = sql + sqlWhere;

                IData_FoodInfoService data_FoodInfoService = Data_FoodInfoService.I;
                var data_FoodInfo = data_FoodInfoService.SqlQuery<Data_FoodInfoListModel>(sql, parameters).ToList();
                
                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, data_FoodInfo);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 获取入会套餐
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetMemberFoodDic(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as MerchDataModel).MerchID;

                var linq = from a in Data_FoodInfoService.I.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && p.FoodState == (int)FoodState.Valid && p.FoodType == (int)FoodType.Member)                          
                           select new
                           {
                               FoodID = a.FoodID,
                               FoodName = a.FoodName
                           };

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, linq);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetFoodInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;               
                string foodId = dicParas.ContainsKey("foodId") ? (dicParas["foodId"] + "") : string.Empty;

                if (string.IsNullOrEmpty(foodId))
                {
                    errMsg = "套餐ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                int iFoodId = Convert.ToInt32(foodId);                                               
                var FoodInfo = Data_FoodInfoService.I.GetModels(p => p.FoodID == iFoodId).FirstOrDefault();
                if(FoodInfo == null)
                {
                    errMsg = "该套餐不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var FoodLevels = from c in
                                     (from a in Data_Food_LevelService.N.GetModels(p => p.FoodID == iFoodId)
                                      join b in Data_MemberLevelService.N.GetModels(p => p.State == 1) on a.MemberLevelID equals b.MemberLevelID
                                      select new
                                      {
                                          a = a,
                                          MemberLevelID = b.MemberLevelID,
                                          MemberLevelName = b.MemberLevelName
                                      }).AsEnumerable()
                                 group c by new { c.a.TimeType, c.a.Week, c.a.StartTime, c.a.EndTime } into g
                                 select new
                                 {
                                     MemberLevelIDs = string.Join("|", g.OrderBy(o=>o.MemberLevelID).Select(s => s.MemberLevelID)),
                                     MemberLevels = string.Join("|", g.OrderBy(o => o.MemberLevelName).Select(s => s.MemberLevelName)),
                                     TimeType = g.FirstOrDefault().a.TimeType,
                                     Week = g.FirstOrDefault().a.Week,
                                     WeekStr = getWeekName(g.FirstOrDefault().a.Week),
                                     StartTime = string.Format("{0:c}", g.FirstOrDefault().a.StartTime).Substring(0, 5),
                                     EndTime = string.Format("{0:c}", g.FirstOrDefault().a.EndTime).Substring(0, 5),
                                     Time = getTimeName(g.FirstOrDefault().a.StartTime, g.FirstOrDefault().a.EndTime),
                                     ClientPrice = g.FirstOrDefault().a.ClientPrice,
                                     VIPPrice = g.FirstOrDefault().a.VIPPrice,
                                     day_sale_count = g.FirstOrDefault().a.day_sale_count,
                                     member_day_sale_count = g.FirstOrDefault().a.member_day_sale_count,
                                     UpdateLevelID = g.FirstOrDefault().a.UpdateLevelID
                                 };                                 

                int FoodDetailId = Dict_SystemService.I.GetModels(p => p.DictKey.Equals("套餐内容") && p.PID == 0).FirstOrDefault().ID;
                int FoodDetailTypeId = Dict_SystemService.I.GetModels(p => p.DictKey.Equals("套餐类别") && p.PID == FoodDetailId).FirstOrDefault().ID;
                int FeeTypeId = Dict_SystemService.I.GetModels(p => p.DictKey.Equals("计费方式")).FirstOrDefault().ID;
                var FoodDetials = from a in Data_Food_DetialService.N.GetModels(p => p.FoodID == iFoodId && p.Status == 1)
                                  join b in Data_ProjectInfoService.N.GetModels() on new { ContainID = a.ContainID, FoodType = a.FoodType } equals new { ContainID = (int?)b.ID, FoodType = (int?)FoodDetailType.Ticket } into b1
                                  from b in b1.DefaultIfEmpty()
                                  join c in Base_GoodsInfoService.N.GetModels() on new { ContainID = a.ContainID, FoodType = a.FoodType } equals new { ContainID = (int?)c.ID, FoodType = (int?)FoodDetailType.Good } into c1
                                  from c in c1.DefaultIfEmpty()
                                  join d in Dict_SystemService.N.GetModels(p => p.PID == FoodDetailTypeId) on (a.FoodType + "") equals d.DictValue into d1
                                  from d in d1.DefaultIfEmpty()
                                  join f in Dict_BalanceTypeService.N.GetModels(p=>p.State == 1) on a.BalanceType equals f.ID into f1
                                  from f in f1.DefaultIfEmpty()
                                  join g in Data_CouponInfoService.N.GetModels() on new { ContainID = a.ContainID, FoodType = a.FoodType } equals new { ContainID = (int?)g.ID, FoodType = (int?)FoodDetailType.Coupon } into g1
                                  from g in g1.DefaultIfEmpty()
                                  join h in Data_DigitCoinFoodService.N.GetModels() on new { ContainID = a.ContainID, FoodType = a.FoodType } equals new { ContainID = (int?)h.ID, FoodType = (int?)FoodDetailType.Digit } into h1
                                  from h in h1.DefaultIfEmpty()
                                  select new
                                  {
                                      ID = a.ID,
                                      ContainID = a.ContainID,
                                      ProjectName = (a.FoodType == (int)FoodDetailType.Coin) ? "游戏币" : 
                                                    (a.FoodType == (int)FoodDetailType.Digit) ? (h != null ? h.FoodName : string.Empty) : 
                                                    (a.FoodType == (int)FoodDetailType.Good) ? (c != null ? c.GoodName : string.Empty) :
                                                    (a.FoodType == (int)FoodDetailType.Ticket) ? (b != null ? b.ProjectName : string.Empty) : 
                                                    (a.FoodType == (int)FoodDetailType.Coupon) ? (g != null ? g.CouponName : string.Empty) : string.Empty,
                                      FoodDetailType = a.FoodType,                                      
                                      FoodDetailTypeStr = (a.FoodType == (int)FoodDetailType.Coin) ? (f != null ? f.TypeName : string.Empty) : (d != null ? d.DictKey : string.Empty),
                                      ContainCount = a.ContainCount,
                                      Days = a.Days,
                                      WeightValue = a.WeightValue,
                                      OperateType = a.OperateType
                                  };

                var FoodSales = from a in Data_Food_SaleService.N.GetModels(p => p.FoodID == iFoodId)
                                join b in Dict_BalanceTypeService.N.GetModels() on a.BalanceType equals b.ID
                                select new {
                                    BalanceType = a.BalanceType,
                                    BalanceTypeStr = b.TypeName,
                                    UseCount = a.UseCount
                                };

                var result = new
                {
                    FoodID = FoodInfo.FoodID,
                    FoodName = FoodInfo.FoodName,
                    FoodType = FoodInfo.FoodType,
                    StartTime = string.Format("{0:yyyy-MM-dd}", FoodInfo.StartTime),
                    EndTime = string.Format("{0:yyyy-MM-dd}", FoodInfo.EndTime),
                    ImageURL = FoodInfo.ImageURL,
                    AllowInternet = FoodInfo.AllowInternet,
                    MeituanID = FoodInfo.MeituanID,
                    DianpinID = FoodInfo.DianpinID,
                    KoubeiID = FoodInfo.KoubeiID,
                    AllowPrint = FoodInfo.AllowPrint,
                    ForeAuthorize = FoodInfo.ForeAuthorize,
                    ClientPrice = FoodInfo.ClientPrice,
                    MemberPrice = FoodInfo.MemberPrice,
                    RenewDays = FoodInfo.RenewDays,
                    FoodDetials = FoodDetials,
                    FoodLevels = FoodLevels,
                    FoodSales = FoodSales
                };

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, result);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }        

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object SaveFoodInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as MerchDataModel).MerchID;

                string errMsg = string.Empty;
                string foodId = dicParas.ContainsKey("foodId") ? (dicParas["foodId"] + "") : string.Empty;
                string foodName = dicParas.ContainsKey("foodName") ? (dicParas["foodName"] + "") : string.Empty;
                string foodType = dicParas.ContainsKey("foodType") ? (dicParas["foodType"] + "") : string.Empty;
                string startTime = dicParas.ContainsKey("startTime") ? (dicParas["startTime"] + "") : string.Empty;                
                string endTime = dicParas.ContainsKey("endTime") ? (dicParas["endTime"] + "") : string.Empty;                
                string allowInternet = dicParas.ContainsKey("allowInternet") ? (dicParas["allowInternet"] + "") : string.Empty;
                string meituanID = dicParas.ContainsKey("meituanID") ? (dicParas["meituanID"] + "") : string.Empty;
                string dianpinID = dicParas.ContainsKey("dianpinID") ? (dicParas["dianpinID"] + "") : string.Empty;
                string koubeiID = dicParas.ContainsKey("koubeiID") ? (dicParas["koubeiID"] + "") : string.Empty;
                string allowPrint = dicParas.ContainsKey("allowPrint") ? (dicParas["allowPrint"] + "") : string.Empty;
                string foreAuthorize = dicParas.ContainsKey("foreAuthorize") ? (dicParas["foreAuthorize"] + "") : string.Empty;
                string clientPrice = dicParas.ContainsKey("clientPrice") ? (dicParas["clientPrice"] + "") : string.Empty;
                string memberPrice = dicParas.ContainsKey("memberPrice") ? (dicParas["memberPrice"] + "") : string.Empty;
                string renewDays = dicParas.ContainsKey("renewDays") ? (dicParas["renewDays"] + "") : string.Empty;
                string imageUrl = dicParas.ContainsKey("imageUrl") ? (dicParas["imageUrl"] + "") : string.Empty;
                object[] foodSales = dicParas.ContainsKey("foodSales") ? (object[])dicParas["foodSales"] : null;
                object[] foodDetials = dicParas.ContainsKey("foodDetials") ? (object[])dicParas["foodDetials"] : null;
                object[] foodLevels = dicParas.ContainsKey("foodLevels") ? (object[])dicParas["foodLevels"] : null;
                int iFoodId = 0;
                int.TryParse(foodId, out iFoodId);

                #region 验证参数

                if (string.IsNullOrEmpty(foodName))
                {
                    errMsg = "套餐名称不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (string.IsNullOrEmpty(foodType))
                {
                    errMsg = "套餐类别不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (string.IsNullOrEmpty(startTime))
                {
                    errMsg = "有效期开始时间不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (string.IsNullOrEmpty(endTime))
                {
                    errMsg = "有效期结束时间不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (Convert.ToDateTime(startTime) > Convert.ToDateTime(endTime))
                {
                    errMsg = "开始时间不能晚于结束时间";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }                

                if (string.IsNullOrEmpty(clientPrice))
                {
                    errMsg = "散客售价不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (string.IsNullOrEmpty(memberPrice))
                {
                    errMsg = "会员售价不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (Convert.ToDecimal(clientPrice) < 0)
                {
                    errMsg = "散客售价不能为负数";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (Convert.ToDecimal(memberPrice) < 0)
                {
                    errMsg = "会员售价不能为负数";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                #endregion

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        IData_FoodInfoService data_FoodInfoService = Data_FoodInfoService.I;
                        var data_FoodInfo = new Data_FoodInfo();
                        data_FoodInfo.FoodID = iFoodId;
                        data_FoodInfo.FoodName = foodName;
                        data_FoodInfo.FoodType = Convert.ToInt32(foodType);
                        data_FoodInfo.StartTime = Convert.ToDateTime(startTime);
                        data_FoodInfo.EndTime = Convert.ToDateTime(endTime);
                        data_FoodInfo.AllowPrint = !string.IsNullOrEmpty(allowPrint) ? Convert.ToInt32(allowPrint) : (int?)null;
                        data_FoodInfo.AllowInternet = !string.IsNullOrEmpty(allowInternet) ? Convert.ToInt32(allowInternet) : (int?)null;
                        data_FoodInfo.MeituanID = meituanID;
                        data_FoodInfo.DianpinID = dianpinID;
                        data_FoodInfo.KoubeiID = koubeiID;
                        data_FoodInfo.ForeAuthorize = !string.IsNullOrEmpty(foreAuthorize) ? Convert.ToInt32(foreAuthorize) : (int?)null;
                        data_FoodInfo.MemberPrice = Convert.ToDecimal(memberPrice);
                        data_FoodInfo.ClientPrice = Convert.ToDecimal(clientPrice);
                        data_FoodInfo.RenewDays = !string.IsNullOrEmpty(renewDays) ? Convert.ToInt32(renewDays) : (int?)null;
                        data_FoodInfo.MerchID = merchId;
                        data_FoodInfo.ImageURL = imageUrl;
                        if (!data_FoodInfoService.Any(a => a.FoodID == iFoodId))
                        {
                            //新增
                            data_FoodInfo.FoodState = (int)FoodState.Valid;
                            if (!data_FoodInfoService.Add(data_FoodInfo))
                            {
                                errMsg = "添加套餐信息失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }
                        else
                        {
                            //修改
                            if (!data_FoodInfoService.Update(data_FoodInfo))
                            {
                                errMsg = "修改套餐信息失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }

                        iFoodId = data_FoodInfo.FoodID;
                        //保存消耗余额类别设定
                        if (!saveFoodSale(iFoodId, foodSales, out errMsg))
                        {
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }
                        
                        //保存套餐优惠时段信息
                        if (!saveFoodLevel(iFoodId, foodLevels, out errMsg))
                        {
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        //保存套餐内容信息
                        if (!saveFoodDetail(iFoodId, foodDetials, out errMsg))
                        {
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        ts.Complete();
                    }
                    catch (Exception ex)
                    {
                        errMsg = ex.Message;
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }                    
                }

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object DelFoodInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                int foodId = dicParas.Get("foodId").Toint(0);
                
                #region 验证参数

                if (foodId == 0)
                {
                    errMsg = "套餐ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }
               
                #endregion

                var data_FoodInfo = Data_FoodInfoService.I.GetModels(p => p.FoodID == foodId).FirstOrDefault();
                data_FoodInfo.FoodState = (int)FoodState.Invalid;
                if (!Data_FoodInfoService.I.Update(data_FoodInfo))
                {
                    errMsg = "删除套餐信息失败";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }        

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetGoodsInfoList(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as MerchDataModel).MerchID;
                string errMsg = string.Empty;                
                string goodNameOrBarcode = dicParas.ContainsKey("goodNameOrBarcode") ? (dicParas["goodNameOrBarcode"] + "") : string.Empty;

                IDict_SystemService dict_SystemService = Dict_SystemService.N;
                int GoodTypeId = dict_SystemService.GetModels(p => p.DictKey.Equals("商品类别")).FirstOrDefault().ID;

                IBase_GoodsInfoService base_GoodsInfoService = Base_GoodsInfoService.N;
                var query = base_GoodsInfoService.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && p.GoodType == (int)GoodType.Good && p.Status == 1);
                if (!string.IsNullOrEmpty(goodNameOrBarcode))
                {
                    query = query.Where(w => w.GoodName.Contains(goodNameOrBarcode) || w.Barcode.Contains(goodNameOrBarcode));
                }

                var linq = from a in query
                           join b in dict_SystemService.GetModels(p => p.PID == GoodTypeId) on (a.GoodType + "") equals b.DictValue into b1
                           from b in b1.DefaultIfEmpty()
                           select new
                           {
                               ID = a.ID,
                               Barcode = a.Barcode,
                               GoodName = a.GoodName,
                               GoodTypeStr = b != null ? b.DictKey : string.Empty
                           };

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, linq.ToList());
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object UploadFoodPhoto(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                Dictionary<string, string> imageInfo = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                List<string> imageUrls = null;
                if (!Utils.UploadImageFile("/XCCloud/Promotion/Food/", out imageUrls, out errMsg))
                {
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                imageInfo.Add("ImageURL", imageUrls.FirstOrDefault());

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, imageInfo);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetFoodStores(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                int foodId = dicParas.Get("foodId").Toint(0);

                var storeIDs = Data_Food_StoreListService.I.GetModels(p => p.FoodID == foodId).Select(o => new { StoreID = o.StoreID });
                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, storeIDs);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object SaveFoodStores(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string foodId = dicParas.ContainsKey("foodId") ? (dicParas["foodId"] + "") : string.Empty;
                string storeIds = dicParas.ContainsKey("storeIds") ? (dicParas["storeIds"] + "") : string.Empty;

                if (string.IsNullOrEmpty(foodId))
                {
                    errMsg = "套餐ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        int iFoodId = Convert.ToInt32(foodId);
                        var data_Food_StoreListService = Data_Food_StoreListService.I;
                        foreach (var model in data_Food_StoreListService.GetModels(p => p.FoodID == iFoodId))
                        {
                            data_Food_StoreListService.DeleteModel(model);
                        }

                        var storeIdArr = storeIds.Split('|');
                        foreach (var storeId in storeIdArr)
                        {
                            var model = new Data_Food_StoreList();
                            model.FoodID = iFoodId;
                            model.StoreID = storeId;
                            data_Food_StoreListService.AddModel(model);
                        }

                        if (!data_Food_StoreListService.SaveChanges())
                        {
                            errMsg = "更新套餐适用门店表失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        ts.Complete();
                    }
                    catch (Exception ex)
                    {
                        errMsg = ex.Message;
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                }
                
                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }
    }
}