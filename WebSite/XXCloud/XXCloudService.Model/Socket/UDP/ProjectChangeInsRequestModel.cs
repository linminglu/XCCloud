using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudService.Model.Socket.UDP
{
    [DataContract]
    public class ProjectChangeInsRequestModel
    {
        public ProjectChangeInsRequestModel(string sn, string gameIndex)
        {
            this.SN = sn;
            this.GameIndex = gameIndex;
            this.SignKey = "";
        }

        [DataMember(Name = "sn", Order = 1)]
        public string SN { set; get; }

        [DataMember(Name = "gameIndex", Order = 2)]
        public string GameIndex { set; get; }

        [DataMember(Name = "signkey", Order = 3)]
        public string SignKey { set; get; }
    }
}
