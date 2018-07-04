using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using XCCloudWebBar.Base;
using XCCloudWebBar.BLL.CommonBLL;
using XCCloudWebBar.Common;
using XCCloudWebBar.DBService.BLL;
using XCCloudWebBar.Model.CustomModel.XCCloud;
using XCCloudWebBar.Common.Extensions;
using XCCloudWebBar.BLL.XCCloud;
using XCCloudWebBar.Common.Enum;
using System.Transactions;
using XCCloudWebBar.Model.XCCloud;
using Microsoft.SqlServer.Server;
using XCCloudWebBar.CacheService;

namespace XXCloudService.Api.XCCloud
{
    /// <summary>
    /// Discount 的摘要说明
    /// </summary>
    public class Discount : ApiBase
    {
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object getDiscountRuleList(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenModel = (XCCloudUserTokenModel)(dicParas[Constant.XCCloudUserTokenModel]);
                TokenDataModel userTokenDataModel = (TokenDataModel)(userTokenModel.DataModel);

                int customerType = 0;
                int icCardId = 0;
                decimal foodPrice = 0;
                string customerTypeStr = dicParas.ContainsKey("customerType") ? dicParas["customerType"].ToString() : string.Empty;
                string icCardIdStr = dicParas.ContainsKey("icCardId") ? dicParas["icCardId"].ToString() : string.Empty;
                string foodPriceStr = dicParas.ContainsKey("foodPrice") ? dicParas["foodPrice"].ToString() : string.Empty;

                if (string.IsNullOrEmpty(customerTypeStr))
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

                if (!int.TryParse(customerTypeStr, out customerType))
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

                string sql = "GetDiscountRuleList";
                SqlParameter[] parameters = new SqlParameter[10];
                parameters[0] = new SqlParameter("@MerchId", userTokenDataModel.MerchID);
                parameters[1] = new SqlParameter("@StoreId", userTokenDataModel.StoreID);
                parameters[2] = new SqlParameter("@CustomerType", customerType);
                parameters[3] = new SqlParameter("@ICCardId", icCardId);
                parameters[4] = new SqlParameter("@FoodPrice", foodPrice);
                parameters[5] = new SqlParameter("@SubPrice", SqlDbType.Decimal);
                parameters[5].Direction = ParameterDirection.Output;
                parameters[6] = new SqlParameter("@CanUseDiscountRuleID", SqlDbType.Int);
                parameters[6].Direction = ParameterDirection.Output;
                parameters[7] = new SqlParameter("@QueryType", SqlDbType.Int);
                parameters[7].Value = 0;
                parameters[8] = new SqlParameter("@ErrMsg", SqlDbType.VarChar, 200);
                parameters[8].Direction = ParameterDirection.Output;
                parameters[9] = new SqlParameter("@Result", SqlDbType.Int);
                parameters[9].Direction = ParameterDirection.Output;

                System.Data.DataSet ds = XCCloudBLL.GetStoredProcedureSentence(sql, parameters);
                if (parameters[9].Value.ToString() == "1")
                {
                    Data_DistinctModel model = new Data_DistinctModel();
                    List<Data_DistinctDetailModel> detailList = Utils.GetModelList<Data_DistinctDetailModel>(ds.Tables[0]);
                    model.SubPrice = decimal.Parse(parameters[5].Value.ToString());
                    model.DiscountRuleId = int.Parse(parameters[6].Value.ToString());
                    model.DetailList = detailList;
                    return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, model);
                }
                else
                {
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, parameters[8].Value.ToString());
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object getCouponPrice(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenModel = (XCCloudUserTokenModel)(dicParas[Constant.XCCloudUserTokenModel]);
                TokenDataModel userTokenDataModel = (TokenDataModel)(userTokenModel.DataModel);

                decimal foodPrice = 0;
                int authorFlag = 0;
                string foodPriceStr = dicParas.ContainsKey("foodPrice") ? dicParas["foodPrice"].ToString() : string.Empty;
                string authorFlagStr = dicParas.ContainsKey("authorFlag") ? dicParas["authorFlag"].ToString() : string.Empty;
                string couponDetailsJson = dicParas.ContainsKey("couponDetails") ? dicParas["couponDetails"].ToString() : string.Empty;

                if (string.IsNullOrEmpty(authorFlagStr))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "会员级别参数不能为空");
                }

                if (!int.TryParse(authorFlagStr, out authorFlag))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "验证标志");
                }

                if (string.IsNullOrEmpty(foodPriceStr))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "套餐价格参数不能为空");
                }

                if (!decimal.TryParse(foodPriceStr, out foodPrice))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "套餐价格数据类型不正确");
                }

                List<Data_CouponParamModel> buyDetailList = Utils.DataContractJsonDeserializer<List<Data_CouponParamModel>>(couponDetailsJson);

                string sql = "CheckCoupon";

                String[] Ary = new String[] { "数据0", "数据1" };
                List<SqlDataRecord> listSqlDataRecord = new List<SqlDataRecord>();
                SqlMetaData[] MetaDataArr = new SqlMetaData[] { 
                    new SqlMetaData("couponId", SqlDbType.Int), 
                    new SqlMetaData("couponCode", SqlDbType.VarChar,50)
                };

                string flwSendId = RedisCacheHelper.CreateCloudSerialNo(userTokenDataModel.StoreID);

                for (int i = 0; i < buyDetailList.Count; i++)
                {
                    List<object> listParas = new List<object>();
                    listParas.Add(buyDetailList[i].CouponId);
                    listParas.Add(buyDetailList[i].CouponCode);

                    var record = new SqlDataRecord(MetaDataArr);
                    for (int j = 0; j < Ary.Length; j++)
                    {
                        record.SetValue(j, listParas[j]);
                    }
                    listSqlDataRecord.Add(record);
                }

                SqlParameter[] parameters = new SqlParameter[8];
                parameters[0] = new SqlParameter("@CouponsList", SqlDbType.Structured);
                parameters[0].Value = listSqlDataRecord;
                parameters[1] = new SqlParameter("@StoreId", userTokenDataModel.StoreID);
                parameters[1].Value = userTokenDataModel.StoreID;
                parameters[2] = new SqlParameter("@QueryType", SqlDbType.Int);
                parameters[2].Value = 0;
                parameters[3] = new SqlParameter("@FoodPrice", SqlDbType.Decimal);
                parameters[3].Value = foodPrice;
                parameters[4] = new SqlParameter("@AuthorFlag", SqlDbType.Int);
                parameters[4].Value = 0;
                parameters[5] = new SqlParameter("@FeePrice", SqlDbType.Decimal);
                parameters[5].Direction = ParameterDirection.Output;
                parameters[6] = new SqlParameter("@ErrMsg", SqlDbType.VarChar, 200);
                parameters[6].Direction = ParameterDirection.Output;
                parameters[7] = new SqlParameter("@Return", SqlDbType.Int);
                parameters[7].Direction = ParameterDirection.ReturnValue;

                System.Data.DataSet ds = XCCloudBLL.GetStoredProcedureSentence(sql, parameters);
                if (parameters[7].Value.ToString() == "1")
                {
                    List<Data_CouponParamModel> detailList = Utils.GetModelList<Data_CouponParamModel>(ds.Tables[0]);
                    return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, detailList);
                }
                else
                {
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, parameters[6].Value.ToString());
                }
            }
            catch (Exception e)
            {
                throw e;
            }

        }


        [Authorize(Roles = "MerchUser")]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryDiscountRule(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

                string errMsg = string.Empty;
                object[] conditions = dicParas.ContainsKey("conditions") ? (object[])dicParas["conditions"] : null;

                SqlParameter[] parameters = new SqlParameter[1];
                parameters[0] = new SqlParameter("@MerchId", merchId);

                string sqlWhere = string.Empty;
                if (conditions != null && conditions.Length > 0)
                {
                    if (!QueryBLL.GenDynamicSql(conditions, "a.", ref sqlWhere, ref parameters, out errMsg))
                    {
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                }

                string storedProcedure = "QueryDiscountRule";
                Array.Resize(ref parameters, parameters.Length + 1);
                parameters[parameters.Length - 1] = new SqlParameter("@SqlWhere", sqlWhere);
                Array.Resize(ref parameters, parameters.Length + 1);
                parameters[parameters.Length - 1] = new SqlParameter("@Result", SqlDbType.Int);
                parameters[parameters.Length - 1].Direction = ParameterDirection.Output;
                System.Data.DataSet ds = XCCloudBLL.GetStoredProcedureSentence(storedProcedure, parameters);
                if (parameters[parameters.Length - 1].Value.ToString() != "1")
                {
                    errMsg = "查询满减优惠规则信息失败";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var data_DiscountRule = Utils.GetModelList<Data_DiscountRuleModel>(ds.Tables[0]).OrderBy(or => or.RuleLevel).ToList();

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, data_DiscountRule);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [Authorize(Roles = "MerchUser")]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetDiscountRule(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                int id = dicParas.Get("id").Toint(0);
                if (id == 0)
                {
                    errMsg = "规则ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var data_CouponInfo = Data_DiscountRuleService.I.GetModels(p => p.ID == id).FirstOrDefault();
                if (data_CouponInfo == null)
                {
                    errMsg = "该满减优惠规则不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var discountDetails = Data_Discount_DetailService.I.GetModels(p => p.DiscountRuleID == id).Select(o => new
                {
                    LimitCount = o.LimitCount,
                    LimitType = o.LimitType,
                    ConsumeCount = o.ConsumeCount,
                    DiscountCount = o.DiscountCount
                });

                var discountMemberLevels = Data_Discount_MemberLevelService.I.GetModels(p => p.DiscountRuleID == id).Select(o => new
                {
                    MemberLevelID = o.MemberLevelID
                });

                var discountStores = Data_Discount_StoreListService.I.GetModels(p => p.DiscountRuleID == id).Select(o => new
                {
                    StoreID = o.StoreID
                });

                var linq = new
                {
                    data_CouponInfo = data_CouponInfo,
                    discountDetails = discountDetails,
                    discountMemberLevels = discountMemberLevels
                }.AsFlatDictionary();

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, linq);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [Authorize(Roles = "MerchUser")]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object SaveDiscountRule(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

                string errMsg = string.Empty;
                if (!dicParas.Get("ruleName").Nonempty("规则名称", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("startDate").Validdate("使用期限", out errMsg) || !dicParas.Get("endDate").Validdate("使用期限", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("weekType").Validint("时段类型", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("startTime").Validdate("使用时段", out errMsg) || !dicParas.Get("endTime").Validdate("使用时段", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("storeFreq").Validint("门店频率", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("storeCount").Validint("门店次数", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("shareCount").Validint("次数共享", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("memberFreq").Validint("会员频率", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("memberCount").Validint("会员次数", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("allowGuest").Validint("允许散客", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                //时段类型
                if (dicParas.Get("weekType").Toint() == (int)TimeType.Custom)
                {
                    if (!dicParas.Get("week").Nonempty("优惠周天", out errMsg))
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                //使用期限
                if (dicParas.Get("startDate").Todatetime() > dicParas.Get("endDate").Todatetime())
                {
                    errMsg = "使用期限范围不正确，开始时间不能大于结束时间";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                //使用时段
                if (dicParas.Get("startTime").Totimespan() > dicParas.Get("endTime").Totimespan())
                {
                    errMsg = "使用时段范围不正确，开始时段不能大于结束时段";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                //不可用期限
                if (!string.IsNullOrEmpty(dicParas.Get("noStartDate")))
                {
                    if (!dicParas.Get("noStartDate").Validdate("不可用期限", out errMsg) || !dicParas.Get("noEndDate").Validdate("不可用期限", out errMsg))
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    if (dicParas.Get("noStartDate").Todatetime() > dicParas.Get("noEndDate").Todatetime())
                    {
                        errMsg = "不可用期限范围不正确，开始时间不能大于结束时间";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                }

                int id = dicParas.Get("id").Toint(0);
                var discountDetails = dicParas.GetArray("discountDetails");
                var discountMemberLevels = dicParas.GetArray("discountMemberLevels");
                var discountStores = dicParas.GetArray("discountStores");
                var startDate = dicParas.Get("startDate").Todate();
                var enddate = dicParas.Get("enddate").Todate();
                var noStartDate = dicParas.Get("noStartDate").Todate();
                var noEndDate = dicParas.Get("noEndDate").Todate();

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        var data_DiscountRule = new Data_DiscountRule();
                        Utils.GetModel(dicParas, ref data_DiscountRule);
                        data_DiscountRule.MerchID = merchId;
                        data_DiscountRule.StartDate = startDate;
                        data_DiscountRule.Enddate = enddate;
                        data_DiscountRule.NoStartDate = noStartDate;
                        data_DiscountRule.NoEndDate = noEndDate;
                        if (id == 0)
                        {
                            //新增
                            if (!Data_DiscountRuleService.I.Add(data_DiscountRule))
                            {
                                errMsg = "添加满减优惠规则信息失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }
                        else
                        {
                            if (!Data_DiscountRuleService.I.Any(p => p.ID == id))
                            {
                                errMsg = "该满减优惠规则信息不存在";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            //修改
                            if (!Data_DiscountRuleService.I.Update(data_DiscountRule))
                            {
                                errMsg = "修改满减优惠规则信息失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }

                        id = data_DiscountRule.ID;

                        //添加满减优惠明细
                        if (discountDetails != null && discountDetails.Count() >= 0)
                        {
                            //先删除，后添加
                            foreach (var model in Data_Discount_DetailService.I.GetModels(p => p.DiscountRuleID == id))
                            {
                                Data_Discount_DetailService.I.DeleteModel(model);
                            }

                            foreach (IDictionary<string, object> el in discountDetails)
                            {
                                if (el != null)
                                {
                                    var dicPara = new Dictionary<string, object>(el, StringComparer.OrdinalIgnoreCase);
                                    if (!dicPara.Get("limitCount").Validdecimal("消费额度", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPara.Get("limitType").Validint("额度类别", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPara.Get("consumeCount").Validdecimal("消费金额", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPara.Get("discountCount").Validdecimal("优惠金额", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                                    var data_Discount_Detail = new Data_Discount_Detail();
                                    Utils.GetModel(dicPara, ref data_Discount_Detail);
                                    data_Discount_Detail.DiscountRuleID = id;
                                    Data_Discount_DetailService.I.AddModel(data_Discount_Detail);
                                }
                                else
                                {
                                    errMsg = "提交数据包含空对象";
                                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                }
                            }

                            if (!Data_Discount_DetailService.I.SaveChanges())
                            {
                                errMsg = "保存满减优惠规则明细失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }

                        //添加会员级别
                        if (discountMemberLevels != null && discountMemberLevels.Count() >= 0)
                        {
                            //先删除，后添加
                            foreach (var model in Data_Discount_MemberLevelService.I.GetModels(p => p.DiscountRuleID == id))
                            {
                                Data_Discount_MemberLevelService.I.DeleteModel(model);
                            }

                            foreach (IDictionary<string, object> el in discountMemberLevels)
                            {
                                if (el != null)
                                {
                                    var dicPara = new Dictionary<string, object>(el, StringComparer.OrdinalIgnoreCase);
                                    if (!dicPara.Get("memberLevelId").Validint("会员级别ID", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                                    var data_Discount_MemberLevel = new Data_Discount_MemberLevel();
                                    Utils.GetModel(dicPara, ref data_Discount_MemberLevel);
                                    data_Discount_MemberLevel.MerchID = merchId;
                                    data_Discount_MemberLevel.DiscountRuleID = id;
                                    Data_Discount_MemberLevelService.I.AddModel(data_Discount_MemberLevel);
                                }
                                else
                                {
                                    errMsg = "提交数据包含空对象";
                                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                }
                            }

                            if (!Data_Discount_MemberLevelService.I.SaveChanges())
                            {
                                errMsg = "保存满减优惠规则会员级别信息失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }

                        //添加适用门店
                        if (discountStores != null && discountStores.Count() >= 0)
                        {
                            //先删除，后添加
                            foreach (var model in Data_Discount_StoreListService.I.GetModels(p => p.DiscountRuleID == id))
                            {
                                Data_Discount_StoreListService.I.DeleteModel(model);
                            }

                            foreach (IDictionary<string, object> el in discountStores)
                            {
                                if (el != null)
                                {
                                    var dicPara = new Dictionary<string, object>(el, StringComparer.OrdinalIgnoreCase);
                                    if (!dicPara.Get("storeId").Validint("门店ID", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                                    var data_Discount_StoreList = new Data_Discount_StoreList();
                                    Utils.GetModel(dicPara, ref data_Discount_StoreList);
                                    data_Discount_StoreList.DiscountRuleID = id;
                                    Data_Discount_StoreListService.I.AddModel(data_Discount_StoreList);
                                }
                                else
                                {
                                    errMsg = "提交数据包含空对象";
                                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                }
                            }

                            if (!Data_Discount_StoreListService.I.SaveChanges())
                            {
                                errMsg = "保存满减优惠规则适用门店信息失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }

                        ts.Complete();
                    }
                    catch (Exception e)
                    {
                        errMsg = e.Message;
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                }

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [Authorize(Roles = "MerchUser")]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object DelDiscountRule(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                int id = dicParas.Get("id").Toint(0);
                if (id == 0)
                {
                    errMsg = "规则ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        if (!Data_DiscountRuleService.I.Any(a => a.ID == id))
                        {
                            errMsg = "该满减优惠规则信息不存在";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        var data_DiscountRule = Data_DiscountRuleService.I.GetModels(p => p.ID == id).FirstOrDefault();
                        data_DiscountRule.State = 0;
                        if (!Data_DiscountRuleService.I.Update(data_DiscountRule))
                        {
                            errMsg = "删除满减优惠规则信息失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        ts.Complete();
                    }
                    catch (Exception ex)
                    {
                        return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, ex.Message);
                    }
                }

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [Authorize(Roles = "MerchUser")]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetDiscountStores(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                int discountRuleId = dicParas.Get("discountRuleId").Toint(0);

                var storeList = from a in Data_Discount_StoreListService.N.GetModels(p => p.DiscountRuleID == discountRuleId)
                                join b in Base_StoreInfoService.N.GetModels() on a.StoreID equals b.StoreID
                                select new { StoreID = a.StoreID, StoreName = b.StoreName };

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, storeList);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [Authorize(Roles = "MerchUser")]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object SaveDiscountStores(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                int discountRuleId = dicParas.Get("discountRuleId").Toint(0);
                string storeIds = dicParas.ContainsKey("storeIds") ? (dicParas["storeIds"] + "") : string.Empty;

                if (discountRuleId == 0)
                {
                    errMsg = "满减优惠规则ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        foreach (var model in Data_Discount_StoreListService.I.GetModels(p => p.DiscountRuleID == discountRuleId))
                        {
                            Data_Discount_StoreListService.I.DeleteModel(model);
                        }

                        if (!string.IsNullOrEmpty(storeIds))
                        {
                            foreach (var storeId in storeIds.Split('|'))
                            {
                                if (!storeId.Nonempty("门店ID", out errMsg))
                                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                                var model = new Data_Discount_StoreList();
                                model.DiscountRuleID = discountRuleId;
                                model.StoreID = storeId;
                                Data_Discount_StoreListService.I.AddModel(model);
                            }
                        }

                        if (!Data_Discount_StoreListService.I.SaveChanges())
                        {
                            errMsg = "更新满减优惠规则适用门店表失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        ts.Complete();
                    }
                    catch (Exception ex)
                    {
                        errMsg = ex.Message;
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                }

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 升降优先级
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [Authorize(Roles = "MerchUser")]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object UpdateDiscountLevel(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

                string errMsg = string.Empty;
                int discountRuleId = dicParas.Get("discountRuleId").Toint(0);
                if (discountRuleId == 0)
                {
                    errMsg = "满减优惠规则ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (!dicParas.Get("updateState").Nonempty("升降级状态", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                if (dicParas.Get("updateState").Toint() != 1 && dicParas.Get("updateState").Toint() != -1)
                {
                    errMsg = "升降级状态参数值不正确";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                int updateState = dicParas.Get("updateState").Toint(0);

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        if (!Data_DiscountRuleService.I.Any(a => a.ID == discountRuleId))
                        {
                            errMsg = "该满减优惠规则不存在";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        //设置当前优先级别，默认为1,且不超过最大优先级
                        var data_DiscountRule = Data_DiscountRuleService.I.GetModels(p => p.ID == discountRuleId).FirstOrDefault();
                        var oldLevel = data_DiscountRule.RuleLevel;
                        data_DiscountRule.RuleLevel = (data_DiscountRule.RuleLevel ?? 0) + updateState;
                        if (data_DiscountRule.RuleLevel < 1)
                        {
                            data_DiscountRule.RuleLevel = 1;
                        }

                        var max = Data_DiscountRuleService.I.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase)).Max(m => m.RuleLevel);
                        if (data_DiscountRule.RuleLevel > max)
                        {
                            data_DiscountRule.RuleLevel = max;
                        }

                        Data_DiscountRuleService.I.UpdateModel(data_DiscountRule);

                        var newLevel = data_DiscountRule.RuleLevel;
                        if (oldLevel != newLevel || oldLevel == null)
                        {
                            if (oldLevel == null)
                            {
                                //后续优惠券降一级
                                var linq = Data_DiscountRuleService.I.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && p.ID != discountRuleId
                                                               && p.RuleLevel >= newLevel);
                                foreach (var model in linq)
                                {
                                    model.RuleLevel = model.RuleLevel + 1;
                                    Data_DiscountRuleService.I.UpdateModel(model);
                                }
                            }
                            else
                            {
                                //与下一个优惠券交换优先级
                                var nextModel = Data_DiscountRuleService.I.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && p.ID != discountRuleId
                                                               && p.RuleLevel == newLevel).FirstOrDefault();
                                if (nextModel != null)
                                {
                                    nextModel.RuleLevel = nextModel.RuleLevel - updateState;
                                    Data_DiscountRuleService.I.UpdateModel(nextModel);
                                }
                            }
                        }

                        if (!Data_DiscountRuleService.I.SaveChanges())
                        {
                            errMsg = "更新满减优惠优先级失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        ts.Complete();
                    }
                    catch (Exception e)
                    {
                        errMsg = e.Message;
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                }

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }
    }
}