using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Data_JackpotInfo
{
public int ID { get; set; }
public string MerchID { get; set; }
public string ActiveName { get; set; }
public int? Threshold { get; set; }
public int? Concerned { get; set; }
public DateTime? StartTime { get; set; }
public DateTime? EndTime { get; set; }
public string Note { get; set; }
public string Verifiction { get; set; }
}
}
