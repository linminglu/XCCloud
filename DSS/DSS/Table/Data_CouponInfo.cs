using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Data_CouponInfo
{
public int ID { get; set; }
public string MerchID { get; set; }
public int? CouponLevel { get; set; }
public string CouponName { get; set; }
public int? CouponType { get; set; }
public int? EntryCouponFlag { get; set; }
public int? AuthorFlag { get; set; }
public int? AllowOverOther { get; set; }
public int? OverUseCount { get; set; }
public int? PublishCount { get; set; }
public decimal? CouponValue { get; set; }
public decimal? CouponDiscount { get; set; }
public decimal? CouponThreshold { get; set; }
public DateTime? StartDate { get; set; }
public DateTime? EndDate { get; set; }
public int? WeekType { get; set; }
public string Week { get; set; }
public DateTime? StartTime { get; set; }
public DateTime? EndTime { get; set; }
public DateTime? NoStartDate { get; set; }
public DateTime? NoEndDate { get; set; }
public int? SendType { get; set; }
public decimal? OverMoney { get; set; }
public int? FreeCouponCount { get; set; }
public int? JackpotCount { get; set; }
public int? JackpotID { get; set; }
public int? ChargeType { get; set; }
public int? ChargeCount { get; set; }
public int? BalanceIndex { get; set; }
public int? GoodID { get; set; }
public int? ProjectID { get; set; }
public int? AutoSendCycle { get; set; }
public int? AutoSendValue { get; set; }
public int? AutoSendCount { get; set; }
public DateTime? CreateTime { get; set; }
public int? OpUserID { get; set; }
public string Context { get; set; }
public string Verifiction { get; set; }
}
}
