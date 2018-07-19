using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Data_GoodExitInfo
{
public int? ID { get; set; }
public string MerchID { get; set; }
public string StoreID { get; set; }
public string ExitOrderID { get; set; }
public int? SourceType { get; set; }
public string SourceOrderID { get; set; }
public DateTime? ExitTime { get; set; }
public int? UserID { get; set; }
public DateTime? CheckDate { get; set; }
public int? DepotID { get; set; }
public int? ExitCount { get; set; }
public decimal? ExitCost { get; set; }
public decimal? ExitTotal { get; set; }
public int? LogistType { get; set; }
public string LogistOrderID { get; set; }
public string Note { get; set; }
public string Verifiction { get; set; }
}
}
