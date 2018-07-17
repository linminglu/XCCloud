using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Data_GoodRequest_List
{
public int ID { get; set; }
public string MerchID { get; set; }
public int? RequestID { get; set; }
public int? GoodID { get; set; }
public int? RequestCount { get; set; }
public int? SendCount { get; set; }
public int? StorageCount { get; set; }
public decimal? CostPrice { get; set; }
public decimal? Tax { get; set; }
public string InStoreID { get; set; }
public int? InDeportID { get; set; }
public string OutStoreID { get; set; }
public int? OutDepotID { get; set; }
public DateTime? SendTime { get; set; }
public int? LogistType { get; set; }
public string LogistOrderID { get; set; }
public string Verifiction { get; set; }
}
}
