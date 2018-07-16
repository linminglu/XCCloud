using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Sync_DataList
{
public int ID { get; set; }
public string MerchID { get; set; }
public string StoreID { get; set; }
public string SN { get; set; }
public string TableName { get; set; }
public string IDValue { get; set; }
public int SyncType { get; set; }
public DateTime CreateTime { get; set; }
public DateTime SyncTime { get; set; }
public int SyncFlag { get; set; }
public string Verifiction { get; set; }
}
}
