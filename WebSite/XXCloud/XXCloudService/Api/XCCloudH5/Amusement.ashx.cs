using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XCCloudService.Base;
using XCCloudService.BLL.XCCloud;
using XCCloudService.CacheService;
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

                var query = from device in Base_DeviceInfoService.I.GetModels(t => t.Token == deviceToken)
                          join game in Data_GameInfoService.I.GetModels() on device.GameIndexID equals game.ID
                          join dict in Dict_SystemService.I.GetModels() on game.GameType equals dict.ID
                          select new GameCoinInfoModel
                          {
                              GameId = game.ID,
                              GameName = game.GameName,
                              GameType = dict.DictKey,
                              Coins = game.PushCoin1.Value
                          };

                GameCoinInfoModel gameInfo = query.FirstOrDefault();

                //游戏机送分/送局规则
                //if(!string.IsNullOrEmpty(model.MemberId))
                //{

                //}

                return ResponseModelFactory<GameCoinInfoModel>.CreateModel(isSignKeyReturn, gameInfo);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion
    }
}