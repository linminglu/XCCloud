using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Flw_Food_ExitDetail
{
public string ID { get; set; }
public string MerchID { get; set; }
public string ExitID { get; set; }
public int FoodType { get; set; }
public int ContainID { get; set; }
public int ContainCount { get; set; }
public DateTime ExpireDay { get; set; }
public int ValidType { get; set; }
public int Status { get; set; }
public int SyncFlag { get; set; }
public string Verifiction { get; set; }
}
}
