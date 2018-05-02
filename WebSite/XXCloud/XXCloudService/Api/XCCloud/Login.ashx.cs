using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XCCloudService.Base;
using XCCloudService.BLL.CommonBLL;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
using XCCloudService.BLL.XCCloud;
using XCCloudService.BLL.XCCloud;
using XCCloudService.Business.XCGameMana;
using XCCloudService.Common;
using XCCloudService.Common.Enum;
using XCCloudService.Common.Extensions;
using XCCloudService.Model.CustomModel.XCCloud;
using XCCloudService.Model.XCCloud;

namespace XXCloudService.Api.XCCloud
{
    /// <summary>
    /// Login 的摘要说明
    /// </summary>
    public class Login : ApiBase
    {
        private bool getUserLogResponseModel(Base_UserInfo base_UserInfoModel, ref UserLogResponseModel userLogResponseModel, out string errMsg)
        {
            errMsg = string.Empty;
            int userId = base_UserInfoModel.UserID;
            int userType = (int)base_UserInfoModel.UserType;
            int logType = (int)RoleType.XcUser; //默认普通员工登录
            int auditorId = base_UserInfoModel.Auditor ?? 0;
            int switchMerch = base_UserInfoModel.SwitchMerch ?? 0;
            int switchStore = base_UserInfoModel.SwitchStore ?? 0;
            int switchWorkstation = base_UserInfoModel.SwitchWorkstation ?? 0;

            if (userType == (int)UserType.Xc)
            {
                if (auditorId == 0)
                {
                    logType = (int)RoleType.XcAdmin;
                }
                userLogResponseModel.Token = XCCloudUserTokenBusiness.SetUserToken(userId.ToString(), logType);
            }
            else if (userType == (int)UserType.Store || userType == (int)UserType.StoreBoss)
            {
                if (switchStore == 0)
                {
                    errMsg = "您没有访问门店后台的权限";
                    return false;
                }

                logType = (int)RoleType.StoreUser;
                string storeId = base_UserInfoModel.StoreID;
                string merchId = base_UserInfoModel.MerchID;
                var dataModel = new MerchDataModel { StoreID = storeId, MerchID = merchId };
                IBase_StoreInfoService base_StoreInfoService = BLLContainer.Resolve<IBase_StoreInfoService>();
                if (!base_StoreInfoService.Any(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase)))
                {
                    errMsg = "您所访问的门店不存在";
                    return false;
                }
                var base_StoreInfoModel = base_StoreInfoService.GetModels(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                userLogResponseModel.Token = XCCloudUserTokenBusiness.SetUserToken(userId.ToString(), logType, dataModel);
                userLogResponseModel.MerchTag = base_StoreInfoModel.StoreTag;
                userLogResponseModel.MerchID = merchId;
                userLogResponseModel.StoreID = storeId;
            }
            else
            {
                if (switchMerch == 0)
                {
                    errMsg = "您没有访问商户后台的权限";
                    return false;
                }

                logType = (int)RoleType.MerchUser;
                string merchId = base_UserInfoModel.MerchID;
                IBase_MerchantInfoService base_MerchantInfoService = BLLContainer.Resolve<IBase_MerchantInfoService>();
                if (!base_MerchantInfoService.Any(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase)))
                {
                    errMsg = "您所访问的商户不存在";
                    return false;
                }
                var base_MerchantInfoModel = base_MerchantInfoService.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                var dataModel = new MerchDataModel { MerchID = merchId, StoreID = string.Empty, MerchType = base_MerchantInfoModel.MerchType, CreateType = base_MerchantInfoModel.CreateType, CreateUserID = base_MerchantInfoModel.CreateUserID };
                userLogResponseModel.Token = XCCloudUserTokenBusiness.SetUserToken(userId.ToString(), logType, dataModel);
                userLogResponseModel.MerchTag = base_MerchantInfoModel.MerchTag;
                userLogResponseModel.MerchID = merchId;
                userLogResponseModel.StoreID = string.Empty;
            }

            userLogResponseModel.LogType = logType;
            userLogResponseModel.UserType = userType;
            userLogResponseModel.SwitchMerch = switchMerch;
            userLogResponseModel.SwitchStore = switchStore;
            userLogResponseModel.SwitchWorkstation = switchWorkstation;            

            return true;
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MethodToken, SysIdAndVersionNo = false)]
        public object CheckUser(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string token = string.Empty;
                string userName = dicParas.ContainsKey("userName") ? dicParas["userName"].ToString() : string.Empty;
                string password = dicParas.ContainsKey("password") ? dicParas["password"].ToString() : string.Empty;

                if (string.IsNullOrWhiteSpace(userName))
                {
                    errMsg = "用户名不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (string.IsNullOrWhiteSpace(password))
                {
                    errMsg = "密码不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                password = Utils.MD5(password);
                UserLogResponseModel userLogResponseModel = new UserLogResponseModel();
                IBase_UserInfoService base_UserInfoService = BLLContainer.Resolve<IBase_UserInfoService>(resolveNew:true);                                                
                if (base_UserInfoService.Any(p => p.LogName.Equals(userName, StringComparison.OrdinalIgnoreCase) && p.LogPassword.Equals(password, StringComparison.OrdinalIgnoreCase)))
                {
                    var base_UserInfoModel = base_UserInfoService.GetModels(p => p.LogName.Equals(userName, StringComparison.OrdinalIgnoreCase) && p.LogPassword.Equals(password, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    if (!getUserLogResponseModel(base_UserInfoModel, ref userLogResponseModel, out errMsg))
                    {
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }

                    return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, userLogResponseModel);
                }
                else
                {
                    errMsg = "用户名或密码错误";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }                
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            } 
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object SwitchUser(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string logType = dicParas.ContainsKey("logType") ? Convert.ToString(dicParas["logType"]) : string.Empty;
                string merchId = dicParas.ContainsKey("merchId") ? Convert.ToString(dicParas["merchId"]) : string.Empty;
                string storeId = dicParas.ContainsKey("storeId") ? Convert.ToString(dicParas["storeId"]) : string.Empty;
                string workStationId = dicParas.ContainsKey("workStationId") ? Convert.ToString(dicParas["workStationId"]) : string.Empty;
                int? merchTag = (int?)null;

                if (string.IsNullOrEmpty(logType))
                {
                    errMsg = "logType参数不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];                
                var merchDataModel = (userTokenKeyModel.DataModel as MerchDataModel);
                merchDataModel.WorkStationID = workStationId.Toint();

                userTokenKeyModel.LogType = Convert.ToInt32(logType);
                if (userTokenKeyModel.LogType == (int)RoleType.MerchUser)
                {
                    IBase_MerchantInfoService base_MerchantInfoService = BLLContainer.Resolve<IBase_MerchantInfoService>();
                    if (!base_MerchantInfoService.Any(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase)))
                    {
                        errMsg = "您所访问的商户不存在";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                    var base_MerchantInfoModel = base_MerchantInfoService.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    merchTag = base_MerchantInfoModel.MerchTag;
                    merchDataModel.MerchID = merchId;
                    merchDataModel.StoreID = string.Empty;
                }
                else if (userTokenKeyModel.LogType == (int)RoleType.StoreUser)
                {
                    IBase_StoreInfoService base_StoreInfoService = BLLContainer.Resolve<IBase_StoreInfoService>();
                    if (!base_StoreInfoService.Any(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase)))
                    {
                        errMsg = "您所访问的门店不存在";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                    var base_StoreInfoModel = base_StoreInfoService.GetModels(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    merchTag = base_StoreInfoModel.StoreTag;
                    merchDataModel.MerchID = merchId;
                    merchDataModel.StoreID = storeId;
                }

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, new { MerchTag = merchTag });
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object SelectUserToLog(Dictionary<string, object> dicParas)
        {
            try
            {
                string token = dicParas["userToken"].ToString();
                XCCloudUserTokenBusiness.RemoveToken(token);

                string errMsg = string.Empty;                
                string userName = dicParas.ContainsKey("userName") ? Convert.ToString(dicParas["userName"]) : string.Empty;
                
                if (string.IsNullOrEmpty(userName))
                {
                    errMsg = "用户名不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                UserLogResponseModel userLogResponseModel = new UserLogResponseModel();
                IBase_UserInfoService base_UserInfoService = BLLContainer.Resolve<IBase_UserInfoService>();
                if (base_UserInfoService.Any(p => p.LogName.Equals(userName, StringComparison.OrdinalIgnoreCase)))
                {
                    var base_UserInfoModel = base_UserInfoService.GetModels(p => p.LogName.Equals(userName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    if (!getUserLogResponseModel(base_UserInfoModel, ref userLogResponseModel, out errMsg))
                    {
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }

                    return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, userLogResponseModel);
                }
                else
                {
                    errMsg = "用户名不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

    }
}