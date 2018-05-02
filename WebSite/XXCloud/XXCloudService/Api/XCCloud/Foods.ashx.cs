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

                List<Base_FoodType> foodTypeList = Utils.GetModelList<Base_FoodType>(ds.Tables[0]);

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, foodTypeList);
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
            DataTable dtFoodDetailInfo = ds.Tables[1];

            if (dtFoodInfo.Rows.Count > 0)
            {
                List<FoodInfoModel> listFoodInfo = Utils.GetModelList<FoodInfoModel>(dtFoodInfo).ToList();
                List<FoodDetailInfoModel> listFoodDetailInfo = Utils.GetModelList<FoodDetailInfoModel>(dtFoodDetailInfo).ToList();
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
                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, listFoodInfo);
            }

            return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "无数据");
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

            string foodId = dicParas.ContainsKey("foodId") ? dicParas["foodId"].ToString() : string.Empty;

            if (string.IsNullOrEmpty(foodId))
            {
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "套餐名不能为空");
            }

            string sql = "exec GetFoodDetail @StoreId,@FoodId ";
            SqlParameter[] parameters = new SqlParameter[2];
            parameters[0] = new SqlParameter("@StoreId", userTokenDataModel.StoreId);
            parameters[1] = new SqlParameter("@FoodId", foodId);
            System.Data.DataSet ds = XCCloudBLL.ExecuteQuerySentence(sql, parameters);
            DataTable dt = ds.Tables[0];

            if (dt.Rows.Count > 0)
            {
                List<FoodDetailModel> list1 = Utils.GetModelList<FoodDetailModel>(ds.Tables[0]).ToList();
                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, list1);
            }
            else
            { 
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "无数据");
            }   
        }
    }
}