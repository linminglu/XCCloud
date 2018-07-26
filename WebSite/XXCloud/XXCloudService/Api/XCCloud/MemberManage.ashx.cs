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
                            ID = ds.Tables[0].Rows[i]["ID"].Tostring(),
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

        /// <summary>
        /// 会员换卡补卡信息
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryMemberChangeInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

                string errMsg = string.Empty;
                if(!dicParas.Get("iCCardId").Nonempty("会员卡号", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var iCCardId = dicParas.Get("iCCardId");
                object[] conditions = dicParas.ContainsKey("conditions") ? (object[])dicParas["conditions"] : null;
                
                SqlParameter[] parameters = new SqlParameter[0];
                string sqlWhere = string.Empty;

                if (conditions != null && conditions.Length > 0)
                    if (!QueryBLL.GenDynamicSql(conditions, "a.", ref sqlWhere, ref parameters, out errMsg))
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                
                string sql = @"SELECT a.* from (
                                SELECT distinct
                                    a.ID, cd1.ICCardID AS NewICCardID, b.UserName, cd2.ICCardID AS OldICCardID, (case a.OperateType when 0 then '换卡' when 1 then '补卡' else '' end) AS OperateType,
                                    a.CreateTime, a.OpFee, c.StoreName, a.CheckDate, d.ScheduleName, a.WorkStation, u.LogName, a.Note                                    
                                FROM
                                	Flw_MemberCard_Change a
                                INNER JOIN Data_Member_Card cd1 ON a.NewCardID=cd1.ID
                                INNER JOIN Data_Member_Card cd2 ON a.OldCardID=cd2.ID
                                LEFT JOIN Base_MemberInfo b ON a.MemberID=b.ID
                                LEFT JOIN Base_StoreInfo c ON a.StoreID=c.ID
                                LEFT JOIN Flw_Schedule d ON a.ScheduldID=d.ID
                                LEFT JOIN Base_UserInfo u ON a.UserID=u.ID
                                WHERE a.MerchID='" + merchId + "' AND (cd1.StoreID='" + storeId + "' OR cd2.StoreID='" + storeId + @"')) a WHERE 1=1
                            ";
                sql = sql + sqlWhere;
                if(!iCCardId.IsNull())
                    sql = sql + " AND (a.NewICCardID='" + iCCardId + "' OR a.OldICCardID='" + iCCardId + "')";           
                sql = sql + " ORDER BY a.ID";

                var list = Data_GameInfoService.I.SqlQuery<Flw_MemberCard_ChangeList>(sql, parameters).ToList();

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, list);               
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 会员续卡信息
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryMemberRenewInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

                string errMsg = string.Empty;
                if (!dicParas.Get("iCCardId").Nonempty("会员卡号", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var iCCardId = dicParas.Get("iCCardId");
                object[] conditions = dicParas.ContainsKey("conditions") ? (object[])dicParas["conditions"] : null;

                SqlParameter[] parameters = new SqlParameter[0];
                string sqlWhere = string.Empty;

                if (conditions != null && conditions.Length > 0)
                    if (!QueryBLL.GenDynamicSql(conditions, "a.", ref sqlWhere, ref parameters, out errMsg))
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                string sql = @"SELECT a.* from (
                                SELECT distinct
                                    a.ID, cd.ICCardID, b.UserName, a.CreateTime, a.RenewFee, e.TypeName AS BalanceIndexStr, e.PayCount,
                                    a.FoodSaleID, a.OldEndDate, a.NewEndDate, c.StoreName, a.CheckDate, d.ScheduleName, a.WorkStation, u.LogName, a.Note                                    
                                FROM
                                	Flw_MemberCard_Renew a
                                INNER JOIN Data_Member_Card cd ON a.CardID=cd.ID    
                                LEFT JOIN Base_MemberInfo b ON a.MemberID=b.ID
                                LEFT JOIN Base_StoreInfo c ON a.StoreID=c.ID
                                LEFT JOIN Flw_Schedule d ON a.ScheduldID=d.ID
                                LEFT JOIN Base_UserInfo u ON a.UserID=u.ID
                                LEFT JOIN (
                                    SELECT a.ID, b.PayCount, c.TypeName from Flw_Food_Sale a                                                               
                                    INNER JOIN Flw_Food_Sale_Pay b ON a.ID=b.FlwFoodID
                                    LEFT JOIN Dict_BalanceType c ON b.BalanceIndex=c.ID
                                ) e ON a.FoodSaleID=e.ID
                                WHERE a.MerchID='" + merchId + "' AND cd.StoreID='" + storeId + @"') a WHERE 1=1
                            ";
                sql = sql + sqlWhere;
                if (!iCCardId.IsNull())
                    sql = sql + " AND a.ICCardID='" + iCCardId + "'"; 
                sql = sql + " ORDER BY a.ID";

                var list = Data_GameInfoService.I.SqlQuery<Flw_MemberCard_RenewList>(sql, parameters).ToList();

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, list);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 会员信息变更记录
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryMemberInfoChange(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

                string errMsg = string.Empty;
                if (!dicParas.Get("iCCardId").Nonempty("会员卡号", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var iCCardId = dicParas.Get("iCCardId");
                object[] conditions = dicParas.ContainsKey("conditions") ? (object[])dicParas["conditions"] : null;

                SqlParameter[] parameters = new SqlParameter[0];
                string sqlWhere = string.Empty;

                if (conditions != null && conditions.Length > 0)
                    if (!QueryBLL.GenDynamicSql(conditions, "a.", ref sqlWhere, ref parameters, out errMsg))
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                string sql = @"SELECT a.* from (
                                SELECT distinct
                                    a.ID, cd.ICCardID, b.UserName, a.ModifyTime, a.ModifyFied, a.OldContext, a.NewContext,
                                    c.StoreName, a.CheckDate, d.ScheduleName, a.WorkStation, u.LogName, a.Note                                    
                                FROM
                                	Flw_MemberInfo_Change a
                                INNER JOIN Data_Member_Card cd ON a.CardID=cd.ID    
                                LEFT JOIN Base_MemberInfo b ON a.MemberID=b.ID
                                LEFT JOIN Base_StoreInfo c ON a.StoreID=c.ID
                                LEFT JOIN Flw_Schedule d ON a.ScheduldID=d.ID
                                LEFT JOIN Base_UserInfo u ON a.UserID=u.ID
                                WHERE a.MerchID='" + merchId + "' AND cd.StoreID='" + storeId + @"') a WHERE 1=1
                            ";
                sql = sql + sqlWhere;
                if (!iCCardId.IsNull())
                    sql = sql + " AND a.ICCardID='" + iCCardId + "'"; 
                sql = sql + " ORDER BY a.ID";

                var list = Data_GameInfoService.I.SqlQuery<Flw_MemberInfo_ChangeList>(sql, parameters).ToList();

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, list);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 会员级别变更记录
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryMemberLevelChange(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

                string errMsg = string.Empty;
                if (!dicParas.Get("iCCardId").Nonempty("会员卡号", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var iCCardId = dicParas.Get("iCCardId");
                object[] conditions = dicParas.ContainsKey("conditions") ? (object[])dicParas["conditions"] : null;

                SqlParameter[] parameters = new SqlParameter[0];
                string sqlWhere = string.Empty;

                if (conditions != null && conditions.Length > 0)
                    if (!QueryBLL.GenDynamicSql(conditions, "a.", ref sqlWhere, ref parameters, out errMsg))
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                string sql = @"SELECT a.* from (
                                SELECT distinct
                                    a.ID, cd.ICCardID, b.UserName, a.OpTime, a.ChangeType, e.MemberLevelName AS OldMemberLevelName, f.MemberLevelName AS NewMemberLevleName,                                    
                                    a.OrderID, c.StoreName, a.CheckDate, d.ScheduleName, a.WorkStation, u.LogName AS OpUserName, a.Note                                    
                                FROM
                                	Flw_MemberCard_LevelChange a
                                INNER JOIN Data_Member_Card cd ON a.CardID=cd.ID   
                                LEFT JOIN Base_MemberInfo b ON a.MemberID=b.ID
                                LEFT JOIN Base_StoreInfo c ON a.StoreID=c.ID
                                LEFT JOIN Flw_Schedule d ON a.ScheduleID=d.ID
                                LEFT JOIN Base_UserInfo u ON a.OpUserID=u.ID
                                LEFT JOIN Data_MemberLevel e ON a.OldMemberLevelID=e.ID
                                LEFT JOIN Data_MemberLevel f ON a.NewMemberLevleID=f.ID
                                WHERE a.MerchID='" + merchId + "' AND cd.StoreID='" + storeId + @"') a WHERE 1=1
                            ";
                sql = sql + sqlWhere;
                if (!iCCardId.IsNull())
                    sql = sql + " AND a.ICCardID='" + iCCardId + "'"; 
                sql = sql + " ORDER BY a.ID";

                var list = Data_GameInfoService.I.SqlQuery<Flw_MemberCard_LevelChangeList>(sql, parameters).ToList();

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, list);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 会员注销记录
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryMemberExitInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

                string errMsg = string.Empty;
                if (!dicParas.Get("iCCardId").Nonempty("会员卡号", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var iCCardId = dicParas.Get("iCCardId");
                object[] conditions = dicParas.ContainsKey("conditions") ? (object[])dicParas["conditions"] : null;

                SqlParameter[] parameters = new SqlParameter[0];
                string sqlWhere = string.Empty;

                if (conditions != null && conditions.Length > 0)
                    if (!QueryBLL.GenDynamicSql(conditions, "a.", ref sqlWhere, ref parameters, out errMsg))
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                string sql = @"SELECT a.* from (
                                SELECT distinct
                                    a.ID, cd.ICCardID, b.UserName, a.OPTime, a.Deposit, a.ExitMoney,                                     
                                    c.StoreName, a.CheckDate, d.ScheduleName, a.WorkStation, u.LogName, a.Note                                    
                                FROM
                                	Flw_MemberCard_Exit a
                                INNER JOIN Data_Member_Card cd ON a.CardID=cd.ID
                                LEFT JOIN Base_MemberInfo b ON a.MemberID=b.ID
                                LEFT JOIN Base_StoreInfo c ON a.StoreID=c.ID
                                LEFT JOIN Flw_Schedule d ON a.ScheduldID=d.ID
                                LEFT JOIN Base_UserInfo u ON a.UserID=u.ID                                
                                WHERE a.OperateType=0 AND a.MerchID='" + merchId + "' AND cd.StoreID='" + storeId + @"') a WHERE 1=1
                            ";
                sql = sql + sqlWhere;
                if (!iCCardId.IsNull())
                    sql = sql + " AND a.ICCardID='" + iCCardId + "'"; 
                sql = sql + " ORDER BY a.ID";

                var list = Data_GameInfoService.I.SqlQuery<Flw_MemberCard_ExitList>(sql, parameters).ToList();

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, list);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 会员余额变更记录
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryMemberData(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

                string errMsg = string.Empty;
                if (!dicParas.Get("iCCardId").Nonempty("会员卡号", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var iCCardId = dicParas.Get("iCCardId");
                object[] conditions = dicParas.ContainsKey("conditions") ? (object[])dicParas["conditions"] : null;

                SqlParameter[] parameters = new SqlParameter[0];
                string sqlWhere = string.Empty;

                if (conditions != null && conditions.Length > 0)
                    if (!QueryBLL.GenDynamicSql(conditions, "a.", ref sqlWhere, ref parameters, out errMsg))
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                string sql = @"SELECT a.* from (
                                SELECT distinct
                                    a.ID, cd.ICCardID, b.MemberLevelName, a.OperationType, a.OPTime, a.SourceID, 
                                    (a.Balance+a.FreeBalance-a.ChangeValue-a.FreeChangeValue) AS OldBalance, (a.ChangeValue+a.FreeChangeValue) AS ChangeValue,
                                    e.TypeName AS BalanceIndexStr, (a.Balance+a.FreeBalance) AS Balance, c.StoreName, a.CheckDate, d.ScheduleName, a.WorkStation, u.LogName, a.Note                                    
                                FROM
                                	Flw_MemberData a
                                INNER JOIN Data_Member_Card cd ON a.CardIndex=cd.ID     
                                LEFT JOIN Data_MemberLevel b ON cd.MemberLevelID=b.ID
                                LEFT JOIN Base_StoreInfo c ON a.StoreID=c.ID
                                LEFT JOIN Flw_Schedule d ON a.ScheduleID=d.ID
                                LEFT JOIN Base_UserInfo u ON a.UserID=u.ID
                                LEFT JOIN Dict_BalanceType e ON a.BalanceIndex=e.ID
                                WHERE a.MerchID='" + merchId + "' AND cd.StoreID='" + storeId + @"') a WHERE 1=1
                            ";
                sql = sql + sqlWhere;
                if (!iCCardId.IsNull())
                    sql = sql + " AND a.ICCardID='" + iCCardId + "'"; 
                sql = sql + " ORDER BY a.ID";

                var list = Data_GameInfoService.I.SqlQuery<Flw_MemberDataList>(sql, parameters).ToList();

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, list);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 会员兑换记录
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryMemberExchange(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

                string errMsg = string.Empty;
                if (!dicParas.Get("iCCardId").Nonempty("会员卡号", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var iCCardId = dicParas.Get("iCCardId");
                object[] conditions = dicParas.ContainsKey("conditions") ? (object[])dicParas["conditions"] : null;

                SqlParameter[] parameters = new SqlParameter[0];
                string sqlWhere = string.Empty;

                if (conditions != null && conditions.Length > 0)
                    if (!QueryBLL.GenDynamicSql(conditions, "a.", ref sqlWhere, ref parameters, out errMsg))
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                string sql = @"SELECT a.* from (
                                SELECT distinct
                                    a.ID AS OrderID, cd.ICCardID, b.UserName, f.MemberLevelName, a.CreateTime, fsp.Discount, fsp.OrginalPrice, fsp.PayCount, 
                                    e.TypeName AS BalanceIndexStr, fs.TotalMoney, a.OrderStatus, a.OrderSource, 
                                    a.CheckDate, d.ScheduleName, a.WorkStation, u.LogName, a.Note                                    
                                FROM
                                	Flw_Order a
                                INNER JOIN Data_Member_Card cd ON a.CardID=cd.ID  
                                INNER JOIN Flw_Order_Detail od ON a.ID=od.OrderFlwID 
                                INNER JOIN Flw_Food_Sale fs ON od.FoodFlwID=fs.ID 
                                INNER JOIN Flw_Food_Sale_Pay fsp ON fs.ID=fsp.ID 
                                LEFT JOIN Data_MemberLevel f ON cd.MemberLevelID=f.ID
                                LEFT JOIN Base_MemberInfo b ON a.MemberID=b.ID
                                --LEFT JOIN Base_StoreInfo c ON a.StoreID=c.ID
                                LEFT JOIN Flw_Schedule d ON a.ScheduleID=d.ID
                                LEFT JOIN Base_UserInfo u ON a.UserID=u.ID
                                LEFT JOIN Dict_BalanceType e ON fsp.BalanceIndex=e.ID
                                WHERE a.MerchID='" + merchId + "' AND cd.StoreID='" + storeId + @"') a WHERE 1=1
                            ";
                sql = sql + sqlWhere;
                if (!iCCardId.IsNull())
                    sql = sql + " AND a.ICCardID='" + iCCardId + "'"; 
                sql = sql + " ORDER BY a.OrderID";

                var list = Data_GameInfoService.I.SqlQuery<Flw_MemberExchangeList>(sql, parameters).ToList();

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, list);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 会员积分业务情况
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryMemberPointInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

                string errMsg = string.Empty;
                if (!dicParas.Get("iCCardId").Nonempty("会员卡号", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var iCCardId = dicParas.Get("iCCardId");
                object[] conditions = dicParas.ContainsKey("conditions") ? (object[])dicParas["conditions"] : null;

                SqlParameter[] parameters = new SqlParameter[0];
                string sqlWhere = string.Empty;

                if (conditions != null && conditions.Length > 0)
                    if (!QueryBLL.GenDynamicSql(conditions, "a.", ref sqlWhere, ref parameters, out errMsg))
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                string sql = @"SELECT a.* from (                                
                                SELECT distinct
                                    a.ID AS OrderID, cd.ICCardID, b.UserName, a.CreateTime, 
                                    fs.MemberLevelName, fs.PointTypeStr, fs.Direction, fs.OpertationTypeStr, fs.OldBalance, fs.ChangeValue, fs.Balance,
                                    c.StoreName, a.CheckDate, d.ScheduleName, a.WorkStation, u.LogName, a.Note                                    
                                FROM
                                	Flw_Order a
                                INNER JOIN Data_Member_Card cd ON a.CardID=cd.ID  
                                INNER JOIN Flw_Order_Detail od ON a.ID=od.OrderFlwID 
                                INNER JOIN 
                                (
                                 --1 销售积分
                                 SELECT fs.ID, (fs.PointBalance-fs.Point) AS OldBalance, fs.Point AS ChangeValue, fs.PointBalance AS Balance, f.MemberLevelName, f.PointTypeStr, '存入' AS Direction, '销售积分' AS OpertationTypeStr
                                 FROM Flw_Food_Sale fs
                                 INNER JOIN (select f.ID,f.MemberLevelName,e.TypeName AS PointTypeStr from Data_MemberLevel f INNER JOIN Dict_BalanceType e ON f.PointBalanceIndex=e.ID) f ON fs.MemberLevelID=f.ID

                                 UNION

                                 --2 套餐赠送
                                 SELECT fs.ID, (fs.PointBalance-fsd.ContainCount) AS OldBalance, fsd.ContainCount AS ChangeValue, fs.PointBalance AS Balance, f.MemberLevelName, bt.TypeName AS PointTypeStr, '存入' AS Direction, '套餐赠送' AS OpertationTypeStr
                                 FROM Flw_Food_Sale fs
                                 INNER JOIN Data_MemberLevel f ON fs.MemberLevelID=f.ID
                                 INNER JOIN Flw_Food_SaleDetail fsd ON fs.ID=fsd.FlwFoodID
                                 INNER JOIN Dict_BalanceType bt ON fsd.ContainID=bt.ID 
                                 WHERE fsd.FoodType=0 AND bt.MappingType=3

                                 UNION
            
                                 --7 兑换
                                 SELECT fs.ID, (fsp.Balance+fsp.PayCount) AS OldBalance, -fsp.PayCount AS ChangeValue, fsp.Balance AS Balance, f.MemberLevelName, bt.TypeName AS PointTypeStr, '消耗' AS Direction, '兑换' AS OpertationTypeStr
                                 FROM Flw_Food_Sale fs
                                 INNER JOIN Data_MemberLevel f ON fs.MemberLevelID=f.ID
                                 INNER JOIN Flw_Food_Sale_Pay fsp ON fs.ID=fsp.FlwFoodID
                                 INNER JOIN Dict_BalanceType bt ON fsp.BalanceIndex=bt.ID 
                                 WHERE bt.MappingType=2
                                ) fs ON od.FoodFlwID=fs.ID                                                                 
                                LEFT JOIN Base_MemberInfo b ON a.MemberID=b.ID
                                LEFT JOIN Base_StoreInfo c ON a.StoreID=c.ID
                                LEFT JOIN Flw_Schedule d ON a.ScheduleID=d.ID
                                LEFT JOIN Base_UserInfo u ON a.UserID=u.ID
                                WHERE a.MerchID='" + merchId + "' AND cd.StoreID='" + storeId + @"'

                                UNION 

                                --4 消费
                                SELECT distinct
                                    a.OrderID, cd.ICCardID, b.UserName, a.RealTime AS CreateTime, 
                                    f.MemberLevelName, bt.TypeName AS PointTypeStr, '消耗' AS Direction, '消费' AS OpertationTypeStr, (a.RemainBalance+a.Coin) AS OldBalance, -a.Coin AS ChangeValue, a.RemainBalance AS Balance,
                                    c.StoreName, a.CheckDate, '' AS ScheduleName, d.DeviceName AS WorkStation, '' AS LogName, a.Note                                    
                                FROM
                                	Flw_DeviceData a
                                INNER JOIN Data_Member_Card cd ON a.CardID=cd.ID 
                                INNER JOIN Base_DeviceInfo d ON a.DeviceID=d.ID
                                INNER JOIN Dict_BalanceType bt ON a.BalanceIndex=bt.ID 
                                LEFT JOIN Data_MemberLevel f ON cd.MemberLevelID=f.ID
                                LEFT JOIN Base_MemberInfo b ON a.MemberID=b.ID
                                LEFT JOIN Base_StoreInfo c ON a.StoreID=c.ID
                                WHERE bt.MappingType=3 AND a.MerchID='" + merchId + "' AND cd.StoreID='" + storeId + @"' 

                                UNION
                                
                                --6 过户
                                SELECT distinct
                                    '' AS OrderID, cd.ICCardID, b.UserName, a.RealTime AS CreateTime, 
                                    f.MemberLevelName, bt.TypeName AS PointTypeStr, '消耗' AS Direction, '过户' AS OpertationTypeStr, (a.BalanceOut+a.TransferCount) AS OldBalance, -a.TransferCount AS ChangeValue, a.BalanceOut AS Balance,
                                    c.StoreName, a.CheckDate, d.ScheduleName, a.WorkStation, u.LogName, a.Note                                    
                                FROM
                                	Flw_Transfer a
                                INNER JOIN Data_Member_Card cd ON a.CardIDOut=cd.ID  
                                INNER JOIN Dict_BalanceType bt ON a.TransferBalanceIndex=bt.ID  
                                LEFT JOIN Data_MemberLevel f ON cd.MemberLevelID=f.ID                                                              
                                LEFT JOIN Base_MemberInfo b ON a.OutMemberID=b.ID
                                LEFT JOIN Base_StoreInfo c ON a.StoreID=c.ID
                                LEFT JOIN Flw_Schedule d ON a.ScheduleID=d.ID
                                LEFT JOIN Base_UserInfo u ON a.UserID=u.ID
                                WHERE bt.MappingType=3 AND a.MerchID='" + merchId + "' AND cd.StoreID='" + storeId + @"'

                                UNION

                                SELECT distinct
                                    '' AS OrderID, cd.ICCardID, b.UserName, a.RealTime AS CreateTime, 
                                    f.MemberLevelName, bt.TypeName AS PointTypeStr, '存入' AS Direction, '过户' AS OpertationTypeStr, (a.BalanceIn-a.TransferCount) AS OldBalance, a.TransferCount AS ChangeValue, a.BalanceIn AS Balance,
                                    c.StoreName, a.CheckDate, d.ScheduleName, a.WorkStation, u.LogName, a.Note                                    
                                FROM
                                	Flw_Transfer a
                                INNER JOIN Data_Member_Card cd ON a.CardIDIn=cd.ID  
                                INNER JOIN Dict_BalanceType bt ON a.TransferBalanceIndex=bt.ID  
                                LEFT JOIN Data_MemberLevel f ON cd.MemberLevelID=f.ID                                                              
                                LEFT JOIN Base_MemberInfo b ON a.InMemberID=b.ID
                                LEFT JOIN Base_StoreInfo c ON a.StoreID=c.ID
                                LEFT JOIN Flw_Schedule d ON a.ScheduleID=d.ID
                                LEFT JOIN Base_UserInfo u ON a.UserID=u.ID
                                WHERE bt.MappingType=3 AND a.MerchID='" + merchId + "' AND cd.StoreID='" + storeId + @"'
                                
                                UNION

                                --7 兑换
                                SELECT distinct
                                    '' AS OrderID, cd.ICCardID, b.UserName, a.OpTime AS CreateTime, 
                                    f.MemberLevelName, bt.TypeName AS PointTypeStr, '存入' AS Direction, '兑换' AS OpertationTypeStr, (a.TargetRemain-a.TargetCount) AS OldBalance, a.TargetCount AS ChangeValue, a.TargetRemain AS Balance,
                                    c.StoreName, a.CheckDate, d.ScheduleName, a.WorkStation, u.LogName, a.Note                                    
                                FROM
                                	Flw_MemberCard_BalanceCharge a
                                INNER JOIN Data_Member_Card cd ON a.CardIndex=cd.ID  
                                INNER JOIN Dict_BalanceType bt ON a.TargetBalanceIndex=bt.ID  
                                LEFT JOIN Data_MemberLevel f ON cd.MemberLevelID=f.ID                                                              
                                LEFT JOIN Base_MemberInfo b ON a.MemberID=b.ID
                                LEFT JOIN Base_StoreInfo c ON a.StoreID=c.ID
                                LEFT JOIN Flw_Schedule d ON a.ScheduleID=d.ID
                                LEFT JOIN Base_UserInfo u ON a.OpUserID=u.ID
                                WHERE bt.MappingType=3 AND a.MerchID='" + merchId + "' AND cd.StoreID='" + storeId + @"'

                                UNION

                                SELECT distinct
                                    '' AS OrderID, cd.ICCardID, b.UserName, a.OpTime AS CreateTime, 
                                    f.MemberLevelName, bt.TypeName AS PointTypeStr, '消耗' AS Direction, '兑换' AS OpertationTypeStr, (a.SourceRemain+a.SourceCount) AS OldBalance, -a.SourceCount AS ChangeValue, a.SourceRemain AS Balance,
                                    c.StoreName, a.CheckDate, d.ScheduleName, a.WorkStation, u.LogName, a.Note                                    
                                FROM
                                	Flw_MemberCard_BalanceCharge a
                                INNER JOIN Data_Member_Card cd ON a.CardIndex=cd.ID  
                                INNER JOIN Dict_BalanceType bt ON a.SourceBalanceIndex=bt.ID  
                                LEFT JOIN Data_MemberLevel f ON cd.MemberLevelID=f.ID                                                              
                                LEFT JOIN Base_MemberInfo b ON a.MemberID=b.ID
                                LEFT JOIN Base_StoreInfo c ON a.StoreID=c.ID
                                LEFT JOIN Flw_Schedule d ON a.ScheduleID=d.ID
                                LEFT JOIN Base_UserInfo u ON a.OpUserID=u.ID
                                WHERE bt.MappingType=3 AND a.MerchID='" + merchId + "' AND cd.StoreID='" + storeId + @"'

                                UNION

                                --9 礼品回购
                                SELECT distinct
                                    '' AS OrderID, cd.ICCardID, b.UserName, a.OpTime AS CreateTime, 
                                    f.MemberLevelName, bt.TypeName AS PointTypeStr, '存入' AS Direction, '礼品回购' AS OpertationTypeStr, (a.Balance-a.BalanceCount) AS OldBalance, a.BalanceCount AS ChangeValue, a.Balance AS Balance,
                                    c.StoreName, a.CheckDate, d.ScheduleName, a.WorkStation, u.LogName, a.Note                                    
                                FROM
                                	Flw_Good_Buyback a
                                INNER JOIN Data_Member_Card cd ON a.CardID=cd.ID  
                                INNER JOIN Dict_BalanceType bt ON a.BuybackBalanceIndex=bt.ID  
                                LEFT JOIN Data_MemberLevel f ON cd.MemberLevelID=f.ID                                                              
                                LEFT JOIN Base_MemberInfo b ON a.MemberID=b.ID
                                LEFT JOIN Base_StoreInfo c ON a.StoreID=c.ID
                                LEFT JOIN Flw_Schedule d ON a.ScheduleID=d.ID
                                LEFT JOIN Base_UserInfo u ON a.OpUserID=u.ID
                                WHERE bt.MappingType=3 AND a.MerchID='" + merchId + "' AND cd.StoreID='" + storeId + @"'
                            ) a WHERE 1=1
                            ";
                sql = sql + sqlWhere;
                if (!iCCardId.IsNull())
                    sql = sql + " AND a.ICCardID='" + iCCardId + "'";
                sql = sql + " ORDER BY a.OrderID, a.ICCardID";

                var list = Data_GameInfoService.I.SqlQuery<Flw_MemberPointList>(sql, parameters).ToList();

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, list);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 会员彩票业务情况
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryMemberLotteryInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

                string errMsg = string.Empty;
                if (!dicParas.Get("iCCardId").Nonempty("会员卡号", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var iCCardId = dicParas.Get("iCCardId");
                object[] conditions = dicParas.ContainsKey("conditions") ? (object[])dicParas["conditions"] : null;

                SqlParameter[] parameters = new SqlParameter[0];
                string sqlWhere = string.Empty;

                if (conditions != null && conditions.Length > 0)
                    if (!QueryBLL.GenDynamicSql(conditions, "a.", ref sqlWhere, ref parameters, out errMsg))
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                string sql = @"SELECT a.* from (                                
                                SELECT distinct
                                    a.ID AS OrderID, cd.ICCardID, b.UserName, a.CreateTime, 
                                    fs.MemberLevelName, fs.LotteryTypeStr, fs.Direction, fs.OpertationTypeStr, fs.OldBalance, fs.ChangeValue, fs.Balance,
                                    c.StoreName, a.CheckDate, d.ScheduleName, a.WorkStation, u.LogName, a.Note                                    
                                FROM
                                	Flw_Order a
                                INNER JOIN Data_Member_Card cd ON a.CardID=cd.ID  
                                INNER JOIN Flw_Order_Detail od ON a.ID=od.OrderFlwID 
                                INNER JOIN 
                                (    
                             
                                 --7 销售兑换
                                 SELECT fs.ID, (fsp.Balance+fsp.PayCount) AS OldBalance, -fsp.PayCount AS ChangeValue, fsp.Balance AS Balance, f.MemberLevelName, bt.TypeName AS LotteryTypeStr, '消耗' AS Direction, '销售兑换' AS OpertationTypeStr
                                 FROM Flw_Food_Sale fs
                                 INNER JOIN Data_MemberLevel f ON fs.MemberLevelID=f.ID
                                 INNER JOIN Flw_Food_Sale_Pay fsp ON fs.ID=fsp.FlwFoodID
                                 INNER JOIN Dict_BalanceType bt ON fsp.BalanceIndex=bt.ID 
                                 WHERE bt.MappingType=2
                                ) fs ON od.FoodFlwID=fs.ID                                                                 
                                LEFT JOIN Base_MemberInfo b ON a.MemberID=b.ID
                                LEFT JOIN Base_StoreInfo c ON a.StoreID=c.ID
                                LEFT JOIN Flw_Schedule d ON a.ScheduleID=d.ID
                                LEFT JOIN Base_UserInfo u ON a.UserID=u.ID
                                WHERE a.MerchID='" + merchId + "' AND cd.StoreID='" + storeId + @"'

                                UNION 

                                --1 设备出票
                                SELECT distinct
                                    a.OrderID, cd.ICCardID, b.UserName, a.RealTime AS CreateTime, 
                                    f.MemberLevelName, bt.TypeName AS LotteryTypeStr, '存入' AS Direction, '设备出票' AS OpertationTypeStr, (a.RemainBalance-a.Coin) AS OldBalance, a.Coin AS ChangeValue, a.RemainBalance AS Balance,
                                    c.StoreName, a.CheckDate, '' AS ScheduleName, d.DeviceName AS WorkStation, '' AS LogName, a.Note                                    
                                FROM
                                	Flw_DeviceData a
                                INNER JOIN Data_Member_Card cd ON a.CardID=cd.ID 
                                INNER JOIN Base_DeviceInfo d ON a.DeviceID=d.ID
                                INNER JOIN Dict_BalanceType bt ON a.BalanceIndex=bt.ID 
                                LEFT JOIN Data_MemberLevel f ON cd.MemberLevelID=f.ID
                                LEFT JOIN Base_MemberInfo b ON a.MemberID=b.ID
                                LEFT JOIN Base_StoreInfo c ON a.StoreID=c.ID
                                WHERE bt.MappingType=2 AND a.MerchID='" + merchId + "' AND cd.StoreID='" + storeId + @"'                                 

                                UNION
                                
                                --6 过户
                                SELECT distinct
                                    '' AS OrderID, cd.ICCardID, b.UserName, a.RealTime AS CreateTime, 
                                    f.MemberLevelName, bt.TypeName AS LotteryTypeStr, '消耗' AS Direction, '过户' AS OpertationTypeStr, (a.BalanceOut+a.TransferCount) AS OldBalance, -a.TransferCount AS ChangeValue, a.BalanceOut AS Balance,
                                    c.StoreName, a.CheckDate, d.ScheduleName, a.WorkStation, u.LogName, a.Note                                    
                                FROM
                                	Flw_Transfer a
                                INNER JOIN Data_Member_Card cd ON a.CardIDOut=cd.ID  
                                INNER JOIN Dict_BalanceType bt ON a.TransferBalanceIndex=bt.ID  
                                LEFT JOIN Data_MemberLevel f ON cd.MemberLevelID=f.ID                                                              
                                LEFT JOIN Base_MemberInfo b ON a.OutMemberID=b.ID
                                LEFT JOIN Base_StoreInfo c ON a.StoreID=c.ID
                                LEFT JOIN Flw_Schedule d ON a.ScheduleID=d.ID
                                LEFT JOIN Base_UserInfo u ON a.UserID=u.ID
                                WHERE bt.MappingType=2 AND a.MerchID='" + merchId + "' AND cd.StoreID='" + storeId + @"'

                                UNION

                                SELECT distinct
                                    '' AS OrderID, cd.ICCardID, b.UserName, a.RealTime AS CreateTime, 
                                    f.MemberLevelName, bt.TypeName AS LotteryTypeStr, '存入' AS Direction, '过户' AS OpertationTypeStr, (a.BalanceIn-a.TransferCount) AS OldBalance, a.TransferCount AS ChangeValue, a.BalanceIn AS Balance,
                                    c.StoreName, a.CheckDate, d.ScheduleName, a.WorkStation, u.LogName, a.Note                                    
                                FROM
                                	Flw_Transfer a
                                INNER JOIN Data_Member_Card cd ON a.CardIDIn=cd.ID  
                                INNER JOIN Dict_BalanceType bt ON a.TransferBalanceIndex=bt.ID  
                                LEFT JOIN Data_MemberLevel f ON cd.MemberLevelID=f.ID                                                              
                                LEFT JOIN Base_MemberInfo b ON a.InMemberID=b.ID
                                LEFT JOIN Base_StoreInfo c ON a.StoreID=c.ID
                                LEFT JOIN Flw_Schedule d ON a.ScheduleID=d.ID
                                LEFT JOIN Base_UserInfo u ON a.UserID=u.ID
                                WHERE bt.MappingType=2 AND a.MerchID='" + merchId + "' AND cd.StoreID='" + storeId + @"'
                                
                                UNION

                                --7 余额兑换
                                SELECT distinct
                                    '' AS OrderID, cd.ICCardID, b.UserName, a.OpTime AS CreateTime, 
                                    f.MemberLevelName, bt.TypeName AS LotteryTypeStr, '存入' AS Direction, '余额兑换' AS OpertationTypeStr, (a.TargetRemain-a.TargetCount) AS OldBalance, a.TargetCount AS ChangeValue, a.TargetRemain AS Balance,
                                    c.StoreName, a.CheckDate, d.ScheduleName, a.WorkStation, u.LogName, a.Note
                                FROM
                                	Flw_MemberCard_BalanceCharge a
                                INNER JOIN Data_Member_Card cd ON a.CardIndex=cd.ID  
                                INNER JOIN Dict_BalanceType bt ON a.TargetBalanceIndex=bt.ID  
                                LEFT JOIN Data_MemberLevel f ON cd.MemberLevelID=f.ID                                                              
                                LEFT JOIN Base_MemberInfo b ON a.MemberID=b.ID
                                LEFT JOIN Base_StoreInfo c ON a.StoreID=c.ID
                                LEFT JOIN Flw_Schedule d ON a.ScheduleID=d.ID
                                LEFT JOIN Base_UserInfo u ON a.OpUserID=u.ID
                                WHERE bt.MappingType=2 AND a.MerchID='" + merchId + "' AND cd.StoreID='" + storeId + @"'

                                UNION

                                SELECT distinct
                                    '' AS OrderID, cd.ICCardID, b.UserName, a.OpTime AS CreateTime, 
                                    f.MemberLevelName, bt.TypeName AS LotteryTypeStr, '消耗' AS Direction, '余额兑换' AS OpertationTypeStr, (a.SourceRemain+a.SourceCount) AS OldBalance, -a.SourceCount AS ChangeValue, a.SourceRemain AS Balance,
                                    c.StoreName, a.CheckDate, d.ScheduleName, a.WorkStation, u.LogName, a.Note                                    
                                FROM
                                	Flw_MemberCard_BalanceCharge a
                                INNER JOIN Data_Member_Card cd ON a.CardIndex=cd.ID  
                                INNER JOIN Dict_BalanceType bt ON a.SourceBalanceIndex=bt.ID  
                                LEFT JOIN Data_MemberLevel f ON cd.MemberLevelID=f.ID                                                              
                                LEFT JOIN Base_MemberInfo b ON a.MemberID=b.ID
                                LEFT JOIN Base_StoreInfo c ON a.StoreID=c.ID
                                LEFT JOIN Flw_Schedule d ON a.ScheduleID=d.ID
                                LEFT JOIN Base_UserInfo u ON a.OpUserID=u.ID
                                WHERE bt.MappingType=2 AND a.MerchID='" + merchId + "' AND cd.StoreID='" + storeId + @"'

                                UNION

                                --9 礼品回购
                                SELECT distinct
                                    '' AS OrderID, cd.ICCardID, b.UserName, a.OpTime AS CreateTime, 
                                    f.MemberLevelName, bt.TypeName AS LotteryTypeStr, '存入' AS Direction, '礼品回购' AS OpertationTypeStr, (a.Balance-a.BalanceCount) AS OldBalance, a.BalanceCount AS ChangeValue, a.Balance AS Balance,
                                    c.StoreName, a.CheckDate, d.ScheduleName, a.WorkStation, u.LogName, a.Note                                    
                                FROM
                                	Flw_Good_Buyback a
                                INNER JOIN Data_Member_Card cd ON a.CardID=cd.ID  
                                INNER JOIN Dict_BalanceType bt ON a.BuybackBalanceIndex=bt.ID  
                                LEFT JOIN Data_MemberLevel f ON cd.MemberLevelID=f.ID                                                              
                                LEFT JOIN Base_MemberInfo b ON a.MemberID=b.ID
                                LEFT JOIN Base_StoreInfo c ON a.StoreID=c.ID
                                LEFT JOIN Flw_Schedule d ON a.ScheduleID=d.ID
                                LEFT JOIN Base_UserInfo u ON a.OpUserID=u.ID
                                WHERE bt.MappingType=2 AND a.MerchID='" + merchId + "' AND cd.StoreID='" + storeId + @"'
                            ) a WHERE 1=1
                            ";
                sql = sql + sqlWhere;
                if (!iCCardId.IsNull())
                    sql = sql + " AND a.ICCardID='" + iCCardId + "'";
                sql = sql + " ORDER BY a.OrderID, a.ICCardID";

                var list = Data_GameInfoService.I.SqlQuery<Flw_MemberLotteryList>(sql, parameters).ToList();

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, list);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 会员卡内余额互转记录
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryMemberCardBalanceChange(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

                string errMsg = string.Empty;
                if (!dicParas.Get("iCCardId").Nonempty("会员卡号", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var iCCardId = dicParas.Get("iCCardId");
                object[] conditions = dicParas.ContainsKey("conditions") ? (object[])dicParas["conditions"] : null;

                SqlParameter[] parameters = new SqlParameter[0];
                string sqlWhere = string.Empty;

                if (conditions != null && conditions.Length > 0)
                    if (!QueryBLL.GenDynamicSql(conditions, "a.", ref sqlWhere, ref parameters, out errMsg))
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                string sql = @"SELECT a.* from (
                                SELECT distinct
                                    a.ID, cd.ICCardID, b.UserName, f.MemberLevelName, a.OpTime, a.SourceCount, a.SourceRemain, a.TargetCount, a.TargetRemain,                                    
                                    e1.TypeName AS SourceBalanceStr, e2.TypeName AS TargetBalanceStr, c.StoreName, a.CheckDate, d.ScheduleName, a.WorkStation, u.LogName, a.Note                                    
                                FROM
                                	Flw_MemberCard_BalanceCharge a
                                INNER JOIN Data_Member_Card cd ON a.CardIndex=cd.ID   
                                LEFT JOIN Data_MemberLevel f ON cd.MemberLevelID=f.ID  
                                LEFT JOIN Base_MemberInfo b ON a.MemberID=b.ID
                                LEFT JOIN Base_StoreInfo c ON a.StoreID=c.ID
                                LEFT JOIN Flw_Schedule d ON a.ScheduldID=d.ID
                                LEFT JOIN Base_UserInfo u ON a.OpUserID=u.ID
                                LEFT JOIN Dict_BalanceType e1 ON a.SourceBalanceIndex=e1.ID   
                                LEFT JOIN Dict_BalanceType e2 ON a.TargetBalanceIndex=e2.ID                             
                                WHERE a.MerchID='" + merchId + "' AND cd.StoreID='" + storeId + @"') a WHERE 1=1
                            ";
                sql = sql + sqlWhere;
                if (!iCCardId.IsNull())
                    sql = sql + " AND a.ICCardID='" + iCCardId + "'"; 
                sql = sql + " ORDER BY a.ID";

                var list = Data_GameInfoService.I.SqlQuery<Flw_MemberCard_BalanceChargeList>(sql, parameters).ToList();

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, list);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 会员赠补币记录
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryMemberCardFree(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

                string errMsg = string.Empty;
                if (!dicParas.Get("iCCardId").Nonempty("会员卡号", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var iCCardId = dicParas.Get("iCCardId");
                object[] conditions = dicParas.ContainsKey("conditions") ? (object[])dicParas["conditions"] : null;

                SqlParameter[] parameters = new SqlParameter[0];
                string sqlWhere = string.Empty;

                if (conditions != null && conditions.Length > 0)
                    if (!QueryBLL.GenDynamicSql(conditions, "a.", ref sqlWhere, ref parameters, out errMsg))
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                string sql = @"SELECT a.* from (
                                SELECT distinct
                                    a.ID, cd.ICCardID, b.UserName, f.MemberLevelName, a.OpTime, a.FreeType, a.FreeCount,
                                    ('满' + convert(varchar,mlf.ChargeTotal) + '赠' + convert(varchar,mlf.FreeCount)) AS FreeName, '已完成' AS [StateStr],
                                    e.TypeName AS BalanceIndexStr, c.StoreName, a.CheckDate, d.ScheduleName, a.WorkStation, u.LogName, a.Note                                    
                                FROM
                                	Flw_MemberCard_Free a
                                INNER JOIN Data_Member_Card cd ON a.CardIndex=cd.ID 
                                INNER JOIN Flw_MemberLevelFree mlf ON a.CardIndex=mlf.CardIndex  
                                LEFT JOIN Data_MemberLevel f ON cd.MemberLevelID=f.ID  
                                LEFT JOIN Base_MemberInfo b ON a.MemberID=b.ID
                                LEFT JOIN Base_StoreInfo c ON a.StoreID=c.ID
                                LEFT JOIN Flw_Schedule d ON a.ScheduleID=d.ID
                                LEFT JOIN Base_UserInfo u ON a.UserID=u.ID
                                LEFT JOIN Dict_BalanceType e ON a.BalanceIndex=e.ID                                                                
                                WHERE a.MerchID='" + merchId + "' AND cd.StoreID='" + storeId + @"') a WHERE 1=1
                            ";
                sql = sql + sqlWhere;
                if (!iCCardId.IsNull())
                    sql = sql + " AND a.ICCardID='" + iCCardId + "'"; 
                sql = sql + " ORDER BY a.ID";

                var list = Data_GameInfoService.I.SqlQuery<Flw_MemberCard_FreeList>(sql, parameters).ToList();

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, list);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 会员退款记录
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryMemberCardExitMoney(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

                string errMsg = string.Empty;
                if (!dicParas.Get("iCCardId").Nonempty("会员卡号", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var iCCardId = dicParas.Get("iCCardId");
                object[] conditions = dicParas.ContainsKey("conditions") ? (object[])dicParas["conditions"] : null;

                SqlParameter[] parameters = new SqlParameter[0];
                string sqlWhere = string.Empty;

                if (conditions != null && conditions.Length > 0)
                    if (!QueryBLL.GenDynamicSql(conditions, "a.", ref sqlWhere, ref parameters, out errMsg))
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                string sql = @"SELECT a.* from (
                                SELECT distinct
                                    a.ID, cd.ICCardID, b.UserName, f.MemberLevelName, a.OPTime, bc.SourceCount, bc.TargetCount, '已退款' AS [StateStr],
                                    e.TypeName AS SourceBalanceStr, c.StoreName, a.CheckDate, d.ScheduleName, a.WorkStation, u.LogName, a.Note                                    
                                FROM
                                	Flw_MemberCard_Exit a
                                INNER JOIN Data_Member_Card cd ON a.CardIndex=cd.ID 
                                INNER JOIN Flw_MemberCard_BalanceCharge bc ON a.ID=bc.ExitID 
                                LEFT JOIN Data_MemberLevel f ON cd.MemberLevelID=f.ID  
                                LEFT JOIN Base_MemberInfo b ON a.MemberID=b.ID
                                LEFT JOIN Base_StoreInfo c ON a.StoreID=c.ID
                                LEFT JOIN Flw_Schedule d ON a.ScheduldID=d.ID
                                LEFT JOIN Base_UserInfo u ON a.UserID=u.ID
                                LEFT JOIN Dict_BalanceType e ON bc.SourceBalanceIndex=e.ID                                                                
                                WHERE a.OperateType=1 AND a.MerchID='" + merchId + "' AND cd.StoreID='" + storeId + @"') a WHERE 1=1
                            ";
                sql = sql + sqlWhere;
                if (!iCCardId.IsNull())
                    sql = sql + " AND a.ICCardID='" + iCCardId + "'"; 
                sql = sql + " ORDER BY a.ID";

                var list = Data_GameInfoService.I.SqlQuery<Flw_MemberCard_ExitMoneyList>(sql, parameters).ToList();

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, list);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 会员过户币记录
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryMemberTransfer(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

                string errMsg = string.Empty;
                if (!dicParas.Get("iCCardId").Nonempty("会员卡号", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var iCCardId = dicParas.Get("iCCardId");
                object[] conditions = dicParas.ContainsKey("conditions") ? (object[])dicParas["conditions"] : null;

                SqlParameter[] parameters = new SqlParameter[0];
                string sqlWhere = string.Empty;

                if (conditions != null && conditions.Length > 0)
                    if (!QueryBLL.GenDynamicSql(conditions, "a.", ref sqlWhere, ref parameters, out errMsg))
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                string sql = @"SELECT a.* from (
                                SELECT distinct
                                    a.ID, cd1.ICCardID AS ICCardIDOut, b1.UserName, a.RealTime, a.TransferCount, e.TypeName AS TransferBalanceStr, a.BalanceOut,
                                    cd2.ICCardID AS ICCardIDIn, b2.UserName InUserName, a.BalanceIn, c.StoreName, a.CheckDate, d.ScheduleName, a.WorkStation, u.LogName, a.Note                                    
                                FROM
                                	Flw_Transfer a
                                INNER JOIN Data_Member_Card cd1 ON a.CardIDOut=cd1.ID  
                                INNER JOIN Data_Member_Card cd2 ON a.CardIDIn=cd2.ID        
                                LEFT JOIN Base_MemberInfo b1 ON a.OutMemberID=b1.ID
                                LEFT JOIN Base_MemberInfo b2 ON a.InMemberID=b2.ID
                                LEFT JOIN Base_StoreInfo c ON a.StoreID=c.ID
                                LEFT JOIN Flw_Schedule d ON a.ScheduleID=d.ID
                                LEFT JOIN Base_UserInfo u ON a.UserID=u.ID
                                LEFT JOIN Dict_BalanceType e ON a.TransferBalanceIndex=e.ID
                                WHERE a.MerchID='" + merchId + "' AND (cd1.StoreID='" + storeId + "' OR cd2.StoreID='" + storeId + @"')) a WHERE 1=1
                            ";
                sql = sql + sqlWhere;
                if (!iCCardId.IsNull())
                    sql = sql + " AND (a.ICCardIDOut='" + iCCardId + "' OR a.ICCardIDIn='" + iCCardId + "')";
                sql = sql + " ORDER BY a.ID";

                var list = Data_GameInfoService.I.SqlQuery<Flw_MemberTransferList>(sql, parameters).ToList();

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, list);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 会员返还记录
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryMemberGiveback(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

                string errMsg = string.Empty;
                if (!dicParas.Get("iCCardId").Nonempty("会员卡号", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var iCCardId = dicParas.Get("iCCardId");
                object[] conditions = dicParas.ContainsKey("conditions") ? (object[])dicParas["conditions"] : null;

                SqlParameter[] parameters = new SqlParameter[0];
                string sqlWhere = string.Empty;

                if (conditions != null && conditions.Length > 0)
                    if (!QueryBLL.GenDynamicSql(conditions, "a.", ref sqlWhere, ref parameters, out errMsg))
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                string sql = @"SELECT a.* from (
                                SELECT distinct
                                    a.ID, cd.ICCardID, a.RealTime, a.MayCoins, a.Coins, a.WinMoney, a.LastTime, a.WorkStation, u.LogName, u2.LogName AuthorName, '' AS Note                                    
                                FROM
                                	Flw_Giveback a
                                INNER JOIN Data_Member_Card cd ON a.CardID=cd.ID  
                                --LEFT JOIN Base_MemberInfo b ON a.MemberID=b.ID
                                --LEFT JOIN Base_StoreInfo c ON a.StoreID=c.ID
                                --LEFT JOIN Flw_Schedule d ON a.ScheduldID=d.ID
                                LEFT JOIN Base_UserInfo u ON a.UserID=u.ID
                                LEFT JOIN Base_UserInfo u2 ON a.AuthorID=u2.ID
                                --LEFT JOIN Dict_BalanceType e ON a.BalanceIndex=e.ID
                                WHERE a.MerchID='" + merchId + "' AND cd.StoreID='" + storeId + @"') a WHERE 1=1
                            ";
                sql = sql + sqlWhere;
                if (!iCCardId.IsNull())
                    sql = sql + " AND a.ICCardID='" + iCCardId + "'"; 
                sql = sql + " ORDER BY a.ID";

                var list = Data_GameInfoService.I.SqlQuery<Flw_MemberGivebackList>(sql, parameters).ToList();

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, list);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        #endregion
    }
}