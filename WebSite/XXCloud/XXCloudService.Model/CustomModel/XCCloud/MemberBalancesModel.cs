using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudService.Model.CustomModel.XCCloud
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

    public class MemberCardInfoViewModel
    {
        public MemberCardInfoViewModel()
        {
            MemberBalances = new List<BalanceModel>();
        }

        /// <summary>
        /// 会员卡ID
        /// </summary>
        public string CardId { get; set; }

        /// <summary>
        /// 会员卡编号
        /// </summary>
        public string ICCardId { get; set; }

        /// <summary>
        /// 会员信息
        /// </summary>
        public CardMemberInfoModel MemberInfo { get; set; }

        /// <summary>
        /// 卡权限
        /// </summary>
        public CardPurviewModel CardPurview { get; set; }

        /// <summary>
        /// 余额列表
        /// </summary>
        public List<BalanceModel> MemberBalances { get; set; }
   

    }

    public class CardMemberInfoModel
    {
        public string UserName { get; set; }
        public string Birthday { get; set; }
        public string IDCardNo { get; set; }
        public string Mobile { get; set; }
        public string Gender { get; set; }
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
        /// 卡状态
        /// </summary>
        public int CardStatus { get; set; }
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
        public string Deposit { get; set; }
        public string EndDate { get; set; }
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
}
