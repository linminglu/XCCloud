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
    
    public partial class Flw_MemberCard_BalanceCharge
    {
        public string ID { get; set; }
        public string MerchID { get; set; }
        public string StoreID { get; set; }
        public string MemberID { get; set; }
        public string CardIndex { get; set; }
        public Nullable<int> SourceBalanceIndex { get; set; }
        public Nullable<decimal> SourceCount { get; set; }
        public Nullable<decimal> SourceRemain { get; set; }
        public Nullable<int> TargetBalanceIndex { get; set; }
        public Nullable<decimal> TargetCount { get; set; }
        public Nullable<decimal> TargetRemain { get; set; }
        public Nullable<System.DateTime> OpTime { get; set; }
        public Nullable<int> OpUserID { get; set; }
        public string ScheduleID { get; set; }
        public string Workstation { get; set; }
        public Nullable<System.DateTime> CheckDate { get; set; }
        public string ExitID { get; set; }
        public string Note { get; set; }
        public string Verifiction { get; set; }
    }
}
