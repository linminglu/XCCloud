﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace XCCloudWebBar.Model.XCCloud
{
    using System;
    using System.Collections.Generic;
    
    public partial class Data_FreeGiveRule
    {
        public int ID { get; set; }
        public string MerchID { get; set; }
        public string RuleName { get; set; }
        public Nullable<System.DateTime> StartTime { get; set; }
        public Nullable<System.DateTime> EndTime { get; set; }
        public Nullable<int> FreeBalanceIndex { get; set; }
        public Nullable<int> FreeCount { get; set; }
        public Nullable<int> AllowGuest { get; set; }
        public Nullable<int> IDReadType { get; set; }
        public Nullable<int> PeriodType { get; set; }
        public Nullable<int> SpanCount { get; set; }
        public Nullable<int> SpanType { get; set; }
        public Nullable<int> GetTimes { get; set; }
        public Nullable<int> RuleLevel { get; set; }
        public Nullable<int> State { get; set; }
        public string Verifiction { get; set; }
    }
}