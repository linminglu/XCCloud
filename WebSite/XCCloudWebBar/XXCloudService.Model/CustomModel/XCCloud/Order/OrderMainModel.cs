using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudWebBar.Model.CustomModel.XCCloud.Order
{
    [DataContract]
    public class OrderMainModel
    {
        [DataMember(Name = "storeId", Order = 1)]
        public string StoreId { set; get; }

        [DataMember(Name = "icCardId", Order = 2)]
        public string ICCardId { set; get; }

        [DataMember(Name = "payCount", Order = 3)]
        public decimal PayCount { set; get; }

        [DataMember(Name = "realPay", Order = 4)]
        public decimal RealPay { set; get; }

        [DataMember(Name = "freePay", Order = 5)]
        public decimal FreePay { set; get; }

        [DataMember(Name = "foodCount", Order = 6)]
        public int FoodCount { set; get; }

        [DataMember(Name = "detailGoodsCount", Order = 7)]
        public int DetailGoodsCount { set; get; }

        [DataMember(Name = "customerType", Order = 8)]
        public int CustomerType { set; get; }

        [DataMember(Name = "memberLevelId", Order = 9)]
        public int MemberLevelId { set; get; }

        [DataMember(Name = "memberLevelName", Order = 10)]
        public string MemberLevelName { set; get; }

        [DataMember(Name = "createTime", Order = 11)]
        public DateTime CreateTime { set; get; }
    }
}
