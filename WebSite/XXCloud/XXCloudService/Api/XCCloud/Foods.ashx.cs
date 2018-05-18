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
                StoreIDDataModel userTokenDataModel = (StoreIDDataModel)(userTokenModel.DataModel);

                string sql = "GetFoodType";
                SqlParameter[] parameters = new SqlParameter[1];
                parameters[0] = new SqlParameter("@MerchId", userTokenDataModel.MerchId);
                System.Data.DataSet ds = XCCloudBLL.GetStoredProcedureSentence(sql, parameters);

                List<Base_FoodType> foodTypeMainList = Utils.GetModelList<Base_FoodType>(ds.Tables[0]);
                List<Base_FoodType> foodTypeGoodList = Utils.GetModelList<Base_FoodType>(ds.Tables[1]);
                List<Base_FoodType> foodTypeTicketList = Utils.GetModelList<Base_FoodType>(ds.Tables[2]);

                var obj = new {
                    foodTypeMainList = foodTypeMainList,
                    foodTypeGoodList = foodTypeGoodList,
                    foodTypeTicketList = foodTypeTicketList
                };

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, obj);
            }
            catch(Exception e)
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
            StoreIDDataModel userTokenDataModel = (StoreIDDataModel)(userTokenModel.DataModel);

            string memberLevelId = dicParas.ContainsKey("memberLevelId") ? dicParas["memberLevelId"].ToString() : string.Empty;

            if (string.IsNullOrEmpty(memberLevelId))
            {
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "会员等级无效");
            }

            string sql = "GetMemberOpenCardFoodInfo";
            SqlParameter[] parameters = new SqlParameter[4];
            parameters[0] = new SqlParameter("@StoreId", userTokenDataModel.StoreId);
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
            StoreIDDataModel userTokenDataModel = (StoreIDDataModel)(userTokenModel.DataModel);

            string customerType = dicParas.ContainsKey("customerType") ? dicParas["customerType"].ToString() : string.Empty;
            string memberLevelId = dicParas.ContainsKey("memberLevelId") ? dicParas["memberLevelId"].ToString() : string.Empty;
            string foodTypeStr = dicParas.ContainsKey("foodTypeStr") ? dicParas["foodTypeStr"].ToString() : string.Empty;

            if (string.IsNullOrEmpty(memberLevelId))
            {
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "会员等级无效");
            }

            string sql = "exec GetFoodListInfo @StoreId,@CustomerType,@MemberLevelId,@FoodTypeStr ";
            SqlParameter[] parameters = new SqlParameter[4];
            parameters[0] = new SqlParameter("@StoreId", userTokenDataModel.StoreId);
            parameters[1] = new SqlParameter("@CustomerType", customerType);
            parameters[2] = new SqlParameter("@MemberLevelId", memberLevelId);
            parameters[3] = new SqlParameter("@FoodTypeStr", foodTypeStr);
            System.Data.DataSet ds = XCCloudBLL.ExecuteQuerySentence(sql, parameters);
            DataTable dtFoodInfo = ds.Tables[0];

            List<FoodInfoModel> listFoodInfo = Utils.GetModelList<FoodInfoModel>(ds.Tables[0]).ToList();
            List<GoodModel> listGoodInfo = Utils.GetModelList<GoodModel>(ds.Tables[1]).ToList();
            List<TicketModel> listTicketInfo = Utils.GetModelList<TicketModel>(ds.Tables[2]).ToList();
            List<FoodDetailInfoModel> listFoodDetailInfo = Utils.GetModelList<FoodDetailInfoModel>(ds.Tables[3]).ToList();
            FoodSetModel foodSetModel = new FoodSetModel();

            if (dtFoodInfo.Rows.Count > 0)
            {
                for (int i = 0; i < listFoodInfo.Count; i++)
                {
                    if (listFoodInfo[i].FoodType == 1)
                    {
                        listFoodInfo[i].DetailsCount = 0;
                    }
                    else
                    {
                        List<FoodDetailInfoModel> foodDetialInfo = listFoodDetailInfo.Where<FoodDetailInfoModel>(p => p.FoodId == listFoodInfo[i].FoodID).ToList<FoodDetailInfoModel>();
                        listFoodInfo[i].DetailInfoList = foodDetialInfo;
                        listFoodInfo[i].DetailsCount = listFoodInfo[i].DetailInfoList.Count();
                    }
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
            StoreIDDataModel userTokenDataModel = (StoreIDDataModel)(userTokenModel.DataModel);

            string category = dicParas.ContainsKey("category") ? dicParas["category"].ToString() : string.Empty;
            string foodId = dicParas.ContainsKey("foodId") ? dicParas["foodId"].ToString() : string.Empty;

            if (string.IsNullOrEmpty(foodId))
            {
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "套餐名不能为空");
            }

            string sql = "exec GetFoodDetail @StoreId,@Category,@FoodId ";
            SqlParameter[] parameters = new SqlParameter[3];
            parameters[0] = new SqlParameter("@StoreId", userTokenDataModel.StoreId);
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
                    var obj = new {
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