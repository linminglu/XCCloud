using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudWebBar.Common;
using XCCloudWebBar.WeiXin.Common;
using XCCloudWebBar.Common.Extensions;
using XCCloudWebBar.Common.Redis;
using System.Reflection;
using System.Security.Cryptography;
using XCCloudWebBar.Model.CustomModel.XCGameManager;
using StackExchange.Redis;
using XCCloudWebBar.Common.Enum;
using XCCloudWebBar.WeiXin.WeixinOAuth;
using XCCloudWebBar.Business.WeiXin;
using XCCloudWebBar.Model.WeiXin;

namespace XCCloudWebBar.WeiXin.WeixinPub
{
    public class CommonHelper
    {
        /// <summary>
        /// AccessToken锁
        /// </summary>
        private static readonly Object AccessTokenLock = new Object();

        /// <summary>
        /// jsapi_ticket锁
        /// </summary>
        private static readonly Object ApiTicketLock = new Object();

        /// <summary>
        /// 微信H5JsSDK签名锁
        /// </summary>
        private static readonly Object SignatureLock = new Object();

        #region 获取微信公众号AccessToken
        /// <summary>
        /// 获取微信公众号AccessToken
        /// </summary>
        /// <returns></returns>
        public static string GetAccessToken()
        {
            string accessToken = string.Empty;
            TokenMana.GetAccessToken(out accessToken);
            return accessToken;

            #region MyRegion
            //string url = string.Format("https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={0}&secret={1}", WeiXinConfig.AppId, WeiXinConfig.AppSecret);

            //lock (AccessTokenLock)
            //{
            //    string token = string.Empty;
            //    try
            //    {
            //        RedisStackExchangeHelper redisHelper = new RedisStackExchangeHelper();
            //        //缓存中的当前Token
            //        RedisValue CurrToken = redisHelper.StringGet(CommonConfig.WxPubAccessToken);                    

            //        if (!CurrToken.HasValue)//如果Redis缓存中没有找到对应的值，就重新获取
            //        {
            //            //重新获取Token
            //            string res = Utils.WebClientDownloadString(url);
            //            JObject jo = JObject.Parse(res);

            //            if (jo["access_token"] != null)
            //            {
            //                token = jo["access_token"].ToString();
            //            }
            //            else
            //            {
            //                LogHelper.SaveLog(TxtLogType.Redis, TxtLogContentType.Exception, TxtLogFileType.Day, "获取微信公众号AccessToken失败，错误代码：" + jo["errcode"].ToString());
            //            }

            //            if (!token.IsNull())
            //            {
            //                //将新的Token保存到Redis，设置过期时间为115分钟
            //                redisHelper.StringSet(CommonConfig.WxPubAccessToken, token, TimeSpan.FromMinutes(115));
            //            }
            //        }
            //        else
            //        {
            //            token = CurrToken.ToString();
            //        }
            //    }
            //    catch(Exception ex)
            //    {
            //        LogHelper.SaveLog(TxtLogType.Redis, TxtLogContentType.Exception, TxtLogFileType.Day, "获取微信公众号AccessToken失败，原因：" + ex.InnerException != null ? ex.InnerException.Message : ex.Message);
            //    }
            //    return token;
            //} 
            #endregion
        } 
        #endregion

        #region 获取微信公众号jsapi_ticket
        /// <summary>
        /// 获取微信公众号jsapi_ticket
        /// </summary>
        /// <returns></returns>
        public static string GetJsapiTicket()
        {
            string accessToken = GetAccessToken();

            LogHelper.SaveLog(TxtLogType.WeiXin, accessToken);

            string ticket = GetTicket(accessToken);

            //LogHelper.SaveLog(TxtLogType.WeiXin, ticket);
            return ticket;

            #region MyRegion
            //TokenMana.GetAccessToken(out accessToken);
            //string url = string.Format("https://api.weixin.qq.com/cgi-bin/ticket/getticket?access_token={0}&type=jsapi", accessToken);

            //lock (ApiTicketLock)
            //{
            //    string ticket = string.Empty;
            //    try
            //    {
            //        RedisStackExchangeHelper redisHelper = new RedisStackExchangeHelper();
            //        //缓存中的当前Token
            //        RedisValue CurrTicket = redisHelper.StringGet(CommonConfig.WxPubApiTicket);                    

            //        if (!CurrTicket.HasValue)//如果Redis缓存中没有找到对应的值，就重新获取
            //        {
            //            //重新获取Token
            //            string res = Utils.WebClientDownloadString(url);

            //            JObject jo = JObject.Parse(res);
            //            string errCode = jo["errcode"].ToString();
            //            ticket = jo["ticket"] != null ? jo["ticket"].ToString() : string.Empty;

            //            if (errCode == "0" && !ticket.IsNull())
            //            {
            //                //将新的Token保存到Redis，设置过期时间为115分钟
            //                redisHelper.StringSet(CommonConfig.WxPubApiTicket, ticket, TimeSpan.FromMinutes(115));
            //            }
            //        }
            //        else
            //        {
            //            ticket = CurrTicket.ToString();
            //        }
            //    }
            //    catch(Exception ex)
            //    {
            //        LogHelper.SaveLog(TxtLogType.Redis, TxtLogContentType.Exception, TxtLogFileType.Day, "获取微信jsapi_ticket失败，原因：" + ex.InnerException != null ? ex.InnerException.Message : ex.Message);
            //    }
            //    return ticket;
            //} 
            #endregion
        } 
        #endregion

        #region 获取缓存中的ticket
        public static string GetTicket(string accessToken)
        {
            string ticket = string.Empty;
            if (WeiXinAccessTokenBusiness.GetJsapiTicket(out ticket))
            {
                //如果缓存中存在访问ticket,直接返回缓存的ticket
                return ticket;
            }
            else
            {
                //如果缓存中不存在访问ticket，调用微信接口获取，并写入缓存
                string url = string.Format("https://api.weixin.qq.com/cgi-bin/ticket/getticket?access_token={0}&type=jsapi", accessToken);
                lock (ApiTicketLock)
                {
                    //重新请求获取Token
                    string res = Utils.WebClientDownloadString(url);
                    JObject jo = JObject.Parse(res);
                    string errCode = jo["errcode"].ToString();
                    ticket = jo["ticket"] != null ? jo["ticket"].ToString() : string.Empty;

                    if (errCode == "0" && !ticket.IsNull())
                    {
                        int expires = Convert.ToInt32(jo["expires_in"]);
                        //将新的ticket保存到缓存
                        WeiXinAccessTokenBusiness.AddJsapiTicket(ticket, expires - 60);
                    }
                }
            }
            return ticket;
        }  
        #endregion

        #region 获取微信H5 JSSDK配置
        /// <summary>
        /// 获取微信H5 JSSDK配置
        /// </summary>
        /// <returns></returns>
        public static WxConfigModel GetSignature(string url)
        {
            lock (SignatureLock)
            {
                string ticket = GetJsapiTicket();
                if (string.IsNullOrWhiteSpace(ticket))
                {
                    return null;
                }

                GenerateTicketModel ticketData = new GenerateTicketModel();
                ticketData.noncestr = Guid.NewGuid().ToString("N");
                ticketData.timestamp = GenerateTimeStamp();
                ticketData.jsapi_ticket = ticket;
                ticketData.url = url;

                string signature = SignatureSHA1(ticketData);

                WxConfigModel config = new WxConfigModel();
                config.AppId = WeiXinConfig.AppId;
                config.TimeStamp = ticketData.timestamp;
                config.NonceStr = ticketData.noncestr;
                config.Signature = signature;

                return config;
            }
        }

        private class GenerateTicketModel
        {
            public string noncestr { get; set; }

            public string jsapi_ticket { get; set; }

            public string timestamp { get; set; }

            public string url { get; set; }
        }

        /// <summary>
        /// 生成时间戳，标准北京时间，时区为东八区，自1970年1月1日 0点0分0秒以来的秒数
        /// </summary>
        /// <returns></returns>
        private static string GenerateTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }
        #endregion

        #region 获取会员微信信息
        /// <summary>
        /// 获取会员微信信息
        /// </summary>
        /// <returns></returns>
        public static WechatInfo GetWechatInfo(string openId)
        {
            string accessToken = GetAccessToken();

            string url = string.Format("https://api.weixin.qq.com/cgi-bin/user/info?access_token={0}&openid={1}&lang=zh_CN", accessToken, openId);
            WechatInfo model = null;
            try
            {
                string res = Utils.WebClientDownloadString(url);
                JObject jo = JObject.Parse(res);

                if (jo["headimgurl"] != null)
                {
                    model = new WechatInfo();
                    model.subscribe = jo["subscribe"] != null ? Convert.ToInt32(jo["subscribe"].ToString()) : 0;
                    model.nickname = jo["nickname"].ToString();
                    model.headimgurl = jo["headimgurl"].ToString();
                }
                else
                {
                    LogHelper.SaveLog(TxtLogType.WeiXin, TxtLogContentType.Exception, TxtLogFileType.Day, "获取微信用户基本信息失败，openid：" + openId);
                }
            }
            catch (Exception ex)
            {
                LogHelper.SaveLog(TxtLogType.WeiXin, TxtLogContentType.Exception, TxtLogFileType.Day, "获取微信用户基本信息失败，原因：" + ex.InnerException != null ? ex.InnerException.Message : ex.Message);
            }
            return model;
        }
        #endregion

        #region 通过网页授权获取会员微信信息
        /// <summary>
        /// 通过网页授权获取会员微信信息
        /// </summary>
        /// <returns></returns>
        public static WechatInfo GetWechatInfo(string accessToken, string openId)
        {
            string url = string.Format("https://api.weixin.qq.com/sns/userinfo?access_token={0}&openid={1}&lang=zh_CN", accessToken, openId);
            WechatInfo model = null;
            try
            {
                string res = Utils.WebClientDownloadString(url);
                JObject jo = JObject.Parse(res);

                if (jo["errcode"] != null)
                {
                    LogHelper.SaveLog(TxtLogType.Redis, TxtLogContentType.Exception, TxtLogFileType.Day, "网页授权-获取微信用户基本信息失败，错误代码：" + jo["errcode"].ToString());
                }
                else
                {
                    model = new WechatInfo();
                    model.subscribe = jo["subscribe"] != null ? Convert.ToInt32(jo["subscribe"].ToString()) : 0;
                    model.nickname = jo["nickname"].ToString();
                    model.headimgurl = jo["headimgurl"].ToString();
                }
            }
            catch (Exception ex)
            {
                LogHelper.SaveLog(TxtLogType.WeiXin, TxtLogContentType.Exception, TxtLogFileType.Day, "网页授权-获取微信用户基本信息失败，原因：" + ex.InnerException != null ? ex.InnerException.Message : ex.Message);
            }
            return model;
        }
        #endregion

        #region sha1签名
        /// <summary>
        /// 基于Sha1签名
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static string SignatureSHA1(object o)
        {
            SortedDictionary<string, string> signDic = new SortedDictionary<string, string>();
            Type t = o.GetType();
            foreach (PropertyInfo pi in t.GetProperties())
            {
                if (pi.GetValue(o, null) != null)
                {
                    string v = pi.GetValue(o, null).ToString();
                    if (v != "")
                    {
                        signDic.Add(pi.Name, v);
                    }
                }
            }

            string signStr = "";
            foreach (string key in signDic.Keys)
            {
                signStr += key + "=" + signDic[key] + "&";
            }
            signStr = signStr.Trim('&');

            LogHelper.SaveLog(TxtLogType.WeiXin, signStr);

            string sign = EncryptToSHA1(signStr);

            return sign;
        }


        /// <summary>
        /// 基于Sha1的自定义加密字符串方法：输入一个字符串，返回一个由40个字符组成的十六进制的哈希散列（字符串）。
        /// </summary>
        /// <param name="str">要加密的字符串</param>
        /// <returns>加密后的十六进制的哈希散列（字符串）</returns>
        private static string EncryptToSHA1(string str)
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
    }
}
