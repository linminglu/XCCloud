using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudService.Model.CustomModel.XCGameManager
{
    [DataContract]

    public class DataOrderPageModel
    {
        [DataMember(Name = "Lists", Order = 1)]  
        public List<DataOrderModel> Lists { set; get; }
        [DataMember(Name = "Page", Order = 2)]
        public string Page { set; get; }
        [DataMember(Name = "Count", Order = 3)]
        public string Count { set; get; }
        [DataMember(Name = "Buycoins", Order = 4)]
        public string Buycoins { set; get; }
        [DataMember(Name = "Totalcoins", Order = 5)]
        public string Totalcoins { set; get; }

        [DataMember(Name = "Totalmoney", Order = 6)]
        public string Totalmoney { set; get; }

        [DataMember(Name = "BuyTypelist", Order = 7)]
        public List<string> BuyTypelist { set; get; }
        [DataMember(Name = "StoreNamelist", Order = 8)]
        public List<string> StoreNamelist { set; get; }
        [DataMember(Name = "PayTypelist", Order = 9)]
        public List<string> PayTypelist { set; get; }

    }
    [DataContract]
    public class DataOrderModel
    {

        [DataMember(Name = "OrderID", Order = 1)]
        public string OrderID { set; get; }

        [DataMember(Name = "StoreID", Order = 2)]
        public string StoreID { set; get; }

        [DataMember(Name = "Price", Order = 3)]
        public decimal Price { set; get; }

        [DataMember(Name = "Fee", Order = 4)]
        public decimal Fee { set; get; }   

        [DataMember(Name = "PayTime", Order = 5)]
        public string PayTime { set; get; }

        [DataMember(Name = "CreateTimes", Order = 6)]
        public string CreateTimes { set; get; }

        [DataMember(Name = "Descript", Order = 7)]
        public string Descript { set; get; }

        [DataMember(Name = "Mobile", Order = 8)]
        public string Mobile { set; get; }
        [DataMember(Name = "BuyType", Order = 9)]
        public string BuyType { set; get; }
        [DataMember(Name = "StoreName", Order = 10)]
        public string StoreName { set; get; }
        [DataMember(Name = "Coins", Order = 11)]
        public int Coins { set; get; }
        [DataMember(Name = "payStatus", Order = 12)]
        public int PayStatus { set; get; }
        [DataMember(Name = "orderType", Order = 13)]
        public int OrderType { set; get; }
        [DataMember(Name = "payStatusName", Order = 14)]
        public string PayStatusName { set; get; }
        [DataMember(Name = "orderTypeName", Order = 15)]
        public string OrderTypeName { set; get; }
    }

    [DataContract]
    public class DataOrderModelBuyType
    {
        [DataMember(Name = "BuyTypelist", Order = 1)]
        public string BuyTypelist { set; get; }
    }

    [DataContract]
    public class DataOrderModelStoreName
    {
        [DataMember(Name = "StoreNamelist", Order = 1)]
        public string StoreNamelist { set; get; }
    }
}
