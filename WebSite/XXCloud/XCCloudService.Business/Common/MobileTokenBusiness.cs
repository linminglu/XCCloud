﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XCCloudService.Common;
using System.Threading;
using XCCloudService.CacheService;
using System.Threading.Tasks;
using XCCloudService.BLL.IBLL.XCGameManager;
using XCCloudService.BLL.Container;
using XCCloudService.Model.XCGameManager;
using XCCloudService.BLL.IBLL.XCCloudRS232;
using XCCloudService.Model.XCCloudRS232;
using System.Collections;

namespace XCCloudService.Business.Common
{
    public class MobileTokenBusiness
    {
        public static string SetMobileToken(string mobile)
        {
            string newToken = System.Guid.NewGuid().ToString("N");
            string token = string.Empty;
            if (ExistMobile(mobile, out token))
            {
                MobileTokenCache.RemoveToken(token);
                SetDBMobileToken(newToken, mobile);
                MobileTokenCache.AddToken(mobile, newToken);
            }
            else
            {
                SetDBMobileToken(newToken, mobile);
                MobileTokenCache.AddToken(mobile, newToken);
            }
            return token;
        }


        public static void RemoveThirdIdBinding(string mobile,string thirdType)
        {
            RemoveThirdId(mobile, thirdType);
            RemoveTokenThirdId(mobile, thirdType);
        }

        private static bool RemoveTokenThirdId(string mobile,string thirdType)
        {
            MobileTokenModel model = MobileTokenCache.MobileTokenList.Where(t => t.Mobile.Equals(mobile)).FirstOrDefault();
            if (model == null)
            {
                return false;
            }
            else
            {
                if (thirdType == "0")
                {
                    model.WeiXinId = string.Empty;
                }
                else if (thirdType == "1")
                {
                    model.AliId = string.Empty;
                }
                MobileTokenCache.AddToken(model.Token, model);
                return true;
            }
        }

        public static bool ExistMobileAndThirdIdBinding(string mobile, string thirdType, string userThirdId)
        {
            if (thirdType == "0")
            {
                var model = MobileTokenCache.MobileTokenList.Where(t => (t.Mobile.Equals(mobile) && !string.IsNullOrEmpty(t.WeiXinId)) || t.WeiXinId.Equals(userThirdId)).FirstOrDefault();
                if(model == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else if (thirdType == "1")
            {
                var model = MobileTokenCache.MobileTokenList.Where(t => (t.Mobile.Equals(mobile) && !string.IsNullOrEmpty(t.AliId)) || t.AliId.Equals(userThirdId)).FirstOrDefault();
                if (model == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        public static string SetMobileToken(string mobile, string thirdType, string userThirdId)
        {
            string token = System.Guid.NewGuid().ToString("N");
            SetDBMobileToken(token, mobile, thirdType, userThirdId);
            MobileTokenCache.AddToken(token, mobile, thirdType, userThirdId);
            return token;
        }

        public static void AddToken(string mobile, string token)
        {
            MobileTokenCache.AddToken(mobile, token);
        }

        //public static bool GetMobileTokenModel(string mobile, out string token)
        //{
        //    token = string.Empty;
        //    var query = from item in MobileTokenCache.MobileTokenHt.Cast<DictionaryEntry>()
        //                where ((MobileTokenModel)(item.Value)).Mobile.Equals(mobile)
        //                select item.Key.ToString();
        //    if (query.Count() == 0)
        //    {
        //        return false;
        //    }
        //    else
        //    {
        //        token = query.First();
        //        return true;
        //    }
        //}


        public static MobileTokenModel GetMobileTokenModel(string token)
        {
            if (MobileTokenCache.ExistToken(token))
            {
                MobileTokenModel tokenModel = MobileTokenCache.GetMobileTokenModel(token);
                return tokenModel;
            }
            else
            {
                return null;
            }
        }

        public static bool ExistToken(string token, ref MobileTokenModel mobileTokenModel)
        {
            if (MobileTokenCache.ExistToken(token))
            {
                mobileTokenModel = GetMobileTokenModel(token);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool ExistToken(string token, out string mobile)
        {
            mobile = string.Empty;
            if (MobileTokenCache.ExistToken(token))
            {
                MobileTokenModel model = GetMobileTokenModel(token);
                mobile = model.Mobile;
                return true;
            }
            else
            {
                return false;
            }
        }


        public static bool ExistMobile(string mobile, out string token)
        {
            token = string.Empty;
            var model = MobileTokenCache.MobileTokenList.Where(t => t.Mobile.Equals(mobile)).FirstOrDefault();
            if (model == null)
            {
                return false;
            }
            else
            {
                token = model.Token;
                return true;
            }
        }

        public static bool ExistMobile(string mobile)
        {
            var model = MobileTokenCache.MobileTokenList.Where(t => t.Mobile.Equals(mobile)).FirstOrDefault();
            if (model == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static bool ExistThirdId(string thirdId, ref MobileTokenModel model)
        {
            var item = MobileTokenCache.MobileTokenList.Where(t => t.WeiXinId.Equals(thirdId) || t.AliId.Equals(thirdId)).FirstOrDefault();
            if (item == null)
            {
                return false;
            }
            else
            {
                model = item;
                return true;
            }
        }


        public static bool ExistThirdId(string thirdId, ref MobileTokenModel model, out string token)
        {
            token = string.Empty;
            var item = MobileTokenCache.MobileTokenList.Where(t => t.WeiXinId.Equals(thirdId) || t.AliId.Equals(thirdId)).FirstOrDefault();
            if (item == null)
            {
                return false;
            }
            else
            {
                token = item.Token;
                model = item;
                return true;
            }
        }

        public static void Init()
        {
            if (!RedisCacheHelper.KeyExists(MobileTokenCache.mobileTokenCacheKey))
            {
                IMobileTokenService mobileTokenService = BLLContainer.Resolve<IMobileTokenService>();
                var models = mobileTokenService.GetModels(p => true).ToList<t_MobileToken>();
                if (models.Count > 0)
                {
                    for (int y = 0; y < models.Count; y++)
                    {
                        MobileTokenModel model = new MobileTokenModel(models[y].Phone,
                            string.IsNullOrEmpty(models[y].OpenId) ? string.Empty : models[y].OpenId,
                            string.IsNullOrEmpty(models[y].AliId) ? string.Empty : models[y].AliId);
                        MobileTokenCache.AddToken(models[y].Token, model);
                    }
                }
            }
        }

        public static void SetRS232MobileToken()
        {
            IMerchService mobileTokenService = BLLContainer.Resolve<IMerchService>();
            var models = mobileTokenService.GetModels(p => true).ToList().Where(p=>!string.IsNullOrWhiteSpace(p.Token)).ToList();
            if (models.Count > 0)
            {
                foreach (var item in models)
                {
                    //MobileTokenCache.AddToken(CommonConfig.PrefixKey +  item.Mobile, item.Token);
                    string key = CommonConfig.PrefixKey + item.Mobile;
                    if (!MobileTokenCache.ExistToken(key))
                    {
                        MobileTokenModel model = new MobileTokenModel();
                        model.Mobile = item.Mobile;
                        model.Token = item.Token;
                        MobileTokenCache.AddToken(key, model);
                    }
                }
            }
        }

        public static void UpdateOpenId(string phone,string openId)
        {
            IMobileTokenService mobileTokenService = BLLContainer.Resolve<IMobileTokenService>();
            var model = mobileTokenService.GetModels(p => p.Phone.Equals(phone)).FirstOrDefault<t_MobileToken>();
            if (model != null)
            {
                model.OpenId = openId;
                mobileTokenService.Update(model);                
            }
        }

        public static bool GetOpenId(string phone, out string openId,out string errMsg)
        {
            openId = string.Empty;
            errMsg = string.Empty;
            IMobileTokenService mobileTokenService = BLLContainer.Resolve<IMobileTokenService>();
            var model = mobileTokenService.GetModels(p => p.Phone.Equals(phone)).FirstOrDefault<t_MobileToken>();
            if (model != null)
            {
                openId = model.OpenId;
                if (string.IsNullOrEmpty(openId))
                {
                    errMsg = "用户未绑定openId";
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                errMsg = "用户未不存在";
                return false;
            }
        }

        private static void SetDBMobileToken(string Token, string Phone)
        {
            IMobileTokenService mobileTokenService = BLLContainer.Resolve<IMobileTokenService>();
            var model = mobileTokenService.GetModels(p => p.Phone.Equals(Phone)).FirstOrDefault<t_MobileToken>();
            t_MobileToken mtk = new t_MobileToken();
            if (model == null)
            {
                mtk.Token = Token;
                mtk.CreateTime = DateTime.Now;
                mtk.Phone = Phone;
                mtk.OpenId = string.Empty;
                mobileTokenService.Add(mtk);
            }
            else 
            {
                model.Token = Token;
                model.UpdateTime = DateTime.Now;
                mobileTokenService.Update(model);                
            }
        }

        private static void SetDBMobileToken(string Token, string Phone,string thirdType,string userThirdId)
        {
            IMobileTokenService mobileTokenService = BLLContainer.Resolve<IMobileTokenService>();
            var model = mobileTokenService.GetModels(p => p.Phone.Equals(Phone)).FirstOrDefault<t_MobileToken>();
            t_MobileToken mtk = new t_MobileToken();
            if (model == null)
            {
                mtk.Token = Token;
                mtk.CreateTime = DateTime.Now;
                mtk.Phone = Phone;
                mtk.OpenId = thirdType == "0" && !string.IsNullOrEmpty(userThirdId) ? userThirdId : string.Empty;
                mtk.AliId = thirdType == "1" && !string.IsNullOrEmpty(userThirdId) ? userThirdId : string.Empty;
                mobileTokenService.Add(mtk);
            }
            else
            {
                model.Token = Token;
                model.UpdateTime = DateTime.Now;
                if (thirdType == "0" && !string.IsNullOrEmpty(userThirdId))
                {
                    model.OpenId = userThirdId;
                }
                if (thirdType == "1" && !string.IsNullOrEmpty(userThirdId))
                {
                    model.AliId = userThirdId;
                }
                mobileTokenService.Update(model);
            }
        }


        private static bool RemoveThirdId(string mobile,string thirdType)
        {
            IMobileTokenService mobileTokenService = BLLContainer.Resolve<IMobileTokenService>();
            var model = mobileTokenService.GetModels(p => p.Phone.Equals(mobile)).FirstOrDefault<t_MobileToken>();
            if (model != null)
            {
                if (thirdType == "0")
                {
                    model.OpenId = string.Empty;
                }
                else if (thirdType == "1")
                {
                    model.AliId = string.Empty;
                } 
                return mobileTokenService.Update(model);
            }
            return false;
        }

        public static bool UpdateAliBuyerId(string phone, string buyerId)
        {
            IMobileTokenService mobileTokenService = BLLContainer.Resolve<IMobileTokenService>();
            var model = mobileTokenService.GetModels(p => p.Phone.Equals(phone)).FirstOrDefault<t_MobileToken>();
            if (model != null)
            {
                model.AliId = buyerId;
                return mobileTokenService.Update(model);
            }
            return false;
        }

        public static bool GetAliId(string phone, out string aliId, out string errMsg)
        {
            aliId = string.Empty;
            errMsg = string.Empty;
            IMobileTokenService mobileTokenService = BLLContainer.Resolve<IMobileTokenService>();
            var model = mobileTokenService.GetModels(p => p.Phone.Equals(phone)).FirstOrDefault<t_MobileToken>();
            if (model != null)
            {
                aliId = model.AliId;
                if (string.IsNullOrEmpty(aliId))
                {
                    errMsg = "用户未绑定aliId";
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                errMsg = "用户未不存在";
                return false;
            }
        }

        public static bool IsHasMobile(string aliId, out string mobile)
        {
            mobile = string.Empty;

            IMobileTokenService mobileTokenService = BLLContainer.Resolve<IMobileTokenService>();
            var model = mobileTokenService.GetModels(p => p.AliId.Equals(aliId)).FirstOrDefault<t_MobileToken>();
            if (model != null && !string.IsNullOrEmpty(model.Phone))
            {
                mobile = model.Phone;
                return true;
            }
            return false;
        }

        public static void Clear()
        {
            MobileTokenCache.Clear();
        }
    }
}