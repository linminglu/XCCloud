using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudService.Model.CustomModel.XCCloud
{
    [DataContract]
    public class GoodModel
    {
        [DataMember(Name = "foodId", Order = 1)]
        public int FoodId {set;get;}

        [DataMember(Name = "foodName", Order = 2)]
        public string FoodName {set;get;}

        [DataMember(Name = "foodType", Order = 3)]
        public int FoodType {set;get;}

        [DataMember(Name = "foodPrice", Order = 4)]
        public decimal Price {set;get;}

        [DataMember(Name = "imageUrl", Order = 5)]
        public string ImageUrl {set;get;}

        [DataMember(Name = "detailsCount", Order = 6)]
        public int DetailsCount { get; set; }

        [DataMember(Name = "detailInfoList", Order = 7)]
        public List<FoodDetailInfoModel> DetailInfoList { set; get; }
    }
}
