using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.Common.Enum;
using XCCloudService.Common;

namespace XCCloudService.Model.CustomModel.XCCloud
{
    public class Flw_MemberDataList
    {
        public string ID { get; set; }
        public string ICCardID { get; set; }
        public int? OperationType { get; set; }
        public string OperationTypeStr { get { return ((MemberDataOperationType?)OperationType).GetDescription(); } set { } }
        public DateTime? OPTime { get; set; }
        public string SourceID { get; set; }
        public decimal? OldBalance { get; set; }
        public decimal? ChangeValue { get; set; }
        public string BalanceIndexStr { get; set; }
        public decimal? Balance { get; set; }        
        public string StoreName { get; set; }
        public DateTime? CheckDate { get; set; }
        public string ScheduleName { get; set; }
        public string WorkStation { get; set; }
        public string LogName { get; set; }
        public string Note { get; set; }
    }   
}
