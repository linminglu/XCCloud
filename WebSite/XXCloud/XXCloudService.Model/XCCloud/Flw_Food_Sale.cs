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
    
    public partial class Flw_Food_Sale
    {
        public string ID { get; set; }
        public string StoreID { get; set; }
        public Nullable<int> FlowType { get; set; }
        public Nullable<int> SingleType { get; set; }
        public Nullable<int> FoodID { get; set; }
        public Nullable<int> SaleCount { get; set; }
        public Nullable<int> Point { get; set; }
        public Nullable<int> PointBalance { get; set; }
        public Nullable<int> MemberLevelID { get; set; }
        public Nullable<decimal> Deposit { get; set; }
        public Nullable<decimal> OpenFee { get; set; }
        public Nullable<decimal> RenewFee { get; set; }
        public Nullable<decimal> ChangeFee { get; set; }
        public Nullable<decimal> TotalMoney { get; set; }
        public string Note { get; set; }
        public Nullable<int> BuyFoodType { get; set; }
        public string MerchID { get; set; }
        public Nullable<decimal> TaxFee { get; set; }
        public Nullable<decimal> TaxTotal { get; set; }
        public string Verifiction { get; set; }
    }
}
