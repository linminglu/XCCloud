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
        [DataMember(Name = "foodId", Order = 1)]
        public string FoodId { set; get; }

         [DataMember(Name = "foodName", Order = 2)]
        public string FoodName { set; get; }
    }
}
