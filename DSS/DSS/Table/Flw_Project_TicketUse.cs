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
public int? InDeviceID { get; set; }
public int? OutDeviceID { get; set; }
public string DeviceName { get; set; }
public DateTime? InTime { get; set; }
public DateTime? OutTime { get; set; }
public int? ProjectID { get; set; }
public int? InDeviceType { get; set; }
public int? OutDeviceType { get; set; }
public int? OutMinuteTotal { get; set; }
public string Note { get; set; }
public int? SyncFlag { get; set; }
public string Verifiction { get; set; }
}
}
