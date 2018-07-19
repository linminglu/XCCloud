using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Flw_Giveback
{
public int? ID { get; set; }
public string StoreID { get; set; }
public int? CardID { get; set; }
public int? PICCardID { get; set; }
public string MemberName { get; set; }
public DateTime? RealTime { get; set; }
public int? Coins { get; set; }
public int? AuthorID { get; set; }
public int? UserID { get; set; }
public int? ScheduleID { get; set; }
public string WorkStation { get; set; }
public DateTime? ExitRealTime { get; set; }
public int? ExitAuthorID { get; set; }
public int? ExitUserID { get; set; }
public int? ExitScheduleID { get; set; }
public string ExitWorkStation { get; set; }
public decimal? ExitMoney { get; set; }
public int? ExitAllCoins { get; set; }
public int? ExitRealCoins { get; set; }
public int? ExitBaseCoins { get; set; }
public int? ExitState { get; set; }
public int? ExitMin { get; set; }
public string ExitBackPrincipal { get; set; }
public decimal? WinMoney { get; set; }
public int? MayCoins { get; set; }
public DateTime? LastTime { get; set; }
public int? Balance { get; set; }
}
}
