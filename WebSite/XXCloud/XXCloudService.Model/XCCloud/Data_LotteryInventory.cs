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
    
    public partial class Data_LotteryInventory
    {
        public int ID { get; set; }
        public string MerchID { get; set; }
        public string StoreID { get; set; }
        public Nullable<int> DeviceID { get; set; }
        public Nullable<int> PredictCount { get; set; }
        public string Startcode { get; set; }
        public string Endcode { get; set; }
        public Nullable<int> InventroyCount { get; set; }
        public Nullable<System.DateTime> InventoryTime { get; set; }
        public Nullable<int> UserID { get; set; }
        public string Note { get; set; }
        public string Verifiction { get; set; }
    }
}
