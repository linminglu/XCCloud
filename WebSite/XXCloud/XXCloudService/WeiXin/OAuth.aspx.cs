using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using XCCloudService.Common;
using XCCloudService.WeiXin.Common;
using XCCloudService.WeiXin.WeixinOAuth;
using XCCloudService.WeiXin.Message;
using XCCloudService.Pay.PPosPay;
using XCCloudService.Common.Enum;
using XCCloudService.Business.Common;
using XCCloudService.BLL.IBLL.XCGameManager;
using XCCloudService.BLL.Container;
using XCCloudService.Model.XCGameManager;
using XCCloudService.Common.Extensions;
using XCCloudService.BLL.XCCloud;
using XCCloudService.Model.XCCloud;
using XCCloudService.WeiXin.WeixinPub;
using XCCloudService.Model.WeiXin;
using XCCloudService.CacheService;
using XCCloudService.Business.XCCloud;

namespace XXCloudService.WeiXin
{
    public partial class OAuth : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
         {
            try
            {
                string code = Request["code"] ?? "";
                string state = string.Empty;
                string paramStr = string.Empty;
                AnalysisState(Request["state"] ?? "", out state, out paramStr);

  
                switch (state)
                {
                    case "h5_Auth_Common": H5AuthCommon(code); break;
                    case "work_Auth":
                        {
                            var userId = Request["userId"].Toint();
                            var authorKey = Request["authorKey"].Tostring();
                            WorkAuth(code, userId, authorKey); break;
                        }
                    case "WX_Auth_OrderAudit": OrderAudit(code, paramStr); break;
                    default: break;
                }
            }
            catch (Exception ex)
            {
                LogHelper.SaveLog(TxtLogType.WeiXin, TxtLogContentType.Exception, TxtLogFileType.Day, "errMsg:" + ex.Message);
            }
        }


        private void AnalysisState(string requestState,out string state,out string paramStr)
        {
            paramStr = string.Empty;
            if (requestState.IndexOf(Constant.WX_Auth_OrderAudit) >= 0)
            {
                state = Constant.WX_Auth_OrderAudit;
                paramStr = requestState.Replace(Constant.WX_Auth_OrderAudit + "_", "");
            }
            else
            {
                state = requestState;
            }
        }

        /// <summary>
        /// H5页面微信授权
        /// </summary>
        private void H5AuthCommon(string code)
        {
            string accsess_token = string.Empty;
            string refresh_token = string.Empty;
            string openId = string.Empty;
            string errMsg = string.Empty;
            try
            {
                if (TokenMana.GetTokenForScanQR(code, out accsess_token, out refresh_token, out openId, out errMsg))
                {
                    MemberTokenModel memberCacheModel = MemberTokenCache.GetModel(openId);
                    
                    if (memberCacheModel == null)
                    {
                        //获取微信用户基本信息
                        WechatInfo model = CommonHelper.GetWechatInfo(openId);
                        if (model == null)
                        {
                            model = CommonHelper.GetWechatInfo(accsess_token, openId);
                        }

                        memberCacheModel = new MemberTokenModel();
                        memberCacheModel.Token = openId;
                        memberCacheModel.Info = model;

                        Base_MemberInfo member = Base_MemberInfoService.I.GetModels(t => t.WechatOpenID == openId).FirstOrDefault();
                        if (member == null)//未注册
                        {
                            member = new Base_MemberInfo();
                            member.ID = RedisCacheHelper.CreateCloudSerialNo("0".PadLeft(15, '0'));
                            member.WechatOpenID = openId;
                            member.UserPassword = Utils.MD5("123456");
                            member.UserName = model.nickname;
                            member.Photo = model.headimgurl;
                            member.CreateTime = DateTime.Now;
                            member.MemberState = 1;
                            bool ret = Base_MemberInfoService.I.Add(member);
                            if (!ret)
                            {
                                Response.Redirect(WeiXinConfig.RedirectErrorPage);
                            }
                        }

                        memberCacheModel.MemberId = member.ID;
                        memberCacheModel.Mobile = string.IsNullOrEmpty(member.Mobile) ? string.Empty : member.Mobile;

                        MemberTokenCache.AddToken(openId, memberCacheModel);
                    }
                    //是否关注了公众号
                    int isSubscribe = memberCacheModel.Info.subscribe;
                    string redirectUrl = string.Format("{0}?openId={1}&isReg={2}&isSubscribe={3}", CommonConfig.H5WeiXinAuthRedirectUrl, openId, 1, isSubscribe);
                    Response.Redirect(redirectUrl);
                }
                else
                {
                    LogHelper.SaveLog(TxtLogType.WeiXin, TxtLogContentType.Common, TxtLogFileType.Day, "errMsg:" + errMsg);
                    //重定向的错误页面                 
                    Response.Redirect(WeiXinConfig.RedirectErrorPage);
                }
            }
            catch(Exception e)
            {
                LogHelper.SaveLog(TxtLogType.WeiXin, TxtLogContentType.Exception, TxtLogFileType.Day, "Exception:" + e.Message);
            }

        }

        /// <summary>
        /// 业务微信授权
        /// </summary>
        private void WorkAuth(string code, int? userId, string authorKey)
        {
            string accsess_token = string.Empty;
            string refresh_token = string.Empty;
            string openId = string.Empty;
            string errMsg = string.Empty;
            try
            {
                if (TokenMana.GetTokenForScanQR(code, out accsess_token, out refresh_token, out openId, out errMsg))
                {
                    string redirectUrl = string.Empty;
                    var authors = from a in Dict_SystemService.N.GetModels(p => p.Enabled == 1 && p.MerchID == "1")
                                  join b in Dict_SystemService.N.GetModels(p => p.DictKey == "权限列表") on a.PID equals b.ID
                                  join c in Base_UserGrantService.N.GetModels(p => p.UserID == userId) on a.ID equals c.GrantID
                                  where c.GrantEN == 1
                                  select a;

                    //判断操作权限
                    if (authors.Any(a => a.DictKey.Equals(authorKey, StringComparison.OrdinalIgnoreCase)))
                    {
                       
                        
                    }
                    else
                    {                        

                    }
                    Response.Redirect(redirectUrl);
                }
                else
                {
                    LogHelper.SaveLog(TxtLogType.WeiXin, TxtLogContentType.Common, TxtLogFileType.Day, "errMsg:" + errMsg);
                    //重定向的错误页面                 
                    Response.Redirect(WeiXinConfig.RedirectErrorPage);
                }
            }
            catch (Exception e)
            {
                LogHelper.SaveLog(TxtLogType.WeiXin, TxtLogContentType.Exception, TxtLogFileType.Day, "Exception:" + e.Message);
            }

        }

        private void OrderAudit(string code,string paramStr)
        {
            string accsess_token = string.Empty;
            string refresh_token = string.Empty;
            string openId = string.Empty;
            string errMsg = string.Empty;
            string[] paramArr = paramStr.Split('_');
            if (paramArr.Length != 2)
            {
                LogHelper.SaveLog(TxtLogType.WeiXin, TxtLogContentType.Common, TxtLogFileType.Day, "errMsg:" +
                    string.Format("{0}参数错误[{1}]", "OrderAudit", paramStr));        
                Response.Redirect(WeiXinConfig.RedirectErrorPage);
            }

            int auditOrderType = int.Parse(paramArr[0]);
            int orderId = int.Parse(paramArr[1]);

            if (TokenMana.GetTokenForScanQR(code, out accsess_token, out refresh_token, out openId, out errMsg))
            {
                
                string redirectUrl = string.Format("{0}?openId={1}&isReg={2}", CommonConfig.WX_Auth_OrderAuditRedirectUrl, openId);
                Response.Redirect(redirectUrl);
            }
            else
            {
                LogHelper.SaveLog(TxtLogType.WeiXin, TxtLogContentType.Common, TxtLogFileType.Day, "errMsg:" + errMsg);
                //重定向的错误页面                 
                Response.Redirect(WeiXinConfig.RedirectErrorPage);
            }
        }
    }
}