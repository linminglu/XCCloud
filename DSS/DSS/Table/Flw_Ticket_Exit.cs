using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Flw_Ticket_Exit
{
public int ID { get; set; }
public string StoreID { get; set; }
public string Segment { get; set; }
public string HeadAddress { get; set; }
public string Barcode { get; set; }
public int? Coins { get; set; }
public DateTime? RealTime { get; set; }
public decimal? CoinMoney { get; set; }
public int? UserID { get; set; }
public int? ScheduleID { get; set; }
public int? AuthorID { get; set; }
public string Note { get; set; }
public string WorkStation { get; set; }
public DateTime? ChargeTime { get; set; }
public int? State { get; set; }
public string PWD { get; set; }
public int? isNoAllow { get; set; }
public int? CardID { get; set; }
}
}
