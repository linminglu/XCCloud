using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Base_Goodinfo_Price
{
public int ID { get; set; }
public string MerchID { get; set; }
public int? GoodID { get; set; }
public int? OperateTypei { get; set; }
public int? BalanceIndex { get; set; }
public decimal? CashValue { get; set; }
public decimal? Count { get; set; }
public string Verifiction { get; set; }
}
}
