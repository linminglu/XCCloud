﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Transactions;
using System.Web;
using XCCloudService.Base;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
using XCCloudService.BLL.XCCloud;
using XCCloudService.Common;
using XCCloudService.Common.Enum;
using XCCloudService.DAL;
using XCCloudService.Model.CustomModel.XCCloud;
using XCCloudService.Model.XCCloud;
using XCCloudService.Common.Extensions;
using System.Data.Entity.Validation;
using XXCloudService.Api.XCCloud.Common;

namespace XXCloudService.Api.XCCloud
{
    [Authorize(Roles = "MerchUser")]
    /// <summary>
    /// StoreWeight 的摘要说明
    /// </summary>
    public class StoreWeight : ApiBase
    {
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetStoreList(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
               
                var list = Base_StoreInfoService.I.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase)).Select(o => new 
                {
                    StoreID = o.StoreID,
                    StoreName = o.StoreName
                });

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, list);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetStoreBossList(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string storeId = dicParas.ContainsKey("storeId") ? (dicParas["storeId"] + "") : string.Empty;

                if (string.IsNullOrEmpty(storeId))
                {
                    errMsg = "门店ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                IBase_UserInfoService base_UserInfoService = BLLContainer.Resolve<IBase_UserInfoService>(resolveNew: true);
                IBase_StoreWeightService base_StoreWeightService = BLLContainer.Resolve<IBase_StoreWeightService>(resolveNew: true);
                var result = from a in base_UserInfoService.GetModels(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase) && p.UserType == (int)UserType.StoreBoss)
                             join b in base_StoreWeightService.GetModels(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase)) on a.UserID equals b.BossID into b1
                             from b in b1.DefaultIfEmpty()
                             where b.BossID == (int?)null
                             select new
                             {
                                 RealName = a.RealName,
                                 LogName = a.LogName,
                                 UserID = a.UserID
                             };

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, result);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetStoreWeightList(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string storeId = dicParas.ContainsKey("storeId") ? (dicParas["storeId"] + "") : string.Empty;

                if (string.IsNullOrEmpty(storeId))
                {
                    errMsg = "门店ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                IBase_UserInfoService base_UserInfoService = BLLContainer.Resolve<IBase_UserInfoService>(resolveNew: true);
                IBase_StoreWeightService base_StoreWeightService = BLLContainer.Resolve<IBase_StoreWeightService>(resolveNew: true);
                var result = from a in base_UserInfoService.GetModels(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase) && p.UserType == (int)UserType.StoreBoss)
                             join b in base_StoreWeightService.GetModels(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase)) on a.UserID equals b.BossID
                             select new
                             {
                                 RealName = a.RealName,
                                 LogName = a.LogName,
                                 WeightValue = b.WeightValue,
                                 ID = b.ID
                             };

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, result);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object AddStoreWeight(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

                string errMsg = string.Empty;
                string storeId = dicParas.ContainsKey("storeId") ? (dicParas["storeId"] + "") : string.Empty;
                string userId = dicParas.ContainsKey("userId") ? (dicParas["userId"] + "") : string.Empty;
                string weightValue = dicParas.ContainsKey("weightValue") ? (dicParas["weightValue"] + "") : string.Empty;

                #region 验证参数
                if (string.IsNullOrEmpty(storeId))
                {
                    errMsg = "门店ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (string.IsNullOrEmpty(userId))
                {
                    errMsg = "门店老板ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (string.IsNullOrEmpty(weightValue))
                {
                    errMsg = "门店权重值不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (!Utils.isNumber(weightValue))
                {
                    errMsg = "门店权重值格式不正确";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }
                #endregion

                IBase_StoreWeightService base_StoreWeightService = BLLContainer.Resolve<IBase_StoreWeightService>();
                int bossId = Convert.ToInt32(userId);
                if (base_StoreWeightService.Any(a => a.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase) && a.BossID == bossId))
                {
                    errMsg = "该门店老板权重已分配";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                int iWeightValue = Convert.ToInt32(weightValue);
                int sumWeightValue = base_StoreWeightService.GetModels(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase)).Sum(s => s.WeightValue) ?? 0;
                if ((sumWeightValue + iWeightValue) > 100)
                {
                    errMsg = "门店权重之和不得超过100";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var base_StoreWeight = new Base_StoreWeight();
                base_StoreWeight.BossID = bossId;
                base_StoreWeight.StoreID = storeId;
                base_StoreWeight.WeightType = (int)ChainStoreWeightType.Whole;
                base_StoreWeight.WeightValue = iWeightValue;
                base_StoreWeight.MerchID = merchId;
                if (!base_StoreWeightService.Add(base_StoreWeight))
                {
                    errMsg = "添加门店权重信息失败";
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
        public object DelStoreWeight(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                var idArr = dicParas.GetArray("id");

                if (!idArr.Validarray("门店权重ID列表", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);               

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        foreach (var id in idArr)
                        {
                            if (!id.Validintnozero("门店权重ID", out errMsg))
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                            IBase_StoreWeightService base_StoreWeightService = BLLContainer.Resolve<IBase_StoreWeightService>();
                            int iId = Convert.ToInt32(id);
                            var base_StoreWeight = base_StoreWeightService.GetModels(p => p.ID == iId).FirstOrDefault();
                            if (!base_StoreWeightService.Delete(base_StoreWeight))
                            {
                                errMsg = "删除门店权重信息失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            var dbContext = DbContextFactory.CreateByModelNamespace(typeof(Base_StoreWeight_Game).Namespace);
                            var base_StoreWeight_Game = dbContext.Set<Base_StoreWeight_Game>().Where(p => p.WeightID == iId).ToList();
                            foreach (var model in base_StoreWeight_Game)
                            {
                                dbContext.Entry(model).State = EntityState.Deleted;
                            }

                            if (dbContext.SaveChanges() < 0)
                            {
                                errMsg = "删除门店权重游戏机信息失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
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
        public object GetStoreWeightGameList(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string storeId = dicParas.ContainsKey("storeId") ? (dicParas["storeId"] + "") : string.Empty;

                if (string.IsNullOrEmpty(storeId))
                {
                    errMsg = "门店ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                //排除游乐项目类型的游戏机
                var projectGameTypes = getProjectGameTypes(out errMsg);
                if (!errMsg.IsNull())
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                IData_GameInfoService data_GameInfoService = BLLContainer.Resolve<IData_GameInfoService>(resolveNew: true);
                IBase_StoreWeight_GameService base_StoreWeight_GameService = BLLContainer.Resolve<IBase_StoreWeight_GameService>(resolveNew: true);
                var result = from a in data_GameInfoService.GetModels(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase))
                             join b in base_StoreWeight_GameService.GetModels() on a.ID equals b.GameID into b1
                             from b in b1.DefaultIfEmpty()
                             where b.GameID == (int?)null && !projectGameTypes.Contains(a.GameType ?? 0)
                             select new
                             {
                                 GameName = a.GameName,
                                 GameID = a.ID
                             };

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, result);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetStoreWeightUserList(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string storeId = dicParas.ContainsKey("storeId") ? (dicParas["storeId"] + "") : string.Empty;

                if (string.IsNullOrEmpty(storeId))
                {
                    errMsg = "门店ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                IBase_UserInfoService base_UserInfoService = BLLContainer.Resolve<IBase_UserInfoService>(resolveNew: true);
                IBase_StoreWeightService base_StoreWeightService = BLLContainer.Resolve<IBase_StoreWeightService>(resolveNew: true);
                var result = from a in base_UserInfoService.GetModels(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase) && p.UserType == (int)UserType.StoreBoss)
                             join b in base_StoreWeightService.GetModels(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase)) on a.UserID equals b.BossID
                             select new
                             {
                                 RealName = a.RealName,
                                 LogName = a.LogName,
                                 UserID = a.UserID
                             };

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, result);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetStoreWeightUserGameList(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;                
                string storeId = dicParas.ContainsKey("storeId") ? (dicParas["storeId"] + "") : string.Empty;
                string userId = dicParas.ContainsKey("userId") ? (dicParas["userId"] + "") : string.Empty;

                if (string.IsNullOrEmpty(storeId))
                {
                    errMsg = "门店ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (string.IsNullOrEmpty(userId))
                {
                    errMsg = "用户ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                int iUserId = Convert.ToInt32(userId);
                IBase_StoreWeightService base_StoreWeightService = BLLContainer.Resolve<IBase_StoreWeightService>(resolveNew: true);
                IBase_StoreWeight_GameService base_StoreWeight_GameService = BLLContainer.Resolve<IBase_StoreWeight_GameService>(resolveNew: true);
                IData_GameInfoService data_GameInfoService = BLLContainer.Resolve<IData_GameInfoService>(resolveNew: true);
                var result = from a in base_StoreWeightService.GetModels(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase) && p.BossID == iUserId)
                             join b in base_StoreWeight_GameService.GetModels() on a.ID equals b.WeightID
                             join c in data_GameInfoService.GetModels() on b.GameID equals c.ID
                             select new
                             {
                                 GameName = c.GameName,
                                 GameID = c.ID
                             };

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, result);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object SaveStoreWeightUserGameInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

                string errMsg = string.Empty;
                string storeId = dicParas.ContainsKey("storeId") ? (dicParas["storeId"] + "") : string.Empty;
                string userId = dicParas.ContainsKey("userId") ? (dicParas["userId"] + "") : string.Empty;
                string weightType = dicParas.ContainsKey("weightType") ? (dicParas["weightType"] + "") : string.Empty;
                var gameIDs = dicParas.ContainsKey("gameIDs") ? ((object[])dicParas["gameIDs"]).Cast<int>() : null;

                #region 验证参数
                if (string.IsNullOrEmpty(storeId))
                {
                    errMsg = "门店ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (string.IsNullOrEmpty(userId))
                {
                    errMsg = "门店老板ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (string.IsNullOrEmpty(weightType))
                {
                    errMsg = "门店权重类别不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }
                #endregion
               
                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        IBase_StoreWeightService base_StoreWeightService = BLLContainer.Resolve<IBase_StoreWeightService>();
                        int iWeightType = Convert.ToInt32(weightType);
                        int iUserId = Convert.ToInt32(userId);

                        if (!base_StoreWeightService.Any(a => a.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase) && a.BossID == iUserId))
                        {
                            errMsg = "请先分配权重";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        var base_StoreWeight = base_StoreWeightService.GetModels(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase) && p.BossID == iUserId).FirstOrDefault();
                        base_StoreWeight.WeightType = iWeightType;
                        if (!base_StoreWeightService.Update(base_StoreWeight))
                        {
                            errMsg = "保存权重用户游戏机信息失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        if (iWeightType == (int)ChainStoreWeightType.Game)
                        {
                            if (gameIDs == null)
                            {
                                errMsg = "选择的游戏机列表不能为空";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            //先删除后添加
                            int iId = base_StoreWeight.ID;
                            var dbContext = DbContextFactory.CreateByModelNamespace(typeof(Base_StoreWeight_Game).Namespace);
                            var base_StoreWeight_Game = dbContext.Set<Base_StoreWeight_Game>().Where(p => p.WeightID == iId).ToList();
                            foreach (var model in base_StoreWeight_Game)
                            {
                                dbContext.Entry(model).State = EntityState.Deleted;
                            }

                            foreach (var gId in gameIDs)
                            {
                                var base_StoreWeight_GameModel = new Base_StoreWeight_Game();
                                base_StoreWeight_GameModel.WeightID = iId;
                                base_StoreWeight_GameModel.GameID = gId;
                                base_StoreWeight_GameModel.MerchID = merchId;
                                dbContext.Entry(base_StoreWeight_GameModel).State = EntityState.Added;                            
                            }

                            if (dbContext.SaveChanges() < 0)
                            {
                                errMsg = "保存权重用户游戏机信息失败";
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
        public object GetChainStoreRuleList(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string storeId = dicParas.ContainsKey("storeId") ? (dicParas["storeId"] + "") : string.Empty;
                string ruleType = dicParas.ContainsKey("ruleType") ? (dicParas["ruleType"] + "") : string.Empty;
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

                if (string.IsNullOrEmpty(storeId))
                {
                    errMsg = "门店ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                IBase_StoreInfoService base_StoreInfoService = BLLContainer.Resolve<IBase_StoreInfoService>(resolveNew: true);
                IBase_ChainRuleService base_ChainRuleService = BLLContainer.Resolve<IBase_ChainRuleService>(resolveNew: true);
                IBase_ChainRule_StoreService base_ChainRule_StoreService = BLLContainer.Resolve<IBase_ChainRule_StoreService>(resolveNew: true);
                var result = from a in base_ChainRuleService.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase))
                             join b in base_ChainRule_StoreService.GetModels(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase)) on a.ID equals b.RuleGroupID
                             join c in base_ChainRule_StoreService.GetModels() on b.RuleGroupID equals c.RuleGroupID               
                             join d in base_StoreInfoService.GetModels() on c.StoreID equals d.StoreID 
                             where !c.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase)
                             select new
                             {
                                 ID = c.ID,
                                 StoreName = d.StoreName,
                                 RuleType = a.RuleType
                             };

                if (!string.IsNullOrEmpty(ruleType))
                {
                    int iRuleType = Convert.ToInt32(ruleType);
                    result = result.Where(w => w.RuleType == iRuleType);
                }

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, result);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetUnChainStoreList(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string storeId = dicParas.ContainsKey("storeId") ? (dicParas["storeId"] + "") : string.Empty;
                string ruleType = dicParas.ContainsKey("ruleType") ? (dicParas["ruleType"] + "") : string.Empty;
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

                if (string.IsNullOrEmpty(storeId))
                {
                    errMsg = "门店ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (string.IsNullOrEmpty(ruleType))
                {
                    errMsg = "规则类别不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (!Utils.isNumber(ruleType))
                {
                    errMsg = "规则类别格式不正确";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                int iRuleType = Convert.ToInt32(ruleType);
                IBase_StoreInfoService base_StoreInfoService = BLLContainer.Resolve<IBase_StoreInfoService>(resolveNew: true);
                IBase_ChainRuleService base_ChainRuleService = BLLContainer.Resolve<IBase_ChainRuleService>(resolveNew: true);
                IData_BalanceType_StoreListService data_BalanceType_StoreListService = BLLContainer.Resolve<IData_BalanceType_StoreListService>(resolveNew: true);
                IBase_ChainRule_StoreService base_ChainRule_StoreService = BLLContainer.Resolve<IBase_ChainRule_StoreService>(resolveNew: true);
                var result = from a in base_StoreInfoService.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && (p.StoreState == (int)StoreState.Open || p.StoreState == (int)StoreState.Valid))
                             join d in data_BalanceType_StoreListService.GetModels(p => p.BalanceIndex == iRuleType) on a.StoreID equals d.StroeID  //仅查找适用的余额类别的门店
                             join b in
                                 (
                                     from a in base_ChainRuleService.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && p.RuleType == iRuleType)
                                     join b in base_ChainRule_StoreService.GetModels(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase)) on a.ID equals b.RuleGroupID
                                     join c in base_ChainRule_StoreService.GetModels() on b.RuleGroupID equals c.RuleGroupID
                                     select c
                                 )
                             on a.StoreID equals b.StoreID into b1
                             from b in b1.DefaultIfEmpty()
                             where string.IsNullOrEmpty(b.StoreID) && !a.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase)
                             select new
                             {
                                 StoreID = a.StoreID,
                                 StoreName = a.StoreName
                             };

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, result);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object SaveChainStore(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

                string errMsg = string.Empty;
                string storeId = dicParas.ContainsKey("storeId") ? (dicParas["storeId"] + "") : string.Empty;
                string revStoreId = dicParas.ContainsKey("revStoreId") ? (dicParas["revStoreId"] + "") : string.Empty;
                string ruleType = dicParas.ContainsKey("ruleType") ? (dicParas["ruleType"] + "") : string.Empty;

                if (string.IsNullOrEmpty(storeId))
                {
                    errMsg = "当前门店ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (string.IsNullOrEmpty(revStoreId))
                {
                    errMsg = "接收门店ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (string.IsNullOrEmpty(ruleType))
                {
                    errMsg = "规则类别不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (!Utils.isNumber(ruleType))
                {
                    errMsg = "规则类别格式不正确";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                int iRuleType = Convert.ToInt32(ruleType);
                IBase_ChainRuleService base_ChainRuleService = BLLContainer.Resolve<IBase_ChainRuleService>();
                IBase_ChainRule_StoreService base_ChainRule_StoreService = BLLContainer.Resolve<IBase_ChainRule_StoreService>();
                IDict_BalanceTypeService dict_BalanceTypeService = BLLContainer.Resolve<IDict_BalanceTypeService>();

                if (!dict_BalanceTypeService.Any(a => a.ID == iRuleType))
                {
                    errMsg = "该余额类别不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var dict_BalanceTypeModel = dict_BalanceTypeService.GetModels(p=>p.ID == iRuleType).FirstOrDefault();
                var base_ChainRuleModel = base_ChainRuleService.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && p.RuleType == iRuleType).FirstOrDefault() ?? new Base_ChainRule();
                if (base_ChainRuleModel.ID == 0)
                {
                    base_ChainRuleModel.GroupName = dict_BalanceTypeModel.TypeName + "余额通用";
                    base_ChainRuleModel.MerchID = merchId;
                    base_ChainRuleModel.RuleType = iRuleType;
                    if (!base_ChainRuleService.Add(base_ChainRuleModel))
                    {
                        errMsg = "添加规则类别失败";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                }

                int ruleGroupId = base_ChainRuleModel.ID;

                if (!base_ChainRule_StoreService.Any(a => a.StoreID.Equals(revStoreId, StringComparison.OrdinalIgnoreCase) && a.RuleGroupID == ruleGroupId))
                {
                    var base_ChainRule_StoreModel = new Base_ChainRule_Store();
                    base_ChainRule_StoreModel.RuleGroupID = ruleGroupId;
                    base_ChainRule_StoreModel.StoreID = revStoreId;
                    base_ChainRule_StoreModel.MerchID = merchId;
                    base_ChainRule_StoreService.AddModel(base_ChainRule_StoreModel);                    
                }

                if (!base_ChainRule_StoreService.Any(a => a.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase) && a.RuleGroupID == ruleGroupId))
                {
                    var base_ChainRule_StoreModel = new Base_ChainRule_Store();
                    base_ChainRule_StoreModel.RuleGroupID = ruleGroupId;
                    base_ChainRule_StoreModel.StoreID = storeId;
                    base_ChainRule_StoreModel.MerchID = merchId;
                    base_ChainRule_StoreService.AddModel(base_ChainRule_StoreModel);           
                }
                
                if (!base_ChainRule_StoreService.SaveChanges())
                {
                    errMsg = "保存接收余额的门店失败";
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
        public object DelChainStore(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                var id = dicParas.Get("id");

                if (!id.Validintnozero("门店连锁规则ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                int iId = Convert.ToInt32(id);
                IBase_ChainRule_StoreService base_ChainRule_StoreService = BLLContainer.Resolve<IBase_ChainRule_StoreService>();
                if (!base_ChainRule_StoreService.Any(a => a.ID == iId))
                {
                    errMsg = "该门店连锁规则不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var base_ChainRule_StoreModel = base_ChainRule_StoreService.GetModels(p => p.ID == iId).FirstOrDefault();
                if (!base_ChainRule_StoreService.Delete(base_ChainRule_StoreModel))
                {
                    errMsg = "删除接收余额的门店失败";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
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