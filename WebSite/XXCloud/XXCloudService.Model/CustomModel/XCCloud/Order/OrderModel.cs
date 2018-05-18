using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudService.Model.CustomModel.XCCloud.Order
{
    public class OrderModel
    {

    }

    [DataContract]
    public class OrderInfoModel
    {
        public OrderInfoModel(OrderMainModel orderMainModel, List<OrderDetailModel> orderDetailModel)
        {
            this.orderMainModel = orderMainModel;
            this.orderDetailModel = orderDetailModel;
        }

        [DataMember(Name = "main", Order = 1)]
        public OrderMainModel orderMainModel { set; get; }

        [DataMember(Name = "detail", Order = 2)]
        public List<OrderDetailModel> orderDetailModel { set; get; } 
    }

    [DataContract]
    public class OrderBuyDetailModel
    {
        [DataMember(Name = "foodId", Order = 1)]
        public int FoodId {set;get;}

        [DataMember(Name = "foodCount", Order = 2)]
        public int FoodCount {set;get;}

        [DataMember(Name = "payType", Order = 3)]
        public int PayType {set;get;}

        [DataMember(Name = "payNum", Order = 4)]
        public decimal PayNum { set; get; }
    }

    [DataContract]
    public class OrderInfo1Model
    {
        [DataMember(Name = "customerType", Order = 1)]
        public string CustomerType {set;get;}

        [DataMember(Name = "icCardId", Order = 2)]
        public int ICCardId {set;get;}

        [DataMember(Name = "payCount", Order = 3)]
        public int PayCount {set;get;}

        [DataMember(Name = "realPay", Order = 4)]
        public decimal RealPay {set;get;}

        [DataMember(Name = "freePay", Order = 5)]
        public decimal FreePay {set;get;}

        [DataMember(Name = "foodCount", Order = 6)]
        public decimal FoodCount {set;get;}

        [DataMember(Name = "detailsGoodsCount", Order = 7)]
        public int DetailsGoodsCount {set;get;}

        [DataMember(Name = "memberLevelId", Order = 8)]
        public int MemberLevelId {set;get;}

        [DataMember(Name = "memberLevelName", Order = 9)]
        public string MemberLevelName {set;get;}

        [DataMember(Name = "openFee", Order = 10)]
        public decimal OpenFee {set;get;}

        [DataMember(Name = "deposit", Order = 11)]
        public decimal Deposit {set;get;}

        [DataMember(Name = "renewFee", Order = 12)]
        public decimal RenewFee {set;get;}

        [DataMember(Name = "changeFee", Order = 13)]
        public decimal ChangeFee {set;get;}

        [DataMember(Name = "errMsg", Order = 14)]
        public decimal ErrMsg {set;get;}

        [DataMember(Name = "errMsg", Order = 15)]
        List<OrderBuyDetail1Model> OrderBuyDetail { set; get; }
    }

    [DataContract]
    public class OrderBuyDetail1Model
    { 
        [DataMember(Name = "customerType", Order = 1)]
        public string Category { set; get; }

        [DataMember(Name = "foodId", Order = 2)]
        public int FoodId { set; get; }

        [DataMember(Name = "foodName", Order = 3)]
        public int FoodName { set; get; }

        [DataMember(Name = "foodCount", Order = 4)]
        public int FoodCount { set; get; }

        [DataMember(Name = "payType", Order = 5)]
        public int PayType { set; get; }

        [DataMember(Name = "payTypeName", Order = 6)]
        public string PayTypeName { set; get; }

        [DataMember(Name = "payNum", Order = 7)]
        public decimal PayNum { set; get; }
    }
}
