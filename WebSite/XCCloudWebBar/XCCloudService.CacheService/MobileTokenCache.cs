using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XCCloudWebBar.Common;

namespace XCCloudWebBar.CacheService
{
    public class MobileTokenCache
    {
        public const string mobileTokenCacheKey = "redisMobileTokenCacheKey";

        public static List<MobileTokenModel> MobileTokenList
        {
            get {
                List<MobileTokenModel> List = RedisCacheHelper.HashGetAll<MobileTokenModel>(mobileTokenCacheKey);
                return List;
            }            
        }

        public static void Clear()
        {
            RedisCacheHelper.KeyDelete(mobileTokenCacheKey);  
        }

        public static void AddToken(string mobile,string token)
        {
            MobileTokenModel model = new MobileTokenModel(mobile);
            model.Token = token;
            RedisCacheHelper.HashSet<MobileTokenModel>(mobileTokenCacheKey, token, model);
        }

        public static void AddToken(string token, string mobile, string thirdType, string userThirdId)
        {
            var model = new MobileTokenModel(mobile);
            model.Token = token;
            model.WeiXinId = thirdType == "0" && !string.IsNullOrEmpty(userThirdId) ? userThirdId : string.Empty;
            model.AliId = thirdType == "1" && !string.IsNullOrEmpty(userThirdId) ? userThirdId : string.Empty;

            AddToken(token, model);
        }

        public static void AddToken(string token, MobileTokenModel model)
        {
            RedisCacheHelper.HashSet<MobileTokenModel>(mobileTokenCacheKey, token, model);
        }

        public static bool ExistToken(string token)
        {
            bool isHave = RedisCacheHelper.HashExists(mobileTokenCacheKey, token);
            return isHave;
        }

        public static MobileTokenModel GetMobileTokenModel(string token)
        {
            MobileTokenModel model = RedisCacheHelper.HashGet<MobileTokenModel>(mobileTokenCacheKey, token);
            return model;
        }

        public static void RemoveToken(string token)
        {
            RedisCacheHelper.HashDelete(mobileTokenCacheKey, token);
        }
    }


    public class MobileTokenModel
    {
        public MobileTokenModel()
        { 
            
        }

        public MobileTokenModel(string mobile)
        {
            this.Mobile = mobile;
            this.WeiXinId = string.Empty;
            this.AliId = string.Empty;
        }



        public MobileTokenModel(string mobile, string weixinId, string aliId)
        {
            this.Mobile = mobile;
            this.WeiXinId = weixinId;
            this.AliId = aliId;
        }

        public string Token { get; set; }
        public string Mobile { set; get; }

        public string WeiXinId { set; get; }

        public string AliId { set; get; }
    }
}