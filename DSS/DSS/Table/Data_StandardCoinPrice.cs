using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
    public class Data_StandardCoinPrice
    {
        public int ID { get; set; }
        public string MerchID { get; set; }
        public string StoreID { get; set; }
        public int BalanceIndex { get; set; }
        public int CoinCount { get; set; }
        public decimal CashPrice { get; set; }
        public string Verifiction { get; set; }
    }
}
