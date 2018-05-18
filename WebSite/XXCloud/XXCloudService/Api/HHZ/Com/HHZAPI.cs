using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace XXCloudService.Api.HHZ.Com
{
    public class HHZAPI
    {
        private const string Key = "XSJeGbz92KqYVNai";

        #region 签名
        /// <summary>
        /// 签名
        /// </summary>
        /// <param name="queryParams"></param>
        /// <returns></returns>
        public string GetSign(SortedDictionary<string, string> queryParams)
        {
            string signStr = "";
            foreach (string key in queryParams.Keys)
            {
                if (!string.IsNullOrEmpty(queryParams[key]) && queryParams[key] != "sign")
                {
                    signStr += key + "=" + queryParams[key] + "&";
                }
            }

            signStr += "key=" + Key;
            string sign = EncryptToSHA1(signStr);
            return sign;
        }

        /// <summary>
        /// 基于Sha1的自定义加密字符串方法：输入一个字符串，返回一个由40个字符组成的十六进制的哈希散列（字符串）。
        /// </summary>
        /// <param name="str">要加密的字符串</param>
        /// <returns>加密后的十六进制的哈希散列（字符串）</returns>
        public string EncryptToSHA1(string str)
        {
            var buffer = Encoding.UTF8.GetBytes(str);
            var data = SHA1.Create().ComputeHash(buffer);

            var sb = new StringBuilder();
            foreach (var t in data)
            {
                sb.Append(t.ToString("x2"));
            }

            return sb.ToString();
        }
        #endregion

        #region 获取哈哈转POST过来通知消息，并以“参数名=参数值”的形式组成数组
        /// <summary>
        /// 获取哈哈转POST过来通知消息，并以“参数名=参数值”的形式组成数组
        /// </summary>
        /// <returns>request回来的信息组成的数组</returns>
        public SortedDictionary<string, string> GetRequestPost(HttpContext ctx)
        {
            int i = 0;
            SortedDictionary<string, string> sArray = new SortedDictionary<string, string>();
            NameValueCollection coll;
            //Load Form variables into NameValueCollection variable.
            coll = ctx.Request.Form;

            // Get names of all forms into a string array.
            String[] requestItem = coll.AllKeys;

            for (i = 0; i < requestItem.Length; i++)
            {
                sArray.Add(requestItem[i], ctx.Request.Form[requestItem[i]]);
            }

            return sArray;
        }
        #endregion
    }
}