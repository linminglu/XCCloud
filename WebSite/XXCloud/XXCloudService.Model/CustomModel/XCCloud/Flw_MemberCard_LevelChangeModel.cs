using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.Common.Enum;
using XCCloudService.Common;

namespace XCCloudService.Model.CustomModel.XCCloud
{
    public class Flw_MemberCard_LevelChangeList
    {
        public string ID { get; set; }
        public string ICCardID { get; set; }
        public string UserName { get; set; }
        public DateTime? OpTime { get; set; }
        public string OldMemberLevelName { get; set; }
        public string NewMemberLevleName { get; set; }
        public int? ChangeType { get; set; }
        public string ChangeTypeStr { get { return ((MemberLevelChangeType?)ChangeType).GetDescription(); } set { } }
        public string OrderID { get; set; }
        public string StoreName { get; set; }
        public DateTime? CheckDate { get; set; }
        public string ScheduleName { get; set; }
        public string WorkStation { get; set; }
        public string OpUserName { get; set; }
        public string Note { get; set; }
    }
}
