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
    
    public partial class Flw_485_SaveCoin
    {
        public int ID { get; set; }
        public string StoreID { get; set; }
        public string Segment { get; set; }
        public string HeadAddress { get; set; }
        public Nullable<int> CardID { get; set; }
        public Nullable<int> Coins { get; set; }
        public Nullable<int> Balance { get; set; }
        public Nullable<System.DateTime> RealTime { get; set; }
    }
}
