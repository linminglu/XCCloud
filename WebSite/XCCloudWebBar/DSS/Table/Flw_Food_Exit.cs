using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
    public class Flw_Food_Exit
    {
        public string ExitID { get; set; }
        public string MerchID { get; set; }
        public string StoreID { get; set; }
        public string OrderID { get; set; }
        public int FoodID { get; set; }
        public string CardID { get; set; }
        public decimal ExitFee { get; set; }
        public decimal Tax { get; set; }
        public decimal TaxFee { get; set; }
        public decimal TotalMoney { get; set; }
        public string Note { get; set; }
        public int UserID { get; set; }
        public string ScheduleID { get; set; }
        public int AuthorID { get; set; }
        public DateTime RealTime { get; set; }
        public string WorkStation { get; set; }
        public DateTime CheckDate { get; set; }
        public string Verifiction { get; set; }
    }
}
