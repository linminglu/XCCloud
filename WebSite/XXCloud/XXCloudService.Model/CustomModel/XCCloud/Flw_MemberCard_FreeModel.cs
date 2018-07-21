using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.Common.Enum;
using XCCloudService.Common;

namespace XCCloudService.Model.CustomModel.XCCloud
{
    public class Flw_MemberCard_FreeList
    {
        public string ID { get; set; }
        public string ICCardID { get; set; }
        public string UserName { get; set; }
        public string MemberLevelName { get; set; }
        public DateTime? OpTime { get; set; }
        public int? FreeType { get; set; }
        public string FreeTypeStr { get { return ((FreeType?)FreeType).GetDescription(); } set { } }
        public decimal? FreeCount { get; set; }
        public string FreeName { get; set; }
        public string StateStr { get; set; }
        public string BalanceIndexStr { get; set; }       
        public string StoreName { get; set; }
        public DateTime? CheckDate { get; set; }
        public string ScheduleName { get; set; }
        public string WorkStation { get; set; }
        public string LogName { get; set; }
        public string Note { get; set; }
    }   
}
