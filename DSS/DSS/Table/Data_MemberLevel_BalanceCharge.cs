using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Data_MemberLevel_BalanceCharge
{
public int ID { get; set; }
public string MerchID { get; set; }
public int? MemberLevelID { get; set; }
public int? SourceBalanceIndex { get; set; }
public int? SourceCount { get; set; }
public int? TargetBalanceIndex { get; set; }
public int? TargetCount { get; set; }
}
}
