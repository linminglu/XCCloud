using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudWebBar.CacheService
{
    public class WorkFlowCache
    {
        private static Hashtable _workFlowHt = new Hashtable();

        public static Hashtable WorkFlowHt
        {
            get { return _workFlowHt; }
        }

        public static void Clear()
        {
            _workFlowHt.Clear();
        }

        public static void Add<T>(string key, T t)
        {
            _workFlowHt.Add(key, t);
        }

        public static void Update<T>(string key, T t)
        {
            _workFlowHt[key] = t;
        }

        public static void Remove(string key)
        {
            _workFlowHt.Remove(key);
        }

        public static bool Exist(string key)
        {
            return _workFlowHt.ContainsKey(key);
        }        
    }
}
