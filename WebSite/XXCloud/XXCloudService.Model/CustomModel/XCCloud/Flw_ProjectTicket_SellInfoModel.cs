using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.Common.Enum;
using XCCloudService.Common;

namespace XCCloudService.Model.CustomModel.XCCloud
{
    public class Flw_ProjectTicket_SellInfoList
    {
        public string ID { get; set; }
        public string Barcode { get; set; }
        public string TicketName { get; set; }
        public string OrderID { get; set; }
        public DateTime? CreateTime { get; set; }
        public decimal? TotalMoney { get; set; }
        public int? SaleCount { get; set; }
        public decimal? RealMoney { get; set; }
        public decimal? FreeMoney { get; set; }
        public int? TicketType { get; set; }
        public string TicketTypeStr { get { return TicketType == 0 ? "主票" : TicketType == 1 ? "陪同票" : string.Empty; } set { } }
        public string ICCardID { get; set; }
        public int? OrderSource { get; set; }
        public string OrderSourceStr { get { return ((OrderSource?)OrderSource).GetDescription(); } set { } }
        public DateTime? CheckDate { get; set; }
        public string WorkStation { get; set; }
        public string LogName { get; set; }
        public int? State { get; set; }
        public string StateStr { get { return ((TicketState?)State).GetDescription(); } set { } }
        public DateTime? FirstUseTime { get; set; }
        public DateTime? EffactTime { get; set; }
        public DateTime? ExpiredTime { get; set; }
        public string Note { get; set; }
    }   
}
