using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Flw_Schedule_UserInfo
{
public string ID { get; set; }
public string MerchID { get; set; }
public string StoreID { get; set; }
public string ScheduleID { get; set; }
public string ScheduleName { get; set; }
public DateTime? CheckDate { get; set; }
public int? UserID { get; set; }
public DateTime? OpenTime { get; set; }
public DateTime? ShiftTime { get; set; }
public string WorkStation { get; set; }
public decimal? CashTotle { get; set; }
public decimal? NetTotle { get; set; }
public decimal? CommissionTotle { get; set; }
public decimal? RealCash { get; set; }
public decimal? RealCredit { get; set; }
public string Verifiction { get; set; }
}
}
