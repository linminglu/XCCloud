using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudWebBar.Model.CustomModel.XCCloud
{
    [DataContract]
    public class TicketModel
    {
        [DataMember(Name = "foodId", Order = 1)]
        public int FoodId { set; get; }

        [DataMember(Name = "foodName", Order = 2)]
        public string FoodName { set; get; }

        [DataMember(Name = "foodType", Order = 3)]
        public int FoodType { set; get; }

        [DataMember(Name = "detailsCount", Order = 4)]
        public int DetailsCount { get; set; }

        [DataMember(Name = "detailInfoList", Order = 5)]
        public List<FoodDetailInfoModel> DetailInfoList { set; get; }
    }
}
