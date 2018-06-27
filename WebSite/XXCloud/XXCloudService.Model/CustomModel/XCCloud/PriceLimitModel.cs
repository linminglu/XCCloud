using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudService.Model.CustomModel.XCCloud
{
    [DataContract]
    public class PriceLimitModel
    {
        [DataMember(Name = "category", Order = 1)]
        public int Category { set; get; }

        [DataMember(Name = "balanceIndex", Order = 2)]
        public int BalanceIndex {set;get;}

        [DataMember(Name = "minPrice", Order = 3)]
        public decimal MinPrice {set;get;}

        [DataMember(Name = "maxPrice", Order = 4)]
        public decimal MaxPrice { set; get; }
    }
}
