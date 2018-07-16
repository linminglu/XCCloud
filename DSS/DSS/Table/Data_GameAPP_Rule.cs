using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
    public class Data_GameAPP_Rule
    {
        public int ID { get; set; }
        public string MerchID { get; set; }
        public string StoreID { get; set; }
        public int GameID { get; set; }
        public decimal PayCount { get; set; }
        public int PlayCount { get; set; }
        public string Verifiction { get; set; }
    }
}
