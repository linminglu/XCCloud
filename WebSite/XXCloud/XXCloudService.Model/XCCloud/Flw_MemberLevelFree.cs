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
    
    public partial class Flw_MemberLevelFree
    {
        public string ID { get; set; }
        public Nullable<int> FreeID { get; set; }
        public string MerchID { get; set; }
        public string StoreID { get; set; }
        public string MemberID { get; set; }
        public Nullable<decimal> ChargeTotal { get; set; }
        public Nullable<int> FreeBalanceType { get; set; }
        public Nullable<int> FreeCount { get; set; }
        public Nullable<int> MinSpaceDays { get; set; }
        public Nullable<int> OnceFreeCount { get; set; }
        public Nullable<int> CanGetCount { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public Nullable<System.DateTime> GetFreeTime { get; set; }
        public string Verifiction { get; set; }
        public string CardIndex { get; set; }
    }
}
