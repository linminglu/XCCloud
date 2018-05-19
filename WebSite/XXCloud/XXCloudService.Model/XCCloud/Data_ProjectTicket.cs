﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace XCCloudService.Model.XCCloud
{
    using System;
    using System.Collections.Generic;
    
    public partial class Data_ProjectTicket
    {
        public int ID { get; set; }
        public string MerchID { get; set; }
        public string StoreID { get; set; }
        public string TicketName { get; set; }
        public Nullable<int> TicketType { get; set; }
        public Nullable<int> DivideType { get; set; }
        public Nullable<int> BusinessType { get; set; }
        public Nullable<int> AllowExitTimes { get; set; }
        public Nullable<decimal> Price { get; set; }
        public Nullable<decimal> Tax { get; set; }
        public Nullable<int> GroupStartupCount { get; set; }
        public Nullable<int> ReadFace { get; set; }
        public Nullable<int> AllowCreatePoint { get; set; }
        public Nullable<int> ActiveBar { get; set; }
        public Nullable<int> SaleAuthor { get; set; }
        public Nullable<int> WriteOffDays { get; set; }
        public Nullable<int> EffactType { get; set; }
        public Nullable<int> EffactPeriodType { get; set; }
        public Nullable<int> EffactPeriodValue { get; set; }
        public Nullable<int> VaildPeriodType { get; set; }
        public Nullable<int> VaildPeriodValue { get; set; }
        public Nullable<System.DateTime> VaildStartDate { get; set; }
        public Nullable<System.DateTime> VaildEndDate { get; set; }
        public Nullable<int> WeekType { get; set; }
        public string Week { get; set; }
        public Nullable<System.TimeSpan> StartTime { get; set; }
        public Nullable<System.TimeSpan> EndTime { get; set; }
        public Nullable<System.DateTime> NoStartDate { get; set; }
        public Nullable<System.DateTime> NoEndDate { get; set; }
        public Nullable<decimal> AccompanyCash { get; set; }
        public Nullable<int> BalanceIndex { get; set; }
        public Nullable<decimal> BalanceValue { get; set; }
        public Nullable<int> AllowExitTicket { get; set; }
        public Nullable<int> ExitPeriodType { get; set; }
        public Nullable<int> ExitPeriodValue { get; set; }
        public Nullable<int> ExitTicketType { get; set; }
        public Nullable<decimal> ExitTicketValue { get; set; }
        public Nullable<int> AllowRestrict { get; set; }
        public Nullable<int> RestrictShareCount { get; set; }
        public Nullable<int> RestrictPeriodType { get; set; }
        public Nullable<int> RestrictPreiodValue { get; set; }
        public Nullable<int> RestrctCount { get; set; }
        public string Note { get; set; }
    }
}
