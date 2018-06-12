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
using XCCloudService.Common;
using XCCloudService.Common.Enum;
using XCCloudService.Model.WeiXin;
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
                    if(string.IsNullOrEmpty(aliId))
                    {

                    }

                    bool isReg = false;
                    Base_MemberInfo member = Base_MemberInfoService.I.GetModels(t => t.AlipayOpenID == aliId).FirstOrDefault();
                    if (member != null)
                    {
                        isReg = true;
                    }

                    Response.Redirect(string.Format("{0}?userId={1}&isReg={2}", AliPayConfig.AliAuthRedirectUrl, aliId, Convert.ToInt32(isReg)));


                    ////获取微信用户基本信息
                    //WechatInfo model = CommonHelper.GetWechatInfo(openId);
                    //if (model == null)
                    //{
                    //    model = CommonHelper.GetWechatInfo(accsess_token, openId);
                    //}

                    //MemberTokenModel tokenModel = new MemberTokenModel();
                    //tokenModel.Token = openId;
                    //tokenModel.Info = model;

                    ////是否关注了公众号
                    //int isSubscribe = model.subscribe;

                    ////是否注册了会员
                    //int isReg = 0;
                    //Base_MemberInfo member = Base_MemberInfoService.I.GetModels(t => t.WechatOpenID == openId).FirstOrDefault();
                    //if (member != null)//已注册
                    //{
                    //    MemberTokenModel cacheToken = MemberTokenCache.GetModel(openId);
                    //    //缓存中是否有默认卡信息,有就直接读取，没有就查询数据库
                    //    if (cacheToken != null && cacheToken.CurrentCardInfo != null)
                    //    {
                    //        tokenModel.CurrentCardInfo = cacheToken.CurrentCardInfo;
                    //    }
                    //    else
                    //    {
                    //        Data_Member_Card card = Data_Member_CardService.I.GetModels(t => t.MemberID == member.ID).OrderByDescending(t => t.UpdateTime).FirstOrDefault();
                    //        if (card != null)
                    //        {
                    //            MemberCard cardInfo = new MemberCard();
                    //            cardInfo.CardId = card.ID;
                    //            cardInfo.ICCardId = card.ICCardID;
                    //            cardInfo.MemberLevelId = card.MemberLevelID.Value;
                    //            tokenModel.CurrentCardInfo = cardInfo;
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    //未注册
                    //    member = new Base_MemberInfo();
                    //    member.ID = RedisCacheHelper.CreateCloudSerialNo("0".PadLeft(15, '0'));
                    //    member.WechatOpenID = openId;
                    //    member.UserPassword = Utils.MD5("123456");
                    //    member.UserName = model.nickname;
                    //    member.Photo = model.headimgurl;
                    //    member.CreateTime = DateTime.Now;
                    //    member.MemberState = 1;
                    //    bool ret = Base_MemberInfoService.I.Add(member);
                    //    if (!ret)
                    //    {
                    //        Response.Redirect(WeiXinConfig.RedirectErrorPage);
                    //    }
                    //}

                    //isReg = 1;
                    //tokenModel.MemberId = member.ID;
                    //tokenModel.Mobile = string.IsNullOrEmpty(member.Mobile) ? string.Empty : member.Mobile;

                    //MemberTokenCache.AddToken(openId, tokenModel);

                    //string redirectUrl = string.Format("{0}?openId={1}&isReg={2}&isSubscribe={3}", CommonConfig.H5WeiXinAuthRedirectUrl, openId, isReg, isSubscribe);
                    //Response.Redirect(redirectUrl);
                }
                catch (Exception ex)
                {
                    LogHelper.SaveLog(TxtLogType.AliPay, TxtLogContentType.Exception, TxtLogFileType.Day, "Exception:" + ex.Message);
                }
            }
        }
    }
}