using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
    public class Base_MerchAlipay
    {
        public int ID { get; set; }
        public string MerchID { get; set; }
        public string AppID { get; set; }
        public string PrivateKey { get; set; }
        public string PublicKey { get; set; }
        public decimal Fee { get; set; }
        public string Verifiction { get; set; }
    }
}
