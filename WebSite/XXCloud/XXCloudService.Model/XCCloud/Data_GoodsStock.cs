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
    
    public partial class Data_GoodsStock
    {
        public string ID { get; set; }
        public string DepotID { get; set; }
        public string GoodID { get; set; }
        public Nullable<int> MinValue { get; set; }
        public Nullable<int> MaxValue { get; set; }
        public Nullable<int> InitialValue { get; set; }
        public Nullable<System.DateTime> InitialTime { get; set; }
        public Nullable<int> RemainCount { get; set; }
        public Nullable<decimal> InitialAvgValue { get; set; }
        public string Note { get; set; }
    }
}
