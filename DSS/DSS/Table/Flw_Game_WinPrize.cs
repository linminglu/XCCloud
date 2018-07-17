using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Flw_Game_WinPrize
{
public int ID { get; set; }
public int? GameIndex { get; set; }
public int? GoodID { get; set; }
public string MemberID { get; set; }
public DateTime? WinTime { get; set; }
public int? HeadIndex { get; set; }
public decimal? GoodPrice { get; set; }
}
}
