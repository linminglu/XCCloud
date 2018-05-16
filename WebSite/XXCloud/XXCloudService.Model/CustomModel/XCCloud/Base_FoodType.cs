using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudService.Model.CustomModel.XCCloud
{
    [DataContract]
    public class Base_FoodType
    {
        [DataMember(Name = "foodTypeId", Order = 1)]
        public string FoodTypeId { set; get; }

        [DataMember(Name = "foodTypeName", Order = 2)]
        public string FoodTypeName { set; get; }
    }
}
