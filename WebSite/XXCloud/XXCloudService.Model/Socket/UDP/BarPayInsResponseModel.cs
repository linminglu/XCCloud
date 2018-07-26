using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudService.Model.Socket.UDP
{
    [DataContract]
    public class BarPayInsResponseModel2
    {
        public BarPayInsResponseModel2(string result_Code, string result_msg, string sn)
        {
            this.Result_Code = result_Code;
            this.Result_Msg = result_msg;
            this.SN = sn;
        }

        [DataMember(Name = "result_code", Order = 1)]
        public string Result_Code { set; get; }

        [DataMember(Name = "result_msg", Order = 2)]
        public string Result_Msg { set; get; }

        [DataMember(Name = "sn", Order = 3)]
        public string SN { set; get; }

        [DataMember(Name = "signKey", Order = 4)]
        public string SignKey { set; get; }
    }
}
