using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace PalletService.Model.WorkStation
{
    [DataContract]
    public class WorkStationRegisterModel
    {
        [DataMember(Name = "merchId", Order = 1)]
        public string MerchId { set; get; }

        [DataMember(Name = "storeId", Order = 2)]
        public string StoreId { set; get; }
    }
}
