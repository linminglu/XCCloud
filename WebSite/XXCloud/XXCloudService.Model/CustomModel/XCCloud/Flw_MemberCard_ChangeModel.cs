using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.Common.Enum;
using XCCloudService.Common;

namespace XCCloudService.Model.CustomModel.XCCloud
{
    public class Flw_MemberCard_ChangeList
    {
        public string ID { get; set; }
        public string NewICCardID { get; set; }
        public string UserName { get; set; }
        public string OldICCardID { get; set; }
        public string OperateType { get; set; }
        public DateTime? CreateTime { get; set; }
        public decimal? OpFee { get; set; }
        public string StoreName { get; set; }
        public DateTime? CheckDate { get; set; }
        public string ScheduleName { get; set; }
        public string WorkStation { get; set; }
        public string LogName { get; set; }
        public string Note { get; set; }       
    }
}
