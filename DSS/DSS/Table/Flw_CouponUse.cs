using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Flw_CouponUse
{
public string ID { get; set; }
public string StoreID { get; set; }
public string OrderFlwID { get; set; }
public int? DeviceID { get; set; }
public int? CouponID { get; set; }
public string CouponCode { get; set; }
public decimal? FreeMoney { get; set; }
public int? Coins { get; set; }
public DateTime? UseTime { get; set; }
public int? SyncFlag { get; set; }
public string Verifiction { get; set; }
}
}
