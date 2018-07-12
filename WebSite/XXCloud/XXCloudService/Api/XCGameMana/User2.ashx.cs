using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XCCloudService.Base;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.XCCloud;
using XCCloudService.Business;
using XCCloudService.Business.Common;
using XCCloudService.Business.XCGameMana;
using XCCloudService.CacheService;
using XCCloudService.Common.Enum;
using XCCloudService.Model.CustomModel.Common;
using XCCloudService.Model.CustomModel.XCGameManager;
using XCCloudService.Model.Socket.UDP;
using XCCloudService.Model.XCCloud;
using XCCloudService.SocketService.UDP.Factory;
using XXCloudService.Utility;

namespace XXCloudService.Api.XCGameMana
{
    /// <summary>
    /// User2 的摘要说明
    /// </summary>
    public class User2 : ApiBase
    {
        /// <summary>
        /// 获取用户注册短信验证码
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MethodToken)]
        public object getRegisterSMSCode(Dictionary<string, object> dicParas)
        {
            try
            {
                string storeId = dicParas.ContainsKey("storeId") ? dicParas["storeId"].ToString() : string.Empty;
                string mobile = dicParas.ContainsKey("mobile") ? dicParas["mobile"].ToString() : string.Empty;
                string imgCode = dicParas.ContainsKey("imgCode") ? dicParas["imgCode"].ToString() : string.Empty;
                string errMsg = string.Empty;

                //验证码
                if (!ValidateImgCache.Exist(imgCode.ToUpper()))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "验证码无效");
                }
                ValidateImgCache.Remove(imgCode.ToUpper());

                if (string.IsNullOrEmpty(storeId))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "门店号码不正确");
                }
                if (string.IsNullOrEmpty(mobile))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "手机号码不正确");
                }
  
                bool isSMSTest = bool.Parse(System.Configuration.ConfigurationManager.AppSettings["isSMSTest"].ToString());

                StoreBusiness sb = new StoreBusiness();
                StoreCacheModel storeModel = null;
                if (!sb.IsEffectiveStore(storeId, ref storeModel,out errMsg))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, errMsg);
                }

                if (storeModel.StoreType == 0)
                { 
                    if (storeModel.StoreDBDeployType == 0)
                    { 
                        //验证用户在分库是否存在
                        XCCloudService.BLL.IBLL.XCGame.IUserService userService = BLLContainer.Resolve<XCCloudService.BLL.IBLL.XCGame.IUserService>(storeModel.StoreDBName);
                        var gameUserModel = userService.GetModels(p => p.Mobile.Equals(mobile, StringComparison.OrdinalIgnoreCase)).FirstOrDefault<XCCloudService.Model.XCGame.u_users>();
                        if (gameUserModel == null)
                        {
                            return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "未查询到该用户");
                        }                    
                    }
                    else if (storeModel.StoreDBDeployType == 1)
                    {
                        if (!UDPApiService.UserPhoneQuery(storeModel.StoreID, storeModel.StorePassword, mobile, out errMsg))
                        {
                            return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, errMsg);
                        }
                    }
                    else
                    {
                        return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "门店设置不正确");
                    }                    
                }
                else if (storeModel.StoreType == 1)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "当前门店不能进行操作");
                }
                else if (storeModel.StoreType == 2)
                {
                    //验证用户在分库是否存在
                    XCCloudService.BLL.IBLL.XCCloud.IBase_UserInfoService userService = BLLContainer.Resolve<XCCloudService.BLL.IBLL.XCCloud.IBase_UserInfoService>();
                    var userModel = userService.GetModels(p => p.Mobile.Equals(mobile, StringComparison.OrdinalIgnoreCase) && p.StoreID.Equals(storeModel.StoreID)).FirstOrDefault<XCCloudService.Model.XCCloud.Base_UserInfo>();
                    if (userModel == null)
                    {
                        return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "未查询到该用户");
                    }
                }
                else
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "门店配置无效");
                }

                string templateId = "2";
                string key = string.Empty;
                if (!isSMSTest && !FilterMobileBusiness.ExistMobile(mobile))
                {
                    string smsCode = string.Empty;
                    if (SMSBusiness.GetSMSCode(out smsCode))
                    {
                        key = mobile + "_" + smsCode;
                        SMSCodeCache.Add(key, mobile, CacheExpires.SMSCodeExpires);
                    
                        if (SMSBusiness.SendSMSCode(templateId, mobile, smsCode, out errMsg))
                        {
                            return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.T, "");
                        }
                        else
                        {
                            return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, errMsg);
                        }
                    }
                    else
                    {
                        return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "发送验证码出错");
                    }
                }
                else
                {
                    key = mobile + "_" + "123456";
                    SMSCodeCache.Add(key, mobile, CacheExpires.SMSCodeExpires);
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.T, "");
                }
            }
            catch(Exception e)
            {
                throw e;
            }
        }


        /// <summary>
        /// 用户注册
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MethodToken)]
        public object registerUser(Dictionary<string, object> dicParas)
        {
            try
            {
                string storeId = dicParas.ContainsKey("storeId") ? dicParas["storeId"].ToString() : string.Empty;
                string mobile = dicParas.ContainsKey("mobile") ? dicParas["mobile"].ToString() : string.Empty;
                string smsCode = dicParas.ContainsKey("smsCode") ? dicParas["smsCode"].ToString() : string.Empty;
                string errMsg = string.Empty;
                int userId = 0;

                if (string.IsNullOrEmpty(storeId))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "门店号码不正确");
                }
                if (string.IsNullOrEmpty(mobile))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "手机号码不正确");
                }
                if (string.IsNullOrEmpty(smsCode))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "请输入验证码");
                }

                //验证短信验证码
                string key = mobile + "_" + smsCode;
                if (!FilterMobileBusiness.IsTestSMS && !FilterMobileBusiness.ExistMobile(mobile))
                {
                    if (!SMSCodeCache.IsExist(key))
                    {
                        return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "短信验证码无效");
                    }
                }

                if (SMSCodeCache.IsExist(key))
                {
                    SMSCodeCache.Remove(key);
                }

                //验证门店是否有效
                StoreBusiness sb = new StoreBusiness();
                StoreCacheModel storeModel = null;
                if (!sb.IsEffectiveStore(storeId, ref storeModel, out errMsg))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, errMsg);
                }

                if (storeModel.StoreType == 0)
                {
                    if (storeModel.StoreDBDeployType == 0)
                    {
                        //验证用户在分库是否存在
                        XCCloudService.BLL.IBLL.XCCloud.IBase_UserInfoService userService = BLLContainer.Resolve<XCCloudService.BLL.IBLL.XCCloud.IBase_UserInfoService>();
                        var userModel = userService.GetModels(p => p.Mobile.Equals(mobile, StringComparison.OrdinalIgnoreCase) && p.StoreID.Equals(storeModel.StoreID)).FirstOrDefault<XCCloudService.Model.XCCloud.Base_UserInfo>();
                        if (userModel == null)
                        {
                            return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "未查询到该用户");
                        }
                        userId = userModel.ID;
                    }
                    else if(storeModel.StoreDBDeployType == 1)
                    {
                        if (!UDPApiService.UserPhoneQuery(storeModel.StoreID, storeModel.StorePassword, mobile, out errMsg))
                        {
                            return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, errMsg);
                        }
                        userId = 0;//本地库用0
                    }
                    else
                    {
                        return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "门店配置无效");
                    }
                }
                else if (storeModel.StoreType == 1)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "当前门店不能进行操作");
                }
                else if (storeModel.StoreType == 2)
                {
                    //验证用户在分库是否存在
                    XCCloudService.BLL.IBLL.XCCloud.IBase_UserInfoService userService = BLLContainer.Resolve<XCCloudService.BLL.IBLL.XCCloud.IBase_UserInfoService>();
                    var userModel = userService.GetModels(p => p.Mobile.Equals(mobile, StringComparison.OrdinalIgnoreCase) && p.StoreID.Equals(storeModel.StoreID)).FirstOrDefault<XCCloudService.Model.XCCloud.Base_UserInfo>();
                    if (userModel == null)
                    {
                        return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "未查询到该用户");
                    }
                    userId = userModel.ID;

                    //添加工作站信息
                    var data_WorkstationModel = Data_WorkstationService.I.GetModels(p => p.WorkStation.Equals(mobile, StringComparison.OrdinalIgnoreCase)).FirstOrDefault() ?? new Data_Workstation();
                    if (data_WorkstationModel.ID == 0)
                    {
                        data_WorkstationModel.MerchID = storeId.Substring(0, 6);
                        data_WorkstationModel.StoreID = storeId;
                        data_WorkstationModel.WorkStation = mobile;
                        data_WorkstationModel.StationType = (int)StationType.Mobile;
                        data_WorkstationModel.DepotID = 0;
                        data_WorkstationModel.MacAddress = string.Empty;
                        data_WorkstationModel.DiskID = string.Empty;
                        data_WorkstationModel.State = 1;
                        if(!Data_WorkstationService.I.Add(data_WorkstationModel))
                            return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "手机注册工作站设备失败");
                    }
                }
                else
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "门店配置无效");
                }

                //获取用户token
                //云库不用userId
                string token = XCManaUserHelperTokenBusiness.SetToken(mobile, storeId, storeModel.StoreName, userId);
                List<XCManaUserHelperTokenResultModel> list = null;
                XCManaUserHelperTokenBusiness.GetUserTokenModel(mobile, ref list);
                var obj = new
                {
                    userToken = token,
                    storeList = list
                };
                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, obj);
            }
            catch(Exception e)
            {
                throw e;
            }
        }
    }
}