using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.Common.Enum;
using XCCloudService.Common;

namespace XCCloudService.Model.CustomModel.XCCloud
{
    public class Data_ProjectTicketList
    {
        public int ID { get; set; }
        public string TicketName { get; set; }
        public decimal? Price { get; set; }
        public int? EffactType { get; set; }
        public int? EffactPeriodType { get; set; }
        public int? EffactPeriodValue { get; set; }
        public int? VaildPeriodType { get; set; }
        public int? VaildPeriodValue { get; set; }
        public DateTime? VaildStartDate { get; set; }
        public DateTime? VaildEndDate { get; set; }
        public string ValidDate
        {
            get
            {
                if (EffactType == (int)XCCloudService.Common.Enum.EffactType.Period)
                    return "销售后" + EffactPeriodValue + ((XCCloudService.Common.Enum.FreqType?)EffactPeriodType).GetDescription()
                        + "生效，有效期" + VaildPeriodValue + ((XCCloudService.Common.Enum.FreqType?)VaildPeriodType).GetDescription();
                else if (EffactType == (int)XCCloudService.Common.Enum.EffactType.Date)
                    return Utils.ConvertFromDatetime(VaildStartDate, "yyyy-MM-dd") + "~" + Utils.ConvertFromDatetime(VaildEndDate, "yyyy-MM-dd");
                else
                    return string.Empty;
            }
            set { }
        }
        public int? WeekType { get; set; }
        public string Week { get; set; }
        public string WeekTypeStr 
        {
            get
            {
                if (WeekType == (int)XCCloudService.Common.Enum.TimeType.Custom)
                    return Week;
                else if (WeekType != null)
                    return ((XCCloudService.Common.Enum.TimeType?)WeekType).GetDescription();
                else
                    return string.Empty;
            }
            set { }
        }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string PeriodLimit 
        {
            get
            {
                return Utils.TimeSpanToStr(StartTime) + "~" + Utils.TimeSpanToStr(EndTime);
            }
            set { }
        }
        public DateTime? NoStartDate { get; set; }
        public DateTime? NoEndDate { get; set; }
        public string NoDate
        {
            get
            {
                return Utils.ConvertFromDatetime(NoStartDate, "yyyy-MM-dd") + "~" + Utils.ConvertFromDatetime(NoEndDate, "yyyy-MM-dd");
            }
            set { }
        }
        public int? AllowExitTicket { get; set; }
        public int? ExitPeriodType { get; set; }
        public int? ExitPeriodValue { get; set; }
        public int? ExitTicketType { get; set; }
        public int? ExitTicketValue { get; set; }
        public string ExitLimit 
        {
            get 
            {
                if (AllowExitTicket == 1)
                {
                    return "销售" + ExitPeriodValue + ((XCCloudService.Common.Enum.FreqType?)ExitPeriodType).GetDescription()
                        + "后不可退票，退票手续费" + ((XCCloudService.Common.Enum.ExitTicketType?)ExitTicketType).GetDescription() + ExitTicketValue
                        + (ExitTicketType == (int)XCCloudService.Common.Enum.ExitTicketType.Money ? "元" :
                        ExitTicketType == (int)XCCloudService.Common.Enum.ExitTicketType.Percent ? "%" : string.Empty) + "计";
                }
                else
                {
                    return "不允许退票";
                }
            }
            set { }
        }
        public string BindProjects { get; set; }
        public int? AllowExitTimes { get; set; }        
    }
}
