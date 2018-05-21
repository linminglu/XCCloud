using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XCCloudService.Model.CustomModel.XCCloud;

namespace XCCloudService.CacheService
{
    public class XCCloudUserTokenCache
    {
        public const string XCCloudUserTokenCacheKey = "redisXCCloudUserTokenCacheKey";
        private static List<XCCloudUserTokenModel> _userTokenList;

        public static List<XCCloudUserTokenModel> UserTokenList
        {
            get
            {
                if (_userTokenList == null)
                {
                    _userTokenList = RedisCacheHelper.HashGetAll<XCCloudUserTokenModel>(XCCloudUserTokenCacheKey);
                }
                return _userTokenList;
            }
        }

        public static void AddToken(string key, XCCloudUserTokenModel model)
        {
            RedisCacheHelper.HashSet<XCCloudUserTokenModel>(XCCloudUserTokenCacheKey, key, model);
            SetKeyExpire(XCCloudUserTokenCacheKey);
            UserTokenList.Add(model);
        }

        public static XCCloudUserTokenModel GetModel(string token)
        {
            if (ExistToken(token))
            {
                XCCloudUserTokenModel model = UserTokenList.FirstOrDefault(t => t.Token == token);
                if (model != null)
                {
                    SetKeyExpire(XCCloudUserTokenCacheKey);
                }
                return model;
            }
            else
            {
                Remove(token);
                return null;
            }
        }

        /// <summary>
        /// 设置过期时间
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool SetKeyExpire(string key)
        {
            return RedisCacheHelper.KeyExpire(key, new TimeSpan(0, 0, 0, CacheExpires.CommonPageQueryDataCacheTime));
        }

        public static bool ExistToken(string key)
        {
            bool isHave = RedisCacheHelper.HashExists(XCCloudUserTokenCacheKey, key);
            return isHave;
        }

        public static void Remove(string key)
        {
            RedisCacheHelper.HashDelete(XCCloudUserTokenCacheKey, key);

            XCCloudUserTokenModel model = UserTokenList.FirstOrDefault(t => t.Token == key);
            if (model != null)
            {
                UserTokenList.Remove(model);
            }
        }
    }
}