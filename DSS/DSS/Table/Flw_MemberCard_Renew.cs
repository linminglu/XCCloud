using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Flw_MemberCard_Renew
{
public string ID { get; set; }
public string MerchID { get; set; }
public string StoreID { get; set; }
public string MemberID { get; set; }
public string CardID { get; set; }
public DateTime? OldEndDate { get; set; }
public DateTime? NewEndDate { get; set; }
public string FoodSaleID { get; set; }
public decimal? RenewFee { get; set; }
public string WorkStation { get; set; }
public int? UserID { get; set; }
public string ScheduldID { get; set; }
public DateTime? CreateTime { get; set; }
public DateTime? CheckDate { get; set; }
public string Note { get; set; }
public string Verifiction { get; set; }
}
}
