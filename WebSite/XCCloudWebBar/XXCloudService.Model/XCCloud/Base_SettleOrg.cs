﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace XCCloudWebBar.Model.XCCloud
{
    using System;
    using System.Collections.Generic;
    
    public partial class Base_SettleOrg
    {
        public int ID { get; set; }
        public string MerchID { get; set; }
        public string WXPayOpenID { get; set; }
        public string WXName { get; set; }
        public string AliPay { get; set; }
        public string AliPayName { get; set; }
        public Nullable<decimal> SettleFee { get; set; }
        public Nullable<int> SettleCycle { get; set; }
        public Nullable<int> SettleCount { get; set; }
        public string Verifiction { get; set; }
    }
}
