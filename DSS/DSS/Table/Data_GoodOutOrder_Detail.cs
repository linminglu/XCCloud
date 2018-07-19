using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Data_GoodOutOrder_Detail
{
public int? ID { get; set; }
public string MerchID { get; set; }
public string StoreID { get; set; }
public string OrderID { get; set; }
public int? DepotID { get; set; }
public int? GoodID { get; set; }
public int? OutCount { get; set; }
public decimal? OutPrice { get; set; }
public decimal? OutTotal { get; set; }
public string Note { get; set; }
public string Verifiction { get; set; }
}
}
