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
    
    public partial class Data_GameFreeRule
    {
        public int ID { get; set; }
        public Nullable<int> RuleType { get; set; }
        public Nullable<int> MemberLevelID { get; set; }
        public Nullable<System.DateTime> StartTime { get; set; }
        public Nullable<System.DateTime> EndTime { get; set; }
        public Nullable<int> NeedCoin { get; set; }
        public Nullable<int> FreeCoin { get; set; }
        public Nullable<int> ExitCoin { get; set; }
        public Nullable<int> State { get; set; }
        public Nullable<System.DateTime> CreateTime { get; set; }
    }
}
