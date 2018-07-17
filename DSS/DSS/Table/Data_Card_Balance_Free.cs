using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Data_Card_Balance_Free
{
public string ID { get; set; }
public string MerchID { get; set; }
public string CardIndex { get; set; }
public int? BalanceIndex { get; set; }
public decimal? Balance { get; set; }
public DateTime? UpdateTime { get; set; }
public string Verifiction { get; set; }
}
}
