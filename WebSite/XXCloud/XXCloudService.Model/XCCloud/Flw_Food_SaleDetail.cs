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
    
    public partial class Flw_Food_SaleDetail
    {
        public string ID { get; set; }
        public string FlwFoodID { get; set; }
        public Nullable<int> FoodType { get; set; }
        public Nullable<int> ContainID { get; set; }
        public Nullable<int> ContainCount { get; set; }
        public Nullable<System.DateTime> ExpireDay { get; set; }
        public Nullable<int> ValidType { get; set; }
        public Nullable<int> Status { get; set; }
    }
}
