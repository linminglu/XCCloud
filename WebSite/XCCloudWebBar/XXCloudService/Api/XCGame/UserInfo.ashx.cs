using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XCCloudWebBar.Base;
using XCCloudWebBar.BLL.Container;
using XCCloudWebBar.BLL.IBLL.XCGame;
using XCCloudWebBar.BLL.IBLL.XCGameManager;
using XCCloudWebBar.Business.Common;
using XCCloudWebBar.Business.XCGame;
using XCCloudWebBar.CacheService;
using XCCloudWebBar.Model.XCGame;
using XCCloudWebBar.Model.XCGameManager;

namespace XXCloudService.Api.XCGame
{
    /// <summary>
    /// UserInfo 的摘要说明
    /// </summary>
    public class UserInfo : ApiBase
    {
        /// <summary>
        /// 获取用户token
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MethodToken)]
        public object getUserToken(Dictionary<string, object> dicParas)
        {
            try
            {
                string storeId = dicParas.ContainsKey("storeId") ? dicParas["storeId"].ToString() : string.Empty;
                string mobile = dicParas.ContainsKey("mobile") ? dicParas["mobile"].ToString() : string.Empty;
                string smsCode = dicParas.ContainsKey("smsCode") ? dicParas["smsCode"].ToString() : string.Empty;
                if (string.IsNullOrEmpty(smsCode))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "请输入验证码");
                }
                bool isSMSTest = bool.Parse(System.Configuration.ConfigurationManager.AppSettings["isSMSTest"].ToString());
                string key = mobile + "_" + smsCode;
                if (!isSMSTest && !FilterMobileBusiness.ExistMobile(mobile))
                {
                    if (!SMSCodeCache.IsExist(key))
                    {
                        return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "短信验证码无效");
                    }
                }
                if (string.IsNullOrEmpty(storeId))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "门店号不能为空");
                }
                if (string.IsNullOrEmpty(mobile))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "手机号码不能为空");
                }
                int storeids = int.Parse(storeId);
                IStoreService storeService = BLLContainer.Resolve<IStoreService>();
                var menlist = storeService.GetModels(x => x.id == storeids.ToString()).FirstOrDefault<t_store>();
                if (menlist == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "门店号无效");
                }
                string dbname = menlist.store_dbname;
                IMemberService memberService = BLLContainer.Resolve<IMemberService>(dbname);
                var memberlist = memberService.GetModels(x => x.Mobile == mobile).FirstOrDefault<t_member>();
                if (memberlist == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "手机号码无效");
                }
                var UserToken = UserInfoBusiness.SetUserToken(storeId, mobile);
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, UserToken, Result_Code.T, "");
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        /// <summary>
        /// 验证usertokne是否合法
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MethodToken)]
        public object addUserToken(Dictionary<string, object> dicParas)
        {
            string userToken = dicParas.ContainsKey("userToken") ? dicParas["userToken"].ToString() : string.Empty;
            var list = UserInfoBusiness.GetUserTokenModel(userToken);
            if (list == null)
            {
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "用户token无效");
            }

            return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.T, "");
        }
            
    }
}