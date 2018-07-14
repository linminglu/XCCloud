using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
    public class Flw_Project_TicketUse
    {
        public string ID { get; set; }
        public string MerchID { get; set; }
        public string StoreID { get; set; }
        public string ProjectTicketCode { get; set; }
        public string MemberID { get; set; }
        public int DeviceID { get; set; }
        public string DeviceName { get; set; }
        public DateTime? InTime { get; set; }
        public DateTime? OutTime { get; set; }
        public int OutMinuteTotal { get; set; }
        public string Verifiction { get; set; }
    }
}
