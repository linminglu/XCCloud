using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Flw_Schedule
{
public string ID { get; set; }
public string MerchID { get; set; }
public string StoreID { get; set; }
public string ScheduleName { get; set; }
public DateTime? OpenTime { get; set; }
public DateTime? CheckDate { get; set; }
public int? State { get; set; }
public decimal? RealCash { get; set; }
public decimal? RealCredit { get; set; }
public int? AuthorID { get; set; }
public DateTime? ShiftTime { get; set; }
public string Note { get; set; }
public string Verifiction { get; set; }
}
}
