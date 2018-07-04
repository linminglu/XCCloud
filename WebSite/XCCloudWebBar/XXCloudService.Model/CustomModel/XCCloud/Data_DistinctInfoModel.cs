using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudWebBar.Model.CustomModel.XCCloud
{
    [DataContract]
    public class Data_DistinctDetailModel
    {
        [DataMember(Name = "id", Order = 1)]
        public int ID { set; get; }

        [DataMember(Name = "ruleName", Order = 2)]
        public string RuleName { set; get; }

        [DataMember(Name = "storeCount", Order = 3)]
        public int StoreCount { set; get; }

        [DataMember(Name = "shareCount", Order = 4)]
        public int ShareCount { set; get; }

        [DataMember(Name = "memberCount", Order = 5)]
        public int MemberCount { set; get; }
    }

    [DataContract]
    public class Data_DistinctModel
    {
        [DataMember(Name = "discountRuleId", Order = 1)]
        public int DiscountRuleId {set;get;}

        [DataMember(Name = "subPrice", Order = 2)]
        public decimal SubPrice {set;get;}

        [DataMember(Name = "detailList", Order = 3)]
        public List<Data_DistinctDetailModel> DetailList { set; get; }
    }
}
