using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Flw_Good_Buyback
{
public string ID { get; set; }
public string MerchID { get; set; }
public string StoreID { get; set; }
public string MemberID { get; set; }
public string CardID { get; set; }
public int? GoodID { get; set; }
public int? GoodCount { get; set; }
public int? BuybackBalanceIndex { get; set; }
public decimal? BalanceCount { get; set; }
public decimal? Balance { get; set; }
public DateTime? OpTime { get; set; }
public int? OpUserID { get; set; }
public string ScheduleID { get; set; }
public string Workstation { get; set; }
public DateTime? CheckDate { get; set; }
public string Note { get; set; }
public int? SyncFlag { get; set; }
public string Verifiction { get; set; }
}
}
