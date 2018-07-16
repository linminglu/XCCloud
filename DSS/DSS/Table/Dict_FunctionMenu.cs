using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Dict_FunctionMenu
{
public int ID { get; set; }
public int ParentID { get; set; }
public string FunctionName { get; set; }
public int OrderID { get; set; }
public string Descript { get; set; }
public string PageName { get; set; }
public string ICON { get; set; }
public int MenuType { get; set; }
public int UseType { get; set; }
}
}
