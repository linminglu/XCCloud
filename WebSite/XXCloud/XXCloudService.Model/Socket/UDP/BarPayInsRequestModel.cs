using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudService.Model.Socket.UDP
{
    [DataContract]
    public class BarPayInsRequestModel
    {
        public BarPayInsRequestModel(string orderId, string authCode, string sn)
        {
            this.OrderId = orderId;
            this.AuthCode = authCode;
            this.SN = sn;
        }

        [DataMember(Name = "orderId", Order = 1)]
        public string OrderId { set; get; }

        [DataMember(Name = "authCode", Order = 2)]
        public string AuthCode { set; get; }

        [DataMember(Name = "sn", Order = 3)]
        public string SN { set; get; }

        [DataMember(Name = "signKey", Order = 3)]
        public string SignKey { set; get; }
    }
}
