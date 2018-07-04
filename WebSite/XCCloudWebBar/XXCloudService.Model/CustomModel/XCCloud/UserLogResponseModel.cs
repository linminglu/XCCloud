using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using XCCloudWebBar.Common.Enum;

namespace XCCloudWebBar.Model.CustomModel.XCCloud
{
    [DataContract]
    public class UserLogResponseModel
    {
        [DataMember(Name = "token", Order = 1)]
        public string Token { get; set; }

        [DataMember(Name = "logType", Order = 2)]
        public int LogType { get; set; }

        [DataMember(Name = "tag", Order = 3)]
        public int? Tag { get; set; }        

        [DataMember(Name = "userType", Order = 4)]
        public int? UserType { get; set; }

        [DataMember(Name = "switchMerch", Order = 5)]
        public int? SwitchMerch { get; set; }

        [DataMember(Name = "switchStore", Order = 6)]
        public int? SwitchStore { get; set; }

        [DataMember(Name = "switchWorkstation", Order = 7)]
        public int? SwitchWorkstation { get; set; }

        [DataMember(Name = "merchId", Order = 8)]
        public string MerchID { get; set; }

        [DataMember(Name = "storeId", Order = 9)]
        public string StoreID { get; set; }

        [DataMember(Name = "isSingle", Order = 10)]
        public int? IsSingle { get; set; }
    }    
}
