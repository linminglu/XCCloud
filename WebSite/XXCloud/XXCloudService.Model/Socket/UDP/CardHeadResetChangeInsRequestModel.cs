using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudService.Model.Socket.UDP
{
    [DataContract]
    public class CardHeadResetChangeInsRequestModel
    {
        public CardHeadResetChangeInsRequestModel(string sn, string mucId)
        {
            this.SN = sn;
            this.McuId = mucId;
            this.SignKey = "";
        }

        [DataMember(Name = "sn", Order = 1)]
        public string SN { set; get; }

        [DataMember(Name = "mcuid", Order = 2)]
        public string McuId { set; get; }

        [DataMember(Name = "signkey", Order = 3)]
        public string SignKey { set; get; }
    }
}
