//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace XCCloudWebBar.Model.XCGame
{
    using System;
    using System.Collections.Generic;
    
    public partial class t_game_free_rule
    {
        public int ID { get; set; }
        public string GameID { get; set; }
        public int MemberLevelID { get; set; }
        public System.DateTime StartTime { get; set; }
        public System.DateTime EndTime { get; set; }
        public int NeedCoin { get; set; }
        public int FreeCoin { get; set; }
        public int ExitCoin { get; set; }
        public string State { get; set; }
        public System.DateTime CreateTime { get; set; }
    }
}
