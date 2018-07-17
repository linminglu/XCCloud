using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Base_DeviceInfo_Ext
{
public int ID { get; set; }
public int? DeviceID { get; set; }
public int? Motor1EN { get; set; }
public int? Motor2EN { get; set; }
public int? Motor1Coin { get; set; }
public int? Motor2Coin { get; set; }
public int? MaxSaveCount { get; set; }
public int? FromDevice { get; set; }
public decimal? ToCard { get; set; }
public int? BalanceIndex { get; set; }
public int? SSRLevel { get; set; }
public int? DigitCoinEN { get; set; }
public int? DubleCheck { get; set; }
}
}
