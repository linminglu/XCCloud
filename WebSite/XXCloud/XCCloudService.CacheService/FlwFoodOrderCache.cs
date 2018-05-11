using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudService.CacheService
{
    public class FlwFoodOrderCache
    {
        private static Hashtable _foodOrderHt = new Hashtable();

        public static void Add(FoodOrderCacheModel model)
        {
            _foodOrderHt.Add(model.OrderId, model);
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
        public FoodOrderCacheModel(string merchId, string storeId, string orderId, int customerType, int icCardId,string workStation)
        {
            this.MerchId = merchId;
            this.StoreId = storeId;
            this.OrderId = orderId;
            this.CustomerType = customerType;
            this.ICCardId = icCardId;
            this.WorkStation = workStation;
            this.CreateTime = System.DateTime.Now;
        }

        public string MerchId { set; get; }

        public string StoreId { set; get; }

        public string OrderId { set; get; }

        public DateTime CreateTime { set; get; }

        public int CustomerType { set; get; }

        public int ICCardId { set; get; }

        public string WorkStation { set; get; }
    }
}
