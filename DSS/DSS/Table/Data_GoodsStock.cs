using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Data_GoodsStock
{
public int? ID { get; set; }
public string MerchID { get; set; }
public string StoreID { get; set; }
public int? StockType { get; set; }
public int? StockIndex { get; set; }
public int? GoodID { get; set; }
public int? MinValue { get; set; }
public int? MaxValue { get; set; }
public int? InitialValue { get; set; }
public DateTime? InitialTime { get; set; }
public int? RemainCount { get; set; }
public decimal? InitialAvgValue { get; set; }
public string Note { get; set; }
public string Verifiction { get; set; }
}
}
