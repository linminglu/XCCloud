using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Flw_Order
{
public string ID { get; set; }
public string MerchID { get; set; }
public string StoreID { get; set; }
public int FoodCount { get; set; }
public int GoodCount { get; set; }
public string MemberID { get; set; }
public string CardID { get; set; }
public int OrderSource { get; set; }
public DateTime CreateTime { get; set; }
public DateTime ModifyTime { get; set; }
public DateTime PayTime { get; set; }
public int PayType { get; set; }
public decimal PayCount { get; set; }
public decimal RealPay { get; set; }
public decimal FreePay { get; set; }
public int UserID { get; set; }
public string ScheduleID { get; set; }
public string WorkStation { get; set; }
public int AuthorID { get; set; }
public int OrderStatus { get; set; }
public int SettleFlag { get; set; }
public string Note { get; set; }
public decimal PayFee { get; set; }
public string OrderNumber { get; set; }
public decimal ExitPrice { get; set; }
public decimal ExitFee { get; set; }
public DateTime ExitTime { get; set; }
public string Verifiction { get; set; }
}
}
