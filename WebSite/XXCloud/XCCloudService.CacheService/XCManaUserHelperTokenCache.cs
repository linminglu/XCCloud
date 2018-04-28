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
        private static Dictionary<string, object> _userTokenDic = new Dictionary<string, object>();

        public static Dictionary<string, object> UserTokenHTDic
        {
            get { return _userTokenDic; }
        }

        public static void AddToken(string key, object obj)
        {
            _userTokenDic[key] = obj;
        }

        public static bool ExistToken(string key)
        {
            return _userTokenDic.ContainsKey(key);
        }

        public static void Remove(string key)
        {
            _userTokenDic.Remove(key);
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

        public XCManaUserHelperTokenModel(string storeId, string storeName, string mobile, int userId)
        {
            this.StoreId = storeId;
            this.StoreName = storeName;
            this.Mobile = mobile;
            this.UserId = userId;
        }

        public string StoreId { set; get; }

        public string StoreName { set; get; }

        public string Mobile { set; get; }

        public int UserId { set; get; }
    }
}