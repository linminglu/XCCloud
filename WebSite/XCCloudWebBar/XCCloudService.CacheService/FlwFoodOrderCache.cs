using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudWebBar.Model.CustomModel.XCCloud;

namespace XCCloudWebBar.CacheService
{
    public class FlwFoodOrderCache
    {
        private static Dictionary<string, object> _foodOrderHt = new Dictionary<string, object>();

        public static Dictionary<string, object> FoodOrderHt
        {
            get { return _foodOrderHt; }
        }

        public static void Add(FoodOrderCacheModel model)
        {
            _foodOrderHt.Add(model.FlwOrderId, model);
        }

        public static FoodOrderCacheModel GetModel(string orderId)
        {
            return (FoodOrderCacheModel)(_foodOrderHt[orderId]);
        }

        public static bool Exist(string orderId)
        {
            return _foodOrderHt.ContainsKey(orderId);
        }

        public static void Remove(string storeId)
        {
            _foodOrderHt.Remove(storeId);
        }
    }

    public class FoodOrderCacheModel
    {
        public FoodOrderCacheModel(string merchId, string storeId, string flwOrderId, int customerType, int icCardId, string workStation, int workStationId,RegisterMember regMember)
        {
            this.MerchId = merchId;
            this.StoreId = storeId;
            this.FlwOrderId = flwOrderId;
            this.CustomerType = customerType;
            this.ICCardId = icCardId;
            this.WorkStation = workStation;
            this.CreateTime = System.DateTime.Now;
            this.WorkStationId = workStationId;
            this.RegisterMember = regMember;
        }

        public string MerchId { set; get; }

        public string StoreId { set; get; }

        public string FlwOrderId { set; get; }

        public DateTime CreateTime { set; get; }

        public int CustomerType { set; get; }

        public int ICCardId { set; get; }

        public int WorkStationId { set; get; }

        public string WorkStation { set; get; }

        public RegisterMember RegisterMember { set; get; }
    }
}
