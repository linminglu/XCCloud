using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudService.Model.CustomModel.XCCloud
{
    [DataContract]
    public class CouponModel
    {
        [DataMember(Name = "couponId", Order = 1)]
        public string CouponId { set; get; }

        [DataMember(Name = "couponCode", Order = 2)]
        public string CouponCode { set; get; }
    }
}
