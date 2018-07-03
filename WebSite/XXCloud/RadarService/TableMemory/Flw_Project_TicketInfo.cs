using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace RadarService.TableMemory
{
    public class Flw_Project_TicketInfo
    {
        public string ID { get; set; }
        public string MerchID { get; set; }
        public string FoodSaleID { get; set; }
        public string CardID { get; set; }
        public string MemberID { get; set; }
        public string ParentID { get; set; }
        public int TicketType { get; set; }
        public int NeedActive { get; set; }
        public string Barcode { get; set; }
        public DateTime SaleTime { get; set; }
        public DateTime? FirstUseTime { get; set; }
        public int WriteOffDays { get; set; }
        public DateTime EndTime { get; set; }
        public int State { get; set; }
        public decimal BuyPrice { get; set; }
        public int BalanceIndex { get; set; }
        public decimal BalanceCount { get; set; }
        public decimal RemainDividePrice { get; set; }
        public string Verifiction { get; set; }
    }
}
