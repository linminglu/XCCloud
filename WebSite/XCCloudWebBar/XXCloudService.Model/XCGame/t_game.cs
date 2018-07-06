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
    
    public partial class t_game
    {
        public string GameID { get; set; }
        public string GameName { get; set; }
        public string State { get; set; }
        public Nullable<int> PushReduceFromCard { get; set; }
        public Nullable<int> PushAddToGame { get; set; }
        public Nullable<int> OutReduceFromGame { get; set; }
        public Nullable<int> OutAddToCard { get; set; }
        public Nullable<int> AllowDecuplePush { get; set; }
        public Nullable<int> InsertCardOutCoin { get; set; }
        public Nullable<int> AllowRealPush { get; set; }
        public Nullable<int> AllowElecPush { get; set; }
        public Nullable<int> AllowRealOut { get; set; }
        public Nullable<int> AllowElecOut { get; set; }
        public Nullable<int> PushControl { get; set; }
        public Nullable<int> UseSecondPush { get; set; }
        public Nullable<int> SecondReduceFromCard { get; set; }
        public Nullable<int> SecondAddToGame { get; set; }
        public Nullable<int> AllowMemberOut { get; set; }
        public Nullable<int> DefaultPush { get; set; }
        public Nullable<int> OnceOutLimit { get; set; }
        public Nullable<int> OncePureOutLimit { get; set; }
        public Nullable<int> OnceOutValue { get; set; }
        public Nullable<int> OnceOutMaxLimit { get; set; }
        public Nullable<int> GuardConvertCard { get; set; }
        public Nullable<int> StrongGuardConvertCard { get; set; }
        public string GameType { get; set; }
        public Nullable<int> OutsideAlertCheck { get; set; }
        public Nullable<int> ReturnCheck { get; set; }
        public Nullable<int> BanOccupy { get; set; }
        public Nullable<int> ICTicketOperation { get; set; }
        public Nullable<int> SSRTimeOut { get; set; }
        public Nullable<int> NotGiveBack { get; set; }
        public Nullable<int> OnlyExitLottery { get; set; }
        public Nullable<int> LotteryMode { get; set; }
    }
}