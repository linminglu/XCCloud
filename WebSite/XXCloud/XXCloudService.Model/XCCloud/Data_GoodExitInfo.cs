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
    
    public partial class Data_GoodExitInfo
    {
        public int ID { get; set; }
        public string ExitOrderID { get; set; }
        public string StorageOrderID { get; set; }
        public Nullable<int> DepotID { get; set; }
        public Nullable<int> ExitCount { get; set; }
        public Nullable<decimal> ExitCost { get; set; }
        public Nullable<decimal> ExitTotal { get; set; }
        public string Note { get; set; }
    }
}