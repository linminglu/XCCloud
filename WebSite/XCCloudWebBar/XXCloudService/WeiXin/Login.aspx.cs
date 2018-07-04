using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using XCCloudWebBar.Common;
using XCCloudWebBar.WeiXin.Common;
using XCCloudWebBar.WeiXin.WeixinOAuth;
using XCCloudWebBar.WeiXin.Message;
using XCCloudWebBar.Pay.PPosPay;
using XCCloudWebBar.Common.Enum;
using XCCloudWebBar.BLL.IBLL.XCCloud;
using XCCloudWebBar.BLL.Container;
using XCCloudWebBar.BLL.XCCloud;
using XCCloudWebBar.Model.XCCloud;
using XCCloudWebBar.Model.CustomModel.XCCloud;
using XCCloudWebBar.Business.XCGameMana;
using XCCloudWebBar.Business.Common;
using XCCloudWebBar.Business.XCCloud;

namespace XXCloudService.WeiXin
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                string errMsg = string.Empty;
                string md5 = Request["state"] ?? "";
                string url = Request.Url.GetLeftPart(UriPartial.Path);
                string code = Request["code"] ?? "";
                LogHelper.SaveLog("code:" + code);

                //if (!TokenMana.GetTokenMd5(url, md5))
                //{
                //    errMsg = url + WeiXinConfig.Md5key;
                //    LogHelper.SaveLog("错误:" + errMsg);
                //    Response.Redirect(WeiXinConfig.RedirectErrorPage + "?title=" + HttpUtility.UrlEncode("登录失败") + "&message=" + HttpUtility.UrlEncode(errMsg), false);
                //    return;
                //}       
         
                string accsess_token = string.Empty;
                string refresh_token = string.Empty;
                string openId = string.Empty;
                string unionId = string.Empty;
                string token = string.Empty;
                int? tag = null;
                if (TokenMana.GetOpenTokenForScanQR(code, out accsess_token, out refresh_token, out openId, out unionId))
                {
                    if (string.IsNullOrEmpty(unionId))
                    {
                        if (!TokenMana.GetUnionIdFromOpen(openId, accsess_token, out unionId, out errMsg))
                        {
                            Response.Redirect(WeiXinConfig.RedirectErrorPage + "?title=" + HttpUtility.UrlEncode("登录失败") + "&message=" + HttpUtility.UrlEncode(errMsg), false);
                            return;
                        }                        
                    }

                    //验证用户 
                    IBase_UserInfoService userInfoService = BLLContainer.Resolve<IBase_UserInfoService>();                    
                    int count = userInfoService.GetCount(w => w.UnionID == unionId);
                    if (count == 1)
                    {
                        var base_UserInfoModel = userInfoService.GetModels(w => w.UnionID == unionId).FirstOrDefault();
                        int userId = base_UserInfoModel.UserID;
                        int userType = (int)base_UserInfoModel.UserType;
                        int logType = (int)RoleType.XcUser; //默认普通员工登录
                        int auditorId = base_UserInfoModel.Auditor ?? 0;
                        int switchMerch = base_UserInfoModel.SwitchMerch ?? 0;
                        int switchStore = base_UserInfoModel.SwitchStore ?? 0;
                        int switchWorkstation = base_UserInfoModel.SwitchWorkstation ?? 0;
                        string merchId = string.Empty;
                        string storeId = string.Empty;
                        int isSingle = 0;

                        if (userType == (int)UserType.Xc)
                        {
                            if (auditorId == 0)
                            {
                                logType = (int)RoleType.XcAdmin;
                            }
                            token = XCCloudUserTokenBusiness.SetUserToken(userId.ToString(), logType);
                        }
                        else if (userType == (int)UserType.Store || userType == (int)UserType.StoreBoss)
                        {
                            if (switchStore == 0)
                            {
                                errMsg = "您没有访问门店后台的权限";
                                Response.Redirect(WeiXinConfig.RedirectErrorPage + "?title=" + HttpUtility.UrlEncode("登录失败") + "&message=" + HttpUtility.UrlEncode(errMsg), false);
                            }

                            logType = (int)RoleType.StoreUser;
                            storeId = base_UserInfoModel.StoreID;
                            merchId = base_UserInfoModel.MerchID;
                            var dataModel = new TokenDataModel { StoreID = storeId, MerchID = merchId };
                            token = XCCloudUserTokenBusiness.SetUserToken(userId.ToString(), logType, dataModel);
                            IBase_StoreInfoService base_StoreInfoService = BLLContainer.Resolve<IBase_StoreInfoService>();
                            if (!base_StoreInfoService.Any(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase)))
                            {
                                errMsg = "您所访问的门店不存在";
                                Response.Redirect(WeiXinConfig.RedirectErrorPage + "?title=" + HttpUtility.UrlEncode("登录失败") + "&message=" + HttpUtility.UrlEncode(errMsg), false);
                            }
                            var base_StoreInfoModel = base_StoreInfoService.GetModels(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                            tag = base_StoreInfoModel.StoreTag;
                            isSingle = XCCloudStoreBusiness.IsSingleStore(merchId) ? 1 : 0;                            
                        }
                        else if (userType == (int)UserType.Agent || userType == (int)UserType.Normal || userType == (int)UserType.Heavy)
                        {
                            if (switchMerch == 0)
                            {
                                errMsg = "您没有访问商户后台的权限";
                                Response.Redirect(WeiXinConfig.RedirectErrorPage + "?title=" + HttpUtility.UrlEncode("登录失败") + "&message=" + HttpUtility.UrlEncode(errMsg), false);
                            }

                            logType = (int)RoleType.MerchUser;
                            merchId = base_UserInfoModel.MerchID;                            
                            IBase_MerchantInfoService base_MerchantInfoService = BLLContainer.Resolve<IBase_MerchantInfoService>();
                            if (!base_MerchantInfoService.Any(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase)))
                            {
                                errMsg = "您所访问的商户不存在";
                                Response.Redirect(WeiXinConfig.RedirectErrorPage + "?title=" + HttpUtility.UrlEncode("登录失败") + "&message=" + HttpUtility.UrlEncode(errMsg), false);
                            }
                            var base_MerchantInfoModel = base_MerchantInfoService.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                            var dataModel = new TokenDataModel { MerchID = merchId, StoreID = string.Empty, MerchType = base_MerchantInfoModel.MerchType, CreateType = base_MerchantInfoModel.CreateType, CreateUserID = base_MerchantInfoModel.CreateUserID };
                            token = XCCloudUserTokenBusiness.SetUserToken(userId.ToString(), logType, dataModel);
                            tag = base_MerchantInfoModel.MerchTag;
                            isSingle = XCCloudStoreBusiness.IsSingleStore(merchId) ? 1 : 0;
                        }

                        Response.Redirect(WeiXinConfig.RedirectMainPage + "?token=" + token + "&logType=" + logType + "&userType=" + userType + "&tag=" + tag + "&isSingle=" + isSingle +
                            "&switchMerch=" + switchMerch + "&switchStore=" + switchStore + "&switchWorkstation=" + switchWorkstation + "&merchId=" + merchId + "&storeId=" + storeId,
                            false);
                    }
                    else if (count > 1)
                    {
                        token = XCCloudUserTokenBusiness.SetUserToken(unionId, -1);                        
                        var userNames = string.Join(",", userInfoService.GetModels(w => w.UnionID == unionId).Select(o => o.LogName).ToList());
                        Response.Redirect(WeiXinConfig.RedirectSelectUserPage + "?token=" + token + "&userNames=" + HttpUtility.UrlEncode(userNames),
                            false);
                    }
                    else
                    {
                        errMsg = "用户未注册";
                        LogHelper.SaveLog("失败:" + errMsg);
                        Response.Redirect(WeiXinConfig.RedirectErrorPage + "?title=" + HttpUtility.UrlEncode("登录失败") + "&message=" + HttpUtility.UrlEncode(errMsg), false);
                    }                 
                }
                else
                {
                    errMsg = "获取openId失败";
                    LogHelper.SaveLog("错误:" + errMsg);
                    Response.Redirect(WeiXinConfig.RedirectLogoutPage, false);
                }
            }
            catch (Exception ex)
            {
                LogHelper.SaveLog("错误:" + ex.Message);
                Response.Redirect(WeiXinConfig.RedirectErrorPage + "?title=" + HttpUtility.UrlEncode("登录失败") + "&message=" + HttpUtility.UrlEncode(ex.Message), false);
            }
        }
    }
}