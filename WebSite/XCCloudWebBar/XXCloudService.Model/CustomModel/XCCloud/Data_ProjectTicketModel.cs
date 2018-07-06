﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudWebBar.Common.Enum;
using XCCloudWebBar.Common;

namespace XCCloudWebBar.Model.CustomModel.XCCloud
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
        public int? WriteOffDays { get; set; }
        public DateTime? VaildStartDate { get; set; }
        public DateTime? VaildEndDate { get; set; }
        public string ValidDate
        {
            get
            {
                if (EffactType == (int)XCCloudWebBar.Common.Enum.EffactType.Period)
                    return "销售后" + ((EffactPeriodValue ?? 0) == 0 ? "当" : Convert.ToString(EffactPeriodValue)) + ((XCCloudWebBar.Common.Enum.FreqType?)EffactPeriodType).GetDescription()
                        + "生效，有效期" + (VaildPeriodValue ?? 0) + ((XCCloudWebBar.Common.Enum.FreqType?)VaildPeriodType).GetDescription();
                else if (EffactType == (int)XCCloudWebBar.Common.Enum.EffactType.Date)
                {
                    var validDate = Utils.ConvertFromDatetime(VaildStartDate, "yyyy-MM-dd") + "~" + Utils.ConvertFromDatetime(VaildEndDate, "yyyy-MM-dd");
                    if (validDate.Trim() == "~")
                        validDate = string.Empty;
                    return validDate;
                }
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
                if (WeekType == (int)XCCloudWebBar.Common.Enum.TimeType.Custom)
                    return Week;
                else if (WeekType != null)
                    return ((XCCloudWebBar.Common.Enum.TimeType?)WeekType).GetDescription();
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
                var periodLimit = Utils.TimeSpanToStr(StartTime) + "~" + Utils.TimeSpanToStr(EndTime);
                if (periodLimit.Trim() == "~")
                    periodLimit = string.Empty;
                return periodLimit;
            }
            set { }
        }
        public DateTime? NoStartDate { get; set; }
        public DateTime? NoEndDate { get; set; }
        public string NoDate
        {
            get
            {
                var noDate = Utils.ConvertFromDatetime(NoStartDate, "yyyy-MM-dd") + "~" + Utils.ConvertFromDatetime(NoEndDate, "yyyy-MM-dd");
                if (noDate.Trim() == "~")
                    noDate = string.Empty;
                return noDate;
            }
            set { }
        }
        public int? AllowExitTicket { get; set; }
        public int? ExitPeriodType { get; set; }
        public int? ExitPeriodValue { get; set; }
        public int? ExitTicketType { get; set; }
        public decimal? ExitTicketValue { get; set; }
        public string ExitLimit 
        {
            get 
            {
                if (AllowExitTicket == 1 && (ExitPeriodValue ?? 0) > 0)
                {
                    return "销售" + ExitPeriodValue + ((XCCloudWebBar.Common.Enum.FreqType?)ExitPeriodType).GetDescription()
                        + "后不可退票，退票手续费" + ((XCCloudWebBar.Common.Enum.ExitTicketType?)ExitTicketType).GetDescription() + Math.Round(ExitTicketValue ?? 0M, 2, MidpointRounding.AwayFromZero)
                        + (ExitTicketType == (int)XCCloudWebBar.Common.Enum.ExitTicketType.Money ? "元" :
                        ExitTicketType == (int)XCCloudWebBar.Common.Enum.ExitTicketType.Percent ? "%" : string.Empty) + "计";
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