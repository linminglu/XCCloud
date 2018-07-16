using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using XCCloudService.Base;
using XCCloudService.BLL.CommonBLL;
using XCCloudService.BLL.XCCloud;
using XCCloudService.Common;
using XCCloudService.Common.Enum;
using XCCloudService.Common.Extensions;
using XCCloudService.DBService.BLL;
using XCCloudService.Model.CustomModel.XCCloud;

namespace XXCloudService.Api.XCCloud
{
    [Authorize(Roles = "StoreUser")]
    /// <summary>
    /// MemberManage 的摘要说明
    /// </summary>
    public class MemberManage : ApiBase
    {
        #region 会员资料

        /// <summary>
        /// 会员档案查询
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryMemberInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                //string merchId = dicParas.Get("merchId");
                //string storeId = dicParas.Get("storeId");

                string errMsg = string.Empty;
                object[] conditions = dicParas.ContainsKey("conditions") ? (object[])dicParas["conditions"] : null;

                SqlParameter[] parameters = new SqlParameter[2];
                parameters[0] = new SqlParameter("@MerchID", merchId);
                parameters[1] = new SqlParameter("@StoreID", storeId);

                string sqlWhere = string.Empty;
                if (conditions != null && conditions.Length > 0)
                {
                    if (!QueryBLL.GenDynamicSql(conditions, "a.", ref sqlWhere, out errMsg))
                    {
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                }

                string storedProcedure = "QueryMemberInfo";
                Array.Resize(ref parameters, parameters.Length + 1);
                parameters[parameters.Length - 1] = new SqlParameter("@SqlWhere", sqlWhere);
                Array.Resize(ref parameters, parameters.Length + 1);
                parameters[parameters.Length - 1] = new SqlParameter("@Result", SqlDbType.Int);
                parameters[parameters.Length - 1].Direction = ParameterDirection.Output;

                System.Data.DataSet ds = XCCloudBLL.GetStoredProcedureSentence(storedProcedure, parameters);
                if (parameters[parameters.Length - 1].Value.ToString() != "1")
                {
                    errMsg = "查询会员档案数据失败";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }
                                
                if (ds.Tables.Count > 1)
                {
                    var jsonArr = new {
                        table1 = Utils.DataTableToJson(ds.Tables[1]), //会员档案信息
                        table2 = ds.Tables[0].Rows.Cast<DataRow>().ToDictionary(x => x[0].ToString(), x => x[1].ToString()) //余额类别列表
                    };

                    return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, jsonArr);
                }

                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, "查询数据失败");
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 会员档案查询-押金
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryMemberDepositInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

                string errMsg = string.Empty;
                if(!dicParas.Get("id").Nonempty("卡ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var id = dicParas.Get("id");

                var data_Member_CardService = Data_Member_CardService.I;
                if (!data_Member_CardService.Any(p => p.ID.Equals(id, StringComparison.OrdinalIgnoreCase)))
                {
                    errMsg = "该卡信息不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                //如果是主卡, 查出所有附属卡（包括主卡)
                var data_Member_CardModel = data_Member_CardService.GetModels(p => p.ID.Equals(id, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if(data_Member_CardModel.CardType == 0) 
                {
                    var data_Member_Card = data_Member_CardService.GetModels(p => p.ParentCard.Equals(id, StringComparison.OrdinalIgnoreCase))
                                           .Select(o => new
                                           {
                                               ICCardID = o.ICCardID,
                                               CardType = "附属卡",
                                               Deposit = o.Deposit,
                                               CardName = o.CardName,
                                               CardBirthDay = o.CardBirthDay,
                                               CardSex = o.CardSex == 0 ? "男" : o.CardSex == 1 ? "女" : string.Empty
                                           }).ToList();
                    data_Member_Card.Insert(0, new{
                        ICCardID = data_Member_CardModel.ICCardID,
                        CardType = "主卡",
                        Deposit = data_Member_CardModel.Deposit,
                        CardName = data_Member_CardModel.CardName,
                        CardBirthDay = data_Member_CardModel.CardBirthDay,
                        CardSex = data_Member_CardModel.CardSex == 0 ? "男" : data_Member_CardModel.CardSex == 1 ? "女" : string.Empty
                    });

                    return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, data_Member_Card);
                }
                else
                {
                    var data_Member_Card = new List<Object>();
                    data_Member_Card.Add(new
                    {
                        ICCardID = data_Member_CardModel.ICCardID,
                        CardType = data_Member_CardModel.CardType == 1 ? "附属卡" : string.Empty,
                        Deposit = data_Member_CardModel.Deposit,
                        CardName = data_Member_CardModel.CardName,
                        CardBirthDay = data_Member_CardModel.CardBirthDay,
                        CardSex = data_Member_CardModel.CardSex == 0 ? "男" : data_Member_CardModel.CardSex == 1 ? "女" : string.Empty
                    });

                    return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, data_Member_Card);
                }
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 入会记录查询
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryMemberEntryInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                //string merchId = dicParas.Get("merchId");
                //string storeId = dicParas.Get("storeId");

                string errMsg = string.Empty;
                object[] conditions = dicParas.ContainsKey("conditions") ? (object[])dicParas["conditions"] : null;

                SqlParameter[] parameters = new SqlParameter[2];
                parameters[0] = new SqlParameter("@MerchID", merchId);
                parameters[1] = new SqlParameter("@StoreID", storeId);

                string sqlWhere = string.Empty;
                if (conditions != null && conditions.Length > 0)
                {
                    if (!QueryBLL.GenDynamicSql(conditions, "a.", ref sqlWhere, out errMsg))
                    {
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                }

                string storedProcedure = "QueryMemberEntryInfo";
                Array.Resize(ref parameters, parameters.Length + 1);
                parameters[parameters.Length - 1] = new SqlParameter("@SqlWhere", sqlWhere);
                Array.Resize(ref parameters, parameters.Length + 1);
                parameters[parameters.Length - 1] = new SqlParameter("@Result", SqlDbType.Int);
                parameters[parameters.Length - 1].Direction = ParameterDirection.Output;

                System.Data.DataSet ds = XCCloudBLL.GetStoredProcedureSentence(storedProcedure, parameters);
                if (parameters[parameters.Length - 1].Value.ToString() != "1")
                {
                    errMsg = "查询入会记录信息失败";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (ds.Tables.Count > 0)
                {
                    List<object> listObj = new List<object>();
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        var obj = new
                        {
                            ICCardID = ds.Tables[0].Rows[i]["ICCardID"].Tostring(),
                            CardName = ds.Tables[0].Rows[i]["CardName"].Tostring(),
                            MemberLevelID = ds.Tables[0].Rows[i]["MemberLevelID"].Toint(),
                            MemberLevelName = ds.Tables[0].Rows[i]["MemberLevelName"].Tostring(),
                            CreateTime = ds.Tables[0].Rows[i]["CreateTime"].Todatetime(),
                            JoinChannel = ((JoinChannel?)ds.Tables[0].Rows[i]["JoinChannel"].Toint()).GetDescription(),
                            OrderID = ds.Tables[0].Rows[i]["OrderID"].Tostring(),
                            Deposit = ds.Tables[0].Rows[i]["Deposit"].Todecimal(),
                            OpenFee = ds.Tables[0].Rows[i]["OpenFee"].Todecimal(),
                            EndDate = ds.Tables[0].Rows[i]["EndDate"].Todatetime(),
                            StoreName = ds.Tables[0].Rows[i]["StoreName"].Tostring(),
                            CheckDate = ds.Tables[0].Rows[i]["CheckDate"].Todate(),
                            ScheduleName = ds.Tables[0].Rows[i]["ScheduleName"].Tostring(),
                            WorkStation = ds.Tables[0].Rows[i]["WorkStation"].Tostring(),
                            UserName = ds.Tables[0].Rows[i]["UserName"].Tostring(),
                            Note = ds.Tables[0].Rows[i]["Note"].Tostring(),
                        };
                        listObj.Add(obj);
                    }

                    return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, listObj);
                }

                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, "查询数据失败");
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 会员入会查询-入会套餐
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryMemberFoodInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

                string errMsg = string.Empty;
                if (!dicParas.Get("id").Nonempty("卡ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var id = dicParas.Get("id");

                var data_Member_CardService = Data_Member_CardService.I;
                if (!data_Member_CardService.Any(p => p.ID.Equals(id, StringComparison.OrdinalIgnoreCase)))
                {
                    errMsg = "该卡信息不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var flw_OrderService = Flw_OrderService.I;
                var flw_Order_DetailService = Flw_Order_DetailService.N;
                var flw_Food_SaleService = Flw_Food_SaleService.N;
                var orderId = data_Member_CardService.GetModels(p => p.ID.Equals(id, StringComparison.OrdinalIgnoreCase)).Select(o => o.OrderID).FirstOrDefault();
                if (!flw_OrderService.Any(a => a.ID.Equals(orderId, StringComparison.OrdinalIgnoreCase)))
                {
                    errMsg = "该卡入会订单不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }
                var orderModel = flw_OrderService.GetModels(p => p.ID.Equals(orderId, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                //购买商品列表
                var data_FoodInfoService = Data_FoodInfoService.N;
                var data_ProjectTicketService = Data_ProjectTicketService.N;
                var base_GoodInfo = Base_GoodsInfoService.N;
                var buyGoodInfos = (from a in flw_Food_SaleService.GetModels(p => p.SingleType == (int)SingleType.Food)
                                   join b in data_FoodInfoService.GetModels() on a.FoodID equals b.ID.ToString()
                                   select new { GoodName = b.FoodName, GoodType = "套餐", a = a }).Union(
                                       from a in flw_Food_SaleService.GetModels(p => p.SingleType == (int)SingleType.ProjectTicket)
                                       join b in data_ProjectTicketService.GetModels() on a.FoodID equals b.ID.ToString()
                                       select new { GoodName = b.TicketName, GoodType = "门票", a = a }
                                       ).Union(from a in flw_Food_SaleService.GetModels(p => p.SingleType == (int)SingleType.Good)
                                               join b in base_GoodInfo.GetModels() on a.FoodID equals b.ID.ToString()
                                               select new { GoodName = b.GoodName, GoodType = "单品", a = a }).Select(o => new
                                               {
                                                   GoodName = o.GoodName,
                                                   GoodType = o.GoodType,
                                                   SaleCount = o.a.SaleCount,
                                                   TotalMoney = o.a.TotalMoney,
                                                   Price = (o.a.SaleCount ?? 0) == 0 ? 0.00M : Math.Round((o.a.TotalMoney ?? 0) / o.a.SaleCount.Value, 2, MidpointRounding.AwayFromZero),
                                                   TaxFee = o.a.TaxFee,
                                                   TaxTotal = o.a.TaxTotal
                                               });

                //支付信息
                var payInfo = new 
                {
                    PayChannel = ((PayType?)orderModel.PayType).ToDescription(),
                    PayType = PayType.Pay.ToDescription(),
                    RealPay = orderModel.RealPay,
                    FreePay = orderModel.FreePay
                };

                var memberFoodInfo = new
                {
                    OrderID = orderId,
                    SellTime = orderModel.CreateTime,
                    Points = (from a in flw_Order_DetailService.GetModels(p => p.OrderFlwID.Equals(orderId, StringComparison.OrdinalIgnoreCase))
                              join b in flw_Food_SaleService.GetModels() on a.FoodFlwID equals b.ID
                              select b.Point).Sum(),
                    BuyGoodInfos = buyGoodInfos,
                    PayInfos = new List<Object> { payInfo }
                };

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, memberFoodInfo);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        #endregion
    }
}