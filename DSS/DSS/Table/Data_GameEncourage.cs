using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Data_GameEncourage
{
public int? ID { get; set; }
public string MerchID { get; set; }
public string StoreID { get; set; }
public int? GameIndex { get; set; }
public DateTime? StartDate { get; set; }
public DateTime? EndDate { get; set; }
public int? WeekType { get; set; }
public string Week { get; set; }
public DateTime? StartTime { get; set; }
public DateTime? EndTime { get; set; }
public int? ContinueTimes { get; set; }
public string Note { get; set; }
public int? StartGames { get; set; }
public int? EndGames { get; set; }
public decimal? EncouragePrice { get; set; }
public int? State { get; set; }
public string Verifiction { get; set; }
}
}
