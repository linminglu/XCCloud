using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
    public class Flw_DiscountRule
    {
        public string ID { get; set; }
        public string MerchID { get; set; }
        public string StoreID { get; set; }
        public string OrderFlwID { get; set; }
        public int DeviceID { get; set; }
        public int DiscountID { get; set; }
        public decimal FreeMoney { get; set; }
        public DateTime UseTime { get; set; }
        public string Verifiction { get; set; }
    }
}
