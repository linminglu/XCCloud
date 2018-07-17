using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Flw_Jackpot
{
public int ID { get; set; }
public string MerchID { get; set; }
public string StoreID { get; set; }
public int? PrizeType { get; set; }
public string OrderID { get; set; }
public string MemberID { get; set; }
public string CardID { get; set; }
public int? MatrixID { get; set; }
public DateTime? RealTime { get; set; }
public int? SyncFlag { get; set; }
public string Verifiction { get; set; }
}
}
