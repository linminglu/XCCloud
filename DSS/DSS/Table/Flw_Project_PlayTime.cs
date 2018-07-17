using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Flw_Project_PlayTime
{
public int ID { get; set; }
public int? PID { get; set; }
public string StoreID { get; set; }
public int? CardID { get; set; }
public int? ProjectID { get; set; }
public int? DeviceID { get; set; }
public int? RecordType { get; set; }
public int? RecordChannel { get; set; }
public DateTime? RecordTime { get; set; }
public int? ChargeType { get; set; }
public int? CycleType { get; set; }
public int? LockMember { get; set; }
public int? FreeMinute { get; set; }
public decimal? BasePrice { get; set; }
public int? CycleTimes { get; set; }
public decimal? TopPrice { get; set; }
public int? OutChargeType { get; set; }
public int? ChargeBalanceIndex { get; set; }
public decimal? ChargeCount { get; set; }
public decimal? Balance { get; set; }
public decimal? Cash { get; set; }
public int? PayType { get; set; }
public string PayOrderID { get; set; }
public int? PayState { get; set; }
public string QrCode { get; set; }
}
}
