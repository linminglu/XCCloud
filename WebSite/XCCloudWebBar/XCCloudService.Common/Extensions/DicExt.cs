using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudWebBar.Common.Extensions
{
    /// <summary>
    /// 字典扩展类
    /// </summary>
    public static class DicExt
    {
        /// <summary>
        /// 返回键值
        /// </summary>
        /// <param name="enumSubitem"></param>
        /// <returns></returns>
        public static string Get(this IDictionary<string, object> dicPara, string key)
        {
            if (dicPara == null || string.IsNullOrEmpty(key)) return string.Empty;
            object o = null;
            dicPara.TryGetValue(key, out o);
            return Convert.ToString(o);
        }

        public static object[] GetArray(this IDictionary<string, object> dicPara, string key)
        {
            if (dicPara == null || string.IsNullOrEmpty(key)) return null;
            object o = null;
            dicPara.TryGetValue(key, out o);
            return !o.IsNull() ? (object[])o : null;
        }

        public static object GetObject(this IDictionary<string, object> dicPara, string key)
        {
            if (dicPara == null || string.IsNullOrEmpty(key)) return null;
            object o = null;
            dicPara.TryGetValue(key, out o);
            return o;
        }

        public static void AddRangeOverride<TKey, TValue>(this IDictionary<TKey, TValue> dic, IDictionary<TKey, TValue> dicToAdd)
        {
            dicToAdd.ForEach(x => dic[x.Key] = x.Value);
        }

        public static void AddRangeNewOnly<TKey, TValue>(this IDictionary<TKey, TValue> dic, IDictionary<TKey, TValue> dicToAdd)
        {
            dicToAdd.ForEach(x => { if (!dic.ContainsKey(x.Key)) dic.Add(x.Key, x.Value); });
        }

        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dic, IDictionary<TKey, TValue> dicToAdd)
        {
            dicToAdd.ForEach(x => dic.Add(x.Key, x.Value));
        }

        public static bool ContainsKeys<TKey, TValue>(this IDictionary<TKey, TValue> dic, IEnumerable<TKey> keys)
        {
            bool result = false;
            keys.ForEachOrBreak((x) => { result = dic.ContainsKey(x); return result; });
            return result;
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
                action(item);
        }

        public static void ForEachOrBreak<T>(this IEnumerable<T> source, Func<T, bool> func)
        {
            foreach (var item in source)
            {
                bool result = func(item);
                if (result) break;
            }
        }
    }
}
