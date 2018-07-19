using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Data_MessageInfo
{
public int? ID { get; set; }
public string MerchID { get; set; }
public string StoreID { get; set; }
public int? ChannelType { get; set; }
public int? SendType { get; set; }
public DateTime? SentTime { get; set; }
public string RecvName { get; set; }
public string Context { get; set; }
}
}
