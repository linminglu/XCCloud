using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XCCloudService.Base;
using XCCloudService.BLL.XCCloud;
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

                Base_DeviceInfo device = Base_DeviceInfoService.I.GetModels().FirstOrDefault(t => t.Token == deviceToken);
                if(device == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "设备令牌无效");
                }

                GameCoinInfoModel gameInfo = new GameCoinInfoModel();
                //卡头
                if (device.type == 0)
                {
                    var query = from game in Data_GameInfoService.I.GetModels()
                                join dict in Dict_SystemService.I.GetModels() on game.GameType equals dict.ID into g
                                from temp in g.DefaultIfEmpty()
                                where game.ID == device.GameIndexID
                                select new GameCoinInfoModel
                                {
                                    //DeviceId = device.ID,
                                    DeviceName = game.GameName,
                                    DeviceType = temp == null ? "0" : temp.ID.ToString()
                                };

                    gameInfo = query.FirstOrDefault();

                    //获取博彩类集合
                    List<Dict_System> gameTypeList = new List<Dict_System>();
                    Dict_System bocai = Dict_SystemService.I.GetModels(t => t.DictKey == "上下分设备").FirstOrDefault();
                    if (bocai != null)
                    {
                        gameTypeList.Add(bocai);
                        GetGamingList(bocai.ID, gameTypeList);
                    }
                    //判断当前游戏机是投币类还是博彩类
                    var gameType = gameTypeList.Where(t => t.ID.ToString() == gameInfo.DeviceType).FirstOrDefault();
                    gameInfo.DeviceType = gameType == null ? "投币类" : "博彩类";

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
                else
                {
                    //gameInfo.DeviceId = device.ID;
                    gameInfo.DeviceName = device.DeviceName;
                    gameInfo.DeviceType = ((DeviceType)device.type.Value).ToDescription();
                }

                ////游戏机送分/送局规则
                //if (model.CurrentCardInfo != null)
                //{
                //    var gameFrees = Data_GameFreeRuleService.I.GetModels(t => t.MemberLevelID == model.CurrentCardInfo.MemberLevelId && t.State == 1).Select(t => new
                //    {
                //        Id = t.ID,
                //        NeedCoin = t.NeedCoin,
                //        FreeCoin = t.FreeCoin,
                //        ExitCoin = t.ExitCoin
                //    }).ToList();
                //}

                return ResponseModelFactory<GameCoinInfoModel>.CreateModel(isSignKeyReturn, gameInfo);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void GetGamingList(int pid, List<Dict_System> gameTypeList)
        {
            var list = Dict_SystemService.I.GetModels(t => t.PID == pid).ToList();
            foreach (var item in list)
            {
                gameTypeList.Add(item);
                GetGamingList(item.ID, gameTypeList);
            }
        }
        #endregion
    }
}