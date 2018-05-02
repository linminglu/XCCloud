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
        IData_CouponListService data_CouponListService = BLLContainer.Resolve<IData_CouponListService>();
        IData_CouponInfoService data_CouponInfoService = BLLContainer.Resolve<IData_CouponInfoService>();
        IFlw_CouponUseService flw_CouponUseService = BLLContainer.Resolve<IFlw_CouponUseService>();
        IData_Coupon_StoreListService data_Coupon_StoreListService = BLLContainer.Resolve<IData_Coupon_StoreListService>();

        private bool isSend(int iId)
        {
            return data_CouponListService.Any(p => p.CouponID == iId && p.MemberID != null);
        }
        private bool isUsed(int iId)
        {
            return (from a in data_CouponListService.GetModels(p => p.CouponID == iId)
                    join b in flw_CouponUseService.GetModels() on a.ID equals b.CouponCode
                    select 1).Any() || flw_CouponUseService.Any(p=>p.CouponID == iId);
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

                string sql = @"select a.ID, a.CouponName, b.DictKey as CouponTypeStr, a.PublishCount, (a.PublishCount - isnull(c.UseCount, 0) - isnull(d.UseCount, 0)) as LeftCount, " +
                    " (case when a.StartTime is null or a.StartTime='' then '' else convert(varchar,a.StartTime,23) end) as StartTime, (case when a.EndTime is null or a.EndTime='' then '' else convert(varchar,a.EndTime,23) end) as EndTime, " +
                    " a.Context from Data_CouponInfo a" +
                    " left join (select b.* from Dict_System a inner join Dict_System b on a.ID=b.PID where a.DictKey='优惠券类别' and a.PID=0) b on convert(varchar, a.CouponType)=b.DictValue " +
                    " left join (select a.ID as CouponID, count(c.ID) as UseCount from Data_CouponInfo a inner join Data_CouponList b on a.ID=b.CouponID inner join Flw_CouponUse c on b.ID=c.CouponCode group by a.ID) c on a.ID=c.CouponID " +
                    " left join (select a.ID as CouponID, count(b.ID) as UseCount from Data_CouponInfo a inner join Flw_CouponUse b on a.ID=b.CouponID group by a.ID) d on a.ID=d.CouponID " + 
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
                string id = dicParas.ContainsKey("id") ? (dicParas["id"] + "") : string.Empty;
                if (string.IsNullOrEmpty(id))
                {
                    errMsg = "规则编号不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                int iId = Convert.ToInt32(id);
                IData_CouponInfoService data_CouponInfoService = BLLContainer.Resolve<IData_CouponInfoService>();
                var data_CouponInfo = data_CouponInfoService.GetModels(p => p.ID == iId).FirstOrDefault();
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
                string id = dicParas.ContainsKey("id") ? (dicParas["id"] + "") : string.Empty;
                string couponName = dicParas.ContainsKey("couponName") ? (dicParas["couponName"] + "") : string.Empty;
                string couponCodeStart = dicParas.ContainsKey("couponCodeStart") ? (dicParas["couponCodeStart"] + "") : string.Empty;
                string couponCodeEnd = dicParas.ContainsKey("couponCodeEnd") ? (dicParas["couponCodeEnd"] + "") : string.Empty;
                string couponType = dicParas.ContainsKey("couponType") ? (dicParas["couponType"] + "") : string.Empty;
                string entryCouponFlag = dicParas.ContainsKey("entryCouponFlag") ? (dicParas["entryCouponFlag"] + "") : string.Empty;
                string authorFlag = dicParas.ContainsKey("authorFlag") ? (dicParas["authorFlag"] + "") : string.Empty;
                string overUseCount = dicParas.ContainsKey("overUseCount") ? (dicParas["overUseCount"] + "") : string.Empty;
                string publishCount = dicParas.ContainsKey("publishCount") ? (dicParas["publishCount"] + "") : string.Empty;
                string couponValue = dicParas.ContainsKey("couponValue") ? (dicParas["couponValue"] + "") : string.Empty;
                string couponDiscount = dicParas.ContainsKey("couponDiscount") ? (dicParas["couponDiscount"] + "") : string.Empty;
                string couponThreshold = dicParas.ContainsKey("couponThreshold") ? (dicParas["couponThreshold"] + "") : string.Empty;
                string startTime = dicParas.ContainsKey("startTime") ? (dicParas["startTime"] + "") : string.Empty;
                string endTime = dicParas.ContainsKey("endTime") ? (dicParas["endTime"] + "") : string.Empty;
                string sendType = dicParas.ContainsKey("sendType") ? (dicParas["sendType"] + "") : string.Empty;
                string overMoney = dicParas.ContainsKey("overMoney") ? (dicParas["overMoney"] + "") : string.Empty;
                string freeCouponCount = dicParas.ContainsKey("freeCouponCount") ? (dicParas["freeCouponCount"] + "") : string.Empty;
                string jackpotCount = dicParas.ContainsKey("jackpotCount") ? (dicParas["jackpotCount"] + "") : string.Empty;
                string jackpotId = dicParas.ContainsKey("jackpotId") ? (dicParas["jackpotId"] + "") : string.Empty;
                string chargeType = dicParas.ContainsKey("chargeType") ? (dicParas["chargeType"] + "") : string.Empty;
                string chargeCount = dicParas.ContainsKey("chargeCount") ? (dicParas["chargeCount"] + "") : string.Empty;
                string goodId = dicParas.ContainsKey("goodId") ? (dicParas["goodId"] + "") : string.Empty;
                string projectId = dicParas.ContainsKey("projectId") ? (dicParas["projectId"] + "") : string.Empty;
                string context = dicParas.ContainsKey("context") ? (dicParas["context"] + "") : string.Empty;
                string storeIds = dicParas.ContainsKey("storeIds") ? (dicParas["storeIds"] + "") : string.Empty;
                string memberIds = dicParas.ContainsKey("memberIds") ? (dicParas["memberIds"] + "") : string.Empty;

                int iId = 0;
                int.TryParse(id, out iId);
                
                //已派发或使用的优惠券规则不能修改                
                if (isSend(iId) || isUsed(iId))
                {
                    errMsg = "已派发或使用的优惠券规则不能修改";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }                 

                #region 验证参数

                if (string.IsNullOrEmpty(couponName))
                {
                    errMsg = "优惠券名称不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (string.IsNullOrEmpty(publishCount))
                {
                    errMsg = "发行张数不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (!Utils.isNumber(publishCount))
                {
                    errMsg = "发行张数格式不正确";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if(string.IsNullOrEmpty(entryCouponFlag))
                {
                    errMsg = "实物券标记不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (string.IsNullOrEmpty(startTime) || string.IsNullOrEmpty(endTime))
                {
                    errMsg = "使用期限不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                int iPublishCount = Convert.ToInt32(publishCount);
                if (iPublishCount < 0)
                {
                    errMsg = "发行张数不能小于0";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (string.IsNullOrEmpty(couponType))
                {
                    errMsg = "优惠券类别不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                int? iOverUseCount = (int?)null;
                int iCouponType = Convert.ToInt32(couponType);
                int iEntryCouponFlag = Convert.ToInt32(entryCouponFlag);
                if (iCouponType == (int)CouponType.Cash || iCouponType == (int)CouponType.Discount)
                {
                    if (string.IsNullOrEmpty(couponValue))
                    {
                        errMsg = "优惠券价值不能为空";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }

                    if (!Utils.IsDecimal(couponValue))
                    {
                        errMsg = "优惠券价值格式不正确";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }

                    if (string.IsNullOrEmpty(couponDiscount))
                    {
                        errMsg = "优惠券折扣不能为空";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }

                    if (!Utils.IsDecimal(couponDiscount))
                    {
                        errMsg = "优惠券折扣格式不正确";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }

                    if (string.IsNullOrEmpty(couponThreshold))
                    {
                        errMsg = "优惠券阈值不能为空";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }

                    if (!Utils.IsDecimal(couponThreshold))
                    {
                        errMsg = "优惠券阈值格式不正确";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }

                    if (string.IsNullOrEmpty(overUseCount))
                    {
                        errMsg = "同时使用张数不能为空";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }

                    if (!Utils.isNumber(overUseCount))
                    {
                        errMsg = "同时使用张数格式不正确";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }

                    iOverUseCount = Convert.ToInt32(overUseCount);
                    if (iOverUseCount < 0)
                    {
                        errMsg = "叠加使用张数不能小于0";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                }

                int? iChargeType = (int?)null, iChargeCount = (int?)null, iGoodId = (int?)null, iProjectId = (int?)null;
                if (iCouponType == (int)CouponType.Charge)
                {
                    if (string.IsNullOrEmpty(chargeType))
                    {
                        errMsg = "兑换类型不能为空";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }

                    iChargeType = Convert.ToInt32(chargeType);
                    if (iChargeType == (int)ChargeType.Good)
                    {
                        if(string.IsNullOrEmpty(goodId))
                        {
                            errMsg = "兑换内容不能为空";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }
                        iGoodId = Convert.ToInt32(goodId);
                    }

                    if (iChargeType == (int)ChargeType.Project)
                    {
                        if (string.IsNullOrEmpty(projectId))
                        {
                            errMsg = "兑换内容不能为空";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }
                        iProjectId = Convert.ToInt32(projectId);
                    }

                    if (string.IsNullOrEmpty(chargeCount))
                    {
                        errMsg = "兑换数量不能为空";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }

                    if (!Utils.isNumber(chargeCount))
                    {
                        errMsg = "兑换数量格式不正确";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }

                    iChargeCount = Convert.ToInt32(chargeCount);
                    if (iChargeCount < 0)
                    {
                        errMsg = "兑换数量不能小于0";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                }

                if (string.IsNullOrEmpty(sendType))
                {
                    errMsg = "派发途径不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                int? iFreeCouponCount = (int?)null, iJackpotCount = (int?)null, iJackpotId = (int?)null;
                int iSendType = Convert.ToInt32(sendType);                
                if (iSendType == (int)SendType.Consume || iSendType == (int)SendType.Jackpot)
                {
                    if (string.IsNullOrEmpty(overMoney))
                    {
                        errMsg = "消费满金额不能为空";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }

                    if (!Utils.IsDecimal(overMoney))
                    {
                        errMsg = "消费满金额格式不正确";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }

                    if (iSendType == (int)SendType.Consume)
                    {
                        if (string.IsNullOrEmpty(freeCouponCount))
                        {
                            errMsg = "送券数不能为空";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        if (!Utils.isNumber(freeCouponCount))
                        {
                            errMsg = "送券数格式不正确";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        iFreeCouponCount = Convert.ToInt32(freeCouponCount);
                        if (iFreeCouponCount < 0)
                        {
                            errMsg = "送券数不能小于0";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }
                    }
                    
                    if (iSendType == (int)SendType.Jackpot)
                    {
                        if (string.IsNullOrEmpty(jackpotCount))
                        {
                            errMsg = "抽奖次数不能为空";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        if (!Utils.isNumber(jackpotCount))
                        {
                            errMsg = "抽奖次数格式不正确";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        if (iJackpotCount < 0)
                        {
                            errMsg = "抽奖次数不能小于0";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        if (string.IsNullOrEmpty(jackpotId))
                        {
                            errMsg = "抽奖活动不能为空";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }
                        iJackpotId = Convert.ToInt32(jackpotId);
                    }

                    if (iSendType == (int)SendType.Orient)
                    {
                        if (string.IsNullOrEmpty(memberIds))
                        {
                            errMsg = "派发会员不能为空";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }
                    }

                    //如果印刷编号为空，派发方式请选择街边派发                   
                    if (iEntryCouponFlag == (int)EntryCouponFlag.Entry && string.IsNullOrEmpty(couponCodeStart) && string.IsNullOrEmpty(couponCodeEnd) && iSendType != (int)SendType.Delivery)
                    {
                        errMsg = "如果印刷编号为空，派发方式请选择街边派发";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                }

                #endregion                

                #region 验证印刷编号
                //验证印刷编号                               
                int iCouponCodeStart = -1, iCouponCodeEnd = -1, len = 0;
                string pre = string.Empty;
                if (!string.IsNullOrEmpty(couponCodeStart) && !string.IsNullOrEmpty(couponCodeEnd))
                {
                    Regex regex = new Regex(@"[0-9]+(?=[^0-9]*$)");
                    int index1 = regex.Match(couponCodeStart).Index;
                    index1 = (index1 < 0) ? 0 : index1;
                    int index2 = regex.Match(couponCodeEnd).Index;
                    index2 = (index2 < 0) ? 0 : index2; 
                    int.TryParse(couponCodeStart.Substring(index1), out iCouponCodeStart);
                    int.TryParse(couponCodeEnd.Substring(index2), out iCouponCodeEnd);
                    string pre1 = couponCodeStart.Substring(0, index1);
                    string pre2 = couponCodeEnd.Substring(0, index2);
                    
                    if (iCouponCodeStart < 0 || iCouponCodeEnd < 0)
                    {
                        errMsg = "印刷编号格式不正确，须以数字结尾";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }

                    if (!pre1.Equals(pre2, StringComparison.OrdinalIgnoreCase))
                    {
                        errMsg = "印刷编号起码与止码的前缀部分须相同";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }

                    if ((iCouponCodeEnd - iCouponCodeStart + 1) != iPublishCount)
                    {
                        errMsg = "印刷编号数量与发行张数不相等";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }

                    len = couponCodeStart.Length - index1;
                    pre = pre1;
                }
                
                #endregion

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {                                   
                        var data_CouponInfo = new Data_CouponInfo();
                        data_CouponInfo.ID = iId;
                        data_CouponInfo.CouponName = couponName;
                        data_CouponInfo.CouponType = iCouponType;
                        data_CouponInfo.EntryCouponFlag = iEntryCouponFlag;
                        data_CouponInfo.AuthorFlag = !string.IsNullOrEmpty(authorFlag) ? Convert.ToInt32(authorFlag) : (int?)null;
                        data_CouponInfo.OverUseCount = iOverUseCount;
                        data_CouponInfo.PublishCount = iPublishCount;
                        data_CouponInfo.CouponValue = !string.IsNullOrEmpty(couponValue) ? Convert.ToDecimal(couponValue) : (decimal?)null;
                        data_CouponInfo.CouponDiscount = !string.IsNullOrEmpty(couponDiscount) ? Convert.ToDecimal(couponDiscount) : (decimal?)null;
                        data_CouponInfo.CouponThreshold = !string.IsNullOrEmpty(couponThreshold) ? Convert.ToDecimal(couponThreshold) : (decimal?)null;
                        data_CouponInfo.StartTime = Convert.ToDateTime(startTime);
                        data_CouponInfo.EndTime = Convert.ToDateTime(endTime);
                        data_CouponInfo.SendType = iSendType;
                        data_CouponInfo.OverMoney = !string.IsNullOrEmpty(overMoney) ? Convert.ToDecimal(overMoney) : (decimal?)null;
                        data_CouponInfo.FreeCouponCount = iFreeCouponCount;
                        data_CouponInfo.JackpotCount = iJackpotCount;
                        data_CouponInfo.JackpotID = iJackpotId;
                        data_CouponInfo.ChargeType = iChargeType;
                        data_CouponInfo.ChargeCount = iChargeCount;
                        data_CouponInfo.GoodID = iGoodId;
                        data_CouponInfo.ProjectID = iProjectId;
                        data_CouponInfo.OpUserID = userId;
                        data_CouponInfo.Context = context;
                        data_CouponInfo.MerchID = merchId;
                        if (!data_CouponInfoService.Any(a => a.ID == iId))
                        {
                            //新增
                            if (!data_CouponInfoService.Add(data_CouponInfo))
                            {
                                errMsg = "添加优惠券信息失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }
                        else
                        {
                            //修改
                            if (!data_CouponInfoService.Update(data_CouponInfo))
                            {
                                errMsg = "修改优惠券信息失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }

                        iId = data_CouponInfo.ID;

                        //添加优惠券记录表  
                        var memberArr = memberIds.Split('|');
                        if (iEntryCouponFlag == (int)EntryCouponFlag.Digit || iCouponCodeStart >= 0)
                        {
                            foreach (var model in data_CouponListService.GetModels(p => p.CouponID == iId))
                            {
                                data_CouponListService.DeleteModel(model);
                            }

                            for (var i = 0; i < iPublishCount; i++)
                            {
                                var data_CouponListModel = new Data_CouponList();
                                if (iEntryCouponFlag == (int)EntryCouponFlag.Digit)
                                {
                                    data_CouponListModel.CouponCode = System.Guid.NewGuid().ToString("N");
                                }
                                else
                                {
                                    data_CouponListModel.CouponCode = pre + (i + iCouponCodeStart).ToString("D" + len);
                                }
                                
                                data_CouponListModel.CouponID = iId;
                                if (memberArr.Length > i)
                                {
                                    data_CouponListModel.MemberID = !string.IsNullOrEmpty(memberArr[i]) ? Convert.ToInt32(memberArr[i]) : (int?)null;
                                }

                                data_CouponListModel.SendAuthorID = userId;
                                data_CouponListModel.PublishType = iEntryCouponFlag;
                                data_CouponListModel.SendType = iSendType;
                                data_CouponListModel.State = (int)CouponState.UnUsed;
                                data_CouponListService.AddModel(data_CouponListModel);
                            }

                            if (!data_CouponListService.SaveChanges())
                            {
                                errMsg = "添加优惠券记录表失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }

                        //更新优惠券适用门店表
                        foreach (var model in data_Coupon_StoreListService.GetModels(p => p.CouponID == iId))
                        {
                            data_Coupon_StoreListService.DeleteModel(model);
                        }

                        var storeIdArr = storeIds.Split('|');
                        foreach (var storeId in storeIdArr)
                        {
                            var model = new Data_Coupon_StoreList();
                            model.CouponID = iId;
                            model.StoreID = storeId;
                            data_Coupon_StoreListService.AddModel(model);
                        }

                        if (!data_Coupon_StoreListService.SaveChanges())
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
        public object DelCouponInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string id = dicParas.ContainsKey("id") ? (dicParas["id"] + "") : string.Empty;
                if (string.IsNullOrEmpty(id))
                {
                    errMsg = "规则ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                int iId = Convert.ToInt32(id);                
                if (!data_CouponInfoService.Any(a => a.ID == iId))
                {
                    errMsg = "该优惠券规则信息不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (isSend(iId) || isUsed(iId))
                {
                    errMsg = "已派发或使用的优惠券规则不能删除";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }
                
                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        var data_CouponInfo = data_CouponInfoService.GetModels(p => p.ID == iId).FirstOrDefault();
                        data_CouponInfoService.DeleteModel(data_CouponInfo);

                        var data_Coupon_StoreList = data_Coupon_StoreListService.GetModels(p => p.CouponID == iId);
                        foreach (var model in data_Coupon_StoreList)
                        {
                            data_Coupon_StoreListService.DeleteModel(model);
                        }

                        if (!data_CouponInfoService.SaveChanges())
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

                var storeIDs = data_Coupon_StoreListService.GetModels(p => p.CouponID == couponId).Select(o => new { StoreID = o.StoreID });
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
                string couponId = dicParas.ContainsKey("couponId") ? (dicParas["couponId"] + "") : string.Empty;
                string storeIds = dicParas.ContainsKey("storeIds") ? (dicParas["storeIds"] + "") : string.Empty;

                if (string.IsNullOrEmpty(couponId))
                {
                    errMsg = "优惠券规则ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        int iCouponId = Convert.ToInt32(couponId);
                        foreach (var model in data_Coupon_StoreListService.GetModels(p => p.CouponID == iCouponId))
                        {
                            data_Coupon_StoreListService.DeleteModel(model);
                        }

                        var storeIdArr = storeIds.Split('|');
                        foreach (var storeId in storeIdArr)
                        {
                            var model = new Data_Coupon_StoreList();
                            model.CouponID = iCouponId;
                            model.StoreID = storeId;
                            data_Coupon_StoreListService.AddModel(model);
                        }

                        if (!data_Coupon_StoreListService.SaveChanges())
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
                string couponId = dicParas.ContainsKey("couponId") ? (dicParas["couponId"] + "") : string.Empty;

                if (string.IsNullOrEmpty(couponId))
                {
                    errMsg = "优惠券规则ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                int iCouponId = Convert.ToInt32(couponId);
                var data_Coupon_MemberList = data_CouponListService.GetModels(p => p.CouponID == iCouponId).Select(o => o.MemberID);
                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, data_Coupon_MemberList);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }
    }
}