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
    
    public partial class Data_DiscountRule
    {
        public int ID { get; set; }
        public string MerchID { get; set; }
        public string RuleName { get; set; }
        public Nullable<int> RuleLevel { get; set; }
        public Nullable<System.DateTime> StartDate { get; set; }
        public Nullable<System.DateTime> Enddate { get; set; }
        public Nullable<int> WeekType { get; set; }
        public string Week { get; set; }
        public Nullable<System.TimeSpan> StartTime { get; set; }
        public Nullable<System.TimeSpan> EndTime { get; set; }
        public Nullable<System.DateTime> NoStartDate { get; set; }
        public Nullable<System.DateTime> NoEndDate { get; set; }
        public Nullable<int> StoreFreq { get; set; }
        public Nullable<int> StoreCount { get; set; }
        public Nullable<int> ShareCount { get; set; }
        public Nullable<int> MemberFreq { get; set; }
        public Nullable<int> MemberCount { get; set; }
        public Nullable<int> AllowGuest { get; set; }
        public string Note { get; set; }
        public Nullable<int> State { get; set; }
        public string Verifiction { get; set; }
    }
}
