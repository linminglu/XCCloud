using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.Common.Enum;
using XCCloudService.Common;

namespace XCCloudService.Model.CustomModel.XCCloud
{
    public class Flw_MemberExchangeList
    {
        public string OrderID { get; set; }
        public string ICCardID { get; set; }
        public string UserName { get; set; }
        public string MemberLevelName { get; set; }
        public DateTime? CreateTime { get; set; }
        public decimal? Discount { get; set; }
        public decimal? OrginalPrice { get; set; }
        public decimal? PayCount { get; set; }
        public string BalanceIndexStr { get; set; }
        public decimal? TotalMoney { get; set; }
        public int? OrderStatus { get; set; }
        public string OrderStatusStr { get { return ((OrderState?)OrderStatus).GetDescription(); } set { } }
        public int? OrderSource { get; set; }
        public string OrderSourceStr { get { return ((OrderSource?)OrderSource).GetDescription(); } set { } }
        public DateTime? CheckDate { get; set; }
        public string ScheduleName { get; set; }
        public string WorkStation { get; set; }
        public string LogName { get; set; }
        public string Note { get; set; }
    }   
}
