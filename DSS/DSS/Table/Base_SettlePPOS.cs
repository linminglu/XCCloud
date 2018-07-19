using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Base_SettlePPOS
{
public int? ID { get; set; }
public string MerchNo { get; set; }
public string TerminalNo { get; set; }
public string Token { get; set; }
public string InstNo { get; set; }
public decimal? SettleFee { get; set; }
}
}
