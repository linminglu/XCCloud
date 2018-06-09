using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudService.Model.CustomModel.XCCloud
{
    [DataContract]
    public class Base_Food
    {
        [DataMember(Name = "foodTypeMainList", Order = 1)]
        public List<Base_FoodType> FoodTypeMainList { set; get; }

        [DataMember(Name = "foodTypeGoodList", Order = 2)]
        public List<Base_FoodType> FoodTypeGoodList { set; get; }

        [DataMember(Name = "foodTypeTicketList", Order = 3)]
        public List<Base_FoodType> FoodTypeTicketList { set; get; }
    }

    [DataContract]
    public class Base_FoodType
    {
        [DataMember(Name = "foodTypeId", Order = 1)]
        public string FoodTypeId { set; get; }

        [DataMember(Name = "foodTypeName", Order = 2)]
        public string FoodTypeName { set; get; }
    }
}
