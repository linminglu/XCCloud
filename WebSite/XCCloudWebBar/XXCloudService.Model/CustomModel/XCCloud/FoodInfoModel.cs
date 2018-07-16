using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudWebBar.Model.CustomModel.XCCloud
{
    [DataContract]
    public class FoodInfoModel
    {
        [DataMember(Name = "foodId", Order = 1)]
        public int FoodID { get; set; }

        [DataMember(Name = "foodName", Order = 2)]
        public string FoodName { get; set; }

        [DataMember(Name = "foodType", Order = 3)]
        public int FoodType { get; set; }

        [DataMember(Name = "allowInternet", Order = 4)]
        public int AllowInternet { get; set; }

        [DataMember(Name = "allowPrint", Order = 5)]
        public int AllowPrint { get; set; }

        [DataMember(Name = "foreAuthorize", Order = 6)]
        public int ForeAuthorize { get; set; }

        [DataMember(Name = "imageUrl", Order = 7)]
        public string ImageUrl { get; set; }

        [DataMember(Name = "detailsCount", Order = 8)]
        public int DetailsCount { get; set; }

        [DataMember(Name = "detailInfoList", Order = 9)]
        public List<FoodDetailInfoModel> DetailInfoList { set; get; }
    }


    public class OpenCardInfoModel
    {
        public OpenCardMemberLevelModel openCardMemberLevelModel { set; get; }
        public List<OpenCardFoodInfoModel> listOpenCardFoodInfoModel { set; get; }
    }


    [DataContract]
    public class OpenCardMemberLevelModel
    {
        [DataMember(Name = "memberLevelId", Order = 1)]
        public int MemberLevelId { get; set; }

        [DataMember(Name = "memberLevelName", Order = 2)]
        public string MemberLevelName { get; set; }

        [DataMember(Name = "coverURL", Order = 3)]
        public string CoverURL { set; get; }

        [DataMember(Name = "openFee", Order = 4)]
        public decimal OpenFee { set; get; }

        [DataMember(Name = "deposit", Order = 5)]
        public decimal Deposit { set; get; }
    }


    [DataContract]
    public class OpenCardFoodInfoModel
    {
        [DataMember(Name = "memberLevelId", Order = 1)]
        public int MemberLevelId { set; get; }

        [DataMember(Name = "foodId", Order = 2)]
        public int FoodID { get; set; }

        [DataMember(Name = "foodName", Order = 3)]
        public string FoodName { get; set; }

        [DataMember(Name = "foodType", Order = 4)]
        public int FoodType { get; set; }

        [DataMember(Name = "allowInternet", Order = 5)]
        public int AllowInternet { get; set; }

        [DataMember(Name = "allowPrint", Order = 6)]
        public int AllowPrint { get; set; }

        [DataMember(Name = "foreAuthorize", Order = 7)]
        public int ForeAuthorize { get; set; }

        [DataMember(Name = "foodPrice", Order = 8)]
        public decimal FoodPrice { get; set; }

        [DataMember(Name = "imageUrl", Order = 9)]
        public string ImageUrl { get; set; }
    }


    [DataContract]
    public class FoodDetailInfoModel
    {
        public FoodDetailInfoModel()
        { 
            
        }

        public FoodDetailInfoModel(int foodId, int balanceType,string typeName,decimal useCount)
        {
            this.FoodId = foodId;
            this.BalanceType = balanceType;
            this.BalanceTypeName = typeName;
            this.UseCount = useCount;
        }

        [DataMember(Name = "foodId", Order = 1)]
        public int FoodId { set; get; }

        [DataMember(Name = "balanceType", Order = 2)]
        public int BalanceType { set; get; }

        [DataMember(Name = "typeName", Order = 3)]
        public string BalanceTypeName { set; get; }

        [DataMember(Name = "useCount", Order = 4)]
        public decimal UseCount { set; get; }
    }

    [DataContract]
    public class FoodInfoNumModel
    {
        public FoodInfoNumModel(int payModel, int payNum)
        {
            this.PayModel = PayModel;
            this.PayNum = PayNum;
        }

        public int PayModel { set; get; }

        public int PayNum { set; get; }
    }

    [DataContract]
    public class FoodInfoPriceModel
    {
        public FoodInfoPriceModel(int payModel,decimal payPrice)
        {
            this.PayModel = payModel;
            this.PayPrice = payPrice;
        }

        [DataMember(Name = "payModel", Order = 1)]
        public int PayModel { set; get; }

        [DataMember(Name = "payNum", Order = 2)]
        public decimal PayPrice { set; get; }
    }

    [DataContract]
    public class FoodDetailModel
    {
        [DataMember(Name = "detailId", Order = 1)]
        public int DetailId { get; set; }

        [DataMember(Name = "detailFoodType", Order = 2)]
        public int DetailFoodType { get; set; }

        [DataMember(Name = "detailFoodTypeName", Order = 3)]
        public string DetailFoodTypeName { get; set; }

        [DataMember(Name = "containCount", Order = 4)]
        public int ContainCount { get; set; }

        [DataMember(Name = "containName", Order = 5)]
        public string ContainName { get; set; }
    }

    [DataContract]
    public class FoodSetModel
    {
        [DataMember(Name = "food", Order = 1)]
        public List<FoodInfoModel> ListFoodInfo { set; get; }

        [DataMember(Name = "ticket", Order = 2)]
        public List<TicketModel> ListTicketInfo { set; get; }

        [DataMember(Name = "good", Order = 3)]
        public List<GoodModel> ListGoodInfo { set; get; }
    }

    public class FoodInfoViewModel
    {
        public FoodInfoViewModel()
        {
            this.Note = string.Empty;
            this.FoodName = string.Empty;
            this.ImageURL = string.Empty;
            this.Price = string.Empty;
        }
        public int FoodId { get; set; }
        public string FoodName { get; set; }
        public string Note { get; set; }
        public string ImageURL { get; set; }
        public int FoodType { get; set; }
        public string Price { get; set; }
    }
}
