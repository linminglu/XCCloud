using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Flw_Project_TicketDeviceLog
{
public string ID { get; set; }
public string TicketUseID { get; set; }
public string ProjectTicketCode { get; set; }
public int DeviceType { get; set; }
public int DeviceID { get; set; }
public DateTime LogTime { get; set; }
public int LogType { get; set; }
public int BalanceIndex { get; set; }
public decimal Total { get; set; }
public int UseType { get; set; }
public decimal CashTotal { get; set; }
public int SyncFlag { get; set; }
public string Verifiction { get; set; }
}
}
