using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
public class Data_ProjectTicket
{
public int? ID { get; set; }
public string MerchID { get; set; }
public string StoreID { get; set; }
public string TicketName { get; set; }
public int? TicketType { get; set; }
public int? DivideType { get; set; }
public int? BusinessType { get; set; }
public int? AllowExitTimes { get; set; }
public decimal? Price { get; set; }
public decimal? Tax { get; set; }
public int? GroupStartupCount { get; set; }
public int? ReadFace { get; set; }
public int? AllowCreatePoint { get; set; }
public int? ActiveBar { get; set; }
public int? SaleAuthor { get; set; }
public int? WriteOffDays { get; set; }
public int? EffactType { get; set; }
public int? EffactPeriodType { get; set; }
public int? EffactPeriodValue { get; set; }
public int? VaildPeriodType { get; set; }
public int? VaildPeriodValue { get; set; }
public DateTime? VaildStartDate { get; set; }
public DateTime? VaildEndDate { get; set; }
public int? WeekType { get; set; }
public string Week { get; set; }
public DateTime? StartTime { get; set; }
public DateTime? EndTime { get; set; }
public DateTime? NoStartDate { get; set; }
public DateTime? NoEndDate { get; set; }
public decimal? AccompanyCash { get; set; }
public int? BalanceIndex { get; set; }
public decimal? BalanceValue { get; set; }
public int? AllowExitTicket { get; set; }
public int? ExitPeriodType { get; set; }
public int? ExitPeriodValue { get; set; }
public int? ExitTicketType { get; set; }
public decimal? ExitTicketValue { get; set; }
public int? AllowRestrict { get; set; }
public int? RestrictShareCount { get; set; }
public int? RestrictPeriodType { get; set; }
public int? RestrictPreiodValue { get; set; }
public int? RestrctCount { get; set; }
public string Note { get; set; }
public string Verifiction { get; set; }
}
}
