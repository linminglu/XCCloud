using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Transactions;
using System.Web;
using XCCloudService.Base;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
using XCCloudService.BLL.XCCloud;
using XCCloudService.Common;
using XCCloudService.Common.Enum;
using XCCloudService.DBService.BLL;
using XCCloudService.Model.CustomModel.XCCloud;
using XCCloudService.Model.XCCloud;
using XCCloudService.Common.Extensions;

namespace XXCloudService.Api.XCCloud
{
    [Authorize(Roles = "MerchUser")]
    /// <summary>
    /// Coupon 的摘要说明
    /// </summary>
    public class Coupon : ApiBase
    {
        private bool isSend(int iId)
        {
            return Data_CouponListService.I.Any(p => p.CouponID == iId && p.State == (int)CouponState.Activated); //已派发
        }

        private bool isUsed(int iId)
        {
            return (from a in Data_CouponListService.N.GetModels(p => p.CouponID == iId)
                    join b in Flw_CouponUseService.N.GetModels() on a.ID equals b.CouponCode
                    select 1).Any() || Flw_CouponUseService.I.Any(p=>p.CouponID == iId);
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryCouponInfo(Dictionary<string, object> dicParas)
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

                string sql = @"select a.ID, a.CouponName, a.EntryCouponFlag, a.CouponType, b.DictKey as CouponTypeStr, a.PublishCount, (isnull(c.UseCount, 0) + isnull(d.UseCount, 0)) as UseCount, " +
                    " isnull(f.NotAssignedCount, 0) as NotAssignedCount, isnull(g.NotActivatedCount, 0) as NotActivatedCount, isnull(h.ActivatedCount, 0) as ActivatedCount, " +
                    " a.AuthorFlag, a.AllowOverOther, a.OpUserID, j.LogName as OpUserName, a.Context, isnull(i.IsLock, 0) as IsLock, " +
                    " (case when isnull(a.StartDate,'')='' then '' else convert(varchar,a.StartDate,23) end) as StartDate, (case when isnull(a.EndDate,'')='' then '' else convert(varchar,a.EndDate,23) end) as EndDate, " +
                    " (case when isnull(a.CreateTime,'')='' then '' else convert(varchar,a.CreateTime,23) end) as CreateTime " +
                    " from Data_CouponInfo a" +
                    " left join (select b.* from Dict_System a inner join Dict_System b on a.ID=b.PID where a.DictKey='优惠券类别' and a.PID=0) b on convert(varchar, a.CouponType)=b.DictValue " +
                    " left join (select a.ID as CouponID, count(c.ID) as UseCount from Data_CouponInfo a inner join Data_CouponList b on a.ID=b.CouponID inner join Flw_CouponUse c on b.ID=c.CouponCode group by a.ID) c on a.ID=c.CouponID " +
                    " left join (select a.ID as CouponID, count(b.ID) as UseCount from Data_CouponInfo a inner join Flw_CouponUse b on a.ID=b.CouponID group by a.ID) d on a.ID=d.CouponID " +
                    " left join (select a.ID as CouponID, count(b.ID) as NotAssignedCount from Data_CouponInfo a inner join Data_CouponList b on a.ID=b.CouponID where isnull(b.State, 0)=0 group by a.ID) f on a.ID=f.CouponID " +
                    " left join (select a.ID as CouponID, count(b.ID) as NotActivatedCount from Data_CouponInfo a inner join Data_CouponList b on a.ID=b.CouponID where isnull(b.State, 0)=1 group by a.ID) g on a.ID=g.CouponID " +
                    " left join (select a.ID as CouponID, count(b.ID) as ActivatedCount from Data_CouponInfo a inner join Data_CouponList b on a.ID=b.CouponID where isnull(b.State, 0)=2 group by a.ID) h on a.ID=h.CouponID " +
                    " left join (select a.ID as CouponID, min(isnull(b.IsLock,0)) as IsLock from Data_CouponInfo a inner join Data_CouponList b on a.ID=b.CouponID group by a.ID) i on a.ID=i.CouponID " +
                    " left join Base_UserInfo j on a.OpUserID=j.UserID " +
                    " where a.MerchID=@MerchId";
                sql = sql + sqlWhere;

                IData_CouponInfoService data_CouponInfoService = BLLContainer.Resolve<IData_CouponInfoService>();
                var data_CouponInfo = data_CouponInfoService.SqlQuery<Data_CouponInfoModel>(sql, parameters).ToList();

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, data_CouponInfo);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 获取优惠券字典
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetCouponDic(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as MerchDataModel).MerchID;

                var linq = from a in Data_CouponInfoService.I.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase))
                           select new
                           {
                               ID = a.ID,
                               CouponName = a.CouponName,
                               EndTime = a.EndTime
                           };

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, linq);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetCouponInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                int id = dicParas.Get("id").Toint(0);
                if (id == 0)
                {
                    errMsg = "规则编号不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var data_CouponInfo = Data_CouponInfoService.I.GetModels(p => p.ID == id).FirstOrDefault();
                if (data_CouponInfo == null)
                {
                    errMsg = "该优惠券不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }               
                          
                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, data_CouponInfo);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object SaveCouponInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as MerchDataModel).MerchID;
                int userId = Convert.ToInt32(userTokenKeyModel.LogId);

                string errMsg = string.Empty;
                if (!dicParas.Get("couponName").Nonempty("优惠券名称", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("publishCount").Validint("发行张数", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("entryCouponFlag").Nonempty("实物券标记", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("startDate").Validdate("使用期限", out errMsg) || !dicParas.Get("endDate").Validdate("使用期限", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("couponType").Nonempty("优惠券类别", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                if (dicParas.Get("couponType").Toint() == (int)CouponType.Cash || dicParas.Get("couponType").Toint() == (int)CouponType.Discount)
                {
                    if (!dicParas.Get("couponValue").Validdecimal("优惠券价值", out errMsg))
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    if (!dicParas.Get("couponDiscount").Validdecimal("优惠券折扣", out errMsg))
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    if (!dicParas.Get("couponThreshold").Validdecimal("优惠券阈值", out errMsg))
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    if (!dicParas.Get("overUseCount").Validint("同时使用张数", out errMsg))
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);                       
                }
                
                if (dicParas.Get("couponType").Toint() == (int)CouponType.Charge)
                {
                    if (!dicParas.Get("chargeType").Nonempty("兑换类型", out errMsg))
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                    if (dicParas.Get("chargeType").Toint() == (int)ChargeType.Good)
                    {
                        if (!dicParas.Get("goodId").Nonempty("兑换内容", out errMsg))
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);                        
                    }
                    else if (dicParas.Get("chargeType").Toint() == (int)ChargeType.Project)
                    {
                        if (!dicParas.Get("projectId").Nonempty("兑换内容", out errMsg))
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);                        
                    }
                    else if (dicParas.Get("chargeType").Toint() == (int)ChargeType.Coin)
                    {
                        if (!dicParas.Get("balanceIndex").Nonempty("兑换内容", out errMsg))
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);                        
                    }

                    if (!dicParas.Get("chargeCount").Validint("兑换数量", out errMsg))
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);                    
                }

                if (!dicParas.Get("sendType").Nonempty("派发途径", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                if (dicParas.Get("sendType").Toint() == (int)SendType.Consume || dicParas.Get("sendType").Toint() == (int)SendType.Jackpot)
                {
                    if (!dicParas.Get("overMoney").Validdecimal("消费满金额", out errMsg))
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                    if (dicParas.Get("sendType").Toint() == (int)SendType.Consume)
                    {
                        if (!dicParas.Get("freeCouponCount").Validint("送券数", out errMsg))
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                    else if (dicParas.Get("sendType").Toint() == (int)SendType.Jackpot)
                    {
                        if (!dicParas.Get("jackpotId").Nonempty("抽奖活动", out errMsg))
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        if (!dicParas.Get("jackpotCount").Validint("抽奖次数", out errMsg))
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }                    
                }
                //else if (dicParas.Get("sendType").Toint() == (int)SendType.Orient)
                //{
                //    if (!dicParas.Get("conditionType").Nonempty("定向派发类型", out errMsg))
                //        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                //    if (dicParas.Get("conditionType").Toint() == (int)ConditionType.Auto)
                //    {
                //        if (!dicParas.Get("autoSendCycle").Validint("自动派发周期", out errMsg))
                //            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                //        if (!dicParas.Get("autoSendValue").Validint("自动派发值", out errMsg))
                //            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                //        if (!dicParas.Get("autoSendCount").Validint("自动派发张数", out errMsg))
                //            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                //    }                        
                //}
                
                ////如果印刷编号为空，派发方式请选择街边派发                   
                //if (iEntryCouponFlag == (int)EntryCouponFlag.Entry && string.IsNullOrEmpty(couponCodeStart) && string.IsNullOrEmpty(couponCodeEnd) && iSendType != (int)SendType.Delivery)
                //{
                //    errMsg = "如果印刷编号为空，派发方式请选择街边派发";
                //    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                //}

                //如果是电子券，不支持街边派送
                if (dicParas.Get("entryCouponFlag").Toint() == (int)CouponFlag.Digit && dicParas.Get("sendType").Toint() == (int)SendType.Delivery)
                {
                    errMsg = "电子券不支持街边派发";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                //如果是实物券，不支持定向派发
                if (dicParas.Get("entryCouponFlag").Toint() == (int)CouponFlag.Entry && dicParas.Get("sendType").Toint() == (int)SendType.Orient)
                {
                    errMsg = "实物券不支持定向派发";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                int id = dicParas.Get("id").Toint(0);
                //string couponName = dicParas.Get("couponName");
                //var couponType = dicParas.Get("couponType").Toint();
                var entryCouponFlag = dicParas.Get("entryCouponFlag").Toint();
                //var authorFlag = dicParas.Get("authorFlag").Toint();
                //var allowOverOther = dicParas.Get("allowOverOther").Toint();
                //var overUseCount = dicParas.Get("overUseCount").Toint();
                var publishCount = dicParas.Get("publishCount").Toint();
                //var couponValue = dicParas.Get("couponValue").Todecimal();
                //var couponDiscount = dicParas.Get("couponDiscount").Todecimal();
                //var couponThreshold = dicParas.Get("couponThreshold").Todecimal();
                //var startDate = dicParas.Get("startDate").Todatetime();
                //var endDate = dicParas.Get("endDate").Todatetime();
                var sendType = dicParas.Get("sendType").Toint();
                //var overMoney = dicParas.Get("overMoney").Todecimal();
                //var freeCouponCount = dicParas.Get("freeCouponCount").Toint();
                //var jackpotCount = dicParas.Get("jackpotCount").Toint();
                //var jackpotId = dicParas.Get("jackpotId").Toint();
                //var chargeType = dicParas.Get("chargeType").Toint();
                //var chargeCount = dicParas.Get("chargeCount").Toint();
                //var balanceIndex = dicParas.Get("balanceIndex").Toint();
                //var goodId = dicParas.Get("goodId").Toint();
                //var projectId = dicParas.Get("projectId").Toint();                
                //var weekType = dicParas.Get("weekType").Toint();
                //var week = dicParas.Get("week");
                //var startTime = dicParas.Get("startTime").Totimespan();
                //var endTime = dicParas.Get("endTime").Totimespan();
                //var noStartDate = dicParas.Get("noStartDate").Todatetime();
                //var noEndDate = dicParas.Get("noEndDate").Todatetime();
                //var autoSendCycle = dicParas.Get("autoSendCycle").Toint();
                //var autoSendValue = dicParas.Get("autoSendValue").Toint();
                //var autoSendCount = dicParas.Get("autoSendCount").Toint();
                //var context = dicParas.Get("context");           
                var couponConditions = dicParas.GetArray("couponConditions");
                var memberIds = dicParas.GetArray("memberIds");
                                                          
                ////验证印刷编号                               
                //int iCouponCodeStart = -1, iCouponCodeEnd = -1, len = 0;
                //string pre = string.Empty;
                //if (!string.IsNullOrEmpty(couponCodeStart) && !string.IsNullOrEmpty(couponCodeEnd))
                //{
                //    Regex regex = new Regex(@"[0-9]+(?=[^0-9]*$)");
                //    int index1 = regex.Match(couponCodeStart).Index;
                //    index1 = (index1 < 0) ? 0 : index1;
                //    int index2 = regex.Match(couponCodeEnd).Index;
                //    index2 = (index2 < 0) ? 0 : index2; 
                //    int.TryParse(couponCodeStart.Substring(index1), out iCouponCodeStart);
                //    int.TryParse(couponCodeEnd.Substring(index2), out iCouponCodeEnd);
                //    string pre1 = couponCodeStart.Substring(0, index1);
                //    string pre2 = couponCodeEnd.Substring(0, index2);
                    
                //    if (iCouponCodeStart < 0 || iCouponCodeEnd < 0)
                //    {
                //        errMsg = "印刷编号格式不正确，须以数字结尾";
                //        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                //    }

                //    if (!pre1.Equals(pre2, StringComparison.OrdinalIgnoreCase))
                //    {
                //        errMsg = "印刷编号起码与止码的前缀部分须相同";
                //        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                //    }

                //    if ((iCouponCodeEnd - iCouponCodeStart + 1) != iPublishCount)
                //    {
                //        errMsg = "印刷编号数量与发行张数不相等";
                //        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                //    }

                //    len = couponCodeStart.Length - index1;
                //    pre = pre1;
                //}

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {                                   
                        var data_CouponInfo = new Data_CouponInfo();
                        Utils.GetModel(dicParas, ref data_CouponInfo);
                        data_CouponInfo.OpUserID = userId;
                        data_CouponInfo.CreateTime = DateTime.Now;
                        data_CouponInfo.MerchID = merchId;
                        if (id == 0)
                        {
                            //新增
                            if (!Data_CouponInfoService.I.Add(data_CouponInfo))
                            {
                                errMsg = "添加优惠券信息失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }
                        else
                        {
                            if (!Data_CouponInfoService.I.Any(p => p.ID == id))
                            {
                                errMsg = "该优惠券信息不存在";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            //已派发或使用的优惠券规则不能修改                
                            if (isSend(id) || isUsed(id))
                            {
                                errMsg = "已派发或使用的优惠券规则不能修改";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            //修改
                            if (!Data_CouponInfoService.I.Update(data_CouponInfo))
                            {
                                errMsg = "修改优惠券信息失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }

                        id = data_CouponInfo.ID;

                        //添加未分配优惠券
                        foreach (var model in Data_CouponListService.I.GetModels(p => p.CouponID == id))
                        {
                            Data_CouponListService.I.DeleteModel(model);
                        }

                        for (var i = 0; i < publishCount; i++)
                        {
                            var data_CouponListModel = new Data_CouponList();
                            data_CouponListModel.CouponCode = System.Guid.NewGuid().ToString("N");
                            data_CouponListModel.CouponID = id;
                            data_CouponListModel.CouponIndex = i + 1;                            
                            data_CouponListModel.SendAuthorID = userId;
                            data_CouponListModel.SendTime = DateTime.Now;
                            data_CouponListModel.PublishType = entryCouponFlag;
                            data_CouponListModel.SendType = sendType;                            
                            data_CouponListModel.MerchID = merchId;
                            if (entryCouponFlag == (int)CouponFlag.Digit)
                            {
                                data_CouponListModel.State = (int)CouponState.Activated;
                                if (memberIds != null && i < memberIds.Count())
                                {
                                    if (!memberIds[i].Validint("会员ID", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    data_CouponListModel.MemberID = memberIds[i].Toint();
                                }
                            }
                            else
                            {
                                data_CouponListModel.State = (int)CouponState.NotAssigned;
                            }                            
                            Data_CouponListService.I.AddModel(data_CouponListModel);
                        }

                        if (!Data_CouponListService.I.SaveChanges())
                        {
                            errMsg = "添加优惠券记录表失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }                        
                           
                        //添加派发条件
                        if (couponConditions != null && couponConditions.Count() >= 0)
                        {
                            //先删除，后添加
                            foreach (var model in Data_CouponConditionService.I.GetModels(p => p.CouponID == id))
                            {
                                Data_CouponConditionService.I.DeleteModel(model);
                            }

                            foreach (IDictionary<string, object> el in couponConditions)
                            {
                                if (el != null)
                                {
                                    var dicPara = new Dictionary<string, object>(el, StringComparer.OrdinalIgnoreCase);
                                    if (!dicPara.Get("conditionType").Nonempty("条件类别", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPara.Get("conditionId").Nonempty("条件ID", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPara.Get("connectType").Nonempty("连接类别", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPara.Get("conditionValue").Nonempty("值", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    var data_CouponCondition = new Data_CouponCondition();
                                    Utils.GetModel(dicPara, ref data_CouponCondition);
                                    data_CouponCondition.CouponID = id;
                                    Data_CouponConditionService.I.AddModel(data_CouponCondition);
                                }
                                else
                                {
                                    errMsg = "提交数据包含空对象";
                                    return false;
                                }
                            }

                            if (!Data_CouponConditionService.I.SaveChanges())
                            {
                                errMsg = "保存优惠券派发条件失败";
                                return false;
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
        public object DelCouponInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                int id = dicParas.Get("id").Toint(0);
                if (id == 0)
                {
                    errMsg = "规则ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }
                           
                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        if (!Data_CouponInfoService.I.Any(a => a.ID == id))
                        {
                            errMsg = "该优惠券规则信息不存在";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        if (isSend(id) || isUsed(id))
                        {
                            errMsg = "已派发或使用的优惠券规则不能删除";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }
                
                        var data_CouponInfo = Data_CouponInfoService.I.GetModels(p => p.ID == id).FirstOrDefault();
                        Data_CouponInfoService.I.DeleteModel(data_CouponInfo);

                        var data_Coupon_StoreList = Data_Coupon_StoreListService.I.GetModels(p => p.CouponID == id);
                        foreach (var model in data_Coupon_StoreList)
                        {
                            Data_Coupon_StoreListService.I.DeleteModel(model);
                        }

                        if (!Data_CouponInfoService.I.SaveChanges())
                        {
                            errMsg = "删除优惠券规则信息失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        ts.Complete();
                    }
                    catch (Exception ex)
                    {
                        return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, ex.Message);
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
        public object GetCouponStores(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                int couponId = dicParas.Get("couponId").Toint(0);

                var storeIDs = Data_Coupon_StoreListService.I.GetModels(p => p.CouponID == couponId).Select(o => new { StoreID = o.StoreID });
                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, storeIDs);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object SaveCouponStores(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                int couponId = dicParas.Get("couponId").Toint(0);
                string storeIds = dicParas.ContainsKey("storeIds") ? (dicParas["storeIds"] + "") : string.Empty;

                if (couponId == 0)
                {
                    errMsg = "优惠券规则ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        foreach (var model in Data_Coupon_StoreListService.I.GetModels(p => p.CouponID == couponId))
                        {
                            Data_Coupon_StoreListService.I.DeleteModel(model);
                        }

                        if (!string.IsNullOrEmpty(storeIds))
                        {
                            foreach (var storeId in storeIds.Split('|'))
                            {
                                if (!storeId.Nonempty("门店ID", out errMsg))
                                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                                var model = new Data_Coupon_StoreList();
                                model.CouponID = couponId;
                                model.StoreID = storeId;
                                Data_Coupon_StoreListService.I.AddModel(model);
                            }
                        }
                        
                        if (!Data_Coupon_StoreListService.I.SaveChanges())
                        {
                            errMsg = "更新优惠券适用门店表失败";
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
        public object GetCouponMembers(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                int couponId = dicParas.Get("couponId").Toint(0);

                var data_Coupon_MemberList = Data_CouponListService.I.GetModels(p => p.CouponID == couponId).Select(o => o.MemberID);
                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, data_Coupon_MemberList);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetCouponConditions(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                int couponId = dicParas.Get("couponId").Toint(0);

                var data_CouponConditionList = Data_CouponConditionService.I.GetModels(p => p.CouponID == couponId).ToList();
                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, data_CouponConditionList);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetCouponNotAssigned(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                int couponId = dicParas.Get("couponId").Toint(0);
                if (couponId == 0)
                {
                    errMsg = "优惠券ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var linq = from c in (from a in Data_CouponInfoService.N.GetModels(p => p.ID == couponId)
                           join b in Data_CouponListService.N.GetModels(p => p.State == (int)CouponState.NotAssigned) on a.ID equals b.CouponID
                           select new { a = a, CouponID = b.CouponID, CouponIndex = b.CouponIndex }).AsEnumerable()
                           group c by c.CouponID into g
                           select new {
                               CouponName = g.FirstOrDefault().a.CouponName,
                               StartDate = Utils.ConvertFromDatetime(g.FirstOrDefault().a.StartDate, "yyyy-MM-dd"),
                               EndDate = Utils.ConvertFromDatetime(g.FirstOrDefault().a.EndDate, "yyyy-MM-dd"),
                               NotAssignedCount = g.Count(),
                               StartNo = g.Min(m => m.CouponIndex),
                               EndNo = g.Max(m => m.CouponIndex)
                           };

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, linq);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }        

        [Authorize(Roles = "StoreUser")]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetCouponNotActivated(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];                

                string errMsg = string.Empty;
                int couponId = dicParas.Get("couponId").Toint(0);
                if (couponId == 0)
                {
                    errMsg = "优惠券ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var storeId = dicParas.Get("storeId") ?? (userTokenKeyModel.DataModel as MerchDataModel).StoreID;

                var query = Data_CouponListService.N.GetModels(p => p.State == (int)CouponState.NotActivated);
                if (!string.IsNullOrEmpty(storeId))
                {
                    query = query.Where(w => w.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase));
                }
                var linq = from c in
                               (from a in Data_CouponInfoService.N.GetModels(p => p.ID == couponId)
                                join b in query on a.ID equals b.CouponID
                                select new { a = a, CouponID = b.CouponID, CouponIndex = b.CouponIndex }).AsEnumerable()
                           group c by c.CouponID into g
                           select new
                           {
                               CouponName = g.FirstOrDefault().a.CouponName,
                               StartDate = Utils.ConvertFromDatetime(g.FirstOrDefault().a.StartDate, "yyyy-MM-dd"),
                               EndDate = Utils.ConvertFromDatetime(g.FirstOrDefault().a.EndDate, "yyyy-MM-dd"),
                               NotActivatedCount = g.Count(),
                               StartNo = g.Min(m => m.CouponIndex),
                               EndNo = g.Max(m => m.CouponIndex),
                           };

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, linq);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }
        
        [Authorize(Roles = "StoreUser")]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetCouponActivated(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];

                string errMsg = string.Empty;
                int couponId = dicParas.Get("couponId").Toint(0);
                if (couponId == 0)
                {
                    errMsg = "优惠券ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var storeId = dicParas.Get("storeId") ?? (userTokenKeyModel.DataModel as MerchDataModel).StoreID;

                var query = Data_CouponListService.N.GetModels(p => p.State == (int)CouponState.Activated);
                if (!string.IsNullOrEmpty(storeId))
                {
                    query = query.Where(w => w.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase));
                }
                var linq = from c in
                               (from a in Data_CouponInfoService.N.GetModels(p => p.ID == couponId)
                                join b in query on a.ID equals b.CouponID
                                select new { a = a, CouponID = b.CouponID, CouponIndex = b.CouponIndex }).AsEnumerable()
                           group c by c.CouponID into g
                           select new
                           {
                               CouponName = g.FirstOrDefault().a.CouponName,
                               StartDate = Utils.ConvertFromDatetime(g.FirstOrDefault().a.StartDate, "yyyy-MM-dd"),
                               EndDate = Utils.ConvertFromDatetime(g.FirstOrDefault().a.EndDate, "yyyy-MM-dd"),
                               ActivatedCount = g.Count(),
                               StartNo = g.Min(m => m.CouponIndex),
                               EndNo = g.Max(m => m.CouponIndex),
                           };

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, linq);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [Authorize(Roles = "StoreUser")]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object SaveCouponNotAssigned(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];

                string errMsg = string.Empty;
                int couponId = dicParas.Get("couponId").Toint(0);
                if (couponId == 0)
                {
                    errMsg = "优惠券ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (!dicParas.Get("startNo").Validint("开始序号", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("endNo").Validint("结束序号", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (dicParas.Get("startNo").Toint() > dicParas.Get("endNo").Toint())
                {
                    errMsg = "开始序号不能大于结束序号";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }
                if (dicParas.Get("startNo").Toint() == 0)
                {
                    errMsg = "开始序号须大于0";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                int startNo = dicParas.Get("startNo").Toint(0);
                int endNo = dicParas.Get("endNo").Toint(0);
                var storeId = dicParas.Get("storeId") ?? (userTokenKeyModel.DataModel as MerchDataModel).StoreID;

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        for (int i = startNo; i <= endNo; i++)
                        {
                            if (!Data_CouponListService.I.Any(p => p.CouponID == couponId && p.CouponIndex == i))
                            {
                                errMsg = "序号" + i + "的优惠券不存在";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            var data_CouponList = Data_CouponListService.I.GetModels(p => p.CouponID == couponId && p.CouponIndex == i).FirstOrDefault();
                            if (data_CouponList.State != (int)CouponState.NotAssigned)
                            {
                                errMsg = "序号" + i + "的优惠券已调拨";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            data_CouponList.StoreID = storeId;
                            data_CouponList.State = (int)CouponState.NotActivated;
                            Data_CouponListService.I.UpdateModel(data_CouponList);
                        }

                        if (!Data_CouponListService.I.SaveChanges())
                        {
                            errMsg = "优惠券调拨失败";
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

        [Authorize(Roles = "StoreUser")]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object SaveCouponNotActivated(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                int couponId = dicParas.Get("couponId").Toint(0);
                if (couponId == 0)
                {
                    errMsg = "优惠券ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (!dicParas.Get("startNo").Validint("开始序号", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("endNo").Validint("结束序号", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (dicParas.Get("startNo").Toint() > dicParas.Get("endNo").Toint())
                {
                    errMsg = "开始序号不能大于结束序号";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }
                if (dicParas.Get("startNo").Toint() == 0)
                {
                    errMsg = "开始序号须大于0";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                int startNo = dicParas.Get("startNo").Toint(0);
                int endNo = dicParas.Get("endNo").Toint(0);

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        for (int i = startNo; i <= endNo; i++)
                        {
                            if (!Data_CouponListService.I.Any(p => p.CouponID == couponId && p.CouponIndex == i))
                            {
                                errMsg = "序号" + i + "的优惠券不存在";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            var data_CouponList = Data_CouponListService.I.GetModels(p => p.CouponID == couponId && p.CouponIndex == i).FirstOrDefault();
                            if (data_CouponList.State != (int)CouponState.NotActivated)
                            {
                                errMsg = "序号" + i + "的优惠券已派发";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            data_CouponList.State = (int)CouponState.Activated;
                            Data_CouponListService.I.UpdateModel(data_CouponList);
                        }

                        if (!Data_CouponListService.I.SaveChanges())
                        {
                            errMsg = "优惠券派发失败";
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
        

        [Authorize(Roles = "StoreUser")]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object LockCoupon(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];

                string errMsg = string.Empty;
                int couponId = dicParas.Get("couponId").Toint(0);
                if (couponId == 0)
                {
                    errMsg = "优惠券ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if(!dicParas.Get("isLock").Validint("锁定状态", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var storeId = dicParas.Get("storeId") ?? (userTokenKeyModel.DataModel as MerchDataModel).StoreID;
                var isLock = dicParas.Get("isLock").Toint();
                var id = dicParas.Get("id").Toint();

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        var query = Data_CouponListService.I.GetModels(p => p.CouponID == couponId);
                        if (id != null)
                        {
                            query = query.Where(w => w.ID == id);
                        }
                        else if (!string.IsNullOrEmpty(storeId))
                        {
                            query = query.Where(w => w.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase));
                        }

                        foreach (var model in query)
                        {
                            model.IsLock = isLock;
                            Data_CouponListService.I.UpdateModel(model);
                        }

                        if (!Data_CouponListService.I.SaveChanges())
                        {
                            errMsg = "锁定操作失败";
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

        [Authorize(Roles = "StoreUser")]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, RespDataTypeEnum = RespDataTypeEnum.FileStream, SysIdAndVersionNo = false)]
        public object ExportCoupon(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                int couponId = dicParas.Get("couponId").Toint(0);
                if (couponId == 0)
                {
                    errMsg = "优惠券规则ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (!dicParas.Get("startNo").Validint("开始序号", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("endNo").Validint("结束序号", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (dicParas.Get("startNo").Toint() > dicParas.Get("endNo").Toint())
                {
                    errMsg = "开始序号不能大于结束序号";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }
                if (dicParas.Get("startNo").Toint() == 0)
                {
                    errMsg = "开始序号须大于0";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                int startNo = dicParas.Get("startNo").Toint(0);
                int endNo = dicParas.Get("endNo").Toint(0);

                var linq = from a in Data_CouponListService.N.GetModels(p => p.CouponID == couponId && p.CouponIndex >= startNo && p.CouponIndex <= endNo)
                           join b in Data_CouponInfoService.N.GetModels().AsEnumerable() on a.CouponID equals b.ID
                           select new
                           {
                               CouponIndex = a.CouponIndex,
                               CouponCode = a.CouponCode,
                               CouponName = b.CouponName,
                               EndDate = Utils.ConvertFromDatetime(b.EndDate, "yyyy-MM-dd"),
                               Note = string.Empty
                           };

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, linq);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 查询调拨详情
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetCouponAssigned(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                int couponId = dicParas.Get("couponId").Toint(0);
                if (couponId == 0)
                {
                    errMsg = "优惠券ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var linq = from d in
                               (from a in Data_CouponInfoService.N.GetModels(p => p.ID == couponId)
                                join b in Data_CouponListService.N.GetModels() on a.ID equals b.CouponID
                                join c in Base_StoreInfoService.N.GetModels() on b.StoreID equals c.StoreID into c1
                                from c in c1.DefaultIfEmpty()
                                select new { a = a, CouponID = b.CouponID, StoreID = b.StoreID, StoreName = (c != null ? c.StoreName : string.Empty), CouponIndex = b.CouponIndex, State = b.State }).AsEnumerable()
                           group d by new { d.CouponID, d.StoreID } into g
                           select new
                           {
                               CouponID = g.Key.CouponID,
                               CouponName = g.FirstOrDefault().a.CouponName,
                               StoreID = g.Key.StoreID,
                               StoreName = g.FirstOrDefault().StoreName,
                               AssignedCount = g.FirstOrDefault().a.PublishCount - g.Count(m => m.State == (int)CouponState.NotAssigned),
                               StartEnd = g.Min(m => m.CouponIndex) + "~" + g.Max(m => m.CouponIndex)
                           };

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, linq);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }
    }
}