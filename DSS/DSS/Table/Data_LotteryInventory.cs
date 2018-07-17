using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Data_LotteryInventory
{
public int ID { get; set; }
public string MerchID { get; set; }
public string StoreID { get; set; }
public int? DeviceID { get; set; }
public int? PredictCount { get; set; }
public string Startcode { get; set; }
public string Endcode { get; set; }
public int? InventroyCount { get; set; }
public DateTime? InventoryTime { get; set; }
public int? UserID { get; set; }
public string Note { get; set; }
public string Verifiction { get; set; }
}
}
