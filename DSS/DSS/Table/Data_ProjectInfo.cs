using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Data_ProjectInfo
{
public int ID { get; set; }
public string MerchID { get; set; }
public string StoreID { get; set; }
public string ProjectName { get; set; }
public int? ProjectType { get; set; }
public int? AreaType { get; set; }
public int? ChargeType { get; set; }
public int? GameIndex { get; set; }
public decimal? GuestPrice { get; set; }
public int? ForceUse { get; set; }
public int? AdjOrder { get; set; }
public int? LockCard { get; set; }
public int? State { get; set; }
public string Note { get; set; }
public string Verifiction { get; set; }
}
}
