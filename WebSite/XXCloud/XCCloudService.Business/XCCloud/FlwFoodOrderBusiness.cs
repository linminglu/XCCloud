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
    }
}
