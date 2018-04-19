using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XCCloudService.Base;
using XCCloudService.Model.CustomModel.XCGameManager;
using XCCloudService.Pay.WeiXinPay.Lib;
using XCCloudService.WeiXin.Common;
using XCCloudService.WeiXin.WeixinPub;

namespace XXCloudService.Api.XCGameMana
{
    /// <summary>
    /// WxCommon 的摘要说明
    /// </summary>
    public class WxCommon : ApiBase
    {
        #region 获取微信H5配置
        /// <summary>
        /// 获取微信H5配置
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MethodToken)]
        public object getWxH5Config(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string fileName = dicParas.ContainsKey("fileName") ? dicParas["fileName"].ToString() : string.Empty;
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "请求微信签名的url错误");
                }

                string url = string.Format(WeiXinConfig.WxJSSDK_Url, fileName);

                WxConfigModel model = CommonHelper.GetSignature(url);
                if(model == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "请求微信SDK错误");
                }

                return ResponseModelFactory<WxConfigModel>.CreateModel(isSignKeyReturn, model);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion
    }
}