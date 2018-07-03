using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.Model.CustomModel.XCCloud;

namespace XCCloudService.CacheService
{
    public class DeviceWorkInfoCache
    {
        private static Hashtable _deviceHt = new Hashtable();

        public static void AddObject(string key, DeviceWorkInfoModel model)
        {
            _deviceHt[key] = model;
        }

        public static bool ExistTokenByKey(string key)
        {
            return _deviceHt.ContainsKey(key);
        }

        public static object GetValueByKey(string key)
        {
            return _deviceHt[key];
        }

        public static Hashtable DeviceHt
        {
            set { _deviceHt = value; }
            get { return _deviceHt; }
        }

        public static string GetKeyByValue(string value)
        {
            var query = from item in _deviceHt.Cast<DictionaryEntry>()
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
}
