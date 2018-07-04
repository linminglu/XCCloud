using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudWebBar.Model.CustomModel.XCCloud
{
    [DataContract]
    public class Data_MemberLevelModel
    {
        [DataMember(Name = "memberLevelId", Order = 1)]
        public int MemberLevelID { set; get; }

        [DataMember(Name = "memberLevelName", Order = 2)]
        public string MemberLevelName { set; get; }

        [DataMember(Name = "coverURL", Order = 3)]
        public string CoverURL { set; get; }

        [DataMember(Name = "openFee", Order = 4)]
        public decimal OpenFee { set; get; }

        [DataMember(Name = "deposit", Order = 5)]
        public decimal Deposit { set; get; }
    }


}
