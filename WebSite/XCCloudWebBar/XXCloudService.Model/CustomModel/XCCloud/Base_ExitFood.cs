using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudWebBar.Model.CustomModel.XCCloud
{
    [DataContract]
    public class Base_ExitFood
    {
        [DataMember(Name = "category", Order = 1)]
        public int Category { set; get; }

        [DataMember(Name = "foodId", Order = 2)]
        public int FoodId { set; get; }

        [DataMember(Name = "foodCount", Order = 3)]
        public int FoodCount { set; get; }
    }
}
