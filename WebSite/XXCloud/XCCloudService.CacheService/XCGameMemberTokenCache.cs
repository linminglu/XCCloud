using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XCCloudService.Model.CustomModel.XCGame;

namespace XCCloudService.CacheService
{
    public class XCGameMemberTokenCache
    {
        public const string xcGameMemberTokenCacheKey = "redisXCGameMemberTokenCacheKey";

        public static List<XCGameMemberTokenModel> MemberTokenModelList
        {
            get {
                List<XCGameMemberTokenModel> List = RedisCacheHelper.HashGetAll<XCGameMemberTokenModel>(xcGameMemberTokenCacheKey);
                return List;
            }
        }

        public static void AddToken(string key, XCGameMemberTokenModel model)
        {
            RedisCacheHelper.HashSet<XCGameMemberTokenModel>(xcGameMemberTokenCacheKey, key, model);
        }


        public static bool ExistToken(string key)
        {
            bool isHave = RedisCacheHelper.HashExists(xcGameMemberTokenCacheKey, key);
            return isHave;
        }

        public static XCGameMemberTokenModel GetModel(string token)
        {
            XCGameMemberTokenModel model = RedisCacheHelper.HashGet<XCGameMemberTokenModel>(xcGameMemberTokenCacheKey, token);
            return model;
        }

        public static void Remove(string key)
        {
            RedisCacheHelper.HashDelete(xcGameMemberTokenCacheKey, key);
        }

        public static void Clear()
        {
            RedisCacheHelper.KeyDelete(xcGameMemberTokenCacheKey);  
        }
    }
}