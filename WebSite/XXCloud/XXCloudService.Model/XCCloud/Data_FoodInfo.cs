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
    
    public partial class Data_FoodInfo
    {
        public int FoodID { get; set; }
        public string FoodName { get; set; }
        public string MerchID { get; set; }
        public string Note { get; set; }
        public string ImageURL { get; set; }
        public Nullable<int> FoodType { get; set; }
        public Nullable<int> AllowInternet { get; set; }
        public string MeituanID { get; set; }
        public string DianpinID { get; set; }
        public string KoubeiID { get; set; }
        public Nullable<int> AllowPrint { get; set; }
        public Nullable<int> FoodState { get; set; }
        public Nullable<int> ForeAuthorize { get; set; }
        public Nullable<System.DateTime> StartTime { get; set; }
        public Nullable<System.DateTime> EndTime { get; set; }
        public Nullable<System.DateTime> ForbidStart { get; set; }
        public Nullable<System.DateTime> ForbidEnd { get; set; }
        public Nullable<decimal> ClientPrice { get; set; }
        public Nullable<decimal> MemberPrice { get; set; }
        public Nullable<int> RenewDays { get; set; }
    }
}
