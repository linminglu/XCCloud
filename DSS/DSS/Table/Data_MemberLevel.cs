using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
    public class Data_MemberLevel
    {
        public int ID { get; set; }
        public string MerchID { get; set; }
        public string MemberLevelName { get; set; }
        public string CoverURL { get; set; }
        public decimal OpenFee { get; set; }
        public decimal Deposit { get; set; }
        public int ClearPointDays { get; set; }
        public int Validday { get; set; }
        public int NeedAuthor { get; set; }
        public int MustPhone { get; set; }
        public int PhoneOnly { get; set; }
        public int MustIDCard { get; set; }
        public int UseReadID { get; set; }
        public int IDCardOnly { get; set; }
        public int ReadFace { get; set; }
        public int ReadPlam { get; set; }
        public decimal ChangeFee { get; set; }
        public decimal RechargeFee { get; set; }
        public decimal ContinueFee { get; set; }
        public int ContinueUsePoint { get; set; }
        public decimal ConsumeTotle { get; set; }
        public int UpdateUsePoint { get; set; }
        public int UpdateLevelID { get; set; }
        public int NonActiveDays { get; set; }
        public int ReduceLevelID { get; set; }
        public int FreeBalanceIndex { get; set; }
        public int WinBalanceIndex { get; set; }
        public int FreeCoin { get; set; }
        public int FreeType { get; set; }
        public int FreeNeedWin { get; set; }
        public int BirthdayFree { get; set; }
        public int AllCreatePoint { get; set; }
        public decimal CashBase { get; set; }
        public int PointBalanceIndex { get; set; }
        public decimal PointGet { get; set; }
        public int FoodID { get; set; }
        public int MinCoin { get; set; }
        public int MaxCoin { get; set; }
        public int AllowExitCard { get; set; }
        public int AllowExitMoney { get; set; }
        public int AllowExitCoinToCard { get; set; }
        public int LockHead { get; set; }
        public int AllowCharge { get; set; }
        public int AllowSaveLottery { get; set; }
        public int AllowSaveCoin { get; set; }
        public int AllowGetCoin { get; set; }
        public int AllowTransferOut { get; set; }
        public int TransferOutLevelID { get; set; }
        public int AllowTransferIn { get; set; }
        public int TransferInLevelID { get; set; }
        public int State { get; set; }
        public string Verifiction { get; set; }
    }
}
