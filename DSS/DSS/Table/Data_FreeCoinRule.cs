using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Data_FreeCoinRule
{
public int? ID { get; set; }
public string MerchID { get; set; }
public string StoreID { get; set; }
public int? BalanceIndex { get; set; }
public int? OnceSigleMax { get; set; }
public int? OnceWarningValue { get; set; }
public int? DayMax { get; set; }
public int? DayWarningValue { get; set; }
public string Verifiction { get; set; }
}
}
