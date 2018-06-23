using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XCCloudService.Common;

namespace XCCloudService.CacheService
{
    public class XCManaDogTokenCache
    {
        public const string XCManaDogTokenCacheKey = "redisXCManaDogTokenCacheKey";

        public static List<XCManaDogTokenModel> DogTokenList
        {
            get {
                List<XCManaDogTokenModel> List = RedisCacheHelper.HashGetAll<XCManaDogTokenModel>(XCManaDogTokenCacheKey);
                return List;
            }  
        }

        public static void AddToken(string key, XCManaDogTokenModel model)
        {
            RedisCacheHelper.HashSet<XCManaDogTokenModel>(XCManaDogTokenCacheKey, key, model);
        }

        public static XCManaDogTokenModel GetModel(string token)
        {
            XCManaDogTokenModel model = RedisCacheHelper.HashGet<XCManaDogTokenModel>(XCManaDogTokenCacheKey, token);
            return model;
        }

        public static void Clear()
        {
            RedisCacheHelper.KeyDelete(XCManaDogTokenCacheKey);  
        }

        public static bool ExistToken(string key)
        {
            return RedisCacheHelper.HashExists(XCManaDogTokenCacheKey, key);
        }

        public static void Remove(string key)
        {
            RedisCacheHelper.HashDelete(XCManaDogTokenCacheKey, key);
        }
    }

    public class XCManaDogTokenModel
    {
        public XCManaDogTokenModel()
        { 

        }

        public XCManaDogTokenModel(string dogId, string storeId, string token)
        {
            this.DogId = dogId;
            this.StoreId = storeId;
            this.Token = token;
        }

        public string DogId { get; set; }
        public string StoreId { set; get; }
        public string Token { set; get; }        
    }
}