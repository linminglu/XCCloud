using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.Common.Enum;
using XCCloudService.Common;

namespace XCCloudService.Model.CustomModel.XCCloud
{
    public class Flw_MemberTransferList
    {
        public string ID { get; set; }
        public string CardIDOut { get; set; }
        public string UserName { get; set; }
        public DateTime? RealTime { get; set; }
        public string TransferBalanceStr { get; set; }
        public decimal? TransferCount { get; set; }
        public decimal? BalanceOut { get; set; }
        public string CardIDIn { get; set; }
        public string InUserName { get; set; }
        public decimal? BalanceIn { get; set; }
        public string StoreName { get; set; }
        public DateTime? CheckDate { get; set; }
        public string ScheduleName { get; set; }
        public string WorkStation { get; set; }
        public string LogName { get; set; }
        public string Note { get; set; }
    }   
}
