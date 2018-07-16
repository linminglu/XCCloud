using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Server
{
    class JsonObject
    {
        public class StoreRegistRequest
        {
            public string MerchID { get; set; }
            public string StoreID { get; set; }
            public string SignKey { get; set; }
        }
        public class StoreRegistResponse
        {
            public string Token { get; set; }
            public string SignKey { get; set; }
        }
        public class StoreTick
        {
            public string StoreID { get; set; }
            public string Token { get; set; }
            public string SignKey { get; set; }
        }
        public class DataSyncRequest
        {
            public string StoreID { get; set; }
            public string Token { get; set; }
            public string IdValue { get; set; }
            public string TableName { get; set; }
            public string JsonText { get; set; }
            public int Action { get; set; }
            public string SN { get; set; }
            public string SignKey { get; set; }
        }
        public class DataSyncResponse
        {
            public string StoreID { get; set; }
            public string Token { get; set; }
            public string SN { get; set; }
            public string SignKey { get; set; }
        }
        /// <summary>
        /// 获取参数排序
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        SortedDictionary<string, string> GetObjectSort(object o)
        {
            SortedDictionary<string, string> dict = new SortedDictionary<string, string>();

            Type t = o.GetType();
            foreach (PropertyInfo pi in t.GetProperties())
            {
                string v = pi.GetValue(o, null) == null ? "" : pi.GetValue(o, null).ToString();
                if (v != "" && pi.Name.ToLower() != "signkey" && pi.PropertyType.Name == "String")
                    dict.Add(pi.Name, v);
            }
            return dict;
        }
        /// <summary>
        /// 获取数字签名
        /// </summary>
        /// <param name="o"></param>
        /// <param name="Pwd"></param>
        /// <returns></returns>
        public string GetSignKey(object o, string Pwd)
        {
            SortedDictionary<string, string> dict = GetObjectSort(o);
            StringBuilder parames = new StringBuilder();
            foreach (string key in dict.Keys)
            {
                parames.Append(dict[key]);
            }
            parames.Append(Pwd);

            var md5 = MD5.Create();
            var bs = md5.ComputeHash(Encoding.UTF8.GetBytes(parames.ToString()));
            var sb = new StringBuilder();
            foreach (byte b in bs)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
