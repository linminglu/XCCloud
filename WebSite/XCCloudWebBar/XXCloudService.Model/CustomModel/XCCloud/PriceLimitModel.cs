using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudWebBar.Model.CustomModel.XCCloud
{
    [DataContract]
    public class PriceLimitModel
    {
        [DataMember(Name = "category", Order = 1)]
        public int Category { set; get; }

        [DataMember(Name = "title", Order = 2)]
        public string  Title { set; get; }

        [DataMember(Name = "balanceIndex", Order = 3)]
        public int BalanceIndex {set;get;}

        [DataMember(Name = "minPrice", Order = 4)]
        public decimal MinPrice {set;get;}

        [DataMember(Name = "maxPrice", Order = 5)]
        public decimal MaxPrice { set; get; }
    }

    [DataContract]
    public class PriceLimitModel2
    {
        [DataMember(Name = "category", Order = 1)]
        public string Category { set; get; }

        [DataMember(Name = "title", Order = 2)]
        public string Title { set; get; }

        [DataMember(Name = "balanceIndex", Order = 3)]
        public int BalanceIndex { set; get; }

        [DataMember(Name = "minPrice", Order = 4)]
        public decimal MinPrice { set; get; }

        [DataMember(Name = "maxPrice", Order = 5)]
        public decimal MaxPrice { set; get; }
    }
}
