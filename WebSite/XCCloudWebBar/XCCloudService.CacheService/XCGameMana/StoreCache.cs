using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudWebBar.Model.CustomModel.XCGameManager;

namespace XCCloudWebBar.CacheService.XCGameMana
{
    public class StoreCache
    {
        public const string storeCacheKey = "redisStoreCacheKey";

        public static void Clear()
        {
            RedisCacheHelper.KeyDelete(storeCacheKey);  
        }

        public static void Remove(StoreCacheModel item)
        {
            RedisCacheHelper.HashDelete(storeCacheKey, item.StoreID);
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="list"></param>
        public static void Init(List<StoreCacheModel> list)
        {
            foreach (var item in list)
            {
                RedisCacheHelper.HashSet<StoreCacheModel>(storeCacheKey, item.StoreID, item);
            }
        }

        public static void Add(StoreCacheModel model)
        {
            RedisCacheHelper.HashSet<StoreCacheModel>(storeCacheKey, model.StoreID, model);
        }

        public static List<StoreCacheModel> GetStore()
        {
            List<StoreCacheModel> storeList = RedisCacheHelper.HashGetAll<StoreCacheModel>(storeCacheKey);
            return storeList;
        }

        public static StoreCacheModel GetStoreModel(string storeId)
        {
            StoreCacheModel store = RedisCacheHelper.HashGet<StoreCacheModel>(storeCacheKey, storeId);
            return store;
        }
    }

    public class StoreDogCache
    {
        private static List<StoreDogCacheModel> storeDogList = null;

        public static void Clear()
        {
            storeDogList = null;
        }

        public static void Add(List<StoreDogCacheModel> list)
        {
            storeDogList = list;
        }

        public static List<StoreDogCacheModel> GetStore()
        {
            return storeDogList;
        }
      
        public static bool ExistDogId(string storeId,string dogId)
        {
            storeDogList = storeDogList.Where(p => p.StoreID == storeId && p.DogId == dogId).ToList() ;
            if (storeDogList.Count > 0)
            {
                return true;
            }
            //int count = storeDogList.Where<StoreDogCacheModel>(p=>p.StoreID==storeId && p.DogId==dogId).Count<StoreDogCacheModel>();
            return false;
        }
    }
}
