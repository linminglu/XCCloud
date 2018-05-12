using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XCCloudService.Common;

namespace XCCloudService.CacheService
{
    public class XCManaUserHelperTokenCache
    {
        public const string XCManaUserTokenCacheKey = "redisXCManaUserTokenCacheKey";

        public static List<XCManaUserHelperTokenModel> UserTokenList
        {
            get {
                List<XCManaUserHelperTokenModel> List = RedisCacheHelper.HashGetAll<XCManaUserHelperTokenModel>(XCManaUserTokenCacheKey);
                return List;
            }  
        }

        public static void AddToken(string key, XCManaUserHelperTokenModel model)
        {
            RedisCacheHelper.HashSet<XCManaUserHelperTokenModel>(XCManaUserTokenCacheKey, key, model);
        }

        public static XCManaUserHelperTokenModel GetModel(string token)
        {
            XCManaUserHelperTokenModel model = RedisCacheHelper.HashGet<XCManaUserHelperTokenModel>(XCManaUserTokenCacheKey, token);
            return model;
        }

        public static void Clear()
        {
            RedisCacheHelper.KeyDelete(XCManaUserTokenCacheKey);  
        }

        public static bool ExistToken(string key)
        {
            return RedisCacheHelper.HashExists(XCManaUserTokenCacheKey, key);
        }

        public static void Remove(string key)
        {
            RedisCacheHelper.HashDelete(XCManaUserTokenCacheKey, key);
        }
    }


    public class XCManaUserHelperTokenResultModel
    {
        public XCManaUserHelperTokenResultModel(string storeId, string storeName, string userToken)
        {
            this.StoreId = storeId;
            this.StoreName = storeName;
            this.UserToken = userToken;
        }

        public string StoreId { set; get; }

        public string StoreName { set; get; }

        public string UserToken { set; get; }
    }

    public class XCManaUserHelperTokenModel
    {
        public XCManaUserHelperTokenModel()
        { 

        }

        public XCManaUserHelperTokenModel(string token, string storeId, string storeName, string mobile, int userId)
        {
            this.Token = token;
            this.StoreId = storeId;
            this.StoreName = storeName;
            this.Mobile = mobile;
            this.UserId = userId;
        }

        public string Token { get; set; }
        public string StoreId { set; get; }

        public string StoreName { set; get; }

        public string Mobile { set; get; }

        public int UserId { set; get; }
    }
}