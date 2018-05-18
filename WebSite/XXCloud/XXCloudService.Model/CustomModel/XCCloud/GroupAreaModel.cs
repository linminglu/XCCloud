using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudService.Model.CustomModel.XCCloud
{
    [DataContract]
    public class GroupAreaModel
    {
        [DataMember(Name = "id", Order = 1)]
        public int ID { get; set; }

        [IgnoreDataMember]
        public Nullable<int> PID { get; set; }

        [DataMember(Name = "areaName", Order = 3)]
        public string AreaName { get; set; }

        [DataMember(Name = "projectCount", Order = 4)]
        public Nullable<int> ProjectCount { get; set; }        

        [DataMember(Name = "children", Order = 5)]
        public List<GroupAreaModel> Children { get; set; }        
    }    
}
