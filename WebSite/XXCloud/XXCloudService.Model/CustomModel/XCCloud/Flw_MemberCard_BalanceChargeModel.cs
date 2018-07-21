using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.Common.Enum;
using XCCloudService.Common;

namespace XCCloudService.Model.CustomModel.XCCloud
{
    public class Flw_MemberCard_BalanceChargeList
    {
        public string ID { get; set; }
        public string ICCardID { get; set; }
        public string UserName { get; set; }
        public string MemberLevelName { get; set; }
        public DateTime? OpTime { get; set; }
        public decimal? SourceCount { get; set; }
        public decimal? SourceRemain { get; set; }
        public decimal? TargetCount { get; set; }
        public decimal? TargetRemain { get; set; }
        public string SourceBalanceStr { get; set; }
        public string TargetBalanceStr { get; set; }
        public string StoreName { get; set; }
        public DateTime? CheckDate { get; set; }
        public string ScheduleName { get; set; }
        public string WorkStation { get; set; }
        public string LogName { get; set; }
        public string Note { get; set; }
    }   
}
