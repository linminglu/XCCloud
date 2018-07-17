using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Data_PushRule
{
public int ID { get; set; }
public string MerchID { get; set; }
public string StoreID { get; set; }
public int? Allow_Out { get; set; }
public int? Allow_In { get; set; }
public int? WeekType { get; set; }
public string Week { get; set; }
public int? PushBalanceIndex1 { get; set; }
public int? PushCoin1 { get; set; }
public int? PushBalanceIndex2 { get; set; }
public int? PushCoin2 { get; set; }
public int? Level { get; set; }
public DateTime? StartTime { get; set; }
public DateTime? EndTime { get; set; }
public DateTime? StartDate { get; set; }
public DateTime? EndDate { get; set; }
public string Verifiction { get; set; }
}
}
