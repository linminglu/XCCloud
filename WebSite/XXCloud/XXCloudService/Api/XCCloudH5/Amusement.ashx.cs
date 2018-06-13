﻿using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Transactions;
using System.Web;
using XCCloudService.Base;
using XCCloudService.BLL.XCCloud;
using XCCloudService.Business.XCCloud;
using XCCloudService.CacheService;
using XCCloudService.Common.Enum;
using XCCloudService.Common.Extensions;
using XCCloudService.Model.CustomModel.XCCloud;
using XCCloudService.Model.WeiXin;
using XCCloudService.Model.XCCloud;

namespace XXCloudService.Api.XCCloudH5
{
    /// <summary>
    /// Amusement 的摘要说明
    /// </summary>
    public class Amusement : ApiBase
    {
        #region 获取设备信息
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MethodToken)]
        public object getDeviceInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                string token = dicParas.ContainsKey("token") ? dicParas["token"].ToString().Trim() : "";
                string deviceToken = dicParas.ContainsKey("deviceToken") ? dicParas["deviceToken"].ToString().Trim() : "";
                if (string.IsNullOrEmpty(deviceToken))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "设备令牌无效");
                }

                MemberTokenModel model = MemberTokenCache.GetModel(token);
                if (model == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "用户令牌无效，请重新登陆");
                }

                Base_MemberInfo member = Base_MemberInfoService.I.GetModels(t => t.ID == model.MemberId).FirstOrDefault();
                if (member == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "用户令牌无效，请重新登陆");
                }

                Base_DeviceInfo device = Base_DeviceInfoService.I.GetModels().FirstOrDefault(t => t.Token == deviceToken);
                if (device == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "设备令牌无效");
                }
                string storeId = device.StoreID;

                //获取默认电子卡开卡级别
                Data_Parameters defaultMemberLevelId = Data_ParametersService.I.GetModels(t => t.StoreID == storeId && t.System == "cmbCardOpenLevel").FirstOrDefault();
                //没有卡就创建电子卡
                Data_Member_Card card = Data_Member_CardService.I.GetModels(t => t.MemberID == member.ID).FirstOrDefault();

                if (card == null)
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeOption.RequiresNew))
                    {
                        card = new Data_Member_Card();
                        card.ID = RedisCacheHelper.CreateCloudSerialNo(storeId);
                        card.MerchID = device.MerchID;
                        card.ICCardID = RedisCacheHelper.CreateCloudSerialNo(storeId);
                        card.ParentCard = "0";
                        card.JoinChannel = 2;
                        card.CardPassword = "";
                        card.CardType = 0;
                        card.CardShape = 0;
                        card.CardName = member.UserName ?? "";
                        card.CardSex = member.Gender;
                        card.CardBirthDay = member.Birthday;
                        card.CardLimit = 0;
                        card.AllowIn = 1;
                        card.AllowOut = 1;
                        card.MemberID = member.ID;
                        card.MemberLevelID = defaultMemberLevelId != null ? Convert.ToInt32(defaultMemberLevelId.ParameterValue) : 0;
                        card.CreateTime = DateTime.Now;
                        card.EndDate = DateTime.Now.AddYears(1);
                        card.LastStore = storeId;
                        card.UpdateTime = DateTime.Now;
                        card.Deposit = 0;
                        card.IsLock = 0;
                        card.CardStatus = 1;
                        bool ret = Data_Member_CardService.I.Add(card, false);
                        if (!ret)
                        {
                            return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "初始化会员卡失败");
                        }

                        //会员卡关联门店，添加卡可用门店
                        //获取当前商户余额互通规则列表
                        var balanceChainList = from a in Base_ChainRuleService.I.GetModels(t => t.MerchID == device.MerchID)
                                               join b in Base_ChainRule_StoreService.I.GetModels(t => t.MerchID == device.MerchID) on a.ID equals b.RuleGroupID
                                               select new
                                               {
                                                   BalanceIndex = a.RuleType,
                                                   StoreId = b.StoreID
                                               };

                        //所有余额互通门店
                        var cardStores = balanceChainList.GroupBy(t => new { storeId = t.StoreId }).Select(t => new { storeId = t.Key.storeId }).ToList();
                        foreach (var item in cardStores)
                        {
                            Data_Member_Card_Store cardStore = new Data_Member_Card_Store();
                            cardStore.ID = RedisCacheHelper.CreateCloudSerialNo(storeId);
                            cardStore.CardID = card.ID;
                            cardStore.StoreID = item.storeId;
                            if (!Data_Member_Card_StoreService.I.Add(cardStore, false))
                            {
                                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "初始化会员卡失败");
                            }
                        }

                        //添加余额
                        // 1. 获取当前商户的余额类别
                        var balanceTypeQuery = Dict_BalanceTypeService.I.GetModels(t => t.MerchID == device.MerchID && t.State == 1);
                        // 2. 添加各类别余额
                        foreach (var item in balanceTypeQuery)
                        {
                            //正价余额
                            Data_Card_Balance balance = new Data_Card_Balance();
                            balance.ID = RedisCacheHelper.CreateCloudSerialNo(storeId);
                            balance.MerchID = device.MerchID;
                            balance.CardIndex = card.ID;
                            balance.BalanceIndex = item.ID;
                            balance.Balance = 0;
                            balance.UpdateTime = DateTime.Now;
                            if (!Data_Card_BalanceService.I.Add(balance, false))
                            {
                                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "初始化余额失败");
                            }
                            //赠送余额
                            Data_Card_Balance_Free balanceFree = new Data_Card_Balance_Free();
                            balanceFree.ID = RedisCacheHelper.CreateCloudSerialNo(storeId);
                            balanceFree.MerchID = device.MerchID;
                            balanceFree.CardIndex = card.ID;
                            balanceFree.BalanceIndex = item.ID;
                            balanceFree.Balance = 0;
                            balanceFree.UpdateTime = DateTime.Now;
                            if (!Data_Card_Balance_FreeService.I.Add(balanceFree, false))
                            {
                                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "初始化余额失败");
                            }
                            //余额关联门店
                            //3. 获取当前商户下当前余额币种的互通门店
                            var balanceStores = balanceChainList.Where(t => t.BalanceIndex == item.ID).ToList();
                            foreach (var bs in balanceStores)
                            {
                                //正价余额关联
                                Data_Card_Balance_StoreList balanceStore = new Data_Card_Balance_StoreList();
                                balanceStore.ID = RedisCacheHelper.CreateCloudSerialNo(storeId);
                                balanceStore.CardBalanceID = balance.ID;
                                balanceStore.StoreID = bs.StoreId;
                                if (!Data_Card_Balance_StoreListService.I.Add(balanceStore, false))
                                {
                                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "初始化余额失败");
                                }
                                //赠送余额关联
                                Data_Card_Balance_StoreList balanceFreeStore = new Data_Card_Balance_StoreList();
                                balanceFreeStore.ID = RedisCacheHelper.CreateCloudSerialNo(storeId);
                                balanceFreeStore.CardBalanceID = balanceFree.ID;
                                balanceFreeStore.StoreID = bs.StoreId;
                                if (!Data_Card_Balance_StoreListService.I.Add(balanceFreeStore, false))
                                {
                                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "初始化余额失败");
                                }
                            }
                        }
                        ts.Complete();
                    }
                }

                MemberCard cacheCard = new MemberCard();
                cacheCard.CardId = card.ID;
                cacheCard.ICCardId = card.ICCardID;
                cacheCard.MemberLevelId = card.MemberLevelID.Value;
                //卡余额
                cacheCard.CardBalanceList = XCCloudStoreBusiness.GetCardStoreBalanceList(device.MerchID, storeId, card.ID);
                model.CurrentCardInfo = cacheCard;
                //更新缓存
                MemberTokenCache.AddToken(token, model);

                GameCoinInfoModel gameInfo = new GameCoinInfoModel();
                gameInfo.DeviceType = device.type.Value;

                //卡头
                if (device.type == 0)
                {
                    Data_GameInfo game = Data_GameInfoService.I.GetModels(t => t.ID == device.GameIndexID).FirstOrDefault();
                    Dict_System dict = Dict_SystemService.I.GetModels(t => t.ID == game.GameType).FirstOrDefault();
                    //博彩机
                    if (game.AllowElecOut == 1 && game.LotteryMode == 0)
                    {
                        gameInfo.GameType = 1;
                    }
                    else
                    {
                        gameInfo.GameType = 0;
                    }

                    string gameCategoryName = dict.DictKey;
                    gameCategoryName = GetParentGameType(dict.PID.Value, gameCategoryName);

                    gameInfo.DeviceName = game.GameName;
                    gameInfo.DeviceCategoryName = gameCategoryName;

                    //获取当前会员卡余额集合

                    //获取当前会员投币规则
                    List<GamePushRule> coinRules = XCCloudStoreBusiness.GetGamePushRule(storeId, game.ID, card.MemberLevelID.Value);
                    if(coinRules.Count > 0)
                    {
                        gameInfo.GamePushRules = coinRules;
                    }
                    else
                    {
                        //散客扫码支付
                        gameInfo.GameCoinList = Data_GameAPP_RuleService.I.GetModels(t => t.GameID == device.GameIndexID && t.StoreID == device.StoreID)
                            .Select(t => new
                            {
                                PlayCount = t.PlayCount,
                                Amount = t.PayCount
                            }).ToList().Select(t => new GameCoinInfo
                            {
                                PlayCount = t.PlayCount.Value,
                                Amount = t.Amount.Value.ToString("0.00")
                            }).ToList();
                    }
                }
                else
                {
                    gameInfo.DeviceName = device.DeviceName;
                    gameInfo.DeviceCategoryName = ((DeviceType)device.type.Value).ToDescription();
                    
                }

                return ResponseModelFactory<GameCoinInfoModel>.CreateModel(isSignKeyReturn, gameInfo);
            }
            catch (DbEntityValidationException ex)
            {
                throw ex;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private string GetParentGameType(int pid, string gameCategoryName)
        {
            var model = Dict_SystemService.I.GetModels(t => t.ID == pid).FirstOrDefault();
            if(model != null)
            {
                gameCategoryName = string.Format("{0}-{1}", model.DictKey, gameCategoryName);
                GetParentGameType(model.PID.Value, gameCategoryName);
            }
            return gameCategoryName;
        }
        #endregion
    }
}