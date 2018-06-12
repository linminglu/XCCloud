using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudService.Model.CustomModel.XCCloud
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

        [DataMember(Name = "foodPrice", Order = 7)]
        public decimal FoodPrice { get; set; }

        [DataMember(Name = "imageUrl", Order = 8)]
        public string ImageUrl { get; set; }

        [DataMember(Name = "detailsCount", Order = 9)]
        public int DetailsCount { get; set; }

        [DataMember(Name = "detailInfoList", Order = 10)]
        public List<FoodDetailInfoModel> DetailInfoList { set; get; }
    }


    [DataContract]
    public class OpenCardFoodInfoModel
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

        [DataMember(Name = "foodPrice", Order = 7)]
        public decimal FoodPrice { get; set; }

        [DataMember(Name = "imageUrl", Order = 8)]
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
            this.TypeName = typeName;
            this.UseCount = useCount;
        }

        [DataMember(Name = "foodId", Order = 1)]
        public int FoodId { set; get; }

        [DataMember(Name = "balanceType", Order = 2)]
        public int BalanceType { set; get; }

        [DataMember(Name = "typeName", Order = 3)]
        public string TypeName { set; get; }

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

        [DataMember(Name = "good", Order = 2)]
        public List<GoodModel> ListGoodModel { set; get; }

        [DataMember(Name = "ticket", Order = 3)]
        public List<TicketModel> ListTicketModel { set; get; }
    }
}
