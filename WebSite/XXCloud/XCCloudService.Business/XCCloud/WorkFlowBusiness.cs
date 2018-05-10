using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.Common;
using XCCloudService.CacheService;
using System.Collections;

namespace XCCloudService.Business.XCCloud
{
    public class WorkFlowBusiness
    {     
        public static void Set<T>(string id, T t)
        {
            string key = Constant.Workflow + "_" + id;
            if (!WorkFlowCache.Exist(key))
            {
                WorkFlowCache.Add<T>(key, t);
            }
            else
            {
                WorkFlowCache.Update<T>(key, t);
            }
        }
        
        public static bool Get<T>(string id, out T t)
        {
            t = default(T);
            string key = Constant.Workflow + "_" + id;
            var query = from item in WorkFlowCache.WorkFlowHt.Cast<DictionaryEntry>()
                        where item.Key.ToString().Equals(key)
                        select item.Value;
            if (query.Count() == 0)
            {
                return false;
            }
            else
            {
                t = (T)query.First();
                return true;
            }
        }

        public static void Remove(string id)
        {
            string key = Constant.Workflow + "_" + id;
            WorkFlowCache.Remove(key);
        }

        public static void Clear()
        {
            WorkFlowCache.Clear();
        }
    }
}
