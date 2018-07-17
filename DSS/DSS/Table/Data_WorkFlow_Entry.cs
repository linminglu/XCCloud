using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Data_WorkFlow_Entry
{
public int ID { get; set; }
public int? WorkID { get; set; }
public int? EventID { get; set; }
public int? EventType { get; set; }
public int? NodeID { get; set; }
public int? UserID { get; set; }
public int? State { get; set; }
public DateTime? CreateTime { get; set; }
public int? AuthorID { get; set; }
public string Note { get; set; }
}
}
