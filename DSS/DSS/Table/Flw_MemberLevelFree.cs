using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Flw_MemberLevelFree
{
public string ID { get; set; }
public int? FreeID { get; set; }
public string MerchID { get; set; }
public string StoreID { get; set; }
public string MemberID { get; set; }
public decimal? ChargeTotal { get; set; }
public int? FreeBalanceType { get; set; }
public int? FreeCount { get; set; }
public int? MinSpaceDays { get; set; }
public int? OnceFreeCount { get; set; }
public int? CanGetCount { get; set; }
public DateTime? EndDate { get; set; }
public DateTime? GetFreeTime { get; set; }
public string Verifiction { get; set; }
}
}
