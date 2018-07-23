using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.Common.Enum;
using XCCloudService.Common;

namespace XCCloudService.Model.CustomModel.XCCloud
{
    public class Flw_MemberLotteryList
    {
        public string OrderID { get; set; }
        public string ICCardID { get; set; }
        public string UserName { get; set; }
        public string MemberLevelName { get; set; }
        public string LotteryTypeStr { get; set; }
        public DateTime? CreateTime { get; set; }
        public string Direction { get; set; }
        public string OpertationTypeStr { get; set; }
        public decimal? OldBalance { get; set; }
        public decimal? ChangeValue { get; set; }
        public decimal? Balance { get; set; }
        public string StoreName { get; set; }
        public DateTime? CheckDate { get; set; }
        public string ScheduleName { get; set; }
        public string WorkStation { get; set; }
        public string LogName { get; set; }
        public string Note { get; set; }
    }   
}
