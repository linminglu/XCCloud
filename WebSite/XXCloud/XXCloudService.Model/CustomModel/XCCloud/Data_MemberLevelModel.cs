using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudService.Model.CustomModel.XCCloud
{
    [DataContract]
    public class Data_MemberLevelModel
    {
        [DataMember(Name = "memberLevelId", Order = 1)]
        public int MemberLevelID { set; get; }

        [DataMember(Name = "memberLevelName", Order = 2)]
        public string MemberLevelName { set; get; }
    }


}
