using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudService.Model.CustomModel.XCCloud
{
    [DataContract]
    public class MemberModel
    {
        public MemberModel(MemberBaseModel memberBaseModel, List<MemberBalanceModel> memberBalanceModelList)
        {
            this.MemberBaseModel = memberBaseModel;
            this.MemberBalanceModelList = memberBalanceModelList;
        }

        [DataMember(Name = "base", Order = 1)]
        public MemberBaseModel MemberBaseModel { set; get; }

        [DataMember(Name = "balance", Order = 2)]
        public List<MemberBalanceModel> MemberBalanceModelList { set; get; }
    }

    [DataContract]
    public class MemberBaseModel
    {
        [DataMember(Name = "icCardID", Order = 1)]
        public string ICCardID { set; get; }

        [DataMember(Name = "memberName", Order = 2)]
        public string MemberName { set; get; }

        [DataMember(Name = "gender", Order = 3)]
        public int Gender { set; get; }

        [DataMember(Name = "birthday", Order = 4)]
        public string Birthday { set; get; }

        [DataMember(Name = "certificalID", Order = 5)]
        public string IDCard { set; get; }

        [DataMember(Name = "mobile", Order = 6)]
        public string Mobile { set; get; }

        [DataMember(Name = "memberState", Order = 7)]
        public int MemberState { set; get; }

        [DataMember(Name = "note", Order = 8)]
        public string Note { set; get; }
        [DataMember(Name = "memberLevelName", Order = 9)]
        public string MemberLevelName { set; get; }

        [DataMember(Name = "endDate", Order = 10)]
        public string EndDate { set; get; }

        [DataMember(Name = "repeatCode", Order = 11)]
        public int RepeatCode { set; get; }

        [DataMember(Name = "storeId", Order = 12)]
        public string StoreId { set; get; }

        [DataMember(Name = "storeName", Order = 13)]
        public string StoreName { set; get; }     
    }

    [DataContract]
    public class MemberBalanceModel
    {
        [DataMember(Name = "balanceIndex", Order = 1)]
        public int BalanceIndex { set; get; }

        [DataMember(Name = "balanceName", Order = 2)]
        public string BalanceName { set; get; }

        [DataMember(Name = "balance", Order = 3)]
        public decimal Balance { set; get; }
    }

    [DataContract]
    public class RegisterMember
    {
        [DataMember(Name = "mobile", Order = 1)]
        public string Mobile {set;get;}

        [DataMember(Name = "wechatOpenID", Order = 2)]
        public string WechatOpenID {set;get;}

        [DataMember(Name = "alipayOpenID", Order = 3)]
        public string AlipayOpenID {set;get;}

        [DataMember(Name = "qq", Order = 4)]
        public string QQ {set;get;}

        [DataMember(Name = "imme", Order = 5)]
        public string IMME {set;get;}

        [DataMember(Name = "cardShape", Order = 6)]
        public int CardShape {set;get;}

        [DataMember(Name = "userName", Order = 7)]
        public string UserName {set;get;}

        [DataMember(Name = "userPassword", Order = 8)]
        public string UserPassword {set;get;}

        [DataMember(Name = "gender", Order = 9)]
        public int Gender {set;get;}

        [DataMember(Name = "birthday", Order = 10)]
        public string Birthday {set;get;}

        [DataMember(Name = "idCard", Order = 11)]
        public string IDCard {set;get;}

        [DataMember(Name = "eMail", Order = 12)]
        public string EMail {set;get;}

        [DataMember(Name = "leftHandCode", Order = 13)]
        public string LeftHandCode {set;get;}

        [DataMember(Name = "rightHandCode", Order = 14)]
        public string RightHandCode {set;get;}

        [DataMember(Name = "photo", Order = 15)]
        public string Photo {set;get;}

        [DataMember(Name = "repeatCode", Order = 16)]
        public string RepeatCode {set;get;}

        [DataMember(Name = "icCardId", Order = 17)]
        public string ICCardId {set;get;}

        [DataMember(Name = "icCardUID", Order = 18)]
        public string ICCardUID {set;get;}

        [DataMember(Name = "note", Order = 19)]
        public string Note {set;get;}

    }
}
