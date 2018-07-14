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
    
    public partial class Flw_Transfer
    {
        public string ID { get; set; }
        public string MerchID { get; set; }
        public string StoreID { get; set; }
        public Nullable<int> OpType { get; set; }
        public string CardIDOut { get; set; }
        public string OutMemberID { get; set; }
        public string CardIDIn { get; set; }
        public string InMemberID { get; set; }
        public Nullable<int> TransferBalanceIndex { get; set; }
        public Nullable<decimal> TransferCount { get; set; }
        public Nullable<decimal> BalanceOut { get; set; }
        public Nullable<decimal> BalanceIn { get; set; }
        public Nullable<System.DateTime> RealTime { get; set; }
        public Nullable<int> UserID { get; set; }
        public string WorkStation { get; set; }
        public string ScheduleID { get; set; }
        public Nullable<System.DateTime> CheckDate { get; set; }
        public Nullable<int> State { get; set; }
        public string Note { get; set; }
        public Nullable<decimal> ChargeFee { get; set; }
        public string OrderNumber { get; set; }
        public Nullable<int> SyncFlag { get; set; }
        public string Verifiction { get; set; }
    }
}
