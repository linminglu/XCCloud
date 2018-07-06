using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.Common.Enum;
using XCCloudService.Common;

namespace XCCloudService.Model.CustomModel.XCCloud
{
    public class Log_GameAlarmModel
    {
        public string GameName { get; set; }
        public string SiteName { get; set; }
        public string DeviceName { get; set; }
        public string segment { get; set; }
        public string Address { get; set; }
        public int? AlertType { get; set; }
        public string AlertContent { get; set; }
        public DateTime? HappenTime { get; set; }
        public string ICCardID { get; set; }
        public int? LockGame { get; set; }
        public int? LockMember { get; set; }
        public DateTime? EndTime { get; set; }
        public int? State { get; set; }
        public string AlertTypeStr { get { return ((AlertType?)AlertType).GetDescription(); } set { } }
        public string LockGameStr { get { return LockGame == 1 ? "是" : LockGame == 0 ? "否" : string.Empty; } set { } }
        public string LockMemberStr { get { return LockMember == 1 ? "是" : LockMember == 0 ? "否" : string.Empty; } set { } }
        public string StateStr { get { return State == 0 ? "活动" : State == 1 ? "确认" : State == 2 ? "解决" : string.Empty; } set { } }
    }
}
