using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class XC_WorkInfo
{
public int ID { get; set; }
public int? WorkType { get; set; }
public int? SenderID { get; set; }
public DateTime? SenderTime { get; set; }
public int? WorkState { get; set; }
public string WorkBody { get; set; }
public int? AuditorID { get; set; }
public DateTime? AuditTime { get; set; }
public string AuditBody { get; set; }
}
}
