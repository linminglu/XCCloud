using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Flw_MemberCard_BalanceCharge
{
public string ID { get; set; }
public string MerchID { get; set; }
public string StoreID { get; set; }
public string MemberID { get; set; }
public string CardIndex { get; set; }
public int? SourceBalanceIndex { get; set; }
public decimal? SourceCount { get; set; }
public decimal? SourceRemain { get; set; }
public int? TargetBalanceIndex { get; set; }
public decimal? TargetCount { get; set; }
public decimal? TargetRemain { get; set; }
public DateTime? OpTime { get; set; }
public int? OpUserID { get; set; }
public string ScheduleID { get; set; }
public string Workstation { get; set; }
public DateTime? CheckDate { get; set; }
public string ExitID { get; set; }
public string Note { get; set; }
public string Verifiction { get; set; }
}
}
