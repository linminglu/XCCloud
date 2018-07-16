using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Flw_MemberCard_LevelChange
{
public string ID { get; set; }
public string MerchID { get; set; }
public string StoreID { get; set; }
public string MemberID { get; set; }
public string ICCardID { get; set; }
public int OldMemberLevelID { get; set; }
public int NewMemberLevleID { get; set; }
public int ChangeType { get; set; }
public string OrderID { get; set; }
public DateTime OpTime { get; set; }
public int OpUserID { get; set; }
public string ScheduleID { get; set; }
public string Workstation { get; set; }
public DateTime CheckDate { get; set; }
public string Note { get; set; }
public string Verifiction { get; set; }
}
}
