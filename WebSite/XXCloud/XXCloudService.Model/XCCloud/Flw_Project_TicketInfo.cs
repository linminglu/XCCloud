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
    
    public partial class Flw_Project_TicketInfo
    {
        public int ID { get; set; }
        public string MerchID { get; set; }
        public string FoodSaleID { get; set; }
        public string CardID { get; set; }
        public string MemberID { get; set; }
        public string ParentID { get; set; }
        public Nullable<int> TicketType { get; set; }
        public Nullable<int> NeedActive { get; set; }
        public string Barcode { get; set; }
        public Nullable<System.DateTime> SaleTime { get; set; }
        public Nullable<System.DateTime> FirstUseTime { get; set; }
        public Nullable<int> WriteOffDays { get; set; }
        public Nullable<System.DateTime> EndTime { get; set; }
        public Nullable<int> State { get; set; }
        public Nullable<int> BuyCount { get; set; }
        public Nullable<int> RemainCount { get; set; }
        public Nullable<decimal> BuyPrice { get; set; }
        public Nullable<int> BalanceIndex { get; set; }
        public Nullable<decimal> BalanceCount { get; set; }
        public Nullable<decimal> RemainDividePrice { get; set; }
        public string Verifiction { get; set; }
    }
}
