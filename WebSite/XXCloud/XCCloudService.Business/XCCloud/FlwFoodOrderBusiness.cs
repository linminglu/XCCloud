using Microsoft.SqlServer.Server;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.Common;
using XCCloudService.Model.CustomModel.XCCloud.Order;

namespace XCCloudService.CacheService
{
    public class FlwFoodOrderBusiness
    {
        public static List<SqlDataRecord> GetOrderBuyDetailList(string storeId, string json)
        {
            String[] Ary = new String[] { "数据0", "数据1", "数据2", "数据3", "数据4" };
            List<SqlDataRecord> listSqlDataRecord = new List<SqlDataRecord>();
            SqlMetaData[] MetaDataArr = new SqlMetaData[] {
                    new SqlMetaData("foodId", SqlDbType.Int), 
                    new SqlMetaData("category", SqlDbType.Int),  
                    new SqlMetaData("foodCount", SqlDbType.Int),
                    new SqlMetaData("payType", SqlDbType.Int),
                    new SqlMetaData("payNum", SqlDbType.Decimal,18,2)
            };
            string flwSendId = RedisCacheHelper.CreateCloudSerialNo(storeId);
            List<OrderBuyDetailModel> buyDetailList = Utils.DataContractJsonDeserializer<List<OrderBuyDetailModel>>(json);

            for (int i = 0; i < buyDetailList.Count; i++)
            {
                List<object> listParas = new List<object>();
                listParas.Add(buyDetailList[i].FoodId);
                listParas.Add(buyDetailList[i].Category);
                listParas.Add(buyDetailList[i].FoodCount);
                listParas.Add(buyDetailList[i].PayType);
                listParas.Add(buyDetailList[i].PayNum);

                var record = new SqlDataRecord(MetaDataArr);
                for (int j = 0; j < Ary.Length; j++)
                {
                    record.SetValue(j, listParas[j]);
                }
                listSqlDataRecord.Add(record);
            }
            return listSqlDataRecord;
        }

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
