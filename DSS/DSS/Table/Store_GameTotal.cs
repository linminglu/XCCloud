using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Store_GameTotal
{
public int? ID { get; set; }
public DateTime? CheckDate { get; set; }
public string GameiD { get; set; }
public int? CoinFromCard { get; set; }
public int? CoinFromDigit { get; set; }
public int? CoinFromReal { get; set; }
public int? CoinFromFree { get; set; }
public int? OutFromCard { get; set; }
public int? OutFromPrint { get; set; }
public int? OutFromReal { get; set; }
}
}
