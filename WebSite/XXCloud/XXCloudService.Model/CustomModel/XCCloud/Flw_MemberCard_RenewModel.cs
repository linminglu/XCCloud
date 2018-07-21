using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.Common.Enum;
using XCCloudService.Common;

namespace XCCloudService.Model.CustomModel.XCCloud
{
    public class Flw_MemberCard_RenewList
    {
        public string ID { get; set; }
        public string ICCardID { get; set; }
        public string UserName { get; set; }
        public DateTime? CreateTime { get; set; }
        public decimal? RenewFee { get; set; }
        public string BalanceIndexStr { get; set; }
        public decimal? PayCount { get; set; }
        public string FoodSaleID { get; set; }
        public DateTime? OldEndDate { get; set; }
        public DateTime? NewEndDate { get; set; }
        public string StoreName { get; set; }
        public DateTime? CheckDate { get; set; }
        public string ScheduleName { get; set; }
        public string WorkStation { get; set; }
        public string LogName { get; set; }
        public string Note { get; set; }
    }   
}
