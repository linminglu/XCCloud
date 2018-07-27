using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Data_FoodInfo
{
public int? ID { get; set; }
public string FoodName { get; set; }
public string MerchID { get; set; }
public string Note { get; set; }
public string ImageURL { get; set; }
public int? FoodType { get; set; }
public int? AllowQuickFood { get; set; }
public int? AllowInternet { get; set; }
public string MeituanID { get; set; }
public string DianpinID { get; set; }
public string KoubeiID { get; set; }
public int? AllowPrint { get; set; }
public int? FoodState { get; set; }
public int? ForeAuthorize { get; set; }
public DateTime? StartTime { get; set; }
public DateTime? EndTime { get; set; }
public DateTime? ForbidStart { get; set; }
public DateTime? ForbidEnd { get; set; }
public decimal? ClientPrice { get; set; }
public decimal? MemberPrice { get; set; }
public decimal? Tax { get; set; }
public int? RenewDays { get; set; }
public int? ReturnTime { get; set; }
public int? TimeType { get; set; }
public decimal? ReturnFee { get; set; }
public int? FeeType { get; set; }
public string Verifiction { get; set; }
}
}
