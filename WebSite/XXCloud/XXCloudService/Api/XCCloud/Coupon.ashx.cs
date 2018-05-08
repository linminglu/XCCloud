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
using System.Data;
using System.IO;
using XCCloudService.Business.XCCloud;
using Microsoft.SqlServer.Server;
using XCCloudService.BLL.CommonBLL;
using XCCloudService.BLL.Extentions;

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
                parameters[0] = new SqlParameter("@MerchId", merchId);

                string sqlWhere = string.Empty;
                if (conditions != null && conditions.Length > 0)
                {
                    if (!QueryBLL.GenDynamicSql(conditions, "a.", ref sqlWhere, ref parameters, out errMsg))
                    {
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                }

                string storedProcedure = "QueryCouponInfo";
                Array.Resize(ref parameters, parameters.Length + 1);
                parameters[parameters.Length - 1] = new SqlParameter("@SqlWhere", sqlWhere);
                Array.Resize(ref parameters, parameters.Length + 1);
                parameters[parameters.Length - 1] = new SqlParameter("@Result", SqlDbType.Int);
                parameters[parameters.Length - 1].Direction = ParameterDirection.Output;
                System.Data.DataSet ds = XCCloudBLL.GetStoredProcedureSentence(storedProcedure, parameters);
                if (parameters[parameters.Length - 1].Value.ToString() != "1")
                {
                    errMsg = "查询优惠券数据失败";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var data_CouponInfo = Utils.GetModelList<Data_CouponInfoModel>(ds.Tables[0]).ToList();                

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
                if (!dicParas.Get("weekType").Validint("时段类型", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("startTime").Validdate("使用时段", out errMsg) || !dicParas.Get("endTime").Validdate("使用时段", out errMsg))
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

                //时段类型
                if (dicParas.Get("weekType").Toint() == (int)TimeType.Custom)
                {
                    if (!dicParas.Get("week").Nonempty("优惠周天", out errMsg))
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                //使用期限
                if (dicParas.Get("startDate").Todatetime() > dicParas.Get("endDate").Todatetime())
                {
                    errMsg = "使用期限范围不正确，开始时间不能大于结束时间";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                //使用时段
                if (dicParas.Get("startTime").Totimespan() > dicParas.Get("endTime").Totimespan())
                {
                    errMsg = "使用时段范围不正确，开始时段不能大于结束时段";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                //不可用期限
                if (!string.IsNullOrEmpty(dicParas.Get("noStartDate")))
                {
                    if (!dicParas.Get("noStartDate").Validdate("不可用期限", out errMsg) || !dicParas.Get("noEndDate").Validdate("不可用期限", out errMsg))
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    if (dicParas.Get("noStartDate").Todatetime() > dicParas.Get("noEndDate").Todatetime())
                    {
                        errMsg = "不可用期限范围不正确，开始时间不能大于结束时间";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                }

                int id = dicParas.Get("id").Toint(0);
                var entryCouponFlag = dicParas.Get("entryCouponFlag").Toint();
                var publishCount = dicParas.Get("publishCount").Toint();
                var sendType = dicParas.Get("sendType").Toint();
                var couponConditions = dicParas.GetArray("couponConditions");
                var memberIds = dicParas.GetArray("memberIds");

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
                                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                }
                            }

                            if (!Data_CouponConditionService.I.SaveChanges())
                            {
                                errMsg = "保存优惠券派发条件失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }

                        //添加未分配优惠券, 单门店直接调拨
                        string singleStoreId = string.Empty;
                        bool isSingle = XCCloudStoreBusiness.IsSingleStore(merchId, out singleStoreId);
                        string storedProcedure = "CreateCouponRecord";
                        List<SqlDataRecord> listSqlDataRecord = new List<SqlDataRecord>();
                        SqlMetaData[] MetaDataArr = new SqlMetaData[] { new SqlMetaData("MemberID", SqlDbType.Int) };
                        if (memberIds != null && memberIds.Length > 0)
                        {
                            for (int i = 0; i < memberIds.Length; i++)
                            {
                                var record = new SqlDataRecord(MetaDataArr);
                                record.SetValue(0, memberIds[i]);
                                listSqlDataRecord.Add(record);
                            }
                        }
                        else
                        {
                            var record = new SqlDataRecord(MetaDataArr);
                            record.SetValue(0, 0);
                            listSqlDataRecord.Add(record);
                        }

                        SqlParameter[] parameters = new SqlParameter[0];
                        Array.Resize(ref parameters, parameters.Length + 1);
                        parameters[parameters.Length - 1] = new SqlParameter("@CouponID", id);
                        Array.Resize(ref parameters, parameters.Length + 1);
                        parameters[parameters.Length - 1] = new SqlParameter("@SendAuthorID", userId);
                        Array.Resize(ref parameters, parameters.Length + 1);
                        parameters[parameters.Length - 1] = new SqlParameter("@SendTime", DateTime.Now);
                        Array.Resize(ref parameters, parameters.Length + 1);
                        parameters[parameters.Length - 1] = new SqlParameter("@PublishType", entryCouponFlag);
                        Array.Resize(ref parameters, parameters.Length + 1);
                        parameters[parameters.Length - 1] = new SqlParameter("@SendType", sendType);
                        Array.Resize(ref parameters, parameters.Length + 1);
                        parameters[parameters.Length - 1] = new SqlParameter("@MerchID", merchId);
                        Array.Resize(ref parameters, parameters.Length + 1);
                        parameters[parameters.Length - 1] = new SqlParameter("@StoreID", singleStoreId);
                        Array.Resize(ref parameters, parameters.Length + 1);
                        parameters[parameters.Length - 1] = new SqlParameter("@IsSingle", isSingle ? 1 : 0);
                        Array.Resize(ref parameters, parameters.Length + 1);
                        parameters[parameters.Length - 1] = new SqlParameter("@PublishCount", publishCount);
                        Array.Resize(ref parameters, parameters.Length + 1);
                        parameters[parameters.Length - 1] = new SqlParameter("@MemberIDsType", SqlDbType.Structured);
                        parameters[parameters.Length - 1].Value = listSqlDataRecord;
                        parameters[parameters.Length - 1].TypeName = "dbo.MemberIDsType";
                        Array.Resize(ref parameters, parameters.Length + 1);
                        parameters[parameters.Length - 1] = new SqlParameter("@Result", 0);
                        parameters[parameters.Length - 1].Direction = ParameterDirection.Output;

                        //XCCloudBLL.ExecuteStoredProcedureSentence(storedProcedure, parameters);
                        XCCloudBLLExt.ExecuteStoredProcedure(storedProcedure, parameters);                        
                        if (parameters[parameters.Length - 1].Value.ToString() != "1")
                        {
                            errMsg = "添加优惠券记录表失败";
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

                var storeList = from a in Data_Coupon_StoreListService.N.GetModels(p => p.CouponID == couponId)
                                join b in Base_StoreInfoService.N.GetModels() on a.StoreID equals b.StoreID
                                                                
                                select new { StoreID = a.StoreID, StoreName = b.StoreName };
                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, storeList);
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

                var linq = from c in
                               (from a in Data_CouponInfoService.N.GetModels()
                                join b in Data_CouponListService.N.GetModels(p => p.State == (int)CouponState.NotAssigned && p.CouponID == couponId) on a.ID equals b.CouponID
                                select new { a = a, CouponID = b.CouponID, CouponIndex = b.CouponIndex }).AsEnumerable()
                           group c by c.CouponID into g
                           select new
                           {
                               CouponName = g.FirstOrDefault().a.CouponName,
                               StartDate = Utils.ConvertFromDatetime(g.FirstOrDefault().a.StartDate, "yyyy-MM-dd"),
                               EndDate = Utils.ConvertFromDatetime(g.FirstOrDefault().a.EndDate, "yyyy-MM-dd"),
                               NotAssignedCount = g.Count(),
                               //StartNo = g.Min(m => m.CouponIndex),
                               //EndNo = g.Max(m => m.CouponIndex)
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

                var query = Data_CouponListService.N.GetModels(p => p.State == (int)CouponState.NotActivated && p.CouponID == couponId);
                if (!string.IsNullOrEmpty(storeId))
                {
                    query = query.Where(w => w.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase));
                }
                var linq = from c in
                               (from a in Data_CouponInfoService.N.GetModels()
                                join b in query on a.ID equals b.CouponID
                                select new { a = a, CouponID = b.CouponID, CouponIndex = b.CouponIndex }).AsEnumerable()
                           group c by c.CouponID into g
                           select new
                           {
                               CouponName = g.FirstOrDefault().a.CouponName,
                               StartDate = Utils.ConvertFromDatetime(g.FirstOrDefault().a.StartDate, "yyyy-MM-dd"),
                               EndDate = Utils.ConvertFromDatetime(g.FirstOrDefault().a.EndDate, "yyyy-MM-dd"),
                               NotActivatedCount = g.Count(),
                               //StartNo = g.Min(m => m.CouponIndex),
                               //EndNo = g.Max(m => m.CouponIndex),
                           };

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, linq);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        private struct NoArrayType
        {
            public int StartNo { get; set; }
            public int EndNo { get; set; }
        }

        private bool checkNoArr(object[] noArr, out List<NoArrayType> nolist, out int total, out string errMsg)
        {
            total = 0;
            errMsg = string.Empty;
            nolist = new List<NoArrayType>();
            if (noArr == null || noArr.Count() <= 0)
            {
                errMsg = "优惠券起止码序列不能为空";
                return false;
            }
            
            foreach (IDictionary<string, object> el in noArr)
            {
                if (el != null)
                {
                    var dicPara = new Dictionary<string, object>(el, StringComparer.OrdinalIgnoreCase);
                    if (!dicPara.Get("startNo").Validint("开始序号", out errMsg))
                        return false;
                    if (!dicPara.Get("endNo").Validint("结束序号", out errMsg))
                        return false;
                    if (dicPara.Get("startNo").Toint() > dicPara.Get("endNo").Toint())
                    {
                        errMsg = "开始序号不能大于结束序号";
                        return false;
                    }
                    if (dicPara.Get("startNo").Toint() == 0)
                    {
                        errMsg = "开始序号须大于0";
                        return false;
                    }

                    int startNo = dicPara.Get("startNo").Toint(0);
                    int endNo = dicPara.Get("endNo").Toint(0);

                    if (!nolist.Any(p => (startNo >= p.StartNo && startNo <= p.EndNo) || (endNo >= p.StartNo && endNo <= p.EndNo)))
                    {
                        nolist.Add(new NoArrayType { StartNo = startNo, EndNo = endNo });
                    }
                    else
                    {
                        errMsg = "优惠券起止码范围不能重叠";
                        return false;
                    }

                    total += endNo - startNo + 1;
                }
                else
                {
                    errMsg = "提交数据包含空对象";
                    return false;
                }
            }   

            return true;
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object SaveCouponNotAssigned(Dictionary<string, object> dicParas)
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
                       
                if (!dicParas.Get("storeId").Nonempty("门店ID不能为空", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                int total = 0;
                var nolist = new List<NoArrayType>();         
                if(!checkNoArr(dicParas.GetArray("noArr"), out nolist, out total, out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var storeId = dicParas.Get("storeId");
                             
                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        string storedProcedure = "SaveCouponNotAssigned";
                        List<SqlDataRecord> listSqlDataRecord = new List<SqlDataRecord>();
                        SqlMetaData[] MetaDataArr = new SqlMetaData[] { new SqlMetaData("StartNo", SqlDbType.Int), new SqlMetaData("EndNo", SqlDbType.Int) };
                        if (nolist != null && nolist.Count() > 0)
                        {
                            for (int i = 0; i < nolist.Count(); i++)
                            {
                                var record = new SqlDataRecord(MetaDataArr);
                                record.SetValue(0, nolist[i].StartNo);
                                record.SetValue(1, nolist[i].EndNo);
                                listSqlDataRecord.Add(record);
                            }
                        }
                        else
                        {
                            var record = new SqlDataRecord(MetaDataArr);
                            record.SetValue(0, 0);
                            record.SetValue(1, 0);
                            listSqlDataRecord.Add(record);
                        }

                        SqlParameter[] parameters = new SqlParameter[0];
                        Array.Resize(ref parameters, parameters.Length + 1);
                        parameters[parameters.Length - 1] = new SqlParameter("@CouponID", couponId);
                        Array.Resize(ref parameters, parameters.Length + 1);
                        parameters[parameters.Length - 1] = new SqlParameter("@StoreID", storeId);
                        Array.Resize(ref parameters, parameters.Length + 1);
                        parameters[parameters.Length - 1] = new SqlParameter("@Total", total);
                        Array.Resize(ref parameters, parameters.Length + 1);
                        parameters[parameters.Length - 1] = new SqlParameter("@NoArrayType", SqlDbType.Structured);
                        parameters[parameters.Length - 1].Value = listSqlDataRecord;
                        parameters[parameters.Length - 1].TypeName = "dbo.NoArrayType";
                        Array.Resize(ref parameters, parameters.Length + 1);
                        parameters[parameters.Length - 1] = new SqlParameter("@Result", 0);
                        parameters[parameters.Length - 1].Direction = ParameterDirection.Output;
                        Array.Resize(ref parameters, parameters.Length + 1);
                        parameters[parameters.Length - 1] = new SqlParameter("@ErrMsg", SqlDbType.VarChar, 200);
                        parameters[parameters.Length - 1].Direction = ParameterDirection.Output;

                        XCCloudBLLExt.ExecuteStoredProcedure(storedProcedure, parameters);                       
                        if (parameters[parameters.Length - 2].Value.ToString() != "1")
                        {
                            errMsg = "优惠券调拨失败：\n";
                            errMsg = errMsg + parameters[parameters.Length - 1].Value.ToString();
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
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as MerchDataModel).StoreID;

                string errMsg = string.Empty;
                int couponId = dicParas.Get("couponId").Toint(0);
                if (couponId == 0)
                {
                    errMsg = "优惠券ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                int total = 0;
                var nolist = new List<NoArrayType>();
                if (!checkNoArr(dicParas.GetArray("noArr"), out nolist, out total, out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                
                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        string storedProcedure = "SaveCouponNotActivated";
                        List<SqlDataRecord> listSqlDataRecord = new List<SqlDataRecord>();
                        SqlMetaData[] MetaDataArr = new SqlMetaData[] { new SqlMetaData("StartNo", SqlDbType.Int), new SqlMetaData("EndNo", SqlDbType.Int) };
                        if (nolist != null && nolist.Count() > 0)
                        {
                            for (int i = 0; i < nolist.Count(); i++)
                            {
                                var record = new SqlDataRecord(MetaDataArr);
                                record.SetValue(0, nolist[i].StartNo);
                                record.SetValue(1, nolist[i].EndNo);
                                listSqlDataRecord.Add(record);
                            }
                        }
                        else
                        {
                            var record = new SqlDataRecord(MetaDataArr);
                            record.SetValue(0, 0);
                            record.SetValue(1, 0);
                            listSqlDataRecord.Add(record);
                        }

                        SqlParameter[] parameters = new SqlParameter[0];
                        Array.Resize(ref parameters, parameters.Length + 1);
                        parameters[parameters.Length - 1] = new SqlParameter("@CouponID", couponId);
                        Array.Resize(ref parameters, parameters.Length + 1);
                        parameters[parameters.Length - 1] = new SqlParameter("@StoreID", storeId);
                        Array.Resize(ref parameters, parameters.Length + 1);
                        parameters[parameters.Length - 1] = new SqlParameter("@Total", total);
                        Array.Resize(ref parameters, parameters.Length + 1);
                        parameters[parameters.Length - 1] = new SqlParameter("@NoArrayType", SqlDbType.Structured);
                        parameters[parameters.Length - 1].Value = listSqlDataRecord;
                        parameters[parameters.Length - 1].TypeName = "dbo.NoArrayType";
                        Array.Resize(ref parameters, parameters.Length + 1);
                        parameters[parameters.Length - 1] = new SqlParameter("@Result", 0);
                        parameters[parameters.Length - 1].Direction = ParameterDirection.Output;
                        Array.Resize(ref parameters, parameters.Length + 1);
                        parameters[parameters.Length - 1] = new SqlParameter("@ErrMsg", SqlDbType.VarChar, 200);
                        parameters[parameters.Length - 1].Direction = ParameterDirection.Output;

                        XCCloudBLLExt.ExecuteStoredProcedure(storedProcedure, parameters);
                        if (parameters[parameters.Length - 2].Value.ToString() != "1")
                        {
                            errMsg = "优惠券派发失败：\n";
                            errMsg = errMsg + parameters[parameters.Length - 1].Value.ToString();
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
                var id = dicParas.Get("id").Toint(0);

                string storedProcedure = "LockCouponRecord";
                SqlParameter[] parameters = new SqlParameter[0];
                Array.Resize(ref parameters, parameters.Length + 1);
                parameters[parameters.Length - 1] = new SqlParameter("@CouponID", couponId);
                Array.Resize(ref parameters, parameters.Length + 1);
                parameters[parameters.Length - 1] = new SqlParameter("@ID", id);
                Array.Resize(ref parameters, parameters.Length + 1);
                parameters[parameters.Length - 1] = new SqlParameter("@StoreID", storeId);
                Array.Resize(ref parameters, parameters.Length + 1);
                parameters[parameters.Length - 1] = new SqlParameter("@IsLock", isLock);           
                Array.Resize(ref parameters, parameters.Length + 1);
                parameters[parameters.Length - 1] = new SqlParameter("@Result", SqlDbType.Int);
                parameters[parameters.Length - 1].Direction = ParameterDirection.Output;

                XCCloudBLL.ExecuteStoredProcedureSentence(storedProcedure, parameters);
                if (parameters[parameters.Length - 1].Value.ToString() != "1")
                {
                    errMsg = "锁定操作失败";                    
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        private bool isEveryDay(string weekDays)
        {
            return !string.IsNullOrEmpty(weekDays) && weekDays.Contains("1") && weekDays.Contains("2") && weekDays.Contains("3") && weekDays.Contains("4") && weekDays.Contains("5") && weekDays.Contains("6") && weekDays.Contains("7");
        }

        private string getWeekName(string weekDays)
        {
            return !string.IsNullOrEmpty(weekDays) ?
                (isEveryDay(weekDays) ? "每天" : "每周" + (weekDays + "|").Replace("1|", "一、").Replace("2|", "二、").Replace("3|", "三、").Replace("4|", "四、").Replace("5|", "五、").Replace("6|", "六、").Replace("7|", "日、"))
                .TrimEnd('、')
                : string.Empty;
        }

        [Authorize(Roles = "StoreUser")]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
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

                if (!Data_CouponListService.I.Any(a => a.CouponIndex >= startNo && a.CouponIndex <= endNo && a.State != (int)CouponState.NotAssigned))
                {
                    errMsg = "序列中存在未分配的实物券";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var data_CouponInfo = (from a in Data_CouponInfoService.N.GetModels(p => p.ID == couponId)
                                      join b in Data_JackpotInfoService.N.GetModels() on a.JackpotID equals b.ID into b1
                                      from b in b1.DefaultIfEmpty()
                                      select new {
                                          CouponType = a.CouponType,
                                          PublishCount = a.PublishCount,
                                          AuthorFlag = a.AuthorFlag,
                                          AllowOverOther = a.AllowOverOther,
                                          SendType = a.SendType,
                                          OverMoney = a.OverMoney,
                                          FreeCouponCount = a.FreeCouponCount,
                                          JackpotCount = a.JackpotCount,
                                          JackpotID = a.JackpotID,
                                          JackpotName = b != null ? b.ActiveName : string.Empty,
                                          CouponThreshold = a.CouponThreshold,
                                          CouponDiscount = a.CouponDiscount,
                                          CouponValue = a.CouponValue,
                                          OverUseCount = a.OverUseCount,
                                          StartDate = a.StartDate,
                                          EndDate = a.EndDate,
                                          WeekType = a.WeekType,
                                          Week = a.Week,
                                          NoStartDate = a.NoStartDate,
                                          NoEndDate = a.NoEndDate,
                                          StartTime = a.StartTime,
                                          EndTime = a.EndTime,
                                          Context = a.Context
                                      }).FirstOrDefault();
                //优惠券说明
                var note = "本" + ((CouponType)data_CouponInfo.CouponType).GetDescription() + "发行数量" + data_CouponInfo.PublishCount + "张";
                if (data_CouponInfo.AuthorFlag == 1)
                    note = note + "，使用需要授权";
                if(data_CouponInfo.AllowOverOther == 1)
                    note = note + "，允许叠加使用";
                else
                    note = note + "，不允许叠加使用";
                note = note + "。\n";
                note = note + "属于" + ((SendType)data_CouponInfo.SendType).GetDescription() + "类，";
                if (data_CouponInfo.SendType == (int)SendType.Consume || data_CouponInfo.SendType == (int)SendType.Jackpot)
                {
                    note = note + "消费满" + data_CouponInfo.OverMoney + "送";
                    if (data_CouponInfo.SendType == (int)SendType.Consume)
                    {
                        note = note + data_CouponInfo.FreeCouponCount + "张";
                    }
                    else
                    {
                        note = note + data_CouponInfo.JackpotCount + "次" + data_CouponInfo.JackpotName + "抽奖活动";
                    }
                }
                note = note + "。\n";
                note = note + "另可享受满" + data_CouponInfo.CouponThreshold + "元打" + data_CouponInfo.CouponDiscount
                    + "折扣，最多可抵扣" + data_CouponInfo.CouponValue + "元，且允许同时使用" + data_CouponInfo.OverUseCount + "张";
                note = note + "。\n";
                note = note + "使用期限为" + Utils.ConvertFromDatetime(data_CouponInfo.StartDate, "yyyy-MM-dd") + "至"
                    + Utils.ConvertFromDatetime(data_CouponInfo.EndDate, "yyyy-MM-dd") + "，";
                if (data_CouponInfo.WeekType == (int)TimeType.Custom)
                {
                    note = note + getWeekName(data_CouponInfo.Week);
                }
                else if (data_CouponInfo.WeekType == (int)TimeType.Workday)
                {
                    note = note + ((TimeType)data_CouponInfo.WeekType).GetDescription();
                }
                if (data_CouponInfo.NoStartDate != null && data_CouponInfo.NoEndDate != null)
                {
                    note = note + "除" + Utils.ConvertFromDatetime(data_CouponInfo.NoStartDate, "yyyy-MM-dd");
                    if ((data_CouponInfo.NoEndDate.Value - data_CouponInfo.StartDate.Value).Days > 1)
                        note = note + "~" + Utils.ConvertFromDatetime(data_CouponInfo.NoEndDate, "yyyy-MM-dd");
                    note = note + "外";
                }
                note = note + " " + Utils.TimeSpanToStr(data_CouponInfo.StartTime) + "至" + Utils.TimeSpanToStr(data_CouponInfo.EndTime);
                note = note + "。\n";
                note = note + "备注：" + data_CouponInfo.Context;

                var linq = from c in
                               (
                                   from a in Data_CouponListService.N.GetModels(p => p.CouponID == couponId && p.CouponIndex >= startNo && p.CouponIndex <= endNo)
                                   join b in Data_CouponInfoService.N.GetModels() on a.CouponID equals b.ID
                                   select new
                                   {
                                       a = a,
                                       b = b
                                   }).AsEnumerable()
                           select new 
                           {
                               CouponIndex = c.a.CouponIndex,
                               CouponCode = c.a.CouponCode,
                               CouponName = c.b.CouponName,
                               EndDate = Utils.ConvertFromDatetime(c.b.EndDate, "yyyy-MM-dd")
                           };

                DataTable dt = new DataTable();
                dt.Columns.Add("序号");
                dt.Columns.Add("票号");
                dt.Columns.Add("名称");
                dt.Columns.Add("过期时间");
                dt.Columns.Add("说明");
                int i = 0;
                foreach (var item in linq)
                {
                    var dr = dt.NewRow();
                    dr["序号"] = item.CouponIndex;
                    dr["票号"] = item.CouponCode;
                    dr["名称"] = item.CouponName;
                    dr["过期时间"] = item.EndDate;
                    dr["说明"] = i == 0 ? note : string.Empty;
                    dt.Rows.Add(dr);
                    i++;
                }

                string filePath = Utils.ExportToExcel(dt);
                string guid = Guid.NewGuid().ToString();
                CacheHelper.Insert(guid, filePath, 5 * 60, onRemoveCallback);
                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, guid);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 过期文件移除
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="reason"></param>
        private void onRemoveCallback(string key, object value, System.Web.Caching.CacheItemRemovedReason reason)
        {            
            if(value != null)
            {
                string filePath = value.ToString();
                if(!Utils.DeleteFile(filePath))
                    LogHelper.SaveLog("错误:过期文件删除失败，文件路径：" + filePath); 
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
                               (from a in Data_CouponInfoService.N.GetModels()
                                join b in Data_CouponListService.N.GetModels(p => p.CouponID == couponId && (p.StoreID ?? "") != "") on a.ID equals b.CouponID
                                join c in Base_StoreInfoService.N.GetModels() on b.StoreID equals c.StoreID into c1
                                from c in c1.DefaultIfEmpty()
                                select new { a = a, CouponID = b.CouponID, StoreID = b.StoreID, StoreName = (c != null ? c.StoreName : string.Empty), CouponIndex = b.CouponIndex, IsLock = b.IsLock }
                                ).AsEnumerable()
                           group d by new { d.CouponID, d.StoreID } into g
                           select new
                           {
                               CouponID = g.Key.CouponID,
                               CouponName = g.FirstOrDefault().a.CouponName,
                               StoreID = g.Key.StoreID,
                               StoreName = g.FirstOrDefault().StoreName,
                               AssignedCount = g.Count(),
                               StartEnd = g.Min(m => m.CouponIndex) + "~" + g.Max(m => m.CouponIndex),
                               IsLock = g.Min(m=>m.IsLock) == 1 ? 1 : 0
                           };

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, linq);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }        

        /// <summary>
        /// 查询优惠券优先级
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryCouponLevel(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as MerchDataModel).MerchID;                

                int couponType = Dict_SystemService.N.GetModels(p => p.DictKey.Equals("优惠券类别") && p.PID == 0).FirstOrDefault().ID;
                var linq = from c in
                               (from a in Data_CouponInfoService.N.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) 
                                   && p.StartDate <= DateTime.Now && p.EndDate >= DateTime.Now)
                                join b in Dict_SystemService.N.GetModels(p => p.PID == couponType) on (a.CouponType + "") equals b.DictValue into b1
                                from b in b1.DefaultIfEmpty()
                                select new
                                {
                                    a = a,
                                    b = b
                                }).AsEnumerable()
                           select new
                           {
                               ID = c.a.ID,
                               CouponName = c.a.CouponName,
                               EntryCouponFlag = c.a.EntryCouponFlag,
                               CouponType = c.a.CouponType,
                               CouponTypeStr = c.b != null ? c.b.DictKey : string.Empty,
                               StartDate = Utils.ConvertFromDatetime(c.a.StartDate, "yyyy-MM-dd"),
                               EndDate = Utils.ConvertFromDatetime(c.a.EndDate, "yyyy-MM-dd"),
                               CouponLevel = c.a.CouponLevel,
                               Context = c.a.Context
                           };                
                                
                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, linq);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 升降优先级
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object UpdateCouponLevel(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as MerchDataModel).MerchID;

                string errMsg = string.Empty;
                int couponId = dicParas.Get("couponId").Toint(0);
                if (couponId == 0)
                {
                    errMsg = "优惠券规则ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (!dicParas.Get("updateState").Nonempty("升降级状态", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                if (dicParas.Get("updateState").Toint() != 1 && dicParas.Get("updateState").Toint() != -1)
                {
                    errMsg = "升降级状态参数值不正确";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                int updateState = dicParas.Get("updateState").Toint(0);

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        if (!Data_CouponInfoService.I.Any(a => a.ID == couponId))
                        {
                            errMsg = "该优惠券规则不存在";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        //设置当前优惠券级别，默认为1,且不超过最大优先级
                        var data_CouponInfo = Data_CouponInfoService.I.GetModels(p => p.ID == couponId).FirstOrDefault();
                        var oldLevel = data_CouponInfo.CouponLevel;
                        data_CouponInfo.CouponLevel = (data_CouponInfo.CouponLevel ?? 0) + updateState;
                        if (data_CouponInfo.CouponLevel < 1)
                        {
                            data_CouponInfo.CouponLevel = 1;
                        }

                        var max = Data_CouponInfoService.I.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase)).Max(m => m.CouponLevel);
                        if (data_CouponInfo.CouponLevel > max)
                        {
                            data_CouponInfo.CouponLevel = max;
                        }

                        Data_CouponInfoService.I.UpdateModel(data_CouponInfo);

                        var newLevel = data_CouponInfo.CouponLevel;
                        if (oldLevel != newLevel || oldLevel == null)
                        {
                            if (oldLevel == null)
                            {
                                //后续优惠券降一级
                                var linq = Data_CouponInfoService.I.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && p.ID != couponId
                                                               && p.CouponLevel >= newLevel);
                                foreach (var model in linq)
                                {
                                    model.CouponLevel = model.CouponLevel + 1;
                                    Data_CouponInfoService.I.UpdateModel(model);
                                }
                            }
                            else
                            {
                                //与下一个优惠券交换优先级
                                var nextModel = Data_CouponInfoService.I.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && p.ID != couponId
                                                               && p.CouponLevel == newLevel).FirstOrDefault();
                                if (nextModel != null)
                                {
                                    nextModel.CouponLevel = nextModel.CouponLevel - updateState;
                                    Data_CouponInfoService.I.UpdateModel(nextModel);
                                }
                            }
                        }

                        if (!Data_CouponInfoService.I.SaveChanges())
                        {
                            errMsg = "更新优惠券优先级失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        ts.Complete();
                    }
                    catch (Exception e)
                    {
                        errMsg = e.Message;
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