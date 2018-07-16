using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Data_WorkFlow_Node
{
public int ID { get; set; }
public int WorkID { get; set; }
public int OrderNumber { get; set; }
public int NodeType { get; set; }
public int UserType { get; set; }
public int AuthorFlag { get; set; }
public int Timeout { get; set; }
}
}
