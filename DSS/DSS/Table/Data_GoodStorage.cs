using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Data_GoodStorage
{
public int? ID { get; set; }
public string MerchID { get; set; }
public string StoreID { get; set; }
public string StorageOrderID { get; set; }
public int? DepotID { get; set; }
public decimal? Payable { get; set; }
public decimal? Payment { get; set; }
public decimal? Discount { get; set; }
public int? AuthorFlag { get; set; }
public int? AuthorID { get; set; }
public string Supplier { get; set; }
public int? UserID { get; set; }
public DateTime? RealTime { get; set; }
public DateTime? CheckDate { get; set; }
public string Note { get; set; }
public string Verifiction { get; set; }
}
}
