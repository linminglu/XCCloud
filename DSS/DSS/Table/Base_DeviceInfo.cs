using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Base_DeviceInfo
{
public int? ID { get; set; }
public string MerchID { get; set; }
public string StoreID { get; set; }
public string DeviceName { get; set; }
public string SiteName { get; set; }
public int? type { get; set; }
public string Token { get; set; }
public int? GameIndexID { get; set; }
public int? BindDeviceID { get; set; }
public int? CmdType { get; set; }
public string segment { get; set; }
public string Address { get; set; }
public string MCUID { get; set; }
public string port_name { get; set; }
public int? baute_rate { get; set; }
public int? parity { get; set; }
public string IPAddress { get; set; }
public string WorkStation { get; set; }
public int? DeviceStatus { get; set; }
public DateTime? create_time { get; set; }
public DateTime? update_time { get; set; }
public string note { get; set; }
public int? AllowPrint { get; set; }
public string BarCode { get; set; }
}
}
