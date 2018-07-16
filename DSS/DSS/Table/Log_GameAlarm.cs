using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Log_GameAlarm
{
public string ID { get; set; }
public string MerchID { get; set; }
public string StoreID { get; set; }
public string ICCardID { get; set; }
public string Segment { get; set; }
public string HeadAddress { get; set; }
public int AlertType { get; set; }
public int GameIndex { get; set; }
public string SiteName { get; set; }
public int DeviceID { get; set; }
public DateTime HappenTime { get; set; }
public DateTime EndTime { get; set; }
public int State { get; set; }
public int LockGame { get; set; }
public int LockMember { get; set; }
public string AlertContent { get; set; }
public string Verifiction { get; set; }
}
}
