using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadarService.TableMemory
{
    public class Flw_Project_TicketDeviceLog
    {
        public string ID { get; set; }
        public string TicketUseID { get; set; }
        public string ProjectTicketCode { get; set; }
        public int DeviceID { get; set; }
        public DateTime LogTime { get; set; }
        /// <summary>
        /// 0 刷卡
        /// 1 验票
        /// 2 刷脸
        /// 3 线上
        /// 4 人工清场
        /// </summary>
        public int LogType { get; set; }
    }
}
