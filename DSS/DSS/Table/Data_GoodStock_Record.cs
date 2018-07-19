using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Data_GoodStock_Record
{
public int? ID { get; set; }
public string MerchID { get; set; }
public string StoreID { get; set; }
public int? SourceType { get; set; }
public string SourceID { get; set; }
public int? GoodID { get; set; }
public int? DepotID { get; set; }
public int? StockFlag { get; set; }
public int? StockCount { get; set; }
public decimal? GoodCost { get; set; }
public DateTime? CreateTime { get; set; }
public string Note { get; set; }
public string Verifiction { get; set; }
}
}
