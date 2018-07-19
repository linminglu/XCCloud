using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Data_Message
{
public int? ID { get; set; }
public int? Sender { get; set; }
public int? SenderType { get; set; }
public int? Receiver { get; set; }
public int? RecvType { get; set; }
public int? MsgType { get; set; }
public DateTime? SendTime { get; set; }
public int? ReadFlag { get; set; }
public DateTime? ReadTime { get; set; }
public string MsgText { get; set; }
}
}
