using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
    public class Data_DigitCoinDestroy
    {
        public int ID { get; set; }
        public string MerchID { get; set; }
        public string StoreID { get; set; }
        public string ICCardID { get; set; }
        public DateTime DestroyTime { get; set; }
        public int UserID { get; set; }
        public string Note { get; set; }
        public string Verifiction { get; set; }
    }
}
