using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
    public class Base_StoreHKConfig
    {
        public int ID { get; set; }
        public string MerchID { get; set; }
        public string StoreID { get; set; }
        public string HKShopID { get; set; }
        public string HKStoreSecret { get; set; }
        public string HKOrgID { get; set; }
        public string HKMerchID { get; set; }
        public string HKAppSecret { get; set; }
        public string HKAppID { get; set; }
        public string Verifiction { get; set; }
    }
}
