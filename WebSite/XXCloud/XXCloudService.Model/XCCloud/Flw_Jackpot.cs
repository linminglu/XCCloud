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
    
    public partial class Flw_Jackpot
    {
        public int ID { get; set; }
        public string MerchID { get; set; }
        public string StoreID { get; set; }
        public Nullable<int> PrizeType { get; set; }
        public string OrderID { get; set; }
        public string MemberID { get; set; }
        public string CardID { get; set; }
        public Nullable<int> MatrixID { get; set; }
        public Nullable<System.DateTime> RealTime { get; set; }
        public Nullable<int> SyncFlag { get; set; }
        public string Verifiction { get; set; }
    }
}
