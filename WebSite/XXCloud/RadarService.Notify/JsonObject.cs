using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Security.Cryptography;

namespace RadarService.Notify
{
    public class JsonObject
    {
        /// <summary>
        /// 设备注册结构
        /// </summary>
        public class RegistDevice
        {
            public string storeid { get; set; }
            public string segment { get; set; }
            public string signkey { get; set; }
        }
        /// <summary>
        /// 设备注册应答结构
        /// </summary>
        public class RegistResponse
        {
            public string token { get; set; }
            public string signkey { get; set; }
        }
        /// <summary>
        /// 设备心跳结构
        /// </summary>
        public class DeviceTick
        {
            public string token { get; set; }
        }
        /// <summary>
        /// 设备心跳应答结构
        /// </summary>
        public class TickResponse
        {
            public string result_code { get; set; }
            public string result_msg { get; set; }
            public string signkey { get; set; }
        }
        public class StatusItem
        {
            public string mcuid { get; set; }
            public string status { get; set; }
        }

        /// <summary>
        /// 设备状态变更结构
        /// </summary>
        public class ChangeStatus
        {
            public string token { get; set; }
            public string signkey { get; set; }
            public List<StatusItem> devicelist { get; set; }
        }
        /// <summary>
        /// 设备控制指令结构
        /// </summary>
        public class DeviceControl
        {
            public string token { get; set; }
            public string mcuid { get; set; }
            public string iccardid { get; set; }
            public string action { get; set; }
            public string count { get; set; }
            public string zkzy { get; set; }
            public string orderid { get; set; }
            public string sn { get; set; }
            public string signkey { get; set; }
        }
        /// <summary>
        /// 设备控制应答指令结构
        /// </summary>
        public class ControlResponse
        {
            public string result_code { get; set; }
            public string result_msg { get; set; }
            public string sn { get; set; }
            public string signkey { get; set; }
        }
        public class ControlResultNotify
        {
            public string token { get; set; }
            public string action { get; set; }
            public string result { get; set; }
            public string orderid { get; set; }
            public string coins { get; set; }
            public string sn { get; set; }
            public string signkey { get; set; }
        }
        public class NotifyResponse
        {
            public string sn { get; set; }
            public string signkey { get; set; }
        }
        public class RemoteAccoutRequest
        {
            public string searchtype { get; set; }
            public string sn { get; set; }
            public string date { get; set; }
            public string iccardid { get; set; }
            public string signkey { get; set; }
        }
        public class RemoteAccountRequestACK
        {
            public string result_code { get; set; }
            public string result_msg { get; set; }
            public string signkey { get; set; }
            public string sn { get; set; }
        }
        public class RemoteAccoutResponse
        {
            public string signkey { get; set; }
            public string token { get; set; }
            public string sn { get; set; }
            public List<TableRowItem> tabledata { get; set; }
        }
        public class TableRowItem
        {
            public string itemKey { get; set; }
            public string itemValue { get; set; }
        }
        public class RemoteAccoutResponseACK
        {
            public string result_code { get; set; }
            public string result_msg { get; set; }
            public string packid { get; set; }
            public string signkey { get; set; }
        }

        public class RemoteICCardRequest
        {
            public string signkey { get; set; }
            public string sn { get; set; }
            public string iccardid { get; set; }
        }

        public class RemoteICCardInfo
        {
            public string storeId { get; set; }
            public string storeName { get; set; }
            public string icCardID { get; set; }
            public string memberName { get; set; }
            public string gender { get; set; }
            public string birthday { get; set; }
            public string certificalID { get; set; }
            public string mobile { get; set; }
            public string balance { get; set; }
            public string point { get; set; }
            public string deposit { get; set; }
            public string memberState { get; set; }
            public string lottery { get; set; }
            public string note { get; set; }
            public string memberLevelName { get; set; }
            public string endDate { get; set; }
        }

        public class RemoteICCardResponse
        {
            public string result_code { get; set; }
            public string result_msg { get; set; }
            public string signkey { get; set; }
            public string sn { get; set; }
            public RemoteICCardInfo result_data { get; set; }
        }

        public class RemoteBarcodeRequest
        {
            public string signkey { get; set; }
            public string sn { get; set; }
            public string barcode { get; set; }
            public string iccardid { get; set; }
            public string mobilename { get; set; }
            public string phone { get; set; }
            public string money { get; set; }
            public string operate { get; set; }
        }

        public class RemoteProjectInfo
        {
            public string id { get; set; }
            public string projectname { get; set; }
            public string state { get; set; }
            public string projecttype { get; set; }
            public string remaincount { get; set; }
            public string endtime { get; set; }
        }

        public class RemoteLotteryInfo
        {
            public string id { get; set; }
            public string lottery { get; set; }
            public string gamename { get; set; }
            public string headinfo { get; set; }
            public string state { get; set; }
            public string printdate { get; set; }
        }

        public class RemotePrintTicketInfo
        {
            public string id { get; set; }
            public string coins { get; set; }
            public string gamename { get; set; }
            public string headinfo { get; set; }
            public string state { get; set; }
            public string printdate { get; set; }
        }

        public class RemoteBarcodeInfoResponse
        {
            public string result_code { get; set; }
            public string result_msg { get; set; }
            public string sn { get; set; }
            public string signkey { get; set; }
            public object result_data { get; set; }
        }

        public class RemoteParametItem
        {
            public string txtCoinPrice { get; set; }
            public string txtTicketDate { get; set; }
        }

        public class RemoteParametResponse
        {
            public string result_code { get; set; }
            public string result_msg { get; set; }
            public string sn { get; set; }
            public string signkey { get; set; }
            public RemoteParametItem result_data { get; set; }
        }
        public class RemoteTransmitRequest
        {
            public string signkey { get; set; }
            public string sn { get; set; }
            public string mobilename { get; set; }
            public string phone { get; set; }
            public string iniccardid { get; set; }
            public string iniccardpwd { get; set; }
            public string outiccardid { get; set; }
            public string coins { get; set; }
        }

        public class RemoteTransmitResponse
        {
            public string result_code { get; set; }
            public string result_msg { get; set; }
            public string sn { get; set; }
            public string signkey { get; set; }
            public string result_data { get; set; }
        }

        public class RemoteCheckYGPhoneRequest
        {
            public string signkey { get; set; }
            public string sn { get; set; }
            public string phone { get; set; }
        }

        public class RemoteGetHuangniuResponse
        {
            public string result_code { get; set; }
            public string result_msg { get; set; }
            public string sn { get; set; }
            public string signkey { get; set; }
            public string cardcount { get; set; }
            public object result_data { get; set; }
        }

        public class RemoteHuangniuRequest
        {
            public string signkey { get; set; }
            public string sn { get; set; }
            public string requesttype { get; set; }
        }

        public class RemoteHuangniuItem
        {
            public string iccardid { get; set; }
            public string name { get; set; }
            public string memberstate { get; set; }
            public string phone { get; set; }
        }

        public class RemoteParametRequest
        {
            public string requsttype { get; set; }
            public string signkey { get; set; }
            public string sn { get; set; }
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
                if (v != "" && pi.Name != "signkey" && pi.PropertyType.Name == "String")
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
