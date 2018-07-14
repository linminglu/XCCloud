using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;
using System.Web;
using XCCloudService.Base;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
using XCCloudService.Common;
using XCCloudService.Common.Extensions;
using XCCloudService.DBService.BLL;
using XCCloudService.Model.CustomModel.XCCloud;
using XCCloudService.Model.XCCloud;

namespace XXCloudService.Api.XCCloud
{
    [Authorize(Roles = "MerchUser")]
    /// <summary>
    /// Jackpot 的摘要说明
    /// </summary>
    public class Jackpot : ApiBase
    {

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryJackpotInfo(Dictionary<string, object> dicParas)
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

                string sql = @"select a.* from Data_JackpotInfo a where a.MerchID=@MerchId";
                sql = sql + sqlWhere;

                IData_JackpotInfoService data_JackpotInfoService = BLLContainer.Resolve<IData_JackpotInfoService>();
                var data_JackpotInfo = data_JackpotInfoService.SqlQuery<Data_JackpotInfo>(sql, parameters).ToList();

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, data_JackpotInfo);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetJackpotDic(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                
                IData_JackpotInfoService data_JackpotInfoService = BLLContainer.Resolve<IData_JackpotInfoService>();
                Dictionary<int, string> pJackpotList = data_JackpotInfoService.GetModels(p=>p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase))
                    .Select(o => new { ID = o.ID, ActiveName = o.ActiveName }).Distinct()
                    .ToDictionary(d => d.ID, d => d.ActiveName);

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, pJackpotList);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetJackpotInfo(Dictionary<string, object> dicParas)
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
                IData_JackpotInfoService data_JackpotInfoService = BLLContainer.Resolve<IData_JackpotInfoService>();
                var data_JackpotInfo = data_JackpotInfoService.GetModels(p => p.ID == iId).FirstOrDefault();
                if (data_JackpotInfo == null)
                {
                    errMsg = "该抽奖规则不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                IData_Jackpot_LevelService data_Jackpot_LevelService = BLLContainer.Resolve<IData_Jackpot_LevelService>(resolveNew: true);
                IData_CouponInfoService data_CouponInfoService = BLLContainer.Resolve<IData_CouponInfoService>(resolveNew: true);
                var JackpotLevels = from a in data_Jackpot_LevelService.GetModels(p => p.ActiveID == iId)
                                    join b in data_CouponInfoService.GetModels() on a.CouponID equals b.ID
                                    select new
                                    {
                                        LevelName = a.LevelName,
                                        Count = a.Count,
                                        Probability = a.Probability,
                                        CouponID = a.ID,
                                        CouponName = b.CouponName
                                    };

                var result = new
                {
                    ID = data_JackpotInfo.ID,
                    ActiveName = data_JackpotInfo.ActiveName,
                    Threshold = data_JackpotInfo.Threshold,
                    Concerned = data_JackpotInfo.Concerned,
                    StartTime = data_JackpotInfo.StartTime,
                    EndTime = data_JackpotInfo.EndTime,
                    JackpotLevels = JackpotLevels
                };

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, result);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object SaveJackpotInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

                string errMsg = string.Empty;
                string id = dicParas.ContainsKey("id") ? (dicParas["id"] + "") : string.Empty;
                string activeName = dicParas.ContainsKey("activeName") ? (dicParas["activeName"] + "") : string.Empty;
                string threshold = dicParas.ContainsKey("threshold") ? (dicParas["threshold"] + "") : string.Empty;
                string concerned = dicParas.ContainsKey("concerned") ? (dicParas["concerned"] + "") : string.Empty;
                string startTime = dicParas.ContainsKey("startTime") ? (dicParas["startTime"] + "") : string.Empty;
                string endTime = dicParas.ContainsKey("endTime") ? (dicParas["endTime"] + "") : string.Empty;
                object[] jackpotLevels = dicParas.ContainsKey("jackpotLevels") ? (object[])dicParas["jackpotLevels"] : null;                
                int iId = 0;
                int.TryParse(id, out iId);

                #region 验证参数
                
                if (string.IsNullOrEmpty(threshold))
                {
                    errMsg = "消费门槛不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (!Utils.isNumber(threshold))
                {
                    errMsg = "消费门槛格式不正确";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (Convert.ToInt32(threshold) < 0)
                {
                    errMsg = "消费门槛不能为负数";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (string.IsNullOrEmpty(startTime) || string.IsNullOrEmpty(endTime))
                {
                    errMsg = "活动时间不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }
                
                if (Convert.ToDateTime(startTime) > Convert.ToDateTime(endTime))
                {
                    errMsg = "开始时间不能晚于结束时间";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                #endregion

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        IData_JackpotInfoService data_JackpotInfoService = BLLContainer.Resolve<IData_JackpotInfoService>();
                        var data_JackpotInfo = data_JackpotInfoService.GetModels(p => p.ID == iId).FirstOrDefault() ?? new Data_JackpotInfo();
                        data_JackpotInfo.ActiveName = activeName;
                        data_JackpotInfo.Concerned = !string.IsNullOrEmpty(concerned) ? Convert.ToInt32(concerned) : (int?)null;
                        data_JackpotInfo.StartTime = Convert.ToDateTime(startTime);
                        data_JackpotInfo.EndTime = Convert.ToDateTime(endTime);
                        data_JackpotInfo.MerchID = merchId;
                        data_JackpotInfo.Threshold = Convert.ToInt32(threshold);
                        if (iId == 0)
                        {
                            //新增
                            if (!data_JackpotInfoService.Add(data_JackpotInfo))
                            {
                                errMsg = "添加抽奖规则信息失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }
                        else
                        {
                            if(data_JackpotInfo.ID == 0)
                            {
                                errMsg = "该抽奖规则信息不存在";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            IData_Jackpot_MatrixService data_Jackpot_MatrixService = BLLContainer.Resolve<IData_Jackpot_MatrixService>();
                            if (data_Jackpot_MatrixService.Any(a => a.ActiveID == iId))
                            {
                                errMsg = "该抽奖规则已使用不能修改";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            //修改
                            if (!data_JackpotInfoService.Update(data_JackpotInfo))
                            {
                                errMsg = "修改抽奖规则信息失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }

                        iId = data_JackpotInfo.ID;
                        if (jackpotLevels != null && jackpotLevels.Count() >= 0)
                        {
                            //先删除已有数据，后添加
                            IData_Jackpot_LevelService data_Jackpot_LevelService = BLLContainer.Resolve<IData_Jackpot_LevelService>();
                            var data_Jackpot_Level = data_Jackpot_LevelService.GetModels(p => p.ActiveID == iId);
                            foreach (var model in data_Jackpot_Level)
                            {
                                data_Jackpot_LevelService.DeleteModel(model);
                            }

                            var levelNames = new List<string>();
                            foreach (IDictionary<string, object> el in jackpotLevels)
                            {
                                if (el != null)
                                {
                                    var dicPara = new Dictionary<string, object>(el, StringComparer.OrdinalIgnoreCase);
                                    string couponId = dicPara.ContainsKey("couponId") ? dicPara["couponId"].ToString() : string.Empty;
                                    string levelName = dicPara.ContainsKey("levelName") ? (dicPara["levelName"] + "") : string.Empty;
                                    string count = dicPara.ContainsKey("count") ? (dicPara["count"] + "") : string.Empty;
                                    string probability = dicPara.ContainsKey("probability") ? (dicPara["probability"] + "") : string.Empty;
                                    if (string.IsNullOrEmpty(couponId))
                                    {
                                        errMsg = "奖品ID不能为空";
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    }
                                    if (string.IsNullOrEmpty(levelName))
                                    {
                                        errMsg = "奖品等级不能为空";
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    }
                                    if (levelNames.Contains(levelName, StringComparer.OrdinalIgnoreCase))
                                    {
                                        errMsg = "奖品等级不能重复";
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    }
                                    if (string.IsNullOrEmpty(count))
                                    {
                                        errMsg = "奖品数量不能为空";
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    }
                                    if (!Utils.isNumber(count))
                                    {
                                        errMsg = "奖品数量格式不正确";
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    }
                                    if (Convert.ToInt32(count) < 0)
                                    {
                                        errMsg = "奖品数量不能为负数";
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    }
                                    if (string.IsNullOrEmpty(probability))
                                    {
                                        errMsg = "中奖概率不能为空";
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    }
                                    if (!Utils.IsDecimal(probability))
                                    {
                                        errMsg = "中奖概率格式不正确";
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    }
                                    if (Convert.ToDecimal(probability) < 0)
                                    {
                                        errMsg = "中奖概率不能为负数";
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    }
                                    var data_Jackpot_LevelModel = new Data_Jackpot_Level();
                                    data_Jackpot_LevelModel.ActiveID = iId;
                                    data_Jackpot_LevelModel.LevelName = levelName;
                                    data_Jackpot_LevelModel.Count = Convert.ToInt32(count);
                                    data_Jackpot_LevelModel.Probability = Convert.ToDecimal(probability);
                                    data_Jackpot_LevelModel.CouponID = Convert.ToInt32(couponId);
                                    data_Jackpot_LevelService.AddModel(data_Jackpot_LevelModel);
                                    levelNames.Add(levelName);
                                }
                                else
                                {
                                    errMsg = "提交数据包含空对象";
                                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                }
                            }

                            if (!data_Jackpot_LevelService.SaveChanges())
                            {
                                errMsg = "保存抽奖规则信息失败";
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
        public object DelJackpotInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                var idArr = dicParas.GetArray("id");

                if (!idArr.Validarray("规则ID列表", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        foreach (var id in idArr)
                        {
                            if (!id.Validintnozero("规则ID", out errMsg))
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                            int iId = Convert.ToInt32(id);
                            IData_Jackpot_LevelService data_Jackpot_LevelService = BLLContainer.Resolve<IData_Jackpot_LevelService>();
                            IData_JackpotInfoService data_JackpotInfoService = BLLContainer.Resolve<IData_JackpotInfoService>();
                            IData_Jackpot_MatrixService data_Jackpot_MatrixService = BLLContainer.Resolve<IData_Jackpot_MatrixService>();
                            if (!data_JackpotInfoService.Any(a => a.ID == iId))
                            {
                                errMsg = "该抽奖规则信息不存在";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            if (data_Jackpot_MatrixService.Any(a => a.ActiveID == iId))
                            {
                                errMsg = "该抽奖规则已使用不能删除";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                            
                            var data_JackpotInfo = data_JackpotInfoService.GetModels(p => p.ID == iId).FirstOrDefault();
                            data_JackpotInfoService.DeleteModel(data_JackpotInfo);

                            var data_Jackpot_Level = data_Jackpot_LevelService.GetModels(p => p.ActiveID == iId);
                            foreach (var model in data_Jackpot_Level)
                            {
                                data_Jackpot_LevelService.DeleteModel(model);
                            }

                            if (!data_JackpotInfoService.SaveChanges())
                            {
                                errMsg = "删除抽奖规则信息失败";
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
    }
}