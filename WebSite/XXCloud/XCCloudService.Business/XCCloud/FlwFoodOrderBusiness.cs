using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudService.CacheService
{
    public class FlwFoodOrderBusiness
    {
        public static void Add(FoodOrderCacheModel model)
        {
            FlwFoodOrderCache.Add(model);
        }

        public static FoodOrderCacheModel GetModel(string orderId)
        {
            return FlwFoodOrderCache.GetModel(orderId);
        }

        public static bool Exist(string orderId)
        {
            return FlwFoodOrderCache.Exist(orderId);
        }

        public static void Remove(string storeId)
        {
            FlwFoodOrderCache.Remove(storeId);
        }

        public static List<FoodOrderCacheModel> GetOrderListByWorkStation(string storeId,string workStation)
        {
            List<FoodOrderCacheModel> list = new List<FoodOrderCacheModel>();
            var query = from item in FlwFoodOrderCache.FoodOrderHt
                        where ((FoodOrderCacheModel)(item.Value)).StoreId.Equals(storeId) && ((FoodOrderCacheModel)(item.Value)).WorkStation == workStation
                        select item.Key.ToString();
            if (query.Count() == 0)
            {
                return list;
            }
            else
            {
                var models = query.ToList<string>();
                foreach (var m in models)
                {
                    list.Add(FlwFoodOrderCache.GetModel(m.ToString()));
                }
                return list;
            }
        }
    }
}
