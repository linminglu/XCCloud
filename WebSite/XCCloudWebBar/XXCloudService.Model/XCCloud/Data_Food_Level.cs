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
    
    public partial class Data_Food_Level
    {
        public int ID { get; set; }
        public string MerchID { get; set; }
        public Nullable<int> FoodID { get; set; }
        public Nullable<int> MemberLevelID { get; set; }
        public Nullable<int> TimeType { get; set; }
        public string Week { get; set; }
        public Nullable<System.TimeSpan> StartTime { get; set; }
        public Nullable<System.TimeSpan> EndTime { get; set; }
        public Nullable<decimal> VIPPrice { get; set; }
        public Nullable<decimal> ClientPrice { get; set; }
        public Nullable<System.DateTime> StartDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public Nullable<int> AllFreqType { get; set; }
        public Nullable<int> AllCount { get; set; }
        public Nullable<int> MemberFreqType { get; set; }
        public Nullable<int> MemberCount { get; set; }
        public Nullable<int> UpdateLevelID { get; set; }
        public Nullable<int> PriorityLevel { get; set; }
        public string Verifiction { get; set; }
    }
}