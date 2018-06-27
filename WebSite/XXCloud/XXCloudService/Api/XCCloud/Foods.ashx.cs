using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using XCCloudService.Base;
using XCCloudService.BLL.CommonBLL;
using XCCloudService.BLL.XCCloud;
using XCCloudService.Business.XCGameMana;
using XCCloudService.CacheService;
using XCCloudService.Common;
using XCCloudService.Model.CustomModel.XCCloud;

namespace XXCloudService.Api.XCCloud
{
    /// <summary>
    /// Foods 的摘要说明
    /// </summary>
    [Authorize(Roles = "StoreUser")]
    public class Foods : ApiBase
    {

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object getFoodType(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenModel = (XCCloudUserTokenModel)(dicParas[Constant.XCCloudUserTokenModel]);
                TokenDataModel userTokenDataModel = (TokenDataModel)(userTokenModel.DataModel);

                string sql = "GetFoodType";
                SqlParameter[] parameters = new SqlParameter[1];
                parameters[0] = new SqlParameter("@MerchId", userTokenDataModel.MerchID);
                System.Data.DataSet ds = XCCloudBLL.GetStoredProcedureSentence(sql, parameters);

                Base_Food baseFood = new Base_Food();
                List<Base_FoodType> foodTypeMainList = Utils.GetModelList<Base_FoodType>(ds.Tables[0]);
                List<Base_FoodType> foodTypeGoodList = Utils.GetModelList<Base_FoodType>(ds.Tables[1]);
                List<Base_FoodType> foodTypeTicketList = Utils.GetModelList<Base_FoodType>(ds.Tables[2]);

                baseFood.FoodTypeMainList = foodTypeMainList;
                baseFood.FoodTypeGoodList = foodTypeGoodList;
                baseFood.FoodTypeTicketList = foodTypeTicketList;

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, baseFood);
            }
            catch (Exception e)
            {
                throw e;
            }
        }



        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object getQuickFood(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenModel = (XCCloudUserTokenModel)(dicParas[Constant.XCCloudUserTokenModel]);
                TokenDataModel userTokenDataModel = (TokenDataModel)(userTokenModel.DataModel);

                string icCardId = dicParas.ContainsKey("icCardId") ? dicParas["icCardId"].ToString() : string.Empty;

                string sql = "GetQuickFood";

                SqlParameter[] parameters = new SqlParameter[14];
                parameters[0] = new SqlParameter("@MerchId", SqlDbType.VarChar, 15);
                parameters[0].Value = userTokenDataModel.MerchID;

                parameters[1] = new SqlParameter("@StoreId", SqlDbType.VarChar, 15);
                parameters[1].Value = userTokenDataModel.StoreID;

                parameters[2] = new SqlParameter("@ICCardId", SqlDbType.Int);
                parameters[2].Value = icCardId;

                parameters[3] = new SqlParameter("@Mobile", SqlDbType.VarChar,11);
                parameters[3].Value = "";

                parameters[4] = new SqlParameter("@FoodId", SqlDbType.Int);
                parameters[4].Direction = System.Data.ParameterDirection.Output;

                parameters[5] = new SqlParameter("@MemberLevelId", SqlDbType.Int);
                parameters[5].Direction = System.Data.ParameterDirection.Output;

                parameters[6] = new SqlParameter("@FoodName", SqlDbType.VarChar,200);
                parameters[6].Direction = System.Data.ParameterDirection.Output;

                parameters[7] = new SqlParameter("@FoodSalePrice", SqlDbType.Decimal);
                parameters[7].Direction = System.Data.ParameterDirection.Output;

                parameters[8] = new SqlParameter("@Coins", SqlDbType.Int);
                parameters[8].Direction = System.Data.ParameterDirection.Output;

                parameters[9] = new SqlParameter("@AllowPrint", SqlDbType.Int);
                parameters[9].Direction = System.Data.ParameterDirection.Output;

                parameters[10] = new SqlParameter("@ForeAuthorize", SqlDbType.Int);
                parameters[10].Direction = System.Data.ParameterDirection.Output;

                parameters[11] = new SqlParameter("@AllowInternet", SqlDbType.Int);
                parameters[11].Direction = System.Data.ParameterDirection.Output;

                parameters[12] = new SqlParameter("@ErrMsg", SqlDbType.VarChar,200);
                parameters[12].Direction = System.Data.ParameterDirection.Output;

                parameters[13] = new SqlParameter("@Return", SqlDbType.Int);
                parameters[13].Direction = System.Data.ParameterDirection.ReturnValue;

                XCCloudBLL.ExecuteStoredProcedureSentence(sql, parameters);

                if (int.Parse(parameters[13].Value.ToString()) == 1)
                {
                    var obj = new
                    {
                        foodId = int.Parse(parameters[4].Value.ToString()),
                        foodName = parameters[6].Value.ToString(),
                        foodSalePrice = decimal.Parse(parameters[7].Value.ToString()),
                        coins = int.Parse(parameters[7].Value.ToString()),
                        allowPrint = int.Parse(parameters[9].Value.ToString()),
                        foreAuthorize = int.Parse(parameters[10].Value.ToString()),
                        allowInternet = int.Parse(parameters[11].Value.ToString())
                    };
                    return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, obj);
                }
                else
                { 
                    string errMsg = parameters[12].Value.ToString();
                    return ResponseModelFactory.CreateAnonymousFailModel(isSignKeyReturn, errMsg);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// 获取套餐列表
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object getOpenCardFoodList(Dictionary<string, object> dicParas)
        {
            string errMsg = string.Empty;
            XCCloudUserTokenModel userTokenModel = (XCCloudUserTokenModel)(dicParas[Constant.XCCloudUserTokenModel]);
            TokenDataModel userTokenDataModel = (TokenDataModel)(userTokenModel.DataModel);

            string memberLevelId = dicParas.ContainsKey("memberLevelId") ? dicParas["memberLevelId"].ToString() : string.Empty;

            if (string.IsNullOrEmpty(memberLevelId))
            {
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "会员等级无效");
            }

            string sql = "GetMemberOpenCardFoodInfo";
            SqlParameter[] parameters = new SqlParameter[4];
            parameters[0] = new SqlParameter("@StoreId", userTokenDataModel.StoreID);
            parameters[1] = new SqlParameter("@MemberLevelId", memberLevelId);
            parameters[2] = new SqlParameter("@Result", SqlDbType.Int);
            parameters[2].Direction = System.Data.ParameterDirection.Output;
            parameters[3] = new SqlParameter("@ErrMsg", SqlDbType.VarChar, 200);
            parameters[3].Direction = System.Data.ParameterDirection.Output;

            System.Data.DataSet ds = XCCloudBLL.GetStoredProcedureSentence(sql, parameters);
            if (int.Parse(parameters[2].Value.ToString()) == 1)
            {
                DataTable dtFoodInfo = ds.Tables[0];
                if (dtFoodInfo.Rows.Count > 0)
                {
                    List<OpenCardFoodInfoModel> listFoodInfo = Utils.GetModelList<OpenCardFoodInfoModel>(dtFoodInfo).ToList();
                    return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, listFoodInfo);
                }
                else
                {
                    List<OpenCardFoodInfoModel> listFoodInfo = new List<OpenCardFoodInfoModel>();
                    return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, listFoodInfo);
                }
            }
            else
            {
                errMsg = parameters[3].Value.ToString();
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, errMsg);
            }
        }


        /// <summary>
        /// 获取套餐列表
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object getFoodList(Dictionary<string, object> dicParas)
        {
            XCCloudUserTokenModel userTokenModel = (XCCloudUserTokenModel)(dicParas[Constant.XCCloudUserTokenModel]);
            TokenDataModel userTokenDataModel = (TokenDataModel)(userTokenModel.DataModel);

            string customerType = dicParas.ContainsKey("customerType") ? dicParas["customerType"].ToString() : string.Empty;
            string memberLevelId = dicParas.ContainsKey("memberLevelId") ? dicParas["memberLevelId"].ToString() : string.Empty;
            string foodTypeStr = dicParas.ContainsKey("foodTypeStr") ? dicParas["foodTypeStr"].ToString() : string.Empty;
            string priceLimitJson = dicParas.ContainsKey("priceLimit") ? dicParas["priceLimit"].ToString() : string.Empty;
            List<PriceLimitModel> priceLimitModelList = Utils.DataContractJsonDeserializer<List<PriceLimitModel>>(priceLimitJson);

            if (string.IsNullOrEmpty(memberLevelId))
            {
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "会员等级无效");
            }

            String[] Ary = new String[] { "数据0", "数据1", "数据2", "数据3" };
            List<SqlDataRecord> listSqlDataRecord = new List<SqlDataRecord>();
            SqlMetaData[] MetaDataArr = new SqlMetaData[] {
                new SqlMetaData("Category", SqlDbType.Int), 
                new SqlMetaData("BalanceIndex", SqlDbType.Int),  
                new SqlMetaData("MinPrice", SqlDbType.Decimal,18,2),
                new SqlMetaData("MaxPrice", SqlDbType.Decimal,18,2)
            };

            for (int i = 0; i < priceLimitModelList.Count; i++)
            {
                List<object> listParas = new List<object>();
                listParas.Add(priceLimitModelList[i].Category);
                listParas.Add(priceLimitModelList[i].BalanceIndex);
                listParas.Add(priceLimitModelList[i].MinPrice);
                listParas.Add(priceLimitModelList[i].MaxPrice);

                var record = new SqlDataRecord(MetaDataArr);
                for (int j = 0; j < Ary.Length; j++)
                {
                    record.SetValue(j, listParas[j]);
                }
                listSqlDataRecord.Add(record);
            }

            string storedProcedure = "GetFoodListInfo";
            SqlParameter[] parameters = new SqlParameter[5];
            parameters[0] = new SqlParameter("@StoreId", userTokenDataModel.StoreID);
            parameters[1] = new SqlParameter("@CustomerType", customerType);
            parameters[2] = new SqlParameter("@MemberLevelId", memberLevelId);
            parameters[3] = new SqlParameter("@FoodTypeStr", foodTypeStr);
            parameters[4] = new SqlParameter("@PriceLimit", SqlDbType.Structured);
            parameters[4].Value = listSqlDataRecord;

            System.Data.DataSet ds = XCCloudBLL.GetStoredProcedureSentence(storedProcedure, parameters);

            List<FoodInfoModel> listFoodInfo = Utils.GetModelList<FoodInfoModel>(ds.Tables[0]).ToList();
            List<GoodModel> listGoodInfo = Utils.GetModelList<GoodModel>(ds.Tables[1]).ToList();
            List<TicketModel> listTicketInfo = Utils.GetModelList<TicketModel>(ds.Tables[2]).ToList();
            List<FoodDetailInfoModel> listFoodDetailInfo1 = Utils.GetModelList<FoodDetailInfoModel>(ds.Tables[3]).ToList();
            List<FoodDetailInfoModel> listFoodDetailInfo2 = Utils.GetModelList<FoodDetailInfoModel>(ds.Tables[4]).ToList();
            FoodSetModel foodSetModel = new FoodSetModel();

            if (listFoodInfo.Count > 0)
            {
                for (int i = 0; i < listFoodInfo.Count; i++)
                {
                    if (listFoodInfo[i].FoodType == 1)
                    {
                        listFoodInfo[i].DetailsCount = 0;
                    }
                    else
                    {
                        List<FoodDetailInfoModel> foodDetialInfo1 = listFoodDetailInfo1.Where<FoodDetailInfoModel>(p => p.FoodId == listFoodInfo[i].FoodID).ToList<FoodDetailInfoModel>().OrderBy(p => p.BalanceType).ToList<FoodDetailInfoModel>();
                        FoodDetailInfoModel model = new FoodDetailInfoModel(listFoodInfo[i].FoodID, 0, "现金", listFoodInfo[i].FoodPrice);
                        foodDetialInfo1.Insert(0, model);
                        listFoodInfo[i].DetailInfoList = foodDetialInfo1;
                        listFoodInfo[i].DetailsCount = listFoodInfo[i].DetailInfoList.Count();
                    }
                }
            }

            if (listGoodInfo.Count > 0)
            {
                for (int i = 0; i < listGoodInfo.Count; i++)
                {
                    List<FoodDetailInfoModel> foodDetialInfo1 = listFoodDetailInfo1.Where<FoodDetailInfoModel>(p => p.FoodId == listGoodInfo[i].FoodId).ToList<FoodDetailInfoModel>().OrderBy(p => p.BalanceType).ToList<FoodDetailInfoModel>();
                    FoodDetailInfoModel model = new FoodDetailInfoModel(listGoodInfo[i].FoodId, 0, "现金", listGoodInfo[i].Price);
                    foodDetialInfo1.Insert(0, model);
                    listGoodInfo[i].DetailInfoList = foodDetialInfo1;
                    listGoodInfo[i].DetailsCount = listGoodInfo[i].DetailInfoList.Count();
                }
            }

            if (listTicketInfo.Count > 0)
            {
                for (int i = 0; i < listTicketInfo.Count; i++)
                {
                    List<FoodDetailInfoModel> foodDetialInfo1 = new List<FoodDetailInfoModel>();
                    FoodDetailInfoModel model = new FoodDetailInfoModel(listTicketInfo[i].FoodId, 0, "现金", listTicketInfo[i].Price);
                    foodDetialInfo1.Insert(0, model);
                    listTicketInfo[i].DetailInfoList = foodDetialInfo1;
                    listTicketInfo[i].DetailsCount = listGoodInfo[i].DetailInfoList.Count();
                }
            }

            foodSetModel.ListFoodInfo = listFoodInfo;
            foodSetModel.ListGoodModel = listGoodInfo;
            foodSetModel.ListTicketModel = listTicketInfo;

            return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, foodSetModel);
        }

        /// <summary>
        /// 获取套餐明细
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object getFoodDetail(Dictionary<string, object> dicParas)
        {
            XCCloudUserTokenModel userTokenModel = (XCCloudUserTokenModel)(dicParas[Constant.XCCloudUserTokenModel]);
            TokenDataModel userTokenDataModel = (TokenDataModel)(userTokenModel.DataModel);

            string category = dicParas.ContainsKey("category") ? dicParas["category"].ToString() : string.Empty;
            string foodId = dicParas.ContainsKey("foodId") ? dicParas["foodId"].ToString() : string.Empty;

            if (string.IsNullOrEmpty(foodId))
            {
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "套餐名不能为空");
            }

            string sql = "exec GetFoodDetail @StoreId,@Category,@FoodId ";
            SqlParameter[] parameters = new SqlParameter[3];
            parameters[0] = new SqlParameter("@StoreId", userTokenDataModel.StoreID);
            parameters[1] = new SqlParameter("@Category", category);
            parameters[2] = new SqlParameter("@FoodId", foodId);
            System.Data.DataSet ds = XCCloudBLL.ExecuteQuerySentence(sql, parameters);

            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                if (category == "0")
                {
                    List<FoodDetailModel> list1 = Utils.GetModelList<FoodDetailModel>(ds.Tables[0]).ToList();
                    return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, list1);
                }
                else if (category == "1")
                {
                    string note = ds.Tables[0].Rows[0][0].ToString();
                    var obj = new
                    {
                        note = note
                    };
                    return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, obj);
                }
                else if (category == "2")
                {
                    string note = ds.Tables[0].Rows[0][0].ToString();
                    var obj = new
                    {
                        note = note
                    };
                    return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, obj);
                }
                else
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "商品分类不正确");
                }
            }
            else
            {
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "无数据");
            }
        }
    }
}