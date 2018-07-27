using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudService.Model.Socket.UDP
{
    [DataContract]
    public class ScanPayRequestModel
    {
        public ScanPayRequestModel(string signkey, string sn, string orderid, string authcode)
        {
            SignKey = signkey;
            SN = sn;
            OrderID = orderid;
            AuthCode = authcode;
        }
        /// <summary>
        /// 签名
        /// </summary>
        [DataMember(Name = "signkey", Order = 1)]
        public string SignKey { get; set; }
        /// <summary>
        /// 指令流水号
        /// </summary>
        [DataMember(Name = "sn", Order = 2)]
        public string SN { get; set; }
        /// <summary>
        /// 支付请求订单号
        /// </summary>
        [DataMember(Name = "orderid", Order = 3)]
        public string OrderID { get; set; }
        /// <summary>
        /// 玩家出示的付款码
        /// </summary>
        [DataMember(Name = "authcode", Order = 4)]
        public string AuthCode { get; set; }
        /// <summary>
        /// 令牌
        /// </summary>
        [DataMember(Name = "token", Order = 4)]
        public string  Token { get; set; }
        public string IP { get; set; }
        public int Port { get; set; }
    }
}
