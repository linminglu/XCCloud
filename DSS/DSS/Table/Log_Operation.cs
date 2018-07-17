using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Log_Operation
{
public string ID { get; set; }
public string StoreID { get; set; }
public DateTime? Realtime { get; set; }
public int? ScheduleID { get; set; }
public int? LogType { get; set; }
public string OperName { get; set; }
public string WorkStation { get; set; }
public int? UserID { get; set; }
public int? AuthorID { get; set; }
public string Content { get; set; }
}
}
