using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudWebBar.Model.CustomModel.XCCloud
{
    public class MemberBalancesModel
    {
        public int BalanceIndex { get; set; }

        public string BanlanceName { get; set; }

        public decimal Banlance { get; set; }

        public int HKType { get; set; }
    }

    [DataContract]
    public class MemberBalanceExchangeRateModel
    {
        public MemberBalanceExchangeRateModel()
        {
            this.DecimalNumber = 0;
            this.TargetTypeName = string.Empty;
        }
        //[DataMember(Name = "BalanceId", Order = 0)]
        //public string BalanceId { get; set; }
        //[DataMember(Name = "BalanceFreeId", Order = 0)]
        //public string BalanceFreeId { get; set; }

        [DataMember(Name = "BalanceIndex", Order = 0)]
        public int BalanceIndex { get; set; }

        [DataMember(Name = "TypeName", Order = 1)]
        public string TypeName { get; set; }

        [DataMember(Name = "Total", Order = 2)]
        public decimal Total { get; set; }

        [DataMember(Name = "Balance", Order = 3)]
        public decimal Balance { get; set; }

        [DataMember(Name = "BalanceFree", Order = 4)]
        public decimal BalanceFree { get; set; }

        [DataMember(Name = "TargetBalanceIndex", Order = 5)]
        public int TargetBalanceIndex { get; set; }

        [DataMember(Name = "TargetTypeName", Order = 6)]
        public string TargetTypeName { get; set; }

        [DataMember(Name = "ExchangeRate", Order = 7)]
        public decimal ExchangeRate { get; set; }

        [DataMember(Name = "DecimalNumber", Order = 8)]
        public int DecimalNumber { get; set; }

        [DataMember(Name = "AddingType", Order = 9)]
        public int AddingType { get; set; }

        //[DataMember(Name = "SourceCount", Order = 5)]
        //public int SourceCount { get; set; }

        //[DataMember(Name = "TargetCount", Order = 6)]
        //public int TargetCount { get; set; }

        //[DataMember(Name = "IsExchange", Order = 7)]
        //public int IsExchange { get; set; }
    }

    public class BalanceExchangeDataModel
    {
        public int BalanceIndex { get; set; }

        public string TypeName { get; set; }

        public decimal ExchangeQty { get; set; }

        public decimal ExchangeValue { get; set; }
    }

    public class CardDepositDataModel
    {
        public string CardId { get; set; }

        public string ICCardID { get; set; }

        public int CardType { get; set; }

        public int Deposit { get; set; }
    }

    public class BackDataModel
    {
        public decimal BackTotal { get; set; }

        public List<BalanceExchangeDataModel> BalanceExchanges { get; set; }

        public List<CardDepositDataModel> CardDeposits { get; set; }
    }

    [DataContract]
    public class MemberCardInfoViewModel
    {
        public MemberCardInfoViewModel()
        {
            MemberBalances = new List<BalanceModel>();
        }

        /// <summary>
        /// 会员卡ID
        /// </summary>
        [DataMember(Order = 0)]
        public string CardId { get; set; }

        /// <summary>
        /// 会员卡编号
        /// </summary>
        [DataMember(Order = 1)]
        public string ICCardId { get; set; }

        /// <summary>
        /// 等级
        /// </summary>
        [DataMember(Order = 2)]
        public string LevelName { get; set; }

        /// <summary>
        /// 级别
        /// </summary>
        [DataMember(Order = 2)]
        public int MemberLevelId { get; set; }

        /// <summary>
        /// 押金
        /// </summary>
        [DataMember(Order = 2)]
        public int Deposit { get; set; }

        /// <summary>
        /// 期限
        /// </summary>
        [DataMember(Order = 2)]
        public string EndDate { get; set; }

        /// <summary>
        /// 主卡头像
        /// </summary>
        [DataMember(Order = 2)]
        public string CardAvatar { get; set; }

        /// <summary>
        /// 会员卡锁定详情列表
        /// </summary>
        [DataMember(Order = 2)]
        public List<CardLockStateModel> LockList { get; set; }

        /// <summary>
        /// 会员信息
        /// </summary>
        [DataMember(Order = 3)]
        public CardMemberInfoModel MemberInfo { get; set; }

        /// <summary>
        /// 卡权限
        /// </summary>
        [DataMember(Order = 4)]
        public CardPurviewModel CardPurview { get; set; }

        /// <summary>
        /// 余额列表
        /// </summary>
        [DataMember(Order = 5)]
        public List<BalanceModel> MemberBalances { get; set; }

        /// <summary>
        /// 附属卡列表
        /// </summary>
        [DataMember(Order = 6)]
        public List<ChildCardModel> ChildCardList { get; set; }
    }

    public class CardLockStateModel
    {
        /// <summary>
        /// 锁定类别
        /// </summary>
        public int LockType { get; set; }
        /// <summary>
        /// 锁定说明
        /// </summary>
        public string LockExplain
        {
            get
            {
                switch (LockType)
                {
                    case 0: return "机台退分限额锁定";
                    case 1: return "机台非法操作锁定";
                    case 2: return "人工锁定";
                    case 3: return "游乐项目入场锁定";
                    default: return "";
                }
            }
            set { }
        }
    }
    public class CardMemberInfoModel
    {
        public string UserName { get; set; }
        public string Birthday { get; set; }
        public string IDCardNo { get; set; }
        public string Mobile { get; set; }
        public int Gender { get; set; }
        public string Avatar { get; set; }
        public string Note { get; set; }
    }
    public class CardPurviewModel
    {
        /// <summary>
        /// 允许投币
        /// </summary>
        public int AllowIn { get; set; }

        /// <summary>
        /// 允许退币
        /// </summary>
        public int AllowOut { get; set; }

        /// <summary>
        /// 允许兑币
        /// </summary>
        public int AllowExitCoin { get; set; }

        /// <summary>
        /// 允许售币
        /// </summary>
        public int AllowSaleCoin { get; set; }

        /// <summary>
        /// 允许存币
        /// </summary>
        public int AllowSaveCoin { get; set; }

        /// <summary>
        /// 允许送币
        /// </summary>
        public int AllowFreeCoin { get; set; }

        /// <summary>
        /// 允许续卡
        /// </summary>
        public int AllowRenew { get; set; }
    }

    public class BalanceModel
    {
        public int BalanceIndex { get; set; }

        public string BalanceName { get; set; }

        public decimal Quantity { get; set; }
    }

    public class ChildCardModel
    {
        public string ChildCardId { get; set; }
        public string ChildICCardId { get; set; }
        public string CardShape { get; set; }
        public string Deposit { get; set; }
        public string EndDate { get; set; }
        public List<CardLockStateModel> LockList { get; set; }
        public ChildCardMemberInfoModel ChildCardMemberInfo { get; set; }
    }

    public class ChildCardMemberInfoModel
    {
        public string UserName { get; set; }
        public string Birthday { get; set; }
        public string Gender { get; set; }
        public string Avatar { get; set; }
    }

    public class SourceBalanceModel
    {
        public string BalanceId { get; set; }
        public int BalanceIndex { get; set; }
        public string BalanceName { get; set; }
        public decimal? Balance { get; set; }
        public decimal? BalanceFree { get; set; }
        public decimal? BalanceTotal { get; set; }
    }

    public class ChargeBalanceModel
    {
        public string BalanceId { get; set; }
        public string SourceBalanceName { get; set; }

        public string BalanceTotal { get; set; }
        public string Balance { get; set; }
        public string BalanceFree { get; set; }

        public List<TargetChargeModel> TargetChargeList { get; set; }
    }

    public class TargetChargeModel
    {
        public int ExchangeRuleId { get; set; }
        public string TargetBalanceName { get; set; }
        public string TargetBalanceQty { get; set; }
        public int SourceCount { get; set; }
        public int TargetCount { get; set; }
        public int DecimalNumber { get; set; }
        public int AddingType { get; set; }
        public string ExchangeRate { get { 
            if(SourceCount != 0 && TargetCount != 0)
            {
                decimal rate = TargetCount / (decimal)SourceCount;
                rate = Math.Round(rate, 2, MidpointRounding.AwayFromZero); 
                return rate.ToString();
            }
            else
            {
                return "0";
            }
        } }
    }

    [DataContract]
    public class MemberFreeModel
    {
        public MemberFreeModel()
        {
            this.BalanceName = string.Empty;
            this.RemainFrees = string.Empty;
            this.FreeDetails = new List<FreeDetailModel>();
        }
        [DataMember(Order = 0)]
        public int FreeType { get; set; }
        [DataMember(Order = 1)]
        public string FreeId { get; set; }
        [DataMember(Order = 1)]
        public string Title { get; set; }
        [DataMember(Order = 2)]
        public string BalanceName { get; set; }
        [DataMember(Order = 2)]
        public string FreeCoinName { get; set; }
        [DataMember(Order = 3)]
        public string Content { get; set; }
        [DataMember(Order = 3)]
        public int RemainCount { get; set; }
        [DataMember(Order = 3)]
        public string RemainFrees { get; set; }
        [DataMember(Order = 4)]
        public string EndDate { get; set; }
        public List<FreeDetailModel> FreeDetails { get; set; }
    }
    public class FreeDetailModel
    {
        public int BalanceIndex { get; set; }
        public string BalanceName { get; set; }
        public int Quantity { get; set; }
        public int IsDeviceOut { get; set; }
    }

    public class ConfirmFreeModel
    {
        public int freeType { get; set; }
        public string freeId { get; set; }
    }

    public class ddddd
    {
        public int DeviceId { get; set; }
        public string DeviceName { get; set; }
    }
}
