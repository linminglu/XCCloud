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
        private string merchId;
        private string merchSecret;

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
                foreach (var model in Data_Food_SaleService.I.GetModels(p => p.FoodID == iFoodId))
                {
                    Data_Food_SaleService.I.DeleteModel(model, true, merchId, merchSecret);
                }

                foreach (IDictionary<string, object> el in foodSales)
                {
                    if (el != null)
                    {
                        var dicPara = new Dictionary<string, object>(el, StringComparer.OrdinalIgnoreCase);
                        var balanceType = dicPara.Get("balanceType").Toint();
                        var useCount = dicPara.Get("useCount").Toint();
                        var cashValue = dicPara.Get("cashValue").Todecimal();

                        if (!balanceType.Nonempty("余额类别", out errMsg))
                            return false;

                        if (!useCount.Validint("消耗数量", out errMsg))
                            return false;

                        if (!cashValue.Validdecimal("现金", out errMsg))
                            return false;   

                        var data_Food_SaleModel = new Data_Food_Sale();
                        data_Food_SaleModel.FoodID = iFoodId;
                        data_Food_SaleModel.BalanceType = balanceType;
                        data_Food_SaleModel.UseCount = useCount;
                        data_Food_SaleModel.CashValue = cashValue;
                        Data_Food_SaleService.I.AddModel(data_Food_SaleModel, true, merchId, merchSecret);
                    }
                    else
                    {
                        errMsg = "提交数据包含空对象";
                        return false;
                    }
                }

                if (!Data_Food_SaleService.I.SaveChanges())
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
                //先删除，后添加
                foreach (var model in Data_Food_LevelService.I.GetModels(p => p.FoodID == iFoodId))
                {
                    Data_Food_LevelService.I.DeleteModel(model, true, merchId, merchSecret);
                }

                foreach (IDictionary<string, object> el in foodLevels)
                {
                    if (el != null)
                    {
                        var dicPara = new Dictionary<string, object>(el, StringComparer.OrdinalIgnoreCase);
                        var memberLevelIDs = dicPara.Get("memberLevelIDs");
                        var timeType = dicPara.Get("timeType").Toint();
                        var week = dicPara.Get("week");
                        var start = dicPara.Get("startTime").Totimespan();
                        var end = dicPara.Get("endTime").Totimespan();
                        var client = dicPara.Get("clientPrice").Todecimal();
                        var vip = dicPara.Get("vipPrice").Todecimal();
                        var allFreqType = dicPara.Get("allFreqType").Toint();
                        var allCount = dicPara.Get("allCount").Toint();
                        var memberFreqType = dicPara.Get("memberFreqType").Toint();
                        var memberCount = dicPara.Get("memberCount").Toint();
                        var updateLevelId = dicPara.Get("updateLevelId").Toint();
                        var priorityLevel = dicPara.Get("priorityLevel").Toint();

                        //参数验证
                        if (!start.Nonempty("优惠时段", out errMsg) || !end.Nonempty("优惠时段", out errMsg))
                            return false;

                        if (TimeSpan.Compare(start.Value, end.Value) > 0)
                        {
                            errMsg = "优惠开始时段不能晚于结束时段";
                            return false;
                        }

                        if (!memberLevelIDs.Nonempty("会员级别", out errMsg))
                            return false;
                        if (!allFreqType.Validint("整场限购频率", out errMsg))
                            return false;
                        if (!allCount.Validint("整场限购数量", out errMsg))
                            return false;
                        if (!memberFreqType.Validint("会员限购频率", out errMsg))
                            return false;
                        if (!memberCount.Validint("会员限购数量", out errMsg))
                            return false;
                        if (!priorityLevel.Validint("优先级", out errMsg))
                            return false;
                        if (!client.Validdecimal("散客优惠价", out errMsg))
                            return false;
                        if (!vip.Validdecimal("会员优惠价", out errMsg))
                            return false;
                        if (!timeType.Nonempty("时段类型", out errMsg))
                            return false;

                        if (timeType == (int)TimeType.Custom && string.IsNullOrEmpty(week))
                        {
                            errMsg = "自定义模式周数不能为空";
                            return false;
                        }                        

                        foreach (var memberLevelID in memberLevelIDs.Split('|'))
                        {
                            if (!memberLevelID.Nonempty("会员等级ID", out errMsg))
                                return false;

                            var data_Food_LevelModel = new Data_Food_Level();
                            data_Food_LevelModel.FoodID = iFoodId;
                            data_Food_LevelModel.MemberLevelID = memberLevelID.Toint();
                            data_Food_LevelModel.TimeType = timeType;
                            data_Food_LevelModel.Week = week;
                            data_Food_LevelModel.StartTime = start;
                            data_Food_LevelModel.EndTime = end;
                            data_Food_LevelModel.ClientPrice = client;
                            data_Food_LevelModel.VIPPrice = vip;
                            data_Food_LevelModel.AllFreqType = allFreqType;
                            data_Food_LevelModel.AllCount = allCount;
                            data_Food_LevelModel.MemberFreqType = memberFreqType;
                            data_Food_LevelModel.MemberCount = memberCount;
                            data_Food_LevelModel.PriorityLevel = priorityLevel;                            
                            data_Food_LevelModel.UpdateLevelID = updateLevelId;
                            Data_Food_LevelService.I.AddModel(data_Food_LevelModel, true, merchId, merchSecret);
                        }                        
                    }
                    else
                    {
                        errMsg = "提交数据包含空对象";
                        return false;
                    }
                }

                //同一会员级别，时段类型为工作模式（即0~2）与自定义模式（即3）不能共存   
                //foreach (var memberLevelId in foodLevelList.GroupBy(g => g.MemberLevelID).Select(o => o.Key))
                //{
                //    if (foodLevelList.Any(w => w.MemberLevelID.Equals(memberLevelId) && w.TimeType.Equals((int)TimeType.Custom)) &&
                //        foodLevelList.Any(w => w.MemberLevelID.Equals(memberLevelId) && !w.TimeType.Equals((int)TimeType.Custom)))
                //    {
                //        string memberLevelName = (from b in Data_MemberLevelService.I.GetModels(p => p.MemberLevelID == memberLevelId) select b.MemberLevelName).FirstOrDefault();
                //        errMsg = string.Format("同一会员级别，自定义模式与其它模式不能混合定义 会员级别:{0}", memberLevelName);
                //        return false;
                //    }
                //}
                //var query = from a in foodLevelList.Where(w => w.TimeType == (int)TimeType.Custom)
                //            join b in foodLevelList.Where(w => w.TimeType != (int)TimeType.Custom) 
                //            on a.MemberLevelID equals b.MemberLevelID
                //            select a;
                //if (query.Any())
                //{
                //    var memberLevelId = query.First().MemberLevelID;
                //    string memberLevelName = (from b in Data_MemberLevelService.I.GetModels(p => p.MemberLevelID == memberLevelId) select b.MemberLevelName).FirstOrDefault();
                //    errMsg = string.Format("同一会员级别，自定义模式与其它模式不能混合定义 会员级别:{0}", memberLevelName);
                //    return false;
                //}
                
                ////同一会员级别，同一个时段类型（如果是自定义模式，即同一天），同一时段只能有一个优惠策略
                //foreach (var data_Food_LevelModel in foodLevelList)
                //{
                //    int memberLevelId = (int)data_Food_LevelModel.MemberLevelID;                                        
                //    if (data_Food_LevelModel.TimeType == (int)TimeType.Custom)
                //    {
                //        var weeks = data_Food_LevelModel.Week.Split('|').ToList();
                //        foreach (var day in weeks)
                //        {
                //            if (foodLevelList.Where(w => w.MemberLevelID.Equals(data_Food_LevelModel.MemberLevelID) && w.Week.Contains(day) &&
                //                 ((TimeSpan.Compare((TimeSpan)data_Food_LevelModel.StartTime, (TimeSpan)w.StartTime) >= 0 && TimeSpan.Compare((TimeSpan)data_Food_LevelModel.StartTime, (TimeSpan)w.EndTime) < 0) ||
                //                 (TimeSpan.Compare((TimeSpan)data_Food_LevelModel.EndTime, (TimeSpan)w.StartTime) > 0 && TimeSpan.Compare((TimeSpan)data_Food_LevelModel.EndTime, (TimeSpan)w.EndTime) <= 0))).Count() > 1)
                //            {
                //                string memberLevelName = (from b in Data_MemberLevelService.I.GetModels(p => p.MemberLevelID == memberLevelId) select b.MemberLevelName).FirstOrDefault();
                //                string timeSpan = Utils.TimeSpanToStr(data_Food_LevelModel.StartTime.Value) + "~" + Utils.TimeSpanToStr(data_Food_LevelModel.EndTime.Value);
                //                errMsg = string.Format("同一会员级别，同一时段只能有一个优惠策略 会员级别:{0} 自定义模式:{1} 优惠时段:{2}", memberLevelName, getWeekName(day), timeSpan);
                //                return false;
                //            }
                //        }
                //    }
                //    else
                //    {
                //        if (foodLevelList.Where(w => w.MemberLevelID.Equals(data_Food_LevelModel.MemberLevelID) && w.TimeType.Equals(data_Food_LevelModel.TimeType) &&
                //                 ((TimeSpan.Compare((TimeSpan)data_Food_LevelModel.StartTime, (TimeSpan)w.StartTime) >= 0 && TimeSpan.Compare((TimeSpan)data_Food_LevelModel.StartTime, (TimeSpan)w.EndTime) < 0) ||
                //                 (TimeSpan.Compare((TimeSpan)data_Food_LevelModel.EndTime, (TimeSpan)w.StartTime) > 0 && TimeSpan.Compare((TimeSpan)data_Food_LevelModel.EndTime, (TimeSpan)w.EndTime) <= 0))).Count() > 1)
                //        {
                //            string memberLevelName = (from b in Data_MemberLevelService.I.GetModels(p => p.MemberLevelID == memberLevelId) select b.MemberLevelName).FirstOrDefault();
                //            string timeSpan = Utils.TimeSpanToStr(data_Food_LevelModel.StartTime.Value) + "~" + Utils.TimeSpanToStr(data_Food_LevelModel.EndTime.Value);
                //            errMsg = string.Format("同一会员级别，同一时段只能有一个优惠策略 会员级别:{0} 其它模式:{1} 优惠时段:{2}", memberLevelName, getPeriodTypeName((TimeType)data_Food_LevelModel.TimeType), timeSpan);
                //            return false;
                //        }
                //    }
                //}
                
                if (!Data_Food_LevelService.I.SaveChanges())
                {
                    errMsg = "保存套餐级别信息失败";
                    return false;
                }
            }

            return true;
        }

        private bool isQuickFood(object el)
        {
            if ((el as IDictionary<string, object>) != null)
            {
                var dicPara = new Dictionary<string, object>((el as IDictionary<string, object>), StringComparer.OrdinalIgnoreCase);
                var foodDetailType = dicPara.Get("foodDetailType").Toint();
                if (foodDetailType == (int)FoodDetailType.Coin)
                {
                    var containId = dicPara.Get("containId").Toint();
                    return Dict_BalanceTypeService.I.Any(a => a.ID == containId && a.MappingType == (int)HKType.Coin);
                }                
            }

            return false;
        }

        private bool saveFoodDetail(string storeIds, int? allowQuickFood, int iFoodId, object[] foodDetials, out string errMsg)
        {
            errMsg = string.Empty;

            //快速售币套餐
            if (allowQuickFood == 1 && (foodDetials == null || foodDetials.Count() != 1 || !isQuickFood(foodDetials[0])))
            {
                errMsg = "快速售币套餐内容必须有且仅有一个代币种类的项目";
                return false;
            }

            if (foodDetials != null && foodDetials.Count() >= 0)
            {                
                //先删除，后添加
                foreach (var model in Data_Food_DetialService.I.GetModels(p=>p.FoodID == iFoodId))
                {
                    Data_Food_DetialService.I.DeleteModel(model, true, merchId, merchSecret);
                }
                
                foreach (IDictionary<string, object> el in foodDetials)
                {
                    if (el != null)
                    {
                        var dicPara = new Dictionary<string, object>(el, StringComparer.OrdinalIgnoreCase);
                        var foodDetailType = dicPara.Get("foodDetailType").Toint();
                        //var balanceType = dicPara.Get("balanceType").Toint();
                        var operateType = dicPara.Get("operateType").Toint();
                        var containId = dicPara.Get("containId").Toint();
                        var containCount = dicPara.Get("containCount").Toint();
                        var weightValue = dicPara.Get("weightValue").Todecimal();
                        //var days = dicPara.Get("days").Toint();

                        if (!foodDetailType.Nonempty("套餐内容类别", out errMsg)) 
                            return false;

                        if (foodDetailType == (int)FoodDetailType.Coin)
                        {
                            if (!containId.Nonempty("会员币种种类", out errMsg))
                                return false;
                            if (!operateType.Nonempty("会员币种处理方式", out errMsg))
                                return false;
                        }
                        
                        if (!containCount.Validint("内容数量", out errMsg)) 
                            return false;
                        if (!weightValue.Validdecimal("权重价值", out errMsg)) 
                            return false;

                        if (foodDetailType == (int)FoodDetailType.Ticket)
                        {
                            if (!Data_ProjectTicketService.I.Any(p => p.ID == containId && (p.StoreID ?? "").Equals(storeIds, StringComparison.OrdinalIgnoreCase)))
                            {
                                errMsg = "套餐门票必须且只能选择单适用门店";
                                return false;
                            }
                        }

                        if (foodDetailType == (int)FoodDetailType.Good)
                        {
                            if (storeIds.IsNull() || storeIds.Contains("|"))
                            {
                                if (Base_GoodsInfoService.I.Any(p => p.ID == containId && p.Status == 1 && (p.StoreID ?? "") != ""))
                                {
                                    if (storeIds.IsNull())
                                    {
                                        errMsg = "无适用门店的套餐只能选择公有商品";
                                    }

                                    if (storeIds.Contains("|"))
                                    {
                                        errMsg = "适用于多个门店的套餐只能选择公有商品";
                                    }

                                    return false;
                                }
                            }
                            else
                            {
                                if (!Base_GoodsInfoService.I.Any(p => p.ID == containId && p.Status == 1 && ((p.StoreID ?? "") == "" || p.StoreID.Equals(storeIds, StringComparison.OrdinalIgnoreCase))))
                                {
                                    errMsg = "兑换的商品不属于选定的适用门店";
                                    return false;
                                }
                            }                            
                        }

                        var data_Food_DetialModel = new Data_Food_Detial();
                        data_Food_DetialModel.FoodID = iFoodId;
                        data_Food_DetialModel.Status = 1;
                        data_Food_DetialModel.FoodType = foodDetailType;
                        //data_Food_DetialModel.BalanceType = balanceType;
                        data_Food_DetialModel.ContainCount = containCount;
                        data_Food_DetialModel.ContainID = containId;
                        //data_Food_DetialModel.WeightType = (int)WeightType.Money;
                        data_Food_DetialModel.WeightValue = weightValue;
                        //data_Food_DetialModel.Days = days;
                        data_Food_DetialModel.OperateType = operateType;
                        Data_Food_DetialService.I.AddModel(data_Food_DetialModel, true, merchId, merchSecret);
                    }
                    else
                    {
                        errMsg = "提交数据包含空对象";
                        return false;
                    }
                }

                if (!Data_Food_DetialService.I.SaveChanges())
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
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
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

                string sql = @"select a.ID as FoodID, a.FoodName,a.ClientPrice,a.MemberPrice, b.DictKey as FoodTypeStr, (case a.AllowInternet when 1 then '允许' when 0 then '禁止' else '' end) as AllowInternet, " +
                    " (case a.AllowPrint when 1 then '允许' when 0 then '禁止' else '' end) as AllowPrint, (case a.ForeAuthorize when 1 then '允许' when 0 then '禁止' else '' end) as ForeAuthorize, (case a.AllowQuickFood when 1 then '是' when 0 then '否' else '' end) as AllowQuickFood, " +
                    " (case when a.StartTime is null or a.StartTime='' then '' else convert(varchar,a.StartTime,23) end) as StartTime, (case when a.EndTime is null or a.EndTime='' then '' else convert(varchar,a.EndTime,23) end) as EndTime from Data_FoodInfo a " +
                    " left join Dict_System b on a.FoodType=b.ID " +
                    " where a.MerchID=@MerchId and a.FoodState=1 ";
                sql = sql + sqlWhere;

                var data_FoodInfo = Data_FoodInfoService.I.SqlQuery<Data_FoodInfoListModel>(sql, parameters).ToList();
                
                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, data_FoodInfo);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 获取公共礼包字典列表
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetFoodInfoDic(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                
                var foodIdForPrivateGood = from a in Data_Food_DetialService.N.GetModels(p => p.FoodType == (int)FoodDetailType.Good)
                              join b in Base_GoodsInfoService.N.GetModels(p => (p.StoreID ?? "") != "") on a.ContainID equals b.ID
                              join c in Data_FoodInfoService.N.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && p.FoodState == (int)FoodState.Valid) on a.FoodID equals c.ID
                              select a.FoodID;

                var foodIdContainedTicket = from a in Data_Food_DetialService.N.GetModels(p => p.FoodType == (int)FoodDetailType.Ticket)
                                            join b in Data_FoodInfoService.N.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && p.FoodState == (int)FoodState.Valid) on a.FoodID equals b.ID
                                            select a.FoodID;

                //排除包含私有属性的套餐
                var linq = from a in Data_FoodInfoService.N.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && p.FoodState == (int)FoodState.Valid)
                           where !foodIdForPrivateGood.Contains(a.ID) && !foodIdContainedTicket.Contains(a.ID)
                           orderby a.FoodName
                           select new
                           {
                               FoodID = a.ID,
                               FoodName = a.FoodName
                           };

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, linq);
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
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

                var linq = from a in Data_FoodInfoService.I.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && p.FoodState == (int)FoodState.Valid)                          
                           select new
                           {
                               FoodID = a.ID,
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
                int foodId = dicParas.Get("foodId").Toint(0);

                if (foodId == 0)
                {
                    errMsg = "套餐ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }
                                             
                var FoodInfo = Data_FoodInfoService.I.GetModels(p => p.ID == foodId).FirstOrDefault();
                if(FoodInfo == null)
                {
                    errMsg = "该套餐不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                //获取适用门店
                var StoreIDs = Data_Food_StoreListService.I.GetModels(p => p.FoodID == foodId).Select(o => new { StoreID = o.StoreID });

                var FoodLevels = from c in
                                     (from a in Data_Food_LevelService.N.GetModels(p => p.FoodID == foodId)
                                      join b in Data_MemberLevelService.N.GetModels(p => p.State == 1) on a.MemberLevelID equals b.ID
                                      join c in Data_MemberLevelService.N.GetModels(p => p.State == 1) on a.UpdateLevelID equals c.ID
                                      select new
                                      {
                                          a = a,
                                          UpdateLevelName = c.MemberLevelName,
                                          MemberLevelID = b.ID,
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
                                     AllFreqType = g.FirstOrDefault().a.AllFreqType,
                                     AllCount = g.FirstOrDefault().a.AllCount,
                                     MemberFreqType = g.FirstOrDefault().a.MemberFreqType,
                                     MemberCount = g.FirstOrDefault().a.MemberCount,
                                     PriorityLevel = g.FirstOrDefault().a.PriorityLevel,
                                     UpdateLevelID = g.FirstOrDefault().a.UpdateLevelID,
                                     UpdateLevelName = g.FirstOrDefault().UpdateLevelName
                                 };                                 

                //int FoodDetailId = Dict_SystemService.I.GetModels(p => p.DictKey.Equals("套餐内容") && p.PID == 0).FirstOrDefault().ID;
                //int FoodDetailTypeId = Dict_SystemService.I.GetModels(p => p.DictKey.Equals("套餐类别") && p.PID == FoodDetailId).FirstOrDefault().ID;
                int FeeTypeId = Dict_SystemService.I.GetModels(p => p.DictKey.Equals("计费方式")).FirstOrDefault().ID;
                var FoodDetials = from a in Data_Food_DetialService.N.GetModels(p => p.FoodID == foodId && p.Status == 1)
                                  join b in Data_ProjectTicketService.N.GetModels() on new { ContainID = a.ContainID, FoodType = a.FoodType } equals new { ContainID = (int?)b.ID, FoodType = (int?)FoodDetailType.Ticket } into b1
                                  from b in b1.DefaultIfEmpty()
                                  join c in Base_GoodsInfoService.N.GetModels() on new { ContainID = a.ContainID, FoodType = a.FoodType } equals new { ContainID = (int?)c.ID, FoodType = (int?)FoodDetailType.Good } into c1
                                  from c in c1.DefaultIfEmpty()
                                  //join d in Dict_SystemService.N.GetModels(p => p.PID == FoodDetailTypeId) on (a.FoodType + "") equals d.DictValue into d1
                                  //from d in d1.DefaultIfEmpty()
                                  join f in Dict_BalanceTypeService.N.GetModels(p => p.State == 1) on a.ContainID equals f.ID into f1
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
                                                    (a.FoodType == (int)FoodDetailType.Ticket) ? (b != null ? b.TicketName : string.Empty) :
                                                    (a.FoodType == (int)FoodDetailType.Coupon) ? (g != null ? g.CouponName : string.Empty) : string.Empty,
                                      FoodDetailType = a.FoodType,
                                      //BalanceType = a.BalanceType,
                                      FoodDetailTypeStr = (a.FoodType == (int)FoodDetailType.Coin) ? (f != null ? f.TypeName : string.Empty) : 
                                                            (a.FoodType == (int)FoodDetailType.Coupon ? "优惠券" : a.FoodType == (int)FoodDetailType.Digit ? "数字币" : a.FoodType == (int)FoodDetailType.Good ? "礼品" : a.FoodType == (int)FoodDetailType.Ticket ? "门票" : string.Empty),
                                      ContainCount = a.ContainCount,
                                      //Days = a.Days,
                                      WeightValue = a.WeightValue,
                                      OperateType = a.OperateType
                                  };

                var FoodSales = from a in Data_Food_SaleService.N.GetModels(p => p.FoodID == foodId)
                                join b in Dict_BalanceTypeService.N.GetModels() on a.BalanceType equals b.ID
                                select new {
                                    BalanceType = a.BalanceType,
                                    BalanceTypeStr = b.TypeName,
                                    UseCount = a.UseCount,
                                    CashValue = a.CashValue
                                };

                var result = new
                {
                    FoodID = FoodInfo.ID,
                    FoodName = FoodInfo.FoodName,
                    FoodType = FoodInfo.FoodType,
                    StartTime = string.Format("{0:yyyy-MM-dd}", FoodInfo.StartTime),
                    EndTime = string.Format("{0:yyyy-MM-dd}", FoodInfo.EndTime),
                    ForbidStart = string.Format("{0:yyyy-MM-dd}", FoodInfo.ForbidStart),
                    ForbidEnd = string.Format("{0:yyyy-MM-dd}", FoodInfo.ForbidEnd),
                    Tax = FoodInfo.Tax,
                    ImageURL = FoodInfo.ImageURL,
                    AllowInternet = FoodInfo.AllowInternet,
                    MeituanID = FoodInfo.MeituanID,
                    DianpinID = FoodInfo.DianpinID,
                    KoubeiID = FoodInfo.KoubeiID,
                    AllowPrint = FoodInfo.AllowPrint,
                    AllowQuickFood = FoodInfo.AllowQuickFood,
                    ForeAuthorize = FoodInfo.ForeAuthorize,
                    ClientPrice = FoodInfo.ClientPrice,
                    MemberPrice = FoodInfo.MemberPrice,
                    RenewDays = FoodInfo.RenewDays,
                    ReturnTime = FoodInfo.ReturnTime,
                    TimeType = FoodInfo.TimeType,
                    ReturnFee = FoodInfo.ReturnFee,
                    FeeType = FoodInfo.FeeType,
                    StoreIDs = StoreIDs,
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
                merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                merchSecret = (userTokenKeyModel.DataModel as TokenDataModel).MerchSecret;

                string errMsg = string.Empty;
                int foodId = dicParas.Get("foodId").Toint(0);
                var foodName = dicParas.Get("foodName");
                var foodType = dicParas.Get("foodType").Toint();
                var startTime = dicParas.Get("startTime").Todatetime();                
                var endTime = dicParas.Get("endTime").Todatetime();
                var forbidStart = dicParas.Get("forbidStart").Todatetime();
                var forbidEnd = dicParas.Get("forbidEnd").Todatetime();
                var tax = dicParas.Get("tax").Todecimal();               
                var allowInternet = dicParas.Get("allowInternet").Toint();
                var allowQuickFood = dicParas.Get("allowQuickFood").Toint();
                var meituanID = dicParas.Get("meituanID");
                var dianpinID = dicParas.Get("dianpinID");
                var koubeiID = dicParas.Get("koubeiID");
                var allowPrint = dicParas.Get("allowPrint").Toint();
                var foreAuthorize = dicParas.Get("foreAuthorize").Toint();
                var clientPrice = dicParas.Get("clientPrice").Todecimal();
                var memberPrice = dicParas.Get("memberPrice").Todecimal();
                var renewDays = dicParas.Get("renewDays").Toint();
                var imageUrl = dicParas.Get("imageUrl");
                var returnTime = dicParas.Get("returnTime").Toint();
                var timeType = dicParas.Get("timeType").Toint();
                var returnFee = dicParas.Get("returnFee").Todecimal();
                var feeType = dicParas.Get("feeType").Toint();
                var storeIds = dicParas.Get("storeIds");
                object[] foodSales = dicParas.GetArray("foodSales");
                object[] foodDetials = dicParas.GetArray("foodDetials");
                object[] foodLevels = dicParas.GetArray("foodLevels");

                if (!foodName.Nonempty("套餐名称", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!foodType.Nonempty("套餐类别", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!startTime.Nonempty("有效期开始时间", out errMsg) || !endTime.Nonempty("有效期结束时间", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (startTime > endTime)
                {
                    errMsg = "开始时间不能晚于结束时间";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }
                if (!forbidStart.IsNull() && !forbidEnd.IsNull())
                {
                    if (forbidStart > forbidEnd)
                    {
                        errMsg = "不可用开始日期不能晚于结束日期";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                }                
                if (!clientPrice.Validdecimal("散客售价", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!memberPrice.Validdecimal("会员售价", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
               

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        var data_FoodInfo = Data_FoodInfoService.I.GetModels(p => p.ID == foodId).FirstOrDefault() ?? new Data_FoodInfo();
                        data_FoodInfo.ID = foodId;
                        data_FoodInfo.FoodName = foodName;
                        data_FoodInfo.FoodType = foodType;
                        data_FoodInfo.StartTime = startTime;
                        data_FoodInfo.EndTime = endTime;
                        data_FoodInfo.ForbidStart = forbidStart.Todate();
                        data_FoodInfo.ForbidEnd = forbidEnd.Todate();
                        data_FoodInfo.Tax = tax;                        
                        data_FoodInfo.AllowPrint = allowPrint;
                        data_FoodInfo.AllowInternet = allowInternet;
                        data_FoodInfo.MeituanID = meituanID;
                        data_FoodInfo.DianpinID = dianpinID;
                        data_FoodInfo.KoubeiID = koubeiID;
                        data_FoodInfo.ForeAuthorize = foreAuthorize;
                        data_FoodInfo.MemberPrice = memberPrice;
                        data_FoodInfo.ClientPrice = clientPrice;
                        data_FoodInfo.RenewDays = renewDays;
                        data_FoodInfo.MerchID = merchId;
                        data_FoodInfo.ImageURL = imageUrl;
                        data_FoodInfo.AllowQuickFood = allowQuickFood;
                        data_FoodInfo.ReturnTime = returnTime;
                        data_FoodInfo.TimeType = timeType;
                        data_FoodInfo.ReturnFee = returnFee;
                        data_FoodInfo.FeeType = feeType;
                        if (foodId == 0)
                        {
                            //新增
                            data_FoodInfo.FoodState = (int)FoodState.Valid;
                            if (!Data_FoodInfoService.I.Add(data_FoodInfo, true, merchId, merchSecret))
                            {
                                errMsg = "添加套餐信息失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }
                        else
                        {
                            if (!Data_FoodInfoService.I.Any(p => p.ID == foodId))
                            {
                                errMsg = "该套餐信息不存在";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            //修改
                            if (!Data_FoodInfoService.I.Update(data_FoodInfo, true, merchId, merchSecret))
                            {
                                errMsg = "修改套餐信息失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }

                        foodId = data_FoodInfo.ID;

                        //保存消耗余额类别设定
                        if (!saveFoodSale(foodId, foodSales, out errMsg))
                        {
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }
                        
                        //保存套餐优惠时段信息
                        if (!saveFoodLevel(foodId, foodLevels, out errMsg))
                        {
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        //保存套餐内容信息
                        if (!saveFoodDetail(storeIds, allowQuickFood, foodId, foodDetials, out errMsg))
                        {
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        //保存适用门店
                        foreach (var model in Data_Food_StoreListService.I.GetModels(p => p.FoodID == foodId))
                        {
                            Data_Food_StoreListService.I.DeleteModel(model, true, merchId, merchSecret);
                        }

                        if (!string.IsNullOrEmpty(storeIds))
                        {
                            foreach (var storeId in storeIds.Split('|'))
                            {
                                if (!storeId.Nonempty("门店ID", out errMsg))
                                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                                var model = new Data_Food_StoreList();
                                model.FoodID = foodId;
                                model.StoreID = storeId;
                                Data_Food_StoreListService.I.AddModel(model, true, merchId, merchSecret);
                            }
                        }

                        if (!Data_Food_StoreListService.I.SaveChanges())
                        {
                            errMsg = "更新套餐适用门店表失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        //保存快速套餐, 设置其他快速套餐为0
                        if (allowQuickFood == 1)
                        {
                            foreach (var model in Data_FoodInfoService.I.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && p.ID != foodId && p.AllowQuickFood == 1))
                            {
                                model.AllowQuickFood = 0;
                                Data_FoodInfoService.I.UpdateModel(model, true, merchId, merchSecret);
                            }

                            if (!Data_FoodInfoService.I.SaveChanges())
                            {
                                errMsg = "更新快速售币套餐失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
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
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string merchSecret = (userTokenKeyModel.DataModel as TokenDataModel).MerchSecret;

                string errMsg = string.Empty;
                var foodIdArr = dicParas.GetArray("foodId");

                if (!foodIdArr.Validarray("套餐ID列表", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        foreach (var foodId in foodIdArr)
                        {
                            if (!foodId.Validintnozero("套餐ID", out errMsg))
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                            if (!Data_FoodInfoService.I.Any(p => p.ID == (int)foodId))
                            {
                                errMsg = "该套餐信息不存在";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            var data_FoodInfo = Data_FoodInfoService.I.GetModels(p => p.ID == (int)foodId).FirstOrDefault();
                            data_FoodInfo.FoodState = (int)FoodState.Invalid;
                            if (!Data_FoodInfoService.I.Update(data_FoodInfo, true, merchId, merchSecret))
                            {
                                errMsg = "删除套餐信息失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
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
        public object GetGoodsInfoList(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                
                var goodNameOrBarcode = dicParas.ContainsKey("goodNameOrBarcode") ? (dicParas["goodNameOrBarcode"] + "") : string.Empty;
                var storeIds = dicParas.Get("storeIds");

                int goodTypeId = Dict_SystemService.I.GetModels(p => p.DictKey.Equals("商品类别")).FirstOrDefault().ID;
                var query = Base_GoodsInfoService.N.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && p.Status == 1);
                if (!string.IsNullOrEmpty(goodNameOrBarcode))
                {
                    query = query.Where(w => w.GoodName.Contains(goodNameOrBarcode) || w.Barcode.Contains(goodNameOrBarcode));
                }
                if (storeIds.IsNull() || storeIds.Contains("|"))
                {
                    query = query.Where(w => (w.StoreID ?? "") == "");
                }
                else
                {
                    query = query.Where(w => ((w.StoreID ?? "") == "" || w.StoreID.Equals(storeIds, StringComparison.OrdinalIgnoreCase)));
                }

                var linq = from a in query
                           join b in Dict_SystemService.N.GetModels(p => p.PID == goodTypeId && p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase)) on a.GoodType equals b.ID into b1
                           from b in b1.DefaultIfEmpty()
                           select new
                           {
                               ID = a.ID,
                               Barcode = a.Barcode,
                               GoodName = a.GoodName,
                               GoodTypeStr = b != null ? b.DictKey : string.Empty
                           };

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, linq);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetProjectTicketList(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

                var storeIds = dicParas.Get("storeIds");
               
                var query = Data_ProjectTicketService.I.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase));                
                if (storeIds.IsNull() || storeIds.Contains("|"))
                {
                    query = query.Where(w => (w.StoreID ?? "") == "");
                }
                else
                {
                    query = query.Where(w => ((w.StoreID ?? "") == "" || w.StoreID.Equals(storeIds, StringComparison.OrdinalIgnoreCase)));
                }

                var linq = from a in query
                           select new
                           {
                               ID = a.ID,
                               TicketName = a.TicketName
                           };

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, linq);
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

                List<string> imageUrls = new List<string>();
                if (!Utils.UploadImageFile("/XCCloud/Promotion/Food/", out imageUrls, out errMsg))
                {
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                imageInfo.Add("ImageURL", imageUrls.First());

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
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string merchSecret = (userTokenKeyModel.DataModel as TokenDataModel).MerchSecret;

                string errMsg = string.Empty;
                int foodId = dicParas.Get("foodId").Toint(0);
                var storeIds = dicParas.Get("storeIds");

                if (foodId == 0)
                {
                    errMsg = "套餐ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        foreach (var model in Data_Food_StoreListService.I.GetModels(p => p.FoodID == foodId))
                        {
                            Data_Food_StoreListService.I.DeleteModel(model, true, merchId, merchSecret);
                        }

                        if (!string.IsNullOrEmpty(storeIds))
                        {
                            foreach (var storeId in storeIds.Split('|'))
                            {
                                if (!storeId.Nonempty("门店ID", out errMsg))
                                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                                var model = new Data_Food_StoreList();
                                model.FoodID = foodId;
                                model.StoreID = storeId;
                                Data_Food_StoreListService.I.AddModel(model, true, merchId, merchSecret);
                            }
                        }

                        if (!Data_Food_StoreListService.I.SaveChanges())
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