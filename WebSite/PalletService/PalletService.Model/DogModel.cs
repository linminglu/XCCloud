using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace PalletService.Model
{
    [DataContract]
    public class DogMD5RequestModel
    {
        [DataMember(Name = "merchId", Order = 1)]
        public string MerchID { set; get; }

        [DataMember(Name = "storeId", Order = 2)]
        public string StoreID { set; get; }
    }
}
