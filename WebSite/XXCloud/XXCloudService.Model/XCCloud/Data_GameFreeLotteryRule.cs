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
    
    public partial class Data_GameFreeLotteryRule
    {
        public int ID { get; set; }
        public string MerchID { get; set; }
        public string StoreID { get; set; }
        public Nullable<int> GameIndex { get; set; }
        public Nullable<int> MemberLevelID { get; set; }
        public Nullable<int> BaseLottery { get; set; }
        public Nullable<int> FreeCount { get; set; }
        public string Verifiction { get; set; }
    }
}
