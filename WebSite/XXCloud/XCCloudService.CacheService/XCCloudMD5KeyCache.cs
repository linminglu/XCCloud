using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudService.CacheService
{
    public class XCCloudMD5KeyCache
    {
        public const string xcCloudMD5KeyCache = "xcCloudMD5KeyCache";

        public static List<XCCloudMD5KeyCacheModel> XCCloudMD5KeyCacheList
        {
            get
            {
                List<XCCloudMD5KeyCacheModel> List = RedisCacheHelper.HashGetAll<XCCloudMD5KeyCacheModel>(xcCloudMD5KeyCache);
                return List;
            }
        }

        public static void Clear()
        {
            RedisCacheHelper.KeyDelete(xcCloudMD5KeyCache);
        }

        public static void AddToken(string token, string merchId, string storeId, string workStation)
        {
            XCCloudMD5KeyCacheModel model = new XCCloudMD5KeyCacheModel(token, merchId, storeId, workStation);
            RedisCacheHelper.HashSet<XCCloudMD5KeyCacheModel>(xcCloudMD5KeyCache, token, model);
        }

        public static void AddToken(string token, XCCloudMD5KeyCacheModel model)
        {
            RedisCacheHelper.HashSet<XCCloudMD5KeyCacheModel>(xcCloudMD5KeyCache, token, model);
        }

        public static bool ExistToken(string token)
        {
            bool isHave = RedisCacheHelper.HashExists(xcCloudMD5KeyCache, token);
            return isHave;
        }

        public static XCCloudMD5KeyCacheModel GetTokenModel(string token)
        {
            XCCloudMD5KeyCacheModel model = RedisCacheHelper.HashGet<XCCloudMD5KeyCacheModel>(xcCloudMD5KeyCache, token);
            return model;
        }

        public static void RemoveToken(string token)
        {
            RedisCacheHelper.HashDelete(xcCloudMD5KeyCache, token);
        }
    }

    public class XCCloudMD5KeyCacheModel
    {
        public XCCloudMD5KeyCacheModel(string token, string merchId, string storeId, string workStation)
        {
            this.Token = token;
            this.MerchId = merchId;
            this.StoreId = storeId;
            this.WorkStation = workStation;
        }

        public string Token { set; get; }

        public string MerchId { set; get; }

        public string StoreId { set; get; }

        public string WorkStation { set; get; }
    }
}
