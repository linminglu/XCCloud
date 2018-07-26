using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudService.Model.Socket.UDP
{
    [DataContract]
    public class BarPayInsRequestModel2
    {
        public BarPayInsRequestModel2(string orderId, string payStatus, string sn)
        {
            this.OrderId = orderId;
            this.PayStatus = payStatus;
            this.SN = sn;
        }

        [DataMember(Name = "orderId", Order = 1)]
        public string OrderId { set; get; }

        [DataMember(Name = "paystatus", Order = 2)]
        public string PayStatus { set; get; }

        [DataMember(Name = "sn", Order = 3)]
        public string SN { set; get; }

        [DataMember(Name = "errinfo", Order = 4)]
        public string ErrInfo { set; get; }

        [DataMember(Name = "signKey", Order = 5)]
        public string SignKey { set; get; }
    }
}
