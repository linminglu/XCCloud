using Microsoft.SqlServer.Server;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudWebBar.Common;
using XCCloudWebBar.Model.CustomModel.XCCloud;
using XCCloudWebBar.Model.CustomModel.XCCloud.Order;

namespace XCCloudWebBar.CacheService
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

        public static List<SqlDataRecord> GetCouponList(string storeId, string json)
        {
            String[] Ary = new String[] { "数据0", "数据1" };
            List<SqlDataRecord> listSqlDataRecord = new List<SqlDataRecord>();
            SqlMetaData[] MetaDataArr = new SqlMetaData[] {
                    new SqlMetaData("couponId", SqlDbType.Int), 
                    new SqlMetaData("couponCode", SqlDbType.VarChar)  
            };
            string flwSendId = RedisCacheHelper.CreateCloudSerialNo(storeId);
            List<OrderBuyDetailModel> buyDetailList = Utils.DataContractJsonDeserializer<List<OrderBuyDetailModel>>(json);

            for (int i = 0; i < buyDetailList.Count; i++)
            {
                List<object> listParas = new List<object>();
                listParas.Add(buyDetailList[i].FoodId);
                listParas.Add(buyDetailList[i].Category);

                var record = new SqlDataRecord(MetaDataArr);
                for (int j = 0; j < Ary.Length; j++)
                {
                    record.SetValue(j, listParas[j]);
                }
                listSqlDataRecord.Add(record);
            }
            return listSqlDataRecord;
        }

        public static List<SqlDataRecord> GetRegisterMember(RegisterMember registerMember)
        {
            String[] Ary = new String[] { 
                "数据0", "数据1", "数据2", "数据3", "数据4", 
                "数据5", "数据6", "数据7", "数据8", "数据9",
                "数据10", "数据11", "数据12", "数据13", "数据14",
                "数据15", "数据16", "数据17", "数据18"
            };

            List<object> listParas = new List<object>();
            listParas.Add(registerMember.Mobile);
            listParas.Add(registerMember.WechatOpenID);
            listParas.Add(registerMember.AlipayOpenID);
            listParas.Add(registerMember.QQ);
            listParas.Add(registerMember.IMME);

            listParas.Add(registerMember.CardShape);
            listParas.Add(registerMember.UserName);
            listParas.Add(registerMember.UserPassword);
            listParas.Add(registerMember.Gender);
            listParas.Add(registerMember.Birthday);

            listParas.Add(registerMember.IDCard);
            listParas.Add(registerMember.EMail);
            listParas.Add(registerMember.LeftHandCode);
            listParas.Add(registerMember.RightHandCode);
            listParas.Add(registerMember.Photo);

            listParas.Add(registerMember.RepeatCode);
            listParas.Add(registerMember.ICCardId);
            listParas.Add(registerMember.ICCardUID);
            listParas.Add(registerMember.Note);

            List<SqlDataRecord> listSqlDataRecord = new List<SqlDataRecord>();
            SqlMetaData[] MetaDataArr = new SqlMetaData[] { 
                new SqlMetaData("Mobile", SqlDbType.VarChar,11), 
                new SqlMetaData("WechatOpenID", SqlDbType.VarChar,64),
                new SqlMetaData("AlipayOpenID", SqlDbType.VarChar,64),
                new SqlMetaData("QQ", SqlDbType.VarChar,64),
                new SqlMetaData("IMME", SqlDbType.VarChar,64),

                new SqlMetaData("CardShape", SqlDbType.Int),
                new SqlMetaData("UserName", SqlDbType.VarChar,50),
                new SqlMetaData("UserPassword", SqlDbType.VarChar,20),
                new SqlMetaData("Gender", SqlDbType.Int),
                new SqlMetaData("Birthday", SqlDbType.VarChar,16),

                new SqlMetaData("IDCard", SqlDbType.VarChar,18),
                new SqlMetaData("EMail", SqlDbType.VarChar,50),
                new SqlMetaData("LeftHandCode", SqlDbType.VarChar,5000),
                new SqlMetaData("RightHandCode", SqlDbType.VarChar,5000),
                new SqlMetaData("Photo", SqlDbType.VarChar,100),

                new SqlMetaData("RepeatCode", SqlDbType.Int),
                new SqlMetaData("ICCardId", SqlDbType.Int),
                new SqlMetaData("ICCardUID", SqlDbType.BigInt),
                new SqlMetaData("Note", SqlDbType.VarChar,200)
            };

            var record = new SqlDataRecord(MetaDataArr);
            for (int i = 0; i < Ary.Length; i++)
            {
                record.SetValue(i, listParas[i]);
            }
            listSqlDataRecord.Add(record);

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
