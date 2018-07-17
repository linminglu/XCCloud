using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Data_Card_BalanceCharge
{
public string ID { get; set; }
public string MerchID { get; set; }
public string StoreID { get; set; }
public string MemberID { get; set; }
public int? SourceBalanceIndex { get; set; }
public decimal? SourceCount { get; set; }
public decimal? SourceRemain { get; set; }
public int? TargetBalanceIndex { get; set; }
public decimal? TargetCount { get; set; }
public decimal? TargetRemain { get; set; }
public string OpStoreID { get; set; }
public DateTime? OpTime { get; set; }
public int? OpUserID { get; set; }
public int? ScheduleID { get; set; }
}
}
