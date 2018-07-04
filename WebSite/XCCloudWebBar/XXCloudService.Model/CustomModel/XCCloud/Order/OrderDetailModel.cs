using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudWebBar.Model.CustomModel.XCCloud.Order
{
    [DataContract]
    public class OrderDetailModel
    {
        [DataMember(Name = "foodId", Order = 1)]
        public int FoodId {set;get;}

        [DataMember(Name = "foodName", Order = 2)]
        public string FoodName {set;get;}

        [DataMember(Name = "foodCount", Order = 3)]
        public int FoodCount { set; get; }

        [DataMember(Name = "foodType", Order = 4)]
        public int FoodType { set; get; }

        [DataMember(Name = "foodTypeName", Order = 5)]
        public int FoodTypeName { set; get; }

        [DataMember(Name = "payNum", Order = 6)]
        public decimal PayNum { set; get; }
    }
}
