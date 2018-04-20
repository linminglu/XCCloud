using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XCCloudService.Common;

namespace XCCloudService.CacheService
{
    public class MobileTokenCache
    {
        private static Hashtable _mobileTokenHt = new Hashtable();

        public static Hashtable MobileTokenHt
        {
            get { return _mobileTokenHt; }
        }

        public static void AddToken(string mobile,string token)
        {
            _mobileTokenHt[token] = new MobileTokenModel(mobile);
        }

        public static void AddToken(string token,MobileTokenModel model)
        {
            _mobileTokenHt[token] = model;
        }

        public static bool ExistToken(string token)
        {
            if (_mobileTokenHt.ContainsKey(token))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void RemoveToken(string token)
        {
            _mobileTokenHt.Remove(token);
        }

        public static bool ExistTokenByKey(string key)
        {
            return _mobileTokenHt.ContainsKey(key);
        }

        //public static bool ExistTokenByKey(string token)
        //{
        //    return _mobileTokenHt.ContainsKey(token);
        //}

        public static void UpdateTokenByKey(string key, string value)
        {
            _mobileTokenHt[key] = value;
        }

        public static string GetKeyByValue(string value)
        {
            var query = from item in _mobileTokenHt.Cast<DictionaryEntry>() 
            where item.Value.ToString().Equals(value) 
            select item.Key.ToString();
            if (query.Count() == 0)
            {
                return null;
            }
            else
            {
                return query.First(); 
            }
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

        public MobileTokenModel(string mobile,string weixinId,string aliId)
        {
            this.Mobile = mobile;
            this.WeiXinId = weixinId;
            this.AliId = aliId;
        }

        public string Mobile { set; get; }

        public string WeiXinId { set; get; }

        public string AliId { set; get; }
    }
}