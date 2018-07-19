using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Data_BalanceChargeRule
{
public int? ID { get; set; }
public string MerchID { get; set; }
public string StoreID { get; set; }
public int? SourceType { get; set; }
public decimal? SourceCount { get; set; }
public int? ChargeType { get; set; }
public decimal? ChargeCount { get; set; }
public decimal? AlertValue { get; set; }
public string Verifiction { get; set; }
}
}
