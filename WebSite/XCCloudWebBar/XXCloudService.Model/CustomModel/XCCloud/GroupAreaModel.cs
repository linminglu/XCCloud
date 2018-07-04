using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudWebBar.Model.CustomModel.XCCloud
{
    [DataContract]
    public class GroupAreaModel
    {
        [DataMember(Name = "id", Order = 1)]
        public int ID { get; set; }

        [DataMember(Name = "areaName", Order = 2)]
        public string AreaName { get; set; }

        [DataMember(Name = "projectCount", Order = 3)]
        public Nullable<int> ProjectCount { get; set; }          
    }    
}
