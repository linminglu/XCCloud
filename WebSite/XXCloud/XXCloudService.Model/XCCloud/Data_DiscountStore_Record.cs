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
    
    public partial class Data_DiscountStore_Record
    {
        public int Id { get; set; }
        public Nullable<int> DiscountRuleID { get; set; }
        public Nullable<System.DateTime> RecordDate { get; set; }
        public string MerchId { get; set; }
        public string StoreId { get; set; }
        public Nullable<int> StoreFreq { get; set; }
        public Nullable<int> UseCount { get; set; }
    }
}
