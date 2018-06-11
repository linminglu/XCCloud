using Aop.Api;
using Aop.Api.Request;
using Aop.Api.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using XCCloudService.BLL.XCCloud;
using XCCloudService.Business.Common;
using XCCloudService.Model.XCCloud;
using XCCloudService.Pay.Alipay;
using XCCloudService.PayChannel.Common;

namespace XXCloudService.CallBack
{
    public partial class AlipayAuthNotify : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string auth_code = Request["auth_code"];
            string appId = Request["app_id"];

            //PayLogHelper.WritePayLog(auth_code + " ------- " + appId);

            if (appId.Trim() == AliPayConfig.authAppId.Trim())
            {
                IAopClient client = new DefaultAopClient(AliPayConfig.serverUrl, AliPayConfig.authAppId, AliPayConfig.merchant_auth_private_key, "json", "1.0", "RSA2", AliPayConfig.alipay_auth_public_key, AliPayConfig.charset, false);
                AlipaySystemOauthTokenRequest request = new AlipaySystemOauthTokenRequest();
                request.Code = auth_code;
                request.GrantType = "authorization_code";

                try
                {
                    AlipaySystemOauthTokenResponse oauthTokenResponse = client.Execute(request);

                    //PayLogHelper.WritePayLog(oauthTokenResponse.Body);

                    //PayLogHelper.WritePayLog(oauthTokenResponse.UserId);

                    string aliId = oauthTokenResponse.UserId;

                    bool isReg = false;
                    Base_MemberInfo member = Base_MemberInfoService.I.GetModels(t => t.AlipayOpenID == aliId).FirstOrDefault();
                    if (member != null)
                    {
                        isReg = true;
                    }

                    Response.Redirect(string.Format("{0}?userId={1}&isReg={2}", AliPayConfig.AliAuthRedirectUrl, aliId, Convert.ToInt32(isReg)));
                }
                catch (Exception ex)
                {

                }
            }
        }
    }
}