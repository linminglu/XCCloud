using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Divide_ProjectInfo
{
public int ID { get; set; }
public string MerchID { get; set; }
public string ProjectCode { get; set; }
public DateTime? CheckDate { get; set; }
public int? DivideType { get; set; }
public decimal? DividePrice { get; set; }
public decimal? TicketPrice { get; set; }
public string Verifiction { get; set; }
}
}
