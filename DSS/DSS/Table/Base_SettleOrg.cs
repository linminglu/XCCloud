using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
    public class Base_SettleOrg
    {
        public int ID { get; set; }
        public string MerchID { get; set; }
        public string WXPayOpenID { get; set; }
        public string WXName { get; set; }
        public string AliPay { get; set; }
        public string AliPayName { get; set; }
        public decimal SettleFee { get; set; }
        public int SettleCycle { get; set; }
        public int SettleCount { get; set; }
        public string Verifiction { get; set; }
    }
}
