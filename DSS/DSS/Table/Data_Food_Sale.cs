using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
    public class Data_Food_Sale
    {
        public int ID { get; set; }
        public string MerchID { get; set; }
        public int FoodID { get; set; }
        public int BalanceType { get; set; }
        public decimal UseCount { get; set; }
        public decimal CashValue { get; set; }
        public string Verifiction { get; set; }
    }
}
