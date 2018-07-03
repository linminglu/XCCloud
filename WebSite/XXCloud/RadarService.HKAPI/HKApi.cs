using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Web.Script.Serialization;
using System.Web;
using System.Net;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace RadarService.HKAPI
{
    public class HKApi
    {
        public static int StoreID = 0;
        public static string VerifyIDGatewayURL = "http://api.mcs.gzhaoku.com/verifyapi/verify";
        public static string SSLCERT_PATH = "";
        public static string SSLCERT_PASSWORD = "";

        public static string GateWayURL = "http://api.mcs.gzhaoku.com";
        //public static string GateWayURL = "http://api.mcs.uwan99.com";

        //public static string Secret = "6340225678905473";

        //public static string CallerSecret = "3567523162";
        //public static string StoreSecret = "887117";
        //public static string Caller = "xinchen";
        //public static string ShopID = "1";

        public static string CallerSecret = "3567523162";
        public static string StoreSecret = "193484";
        public static string Caller = "xinchen";
        public static string ShopID = "906";

        static int PrizeSN = 0;
        public static string GetPrizeSN
        {
            get
            {
                return (PrizeSN++).ToString();
            }
        }

        public HKApi()
        {
            AES.Key = CallerSecret + StoreSecret;
        }

        public HKApi(string caller, string store)
        {
            CallerSecret = caller;
            StoreSecret = store;
            AES.Key = CallerSecret + StoreSecret;
        }

        bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            //直接确认，否则打不开    
            return true;
        }

        string Post(string str, string url, bool isUseCert, int timeout)
        {
            System.GC.Collect();//垃圾回收，回收没有正常关闭的http连接

            string result = "";//返回结果

            HttpWebRequest request = null;
            HttpWebResponse response = null;
            Stream reqStream = null;

            try
            {
                //设置最大连接数
                ServicePointManager.DefaultConnectionLimit = 200;
                //设置https验证方式
                if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                {
                    ServicePointManager.ServerCertificateValidationCallback =
                            new RemoteCertificateValidationCallback(CheckValidationResult);
                }

                /***************************************************************
                * 下面设置HttpWebRequest的相关属性
                * ************************************************************/
                request = (HttpWebRequest)WebRequest.Create(url);

                request.Method = "POST";
                request.Timeout = timeout * 1000;

                //设置代理服务器
                //WebProxy proxy = new WebProxy();                          //定义一个网关对象
                //proxy.Address = new Uri(WxPayConfig.PROXY_URL);              //网关服务器端口:端口
                //request.Proxy = proxy;

                //设置POST的数据类型和长度
                request.ContentType = "application/json";
                byte[] data = System.Text.Encoding.UTF8.GetBytes(str);
                request.ContentLength = data.Length;

                //是否使用证书
                if (isUseCert)
                {
                    string path = HttpContext.Current.Request.PhysicalApplicationPath;
                    X509Certificate2 cert = new X509Certificate2(path + SSLCERT_PATH, SSLCERT_PASSWORD);
                    request.ClientCertificates.Add(cert);
                }

                //往服务器写入数据
                reqStream = request.GetRequestStream();
                reqStream.Write(data, 0, data.Length);
                reqStream.Close();

                //获取服务端返回
                response = (HttpWebResponse)request.GetResponse();

                //获取服务端返回数据
                StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                result = sr.ReadToEnd().Trim();
                sr.Close();
            }
            catch (System.Threading.ThreadAbortException e)
            {
                Thread.ResetAbort();
            }
            catch (WebException e)
            {
                Console.WriteLine(e);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
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
            }
            return result;
        }
        string Get(string url)
        {
            try
            {
                HttpWebRequest webrequest = (HttpWebRequest)HttpWebRequest.Create(url);
                webrequest.Method = "GET";
                HttpWebResponse webreponse = (HttpWebResponse)webrequest.GetResponse();
                Stream stream = webreponse.GetResponseStream();
                string resp = string.Empty;
                using (StreamReader reader = new StreamReader(stream))
                {
                    resp = reader.ReadToEnd();
                }
                return resp;
            }
            catch (Exception ex)
            {
                LogHelper.LogHelper.WriteLog(ex);
                LogHelper.LogHelper.WriteLog(url);
                LogHelper.LogHelper.WriteLog(ex.Message);
                return ex.Message;
            }
        }

        string GetObjectJSON(object o)
        {
            string s = "";
            Type t = o.GetType();
            s += "{";
            foreach (PropertyInfo pi in t.GetProperties())
            {
                s += string.Format("\"{0}\":\"{1}\",", pi.Name, pi.GetValue(o, null));
            }
            s = s.Substring(0, s.Length - 1);
            s += "}";
            return s;
        }
        /// <summary>
        /// 实名认证
        /// </summary>
        /// <param name="user"></param>
        /// <param name="Phone"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool VerifyID(HKData.实名认证结构 user, out string msg)
        {
            msg = "";
            string url = string.Format(GateWayURL + "/verifyapi/verify?shopId={0}&realName={1}&idCard={2}&phone={3}&caller={4}", user.shopId, AES.AESEncrypt(user.realName), AES.AESEncrypt(user.idCard), AES.AESEncrypt(user.phone), Caller);
            string response = Get(url);
            Console.WriteLine(response);

            JavaScriptSerializer jsonSerialize = new JavaScriptSerializer();
            HKData.实名认证应答 ack = jsonSerialize.Deserialize<HKData.实名认证应答>(response);
            if (ack.statusCode == "2000000")
            {
                if (ack.data.code == "110")
                    return true;
                else
                {
                    msg = ack.data.description;
                    return false;
                }
            }
            else
            {
                msg = ack.statusMsg;
                return false;
            }
        }

        public bool CreateCard(HKData.开卡请求 card, out string msg)
        {
            msg = "";
            string url = string.Format(GateWayURL + "/accountapi/bindcard?shopId={0}&cardId={1}&cardName={5}&realName={2}&idCard={3}&phone={4}&caller={6}", card.shopId, AES.AESEncrypt(card.cardId), AES.AESEncrypt(card.realName), AES.AESEncrypt(card.idCard), AES.AESEncrypt(card.phone), AES.AESEncrypt(card.cardName), Caller);
            string response = Get(url);
            Console.WriteLine(response);

            JavaScriptSerializer jsonSerialize = new JavaScriptSerializer();
            try
            {
                HKData.实名认证应答 ack = jsonSerialize.Deserialize<HKData.实名认证应答>(response);
                if (ack.statusCode == "2000000")
                {
                    if (ack.data.code == "100")
                        return true;
                    else
                    {
                        msg = ack.data.description;
                        return false;
                    }
                }
                else
                {
                    msg = ack.statusMsg;
                    return false;
                }
            }
            catch (Exception ex)
            {
                msg = "JSON解析错误：" + response;
                LogHelper.LogHelper.WriteLog(ex);
                LogHelper.LogHelper.WriteLog(msg);
                return false;
            }
        }

        public bool CancelCard(HKData.注销卡请求 card, out string msg)
        {
            msg = "";
            string url = string.Format(GateWayURL + "/accountapi/cancelcard?shopId={0}&cardId={1}&caller={2}", card.shopId, AES.AESEncrypt(card.cardId), Caller);
            string response = Get(url);
            Console.WriteLine(response);

            try
            {
                JavaScriptSerializer jsonSerialize = new JavaScriptSerializer();
                HKData.实名认证应答 ack = jsonSerialize.Deserialize<HKData.实名认证应答>(response);
                if (ack.statusCode == "2000000")
                {
                    if (ack.data.code == "130")
                        return true;
                    else
                    {
                        msg = ack.data.description;
                        return false;
                    }
                }
                else
                {
                    msg = ack.statusMsg;
                    return false;
                }
            }
            catch (Exception ex)
            {
                msg = "JSON解析错误：" + response;
                LogHelper.LogHelper.WriteLog(ex);
                LogHelper.LogHelper.WriteLog(msg);
                return false;
            }
        }

        public bool CheckCard(HKData.注销卡请求 card, out string msg)
        {
            msg = "";
            string url = string.Format(GateWayURL + "/accountapi/cancharge?shopId={0}&cardId={1}&caller={2}", card.shopId, AES.AESEncrypt(card.cardId), Caller);
            string response = Get(url);
            Console.WriteLine(response);

            try
            {
                JavaScriptSerializer jsonSerialize = new JavaScriptSerializer();
                HKData.实名认证应答 ack = jsonSerialize.Deserialize<HKData.实名认证应答>(response);
                if (ack.statusCode == "2000000")
                {
                    if (ack.data.code == "120")
                        return true;
                    else
                    {
                        msg = ack.data.description;
                        return false;
                    }
                }
                else
                {
                    msg = ack.statusMsg;
                    return false;
                }
            }
            catch (Exception ex)
            {
                msg = "JSON解析错误：" + response;
                LogHelper.LogHelper.WriteLog(ex);
                LogHelper.LogHelper.WriteLog(msg);
                return false;
            }
        }

        public bool ChargeCard(HKData.充值通知 card, out string msg)
        {
            msg = "";
            string url = string.Format(GateWayURL + "/accountapi/chargelog?shopId={0}&cardId={1}&amount={2}&caller={3}", card.shopId, AES.AESEncrypt(card.cardId), AES.AESEncrypt(card.amount), Caller);
            string response = Get(url);
            Console.WriteLine(response);

            try
            {
                JavaScriptSerializer jsonSerialize = new JavaScriptSerializer();
                HKData.实名认证应答 ack = jsonSerialize.Deserialize<HKData.实名认证应答>(response);
                if (ack.statusCode == "2000000")
                {
                    if (ack.data.code == "122")
                        return true;
                    else
                    {
                        msg = ack.data.description;
                        return false;
                    }
                }
                else
                {
                    msg = ack.statusMsg;
                    return false;
                }
            }
            catch (Exception ex)
            {
                msg = "JSON解析错误：" + response;
                LogHelper.LogHelper.WriteLog(ex);
                LogHelper.LogHelper.WriteLog(msg);
                return false;
            }
        }

        public bool Recharge(HKData.线上积分兑换 charge, out string msg)
        {
            msg = "";
            string url = string.Format(GateWayURL + "/accountapi/chargepoint?shopId={0}&caller={1}&cardId={2}&type={3}&amount={4}", charge.shopId, Caller, AES.AESEncrypt(charge.cardId), AES.AESEncrypt(charge.typeValue), AES.AESEncrypt(charge.amount));
            string response = Get(url);
            Console.WriteLine(response);

            try
            {
                JavaScriptSerializer jsonSerialize = new JavaScriptSerializer();
                HKData.兑换应答 ack = jsonSerialize.Deserialize<HKData.兑换应答>(response);
                if (ack.data.GetType().Name.ToLower().Contains("dictionary"))
                {
                    HKData.实名认证应答 ack1 = jsonSerialize.Deserialize<HKData.实名认证应答>(response);
                    if (ack1.statusCode == "2000000")
                    {
                        if (ack1.data.code == "140")
                            return true;
                        else
                        {
                            msg = ack1.data.description;
                            return false;
                        }
                    }
                    else
                    {
                        msg = ack1.statusMsg;
                        return false;
                    }
                }
                else
                {
                    msg = ack.data.ToString();
                    return false;
                }
            }
            catch (Exception ex)
            {
                msg = "JSON解析错误：" + response;
                LogHelper.LogHelper.WriteLog(ex);
                LogHelper.LogHelper.WriteLog(msg);
                return false;
            }
        }

        public bool RegistDevice(HKData.设备注册 device, out string msg, out string QRCode)
        {
            QRCode = "";
            msg = "";

            string deviceType = AES.AESEncrypt(device.deviceType);
            string deviceName = AES.AESEncrypt(device.Name);
            string deviceSN = AES.AESEncrypt(device.device);
            string deviceId = AES.AESEncrypt(device.deviceId);
            string cost = AES.AESEncrypt(device.cost);
            string machineSn = AES.AESEncrypt(device.machineSn);

            string url = GateWayURL + "/deviceapi/binddevice?shopId=" + device.shopId + "&caller=" + Caller + "&name=" + deviceName + "&deviceType=" + deviceType + "&dopCode=" + deviceSN + "&sn=" + deviceId + "&cost=" + cost + "&machineSn=" + machineSn + "&machineName=" + deviceName;// , , , , , );
            //LogHelper.WriteLog("好酷注册请求：\r\n" + url);
            string response = Get(url);
            Console.WriteLine(response);
            //LogHelper.WriteLog("好酷注册应答：\r\n" + response);
            try
            {
                JavaScriptSerializer jsonSerialize = new JavaScriptSerializer();
                HKData.兑换应答 ack = jsonSerialize.Deserialize<HKData.兑换应答>(response);
                if (ack.data.GetType().Name.ToLower().Contains("dictionary"))
                {
                    HKData.实名认证应答 ack1 = jsonSerialize.Deserialize<HKData.实名认证应答>(response);
                    if (ack1.statusCode == "2000000")
                    {
                        QRCode = ack1.data.sceneUrl;
                        return true;
                    }
                    else
                    {
                        msg = ack1.statusMsg;
                        return false;
                    }
                }
                else
                {
                    if (ack.statusCode == "2000000")
                        return true;
                    else
                    {
                        msg = ack.statusMsg;
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                msg = "JSON解析错误：" + response;
                LogHelper.LogHelper.WriteLog(ex);
                LogHelper.LogHelper.WriteLog(msg);
                return false;
            }
        }

        public void UpdateDevice(string sn, string state)//, string shopid, string caller)
        {
            HKData.设备状态通知 o = new HKData.设备状态通知();
            o.cardId = Caller;// caller;
            o.shopId = ShopID;// shopid;
            o.sn = sn;
            o.status = state;

            Thread t = new Thread(new ParameterizedThreadStart(TDeviceState));
            t.IsBackground = true;
            t.Start(o);
        }

        void TDeviceState(object o)
        {
            try
            {
                HKData.设备状态通知 n = o as HKData.设备状态通知;
                string DeviceSN = AES.AESEncrypt(n.sn);
                string DeviceState = AES.AESEncrypt(n.status);

                string url = GateWayURL + "/deviceapi/updatedevicestatus?shopId=" + n.shopId + "&caller=" + n.cardId + "&sn=" + DeviceSN + "&status=" + DeviceState;
                //LogHelper.WriteLog("好酷状态变更请求：\r\n"+url);
                string response = Get(url);
                //LogHelper.WriteLog("好酷状态变更应答：\r\n" + response);
                //Console.WriteLine("好酷设备更新：" + response);
            }
            catch (Exception ex)
            {
                LogHelper.LogHelper.WriteLog(ex);
            }
        }

        public void DevicePrize(string orderID, string type, string num)//, string shopid, string caller)
        {
            HKData.设备中奖通知 o = new HKData.设备中奖通知();
            o.orderID = orderID;
            o.type = type;
            o.num = num;
            o.shopid = ShopID;// shopid;
            o.caller = Caller;// caller;
            o.sn = GetPrizeSN;

            Thread t = new Thread(new ParameterizedThreadStart(TDevicePrize));
            t.IsBackground = true;
            t.Start(o);
        }

        void TDevicePrize(object o)
        {
            try
            {
                HKData.设备中奖通知 n = o as HKData.设备中奖通知;
                string PrizeSource = AES.AESEncrypt(n.orderID);
                string PrizeType = AES.AESEncrypt(n.type);
                string PrizeCount = AES.AESEncrypt(n.num);
                string PrizeSN = AES.AESEncrypt(n.sn);

                Console.WriteLine("Source=" + n.orderID + "  type=" + n.type + "  num=" + n.num + "  sn=" + n.sn);
                string url = GateWayURL + "/deviceapi/deviceprize?shopId=" + n.shopid + "&caller=" + n.caller + "&source=" + PrizeSource + "&type=" + PrizeType + "&num=" + PrizeCount + "&sn=" + PrizeSN;
                Console.WriteLine("中奖请求：" + url);
                string response = Get(url);
                Console.WriteLine("好酷中奖通知：" + response);
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("好酷中奖调用");
                sb.AppendLine("Source=" + n.orderID + "  type=" + n.type + "  num=" + n.num + "  sn=" + n.sn);
                sb.AppendLine(url);
                sb.AppendLine("应答结果：");
                sb.AppendLine(response);
                LogHelper.LogHelper.WriteLog(sb.ToString());
            }
            catch (Exception ex)
            {
                LogHelper.LogHelper.WriteLog(ex);
            }
        }

    }
}
