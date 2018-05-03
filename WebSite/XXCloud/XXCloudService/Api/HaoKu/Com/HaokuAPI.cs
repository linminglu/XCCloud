using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;

namespace XXCloudService.Api.HaoKu.Com
{
    public class HaokuAPI
    {
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
                if (!string.IsNullOrEmpty(queryParams[key]))
                {
                    signStr += key + "=" + queryParams[key] + "&";
                }
            }

            signStr += "key=" + AES.Key;

            var md5 = MD5.Create();
            var bs = md5.ComputeHash(Encoding.UTF8.GetBytes(signStr));
            var sb = new StringBuilder();
            foreach (byte b in bs)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        } 
        #endregion

        #region 拼接请求参数
        /// <summary>
        /// 拼接请求参数
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        string GetQueryString(object o)
        {
            string queryString = string.Empty;
            Type t = o.GetType();
            foreach (PropertyInfo pi in t.GetProperties())
            {
                if (pi.GetValue(o, null) != null)
                {
                    string v = pi.GetValue(o, null).ToString();
                    if (v != "")
                    {
                        queryString += pi.Name + "=" + v + "&";
                    }
                }
            }

            queryString = queryString.Trim('&');
            return queryString;
        } 
        #endregion

        #region 发送请求
        /// <summary>
        /// 发送请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="data">请求数据</param>
        /// <param name="timeout">超时时间(秒)</param>
        /// <returns></returns>
        public string Get(string url, object data, int timeout)
        {
            System.GC.Collect();//垃圾回收，回收没有正常关闭的http连接

            string result = "";//返回结果

            HttpWebRequest request = null;
            HttpWebResponse response = null;
            Stream resStream = null;

            try
            {
                string queryString = GetQueryString(data);
                url = url + "?" + queryString;

                request = (HttpWebRequest)WebRequest.Create(url);

                //request.ContentType = "application/x-www-form-urlencoded";
                request.Method = "GET";
                //对发送的数据不使用缓存
                request.AllowWriteStreamBuffering = false;
                request.Timeout = timeout * 1000;
                //request.ServicePoint.Expect100Continue = false;

                //获取服务端返回
                response = (HttpWebResponse)request.GetResponse();

                resStream = response.GetResponseStream();
                if (resStream == null)
                {
                    return "网络错误(Network error)：" + new ArgumentNullException("resStream");
                }
                StreamReader streamReader = new StreamReader(resStream, Encoding.UTF8);

                result = streamReader.ReadToEnd().Trim();

                streamReader.Close();
            }
            catch (System.Threading.ThreadAbortException e)
            {
                Thread.ResetAbort();
            }
            catch (WebException ex)
            {
                return "网络错误(Network error)：" + ex.Message;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            finally
            {
                //关闭连接和流
                if (response != null)
                {
                    response.Close();
                }
                if (request != null)
                {
                    request.Abort();
                }
                if(resStream != null)
                {
                    resStream.Close();
                }
            }
            return result;
        } 
        #endregion

        #region 获取好酷GET过来的请求消息，并以“参数名=参数值”的形式组成数组
        /// <summary>
        /// 获取好酷GET过来的请求消息，并以“参数名=参数值”的形式组成数组
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public SortedDictionary<string, string> GetRequestQuerys(HttpContext ctx)
        {
            int i = 0;
            SortedDictionary<string, string> sArray = new SortedDictionary<string, string>();
            NameValueCollection coll;
            //Load Form variables into NameValueCollection variable.
            coll = ctx.Request.QueryString;

            // Get names of all forms into a string array.
            String[] requestItem = coll.AllKeys;

            for (i = 0; i < requestItem.Length; i++)
            {
                sArray.Add(requestItem[i], ctx.Request.QueryString[requestItem[i]]);
            }

            return sArray;
        } 
        #endregion
    }
}