using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using XCCloudService.Base;
using XCCloudService.BLL.CommonBLL;
using XCCloudService.Common;
using XCCloudService.Model.CustomModel.XCCloud;

namespace XXCloudService.Api.XCCloud
{
    /// <summary>
    /// Discount 的摘要说明
    /// </summary>
    public class Discount : ApiBase
    {
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object getDiscountForAPI(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenModel = (XCCloudUserTokenModel)(dicParas[Constant.XCCloudUserTokenModel]);
                StoreIDDataModel userTokenDataModel = (StoreIDDataModel)(userTokenModel.DataModel);

                int memberLevelId = 0;
                int icCardId = 0;
                decimal foodPrice = 0;
                string memberLevelIdStr = dicParas.ContainsKey("memberLevelId") ? dicParas["memberLevelId"].ToString() : string.Empty;
                string icCardIdStr = dicParas.ContainsKey("icCardId") ? dicParas["icCardId"].ToString() : string.Empty;
                string foodPriceStr = dicParas.ContainsKey("foodPrice") ? dicParas["foodPrice"].ToString() : string.Empty;

                if (string.IsNullOrEmpty(memberLevelIdStr))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "会员级别参数不能为空");
                }

                if (string.IsNullOrEmpty(icCardIdStr))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "会员卡号参数不能为空");
                }

                if (string.IsNullOrEmpty(foodPriceStr))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "套餐价格参数不能为空");
                }

                if (!int.TryParse(memberLevelIdStr, out memberLevelId))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "会员级别数据类型不正确");
                }

                if (!int.TryParse(icCardIdStr, out icCardId))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "会员卡号数据类型不正确");
                }

                if (!decimal.TryParse(foodPriceStr, out foodPrice))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "套餐价格数据类型不正确");
                }

                string sql = "GetDiscountForAPI";
                SqlParameter[] parameters = new SqlParameter[9];
                parameters[0] = new SqlParameter("@MerchId", userTokenDataModel.MerchId);
                parameters[1] = new SqlParameter("@StoreId", userTokenDataModel.StoreId);
                parameters[2] = new SqlParameter("@MemberLevelId", memberLevelId);
                parameters[3] = new SqlParameter("@ICCardId", icCardId);
                parameters[4] = new SqlParameter("@FoodPrice", foodPrice);
                parameters[5] = new SqlParameter("@SubPrice", SqlDbType.Decimal);
                parameters[5].Direction = ParameterDirection.Output;
                parameters[6] = new SqlParameter("@DiscountRuleID", SqlDbType.Int);
                parameters[6].Direction = ParameterDirection.Output;
                parameters[7] = new SqlParameter("@ErrMsg", SqlDbType.VarChar, 200);
                parameters[7].Direction = ParameterDirection.Output;
                parameters[8] = new SqlParameter("@RS", SqlDbType.Int);
                parameters[8].Direction = ParameterDirection.ReturnValue;

                System.Data.DataSet ds = XCCloudBLL.GetStoredProcedureSentence(sql, parameters);
                if (parameters[8].Value.ToString() == "1")
                {
                    decimal subPrice = decimal.Parse(parameters[5].Value.ToString());
                    var obj = new { 
                        subPrice = Convert.ToDecimal(subPrice).ToString("0.00")
                    };
                    return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, obj);
                }
                else
                {
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, parameters[7].Value.ToString());
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}