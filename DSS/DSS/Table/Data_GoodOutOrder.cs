using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Data_GoodOutOrder
{
public int? ID { get; set; }
public string MerchID { get; set; }
public string StoreID { get; set; }
public string OrderID { get; set; }
public int? OrderType { get; set; }
public int? DepotID { get; set; }
public DateTime? CreateTime { get; set; }
public int? LogistType { get; set; }
public string LogistOrderID { get; set; }
public int? OPUserID { get; set; }
public int? AuthorID { get; set; }
public DateTime? AuthorTime { get; set; }
public int? CancelUserID { get; set; }
public DateTime? CancelTime { get; set; }
public string WorkStation { get; set; }
public DateTime? CheckDate { get; set; }
public int? State { get; set; }
public string Note { get; set; }
public string Verifiction { get; set; }
}
}
