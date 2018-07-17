using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Flw_Transfer
{
public string ID { get; set; }
public string MerchID { get; set; }
public string StoreID { get; set; }
public int? OpType { get; set; }
public string CardIDOut { get; set; }
public string OutMemberID { get; set; }
public string CardIDIn { get; set; }
public string InMemberID { get; set; }
public int? TransferBalanceIndex { get; set; }
public decimal? TransferCount { get; set; }
public decimal? BalanceOut { get; set; }
public decimal? BalanceIn { get; set; }
public DateTime? RealTime { get; set; }
public int? UserID { get; set; }
public string WorkStation { get; set; }
public string ScheduleID { get; set; }
public DateTime? CheckDate { get; set; }
public int? State { get; set; }
public string Note { get; set; }
public decimal? ChargeFee { get; set; }
public string OrderNumber { get; set; }
public int? SyncFlag { get; set; }
public string Verifiction { get; set; }
}
}
