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
    
    public partial class Base_GoodsInfo
    {
        public int ID { get; set; }
        public string StoreID { get; set; }
        public string Barcode { get; set; }
        public string MerchID { get; set; }
        public string GoodName { get; set; }
        public string GoodPhoteURL { get; set; }
        public Nullable<int> GoodType { get; set; }
        public Nullable<int> AllowStorage { get; set; }
        public Nullable<int> Status { get; set; }
        public string Note { get; set; }
    }
}
