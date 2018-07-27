﻿using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;
using System.Web;
using XCCloudWebBar.Base;
using XCCloudWebBar.BLL.CommonBLL;
using XCCloudWebBar.BLL.Container;
using XCCloudWebBar.BLL.IBLL.XCCloud;
using XCCloudWebBar.BLL.XCCloud;
using XCCloudWebBar.Business.XCCloud;
using XCCloudWebBar.Business.XCGameMana;
using XCCloudWebBar.CacheService;
using XCCloudWebBar.CacheService.XCCloud;
using XCCloudWebBar.Common;
using XCCloudWebBar.Common.Enum;
using XCCloudWebBar.DAL;
using XCCloudWebBar.DBService.BLL;
using XCCloudWebBar.Model.CustomModel.XCCloud;
using XCCloudWebBar.Model.CustomModel.XCCloud.Order;
using XCCloudWebBar.Model.XCCloud;
using XCCloudWebBar.Common.Extensions;
using System.Text;

namespace XXCloudService.Api.XCCloud
{
    [Authorize(Roles = "StoreUser")]
    /// <summary>
    /// Order 的摘要说明
    /// </summary>
    public class Order : ApiBase
    {
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object getOrderSalePay(Dictionary<string, object> dicParas)
        {
            XCCloudUserTokenModel userTokenModel = (XCCloudUserTokenModel)(dicParas[Constant.XCCloudUserTokenModel]);
            TokenDataModel userTokenDataModel = (TokenDataModel)(userTokenModel.DataModel);

            string storedProcedure = "GetOrderSalePay";
            string flwOrderId = dicParas.ContainsKey("flwOrderId") ? dicParas["flwOrderId"].ToString() : string.Empty;

            SqlParameter[] sqlParameter = new SqlParameter[3];
            sqlParameter[0] = new SqlParameter("@MerchId", SqlDbType.VarChar);
            sqlParameter[0].Value = userTokenDataModel.MerchID;

            sqlParameter[1] = new SqlParameter("@StoreId", SqlDbType.VarChar);
            sqlParameter[1].Value = userTokenDataModel.StoreID;

            sqlParameter[2] = new SqlParameter("@OrderId", SqlDbType.VarChar,32);
            sqlParameter[2].Direction = ParameterDirection.ReturnValue;

            System.Data.DataSet ds = XCCloudBLL.GetStoredProcedureSentence(storedProcedure, sqlParameter);

            if (sqlParameter[3].Value.ToString() == "1")
            {
                List<object> listObj = new List<object>();
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    var obj = new
                    {
                        balanceIndex = ds.Tables[0].Rows[i]["BalanceIndex"].ToString(),
                        payCount = ds.Tables[0].Rows[i]["PayCount"].ToString()
                    };
                    listObj.Add(obj);
                }
                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, listObj);
            }
            else
            {
                return new ResponseModel(Return_Code.T, "", Result_Code.F, sqlParameter[4].Value.ToString());
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object getOrderDetailContain(Dictionary<string, object> dicParas)
        {
            XCCloudUserTokenModel userTokenModel = (XCCloudUserTokenModel)(dicParas[Constant.XCCloudUserTokenModel]);
            TokenDataModel userTokenDataModel = (TokenDataModel)(userTokenModel.DataModel);

            string storedProcedure = "GetOrderDetail";
            string flwOrderId = dicParas.ContainsKey("flwOrderId") ? dicParas["flwOrderId"].ToString() : string.Empty;

            SqlParameter[] sqlParameter = new SqlParameter[5];
            sqlParameter[0] = new SqlParameter("@MerchId", SqlDbType.VarChar);
            sqlParameter[0].Value = userTokenDataModel.MerchID;

            sqlParameter[1] = new SqlParameter("@StoreId", SqlDbType.VarChar);
            sqlParameter[1].Value = userTokenDataModel.StoreID;

            sqlParameter[2] = new SqlParameter("@FlwOrderId", SqlDbType.VarChar);
            sqlParameter[2].Value = flwOrderId;

            sqlParameter[3] = new SqlParameter("@Return", SqlDbType.Int);
            sqlParameter[3].Direction = ParameterDirection.Output;

            sqlParameter[4] = new SqlParameter("@ErrMsg", SqlDbType.VarChar, 200);
            sqlParameter[4].Direction = ParameterDirection.Output;

            System.Data.DataSet ds = XCCloudBLL.GetStoredProcedureSentence(storedProcedure, sqlParameter);

            List<object> listObj2 = new List<object>();
            List<object> listObj3 = new List<object>();

            if (sqlParameter[3].Value.ToString() == "1")
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    var obj = new
                    {
                        orderId = ds.Tables[0].Rows[0]["OrderID"].ToString(),
                        saleTime = ds.Tables[0].Rows[0]["SaleTime"].ToString(),
                        payCount = ds.Tables[0].Rows[0]["PayCount"].ToString(),
                        freePay = Convert.ToDecimal(ds.Tables[0].Rows[0]["FreePay"]).ToString("0.00"),
                        realPay = ds.Tables[0].Rows[0]["RealPay"].ToString(),
                        note = ds.Tables[0].Rows[0]["Note"].ToString(),
                        realName = ds.Tables[0].Rows[0]["RealName"].ToString()
                    };

                    for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                    {
                        var obj2 = new
                        {
                            foodId = ds.Tables[1].Rows[i]["FoodId"].ToString(),
                            category = ds.Tables[1].Rows[i]["Category"].ToString(),
                            categoryName = ds.Tables[1].Rows[i]["CategoryName"].ToString(),
                            foodName = ds.Tables[1].Rows[i]["FoodName"].ToString(),
                            saleCount = ds.Tables[1].Rows[i]["SaleCount"].ToString(),
                            singlePrice = Convert.ToDecimal(ds.Tables[1].Rows[i]["SinglePrice"]).ToString("0.00"),
                            salePrice = Convert.ToDecimal(ds.Tables[1].Rows[i]["SalePrice"]).ToString("0.00"),
                            taxFee = Convert.ToDecimal(ds.Tables[1].Rows[i]["TaxFee"]).ToString("0.00"),
                            taxTotal = Convert.ToDecimal(ds.Tables[1].Rows[i]["TaxTotal"]).ToString("0.00")
                        };
                        listObj2.Add(obj2);
                    }

                    for (int i = 0; i < ds.Tables[2].Rows.Count; i++)
                    {
                        var obj3 = new
                        {
                            id = ds.Tables[2].Rows[i]["ID"].ToString(),
                            balanceIndex = ds.Tables[2].Rows[i]["BalanceIndex"].ToString(),
                            payCount = ds.Tables[2].Rows[i]["PayCount"].ToString()
                        };
                        listObj3.Add(obj3);
                    }

                    var orderObj = new
                    {
                        orderMain = obj,
                        orderDetail = listObj2,
                        orderPay = listObj3
                    };

                    return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, orderObj);
                }
                else
                {
                    return new ResponseModel(Return_Code.T, "", Result_Code.F, "数据不存在");
                }
            }
            else
            {
                return new ResponseModel(Return_Code.T, "", Result_Code.F, sqlParameter[4].Value.ToString());
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object getOrders(Dictionary<string, object> dicParas)
        {
            XCCloudUserTokenModel userTokenModel = (XCCloudUserTokenModel)(dicParas[Constant.XCCloudUserTokenModel]);
            TokenDataModel userTokenDataModel = (TokenDataModel)(userTokenModel.DataModel);

            string storedProcedure = "GetOrders";
            string orderId = dicParas.ContainsKey("orderId") ? dicParas["orderId"].ToString() : string.Empty;
            string icCardId = dicParas.ContainsKey("icCardId") ? dicParas["icCardId"].ToString() : string.Empty;
            string startDate = dicParas.ContainsKey("startDate") ? dicParas["startDate"].ToString() : string.Empty;
            string endDate = dicParas.ContainsKey("endDate") ? dicParas["endDate"].ToString() : string.Empty;

            SqlParameter[] sqlParameter = new SqlParameter[8];
            sqlParameter[0] = new SqlParameter("@MerchId", SqlDbType.VarChar);
            sqlParameter[0].Value = userTokenDataModel.MerchID;

            sqlParameter[1] = new SqlParameter("@StoreID", SqlDbType.VarChar);
            sqlParameter[1].Value = userTokenDataModel.StoreID;

            sqlParameter[2] = new SqlParameter("@OrderId", SqlDbType.VarChar);
            sqlParameter[2].Value = orderId;

            sqlParameter[3] = new SqlParameter("@ICCardId", SqlDbType.Int);
            sqlParameter[3].Value = icCardId;

            sqlParameter[4] = new SqlParameter("@StartDate", SqlDbType.VarChar);
            sqlParameter[4].Value = startDate;

            sqlParameter[5] = new SqlParameter("@EndDate", SqlDbType.VarChar);
            sqlParameter[5].Value = endDate;

            sqlParameter[6] = new SqlParameter("@ErrMsg", SqlDbType.VarChar, 200);
            sqlParameter[6].Direction = ParameterDirection.Output;

            sqlParameter[7] = new SqlParameter("@Return", SqlDbType.Int);
            sqlParameter[7].Direction = ParameterDirection.ReturnValue;

            System.Data.DataSet ds = XCCloudBLL.GetStoredProcedureSentence(storedProcedure, sqlParameter);

            if (sqlParameter[7].Value.ToString() == "1")
            {
                List<object> list = new List<object>();
                if (ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        var obj = new
                        {
                            orderId = ds.Tables[0].Rows[i]["OrderID"].ToString(),
                            saleTime = Convert.ToDateTime(ds.Tables[0].Rows[i]["CreateTime"]).ToString("yyyy-MM-dd HH:mm:ss"),
                            icCardId = ds.Tables[0].Rows[i]["ICCardID"].ToString(),
                            payCount = Convert.ToDecimal(ds.Tables[0].Rows[i]["PayCount"]).ToString("#.00"),
                            orderStatusName = ds.Tables[0].Rows[i]["OrderStatusName"].ToString()
                        };
                        list.Add(obj);
                    }
                    return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, list);
                }
                else
                {
                    return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, list);
                }
            }
            else
            {
                return new ResponseModel(Return_Code.T, "", Result_Code.F, sqlParameter[6].Value.ToString());
            }
        }

        /// <summary>
        /// 获取订单流水号
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MethodToken)] 
        public object getOrder(Dictionary<string, object> dicParas)
        {
            try
            {
                string StoreID = dicParas.ContainsKey("storeId") ? dicParas["storeId"].ToString() : string.Empty;
                string UserToken = dicParas.ContainsKey("userToken") ? dicParas["userToken"].ToString() : string.Empty;
                if (string.IsNullOrEmpty(UserToken))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "用户token无效");
                }

                var list = XCCloudUserTokenBusiness.GetUserTokenModel(UserToken);
                if (list == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "用户token无效");
                }
                if (string.IsNullOrEmpty(StoreID))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "门店号无效");
                }
                string OrderNum = string.Empty;
                IFlw_Order_SerialNumberService flw_Order_SerialNumberService = BLLContainer.Resolve<IFlw_Order_SerialNumberService>();
                var orderlist = flw_Order_SerialNumberService.GetModels(x => x.StoreiD == StoreID).ToList().FirstOrDefault(x => Convert.ToDateTime(x.CreateDate).ToString("yyyy-MM-dd") == DateTime.Now.ToString("yyyy-MM-dd"));


                IBase_StoreInfoService base_StoreInfoService = BLLContainer.Resolve<IBase_StoreInfoService>();
                var menlist = base_StoreInfoService.GetModels(x => x.ID == StoreID).FirstOrDefault<Base_StoreInfo>();
                if (menlist == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "未查询到商户号");
                }
                int num = 0;
                if (orderlist == null)
                {
                    num = +1;
                    Flw_Order_SerialNumber flw_Order_SerialNumber = new Flw_Order_SerialNumber();
                    flw_Order_SerialNumber.StoreiD = StoreID;
                    flw_Order_SerialNumber.CreateDate = DateTime.Now;
                    flw_Order_SerialNumber.CurNumber = num;
                    flw_Order_SerialNumberService.Add(flw_Order_SerialNumber);
                }
                else
                {
                    num = Convert.ToInt32(orderlist.CurNumber + 1);
                    orderlist.CurNumber = num;
                    flw_Order_SerialNumberService.Update(orderlist);
                }
                OrderNum = DateTime.Now.ToString("yyyyMMddHHmm") + menlist.MerchID + num.ToString();
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, OrderNum, Result_Code.T, "");
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        /// <summary>
        /// 购物车结算
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [Authorize(Roles = "XcUser,XcAdmin")]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object addOrder(Dictionary<string, object> dicParas)
        {
            string errMsg = string.Empty;
            XCCloudUserTokenModel userTokenModel = (XCCloudUserTokenModel)(dicParas[Constant.XCCloudUserTokenModel]);
            TokenDataModel userTokenDataModel = (TokenDataModel)(userTokenModel.DataModel);
            if (!CheckAddOrderParams(dicParas, out errMsg))
            {
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, errMsg);
            }

            string buyDetailsJson = dicParas.ContainsKey("buyDetails") ? dicParas["buyDetails"].ToString() : string.Empty;
            List<OrderBuyDetailModel> buyDetailList = Utils.DataContractJsonDeserializer<List<OrderBuyDetailModel>>(buyDetailsJson);
            string customerType = dicParas.ContainsKey("customerType") ? dicParas["customerType"].ToString() : string.Empty;
            string memberLevelId = dicParas.ContainsKey("memberLevelId") ? dicParas["memberLevelId"].ToString() : string.Empty;
            string icCardId = dicParas.ContainsKey("icCardId") ? dicParas["icCardId"].ToString() : string.Empty;
            string payCount = dicParas.ContainsKey("payCount") ? dicParas["payCount"].ToString() : string.Empty;
            string freePay = dicParas.ContainsKey("freePay") ? dicParas["freePay"].ToString() : string.Empty; 
            string deposit = dicParas.ContainsKey("deposit") ? dicParas["deposit"].ToString() : string.Empty;
            string openFee = dicParas.ContainsKey("openFee") ? dicParas["openFee"].ToString() : string.Empty;
            string workStation = dicParas.ContainsKey("workStation") ? dicParas["workStation"].ToString() : string.Empty;
            string authorId = dicParas.ContainsKey("authorId") ? dicParas["authorId"].ToString() : string.Empty;
            string note = dicParas.ContainsKey("note") ? dicParas["note"].ToString() : string.Empty;
            string orderSource = dicParas.ContainsKey("orderSource") ? dicParas["orderSource"].ToString() : string.Empty;
            string saleCoinType = dicParas.ContainsKey("saleCoinType") ? dicParas["saleCoinType"].ToString() : string.Empty;
            string mobile = dicParas.ContainsKey("mobile") ? dicParas["mobile"].ToString() : string.Empty;

            string flwSendId = RedisCacheHelper.CreateCloudSerialNo(userTokenDataModel.StoreID, true);

            StringBuilder sb = new StringBuilder();
            sb.Append("declare @Return int = 0;");
            sb.Append(string.Format("declare @StoreId varchar(15) = '{0}';", userTokenDataModel.StoreID));
            sb.Append(string.Format("declare @CustomerType int = {0};",customerType));
            sb.Append(string.Format("declare @MemberLevelId int = {0};", memberLevelId));
            sb.Append(string.Format("declare @Mobile varchar(11) = '{0}';", mobile));
            sb.Append(string.Format("declare @ICCardId int = {0};", icCardId));
            sb.Append(string.Format("declare @PayCount decimal(18,2) = {0};", payCount));
            sb.Append(string.Format("declare @FreePay decimal(18,2) = {0};", freePay));
            sb.Append(string.Format("declare @Deposit decimal(18,2) = {0};", deposit));
            sb.Append(string.Format("declare @OpenFee decimal(18,2) = {0};", openFee));
            sb.Append(string.Format("declare @UserID int = {0};", userTokenDataModel.CreateUserID));
            sb.Append(string.Format("declare @WorkStation varchar(50) = '{0}';", workStation));
            sb.Append(string.Format("declare @AuthorID int = {0};", authorId));
            sb.Append(string.Format("declare @Note varchar(500) = '{0}';", note));
            sb.Append(string.Format("declare @OrderSource int = {0};", orderSource));
            sb.Append(string.Format("declare @SaleCoinType int = {0};", saleCoinType));
            //sb.Append(string.Format("declare @OrderFlwId varchar(32) = '{0}';", ""));
            sb.Append(string.Format("declare @FlwSeedIndex int = {0};", 0));
            //sb.Append(string.Format("declare @ErrMsg varchar(200) = '{0}';", ""));
            sb.Append("declare @FoodSaleDetailList [FoodSaleDetailListType];");
            for (int i = 0; i < buyDetailList.Count; i++)
            {
                sb.Append(string.Format("insert into @FoodSaleDetailList(FoodId,Category,FoodCount,PayType,PayNum) values({0},{1},{2},{3},{4});",
                    buyDetailList[i].FoodId, buyDetailList[i].Category, buyDetailList[i].FoodCount, buyDetailList[i].PayType, buyDetailList[i].PayNum));
            }
            sb.Append(string.Format("declare @FlwSeedId varchar(32) = {0};", flwSendId));
            sb.Append("create table #TmpBarTableSync(TableName varchar(50),FlwId varchar(32),Status int);");
            sb.Append("exec @Return = CreateOrder @FoodSaleDetailList,@StoreId,@CustomerType,@Mobile,@MemberLevelId,@ICCardId,@PayCount,@FreePay,@OpenFee,@Deposit,@UserID,@WorkStation,@AuthorID,@Note,@OrderSource,@SaleCoinType,@FlwSeedId,@FlwSeedIndex output,@OrderFlwId output,@ErrMsg output;");
            sb.Append("select * from #TmpBarTableSync;");
            sb.Append("drop table #TmpBarTableSync;");

            SqlParameter[] sqlParameter = new SqlParameter[3];

            sqlParameter[0] = new SqlParameter("@ErrMsg", SqlDbType.VarChar,200);
            sqlParameter[0].Direction = ParameterDirection.Output;
            sqlParameter[1] = new SqlParameter("@OrderFlwID", SqlDbType.VarChar,32);
            sqlParameter[1].Direction = ParameterDirection.Output;
            sqlParameter[2] = new SqlParameter("@Return", SqlDbType.Int);
            sqlParameter[2].Direction = ParameterDirection.ReturnValue;

            //System.Data.DataSet ds = XCCloudBLL.GetStoredProcedureSentence(storedProcedure, sqlParameter);
            System.Data.DataSet ds = XCCloudBLL.ExecuteQuerySentence(sb.ToString(), sqlParameter);
            if (sqlParameter[1].Value.ToString() != "")
            {
                var obj = new {
                    orderFlwId = sqlParameter[1].Value.ToString()
                };
                NewOrderDataSync(ds.Tables[0]);
                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, obj);
            }
            else
            {
                return new ResponseModel(Return_Code.T, "", Result_Code.F, sqlParameter[0].Value.ToString());
            }
        }


        private void NewOrderDataSync(System.Data.DataTable dt)
        {
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                XCCloudService.SyncService.UDP.Client.StoreDataSync(dt.Rows[i][0].ToString(), dt.Rows[i][1].ToString(), int.Parse(dt.Rows[i][2].ToString()));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "XcUser,XcAdmin")]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object addOrderToCache(Dictionary<string, object> dicParas)
        {
            string errMsg = string.Empty;
            string orderFlwId = dicParas.ContainsKey("orderFlwId") ? dicParas["orderFlwId"].ToString() : string.Empty;
            string customerType = dicParas.ContainsKey("customerType") ? dicParas["customerType"].ToString() : string.Empty;
            string icCardId = dicParas.ContainsKey("icCardId") ? dicParas["icCardId"].ToString() : string.Empty;
            RegisterMember registerMember = null;
            
            XCCloudUserTokenModel userTokenModel = (XCCloudUserTokenModel)(dicParas[Constant.XCCloudUserTokenModel]);
            TokenDataModel userTokenDataModel = (TokenDataModel)(userTokenModel.DataModel);

            if (string.IsNullOrEmpty(orderFlwId))
            {
                return new ResponseModel(Return_Code.T, "", Result_Code.F, "订单Id参数不能为空");
            }

            if (string.IsNullOrEmpty(customerType))
            {
                return new ResponseModel(Return_Code.T, "", Result_Code.F, "客户类型参数不能为空");
            }

            if (!string.IsNullOrEmpty(icCardId) && !Utils.isNumber(icCardId))
            {
                return new ResponseModel(Return_Code.T, "", Result_Code.F, "会员卡Id参数应为整数类型");
            }

            try
            {
                string storedProcedure = "CheckCacheOrder";
                SqlParameter[] sqlParameter = new SqlParameter[6];
                sqlParameter[0] = new SqlParameter("@StoreID", SqlDbType.VarChar);
                sqlParameter[0].Value = userTokenDataModel.StoreID;
                sqlParameter[1] = new SqlParameter("@FlwOrderId", SqlDbType.VarChar);
                sqlParameter[1].Value = orderFlwId;
                sqlParameter[2] = new SqlParameter("@CustomerType", SqlDbType.Int);
                sqlParameter[2].Value = customerType;
                sqlParameter[3] = new SqlParameter("@ICCardID", SqlDbType.Int);
                sqlParameter[3].Value = icCardId;
                sqlParameter[4] = new SqlParameter("@ErrMsg", SqlDbType.VarChar, 200);
                sqlParameter[4].Direction = ParameterDirection.Output;
                sqlParameter[5] = new SqlParameter("@Return", SqlDbType.Int);
                sqlParameter[5].Direction = ParameterDirection.ReturnValue;
                XCCloudBLL.ExecuteStoredProcedureSentence(storedProcedure, sqlParameter);

                if (sqlParameter[5].Value.ToString() == "1")
                {
                    if (FlwFoodOrderBusiness.Exist(orderFlwId))
                    {
                        return new ResponseModel(Return_Code.T, "", Result_Code.T, "");
                    }
                    else
                    {
                        FoodOrderCacheModel orderModel = new FoodOrderCacheModel(userTokenDataModel.MerchID, userTokenDataModel.StoreID, orderFlwId, int.Parse(customerType), int.Parse(icCardId), userTokenDataModel.WorkStation, Convert.ToInt32(userTokenDataModel.WorkStationID), registerMember);
                        FlwFoodOrderBusiness.Add(orderModel);
                        return new ResponseModel(Return_Code.T, "", Result_Code.T, "");                    
                    }
                }
                else
                {
                    return new ResponseModel(Return_Code.T, "", Result_Code.F, sqlParameter[4].Value.ToString());
                }
            }
            catch(Exception e)
            {
                throw e;
            }
        }


        [Authorize(Roles = "XcUser,XcAdmin")]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object saveRegisterMember(Dictionary<string, object> dicParas)
        {
            XCCloudUserTokenModel userTokenModel = (XCCloudUserTokenModel)(dicParas[Constant.XCCloudUserTokenModel]);
            TokenDataModel userTokenDataModel = (TokenDataModel)(userTokenModel.DataModel);
            string flwOrderId = dicParas.ContainsKey("flwOrderId") ? dicParas["flwOrderId"].ToString() : string.Empty;
            string registerMemberJson = dicParas.ContainsKey("registerMemberJson") ? dicParas["registerMemberJson"].ToString() : string.Empty;
            string smsCode = dicParas.ContainsKey("smsCode") ? dicParas["smsCode"].ToString() : string.Empty;
            RegisterMember registerMember = Utils.DataContractJsonDeserializer<RegisterMember>(registerMemberJson);
            if (FlwMemberRegisterBusiness.Exist(flwOrderId))
            {
                return new ResponseModel(Return_Code.T, "", Result_Code.T, "");
            }
            else
            {
                FlwMemberRegisterCacheModel memberRegisterModel = new FlwMemberRegisterCacheModel(userTokenDataModel.MerchID, userTokenDataModel.StoreID, flwOrderId, userTokenDataModel.WorkStation, Convert.ToInt32(userTokenDataModel.WorkStationID), registerMember);
                FlwMemberRegisterBusiness.Add(memberRegisterModel);
                return new ResponseModel(Return_Code.T, "", Result_Code.T, "");
            }
        }


        [Authorize(Roles = "XcUser,XcAdmin")]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object getCacheOrder(Dictionary<string, object> dicParas)
        {
            string errMsg = string.Empty;
            XCCloudUserTokenModel userTokenModel = (XCCloudUserTokenModel)(dicParas[Constant.XCCloudUserTokenModel]);
            TokenDataModel userTokenDataModel = (TokenDataModel)(userTokenModel.DataModel);

            List<FoodOrderCacheModel> list = FlwFoodOrderBusiness.GetOrderListByWorkStation(userTokenDataModel.StoreID, userTokenDataModel.WorkStation);
            list = list.OrderByDescending(p => p.CreateTime).ToList<FoodOrderCacheModel>();

            List<object> listObj = new List<object>();
            for (int i = 0; i < list.Count; i++)
            {
                var obj = new {
                    flwOrderId = list[i].FlwOrderId,
                    cacheTime = list[i].CreateTime.ToString("yyyy-MM-dd HH:mm")
                    //registerMember = list[i].CustomerType == 1 ? list[i].RegisterMember:null
                };
                listObj.Add(obj);
            }
            return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, listObj);
        }



        /// <summary>
        /// 获取订单详情
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [Authorize(Roles = "XcUser,XcAdmin")]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object getOrderContain(Dictionary<string, object> dicParas)
        {
            string errMsg = string.Empty;
            string orderFlwId = dicParas.ContainsKey("orderFlwId") ? dicParas["orderFlwId"].ToString() : string.Empty;
            XCCloudUserTokenModel userTokenModel = (XCCloudUserTokenModel)(dicParas[Constant.XCCloudUserTokenModel]);
            TokenDataModel userTokenDataModel = (TokenDataModel)(userTokenModel.DataModel);

            if (string.IsNullOrEmpty(orderFlwId))
            {
                return new ResponseModel(Return_Code.T, "", Result_Code.F, "订单Id参数无效");
            }

            string storedProcedure = "GetOrderContainById";
            SqlParameter[] sqlParameter = new SqlParameter[21];
            sqlParameter[0] = new SqlParameter("@StoreId", SqlDbType.VarChar,15);
            sqlParameter[0].Value = userTokenDataModel.StoreID;
            sqlParameter[1] = new SqlParameter("@OrderFlwId", SqlDbType.VarChar,32);
            sqlParameter[1].Value = orderFlwId;
            sqlParameter[2] = new SqlParameter("@QueryType", SqlDbType.Int);
            sqlParameter[2].Value = 1;
            sqlParameter[3] = new SqlParameter("@OrderStatus", SqlDbType.Int);
            sqlParameter[3].Direction = ParameterDirection.Output;
            sqlParameter[4] = new SqlParameter("@CustomerType", SqlDbType.Int);
            sqlParameter[4].Direction = ParameterDirection.Output;
            sqlParameter[5] = new SqlParameter("@ICCardId", SqlDbType.Int);
            sqlParameter[5].Direction = ParameterDirection.Output;
            sqlParameter[6] = new SqlParameter("@UserId", SqlDbType.Int);
            sqlParameter[6].Direction = ParameterDirection.Output;
            sqlParameter[7] = new SqlParameter("@PayCount", SqlDbType.Decimal);
            sqlParameter[7].Direction = ParameterDirection.Output;
            sqlParameter[8] = new SqlParameter("@RealPay", SqlDbType.Decimal);
            sqlParameter[8].Direction = ParameterDirection.Output;
            sqlParameter[9] = new SqlParameter("@FreePay", SqlDbType.Decimal);
            sqlParameter[9].Direction = ParameterDirection.Output;
            sqlParameter[10] = new SqlParameter("@FoodCount", SqlDbType.Int);
            sqlParameter[10].Direction = ParameterDirection.Output;
            sqlParameter[11] = new SqlParameter("@DetailsGoodsCount", SqlDbType.Int);
            sqlParameter[11].Direction = ParameterDirection.Output;
            sqlParameter[12] = new SqlParameter("@MemberLevelId", SqlDbType.Int);
            sqlParameter[12].Direction = ParameterDirection.Output;
            sqlParameter[13] = new SqlParameter("@MemberLevelName", SqlDbType.VarChar,50);
            sqlParameter[13].Direction = ParameterDirection.Output;
            sqlParameter[14] = new SqlParameter("@OpenFee", SqlDbType.Decimal);
            sqlParameter[14].Direction = ParameterDirection.Output;
            sqlParameter[15] = new SqlParameter("@Deposit", SqlDbType.Decimal);
            sqlParameter[15].Direction = ParameterDirection.Output;
            sqlParameter[16] = new SqlParameter("@RenewFee", SqlDbType.Decimal);
            sqlParameter[16].Direction = ParameterDirection.Output;
            sqlParameter[17] = new SqlParameter("@ChangeFee", SqlDbType.Decimal);
            sqlParameter[17].Direction = ParameterDirection.Output;
            sqlParameter[18] = new SqlParameter("@CreateTime", SqlDbType.DateTime);
            sqlParameter[18].Direction = ParameterDirection.Output;
            sqlParameter[19] = new SqlParameter("@ErrMsg", SqlDbType.VarChar,200);
            sqlParameter[19].Direction = ParameterDirection.Output;
            sqlParameter[20] = new SqlParameter("@Result", SqlDbType.Int);
            sqlParameter[20].Direction = ParameterDirection.Output;

            System.Data.DataSet ds = XCCloudBLL.GetStoredProcedureSentence(storedProcedure, sqlParameter);
            if (sqlParameter[20].Value.ToString() == "1")
            {
                OrderInfo1Model model = new OrderInfo1Model();
                model.CustomerType = Convert.ToInt32(sqlParameter[4].Value);
                model.ICCardId = Convert.ToInt32(sqlParameter[5].Value);
                model.PayCount = Convert.ToDecimal(sqlParameter[7].Value);
                model.RealPay = Convert.ToDecimal(sqlParameter[8].Value);
                model.FreePay = Convert.ToDecimal(sqlParameter[9].Value);
                model.FoodCount = Convert.ToInt32(sqlParameter[10].Value);
                model.DetailsGoodsCount = Convert.ToInt32(sqlParameter[11].Value);
                model.MemberLevelId = Convert.ToInt32(sqlParameter[12].Value);
                model.MemberLevelName = sqlParameter[13].Value.ToString();
                model.OpenFee = Convert.ToDecimal(sqlParameter[14].Value);
                model.Deposit = Convert.ToDecimal(sqlParameter[15].Value);
                model.RenewFee = Convert.ToDecimal(sqlParameter[16].Value);
                model.ChangeFee = Convert.ToDecimal(sqlParameter[17].Value);
                model.CreateTime = Convert.ToDateTime(sqlParameter[18].Value);
                model.UserId = Convert.ToInt32(sqlParameter[6].Value);
                model.Detail = Utils.GetModelList<OrderBuyDetail1Model>(ds.Tables[0]).ToList();
                return ResponseModelFactory.CreateSuccessModel<OrderInfo1Model>(isSignKeyReturn, model);
            }
            else
            {
                return new ResponseModel(Return_Code.T, "", Result_Code.F, "订单信息不存在");
            }
        }



        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object updateOrder(Dictionary<string, object> dicParas)
        {
            string errMsg = string.Empty;
            string foodSaleDetailListJson = dicParas.ContainsKey("foodSaleDetailListJson") ? dicParas["foodSaleDetailListJson"].ToString() : string.Empty;
            string couponsListJson = dicParas.ContainsKey("couponsListJson") ? dicParas["couponsListJson"].ToString() : string.Empty;
            string flwOrderId = dicParas.ContainsKey("flwOrderId") ? dicParas["flwOrderId"].ToString() : string.Empty;
            string newFlwOrderId = dicParas.ContainsKey("newFlwOrderId") ? dicParas["newFlwOrderId"].ToString() : string.Empty;
            string mobile = dicParas.ContainsKey("mobile") ? dicParas["mobile"].ToString() : string.Empty;
            string icCardId = dicParas.ContainsKey("icCardId") ? dicParas["icCardId"].ToString() : string.Empty;
            string distinctRuleId = dicParas.ContainsKey("distinctRuleId") ? dicParas["distinctRuleId"].ToString() : string.Empty;
            string payCount = dicParas.ContainsKey("payCount") ? dicParas["payCount"].ToString() : string.Empty;
            //string realPay = dicParas.ContainsKey("realPay") ? dicParas["realPay"].ToString() : string.Empty;
            string workStation = dicParas.ContainsKey("workStation") ? dicParas["workStation"].ToString() : string.Empty;
            string deposit = dicParas.ContainsKey("deposit") ? dicParas["deposit"].ToString() : string.Empty;
            string openFee = dicParas.ContainsKey("openFee") ? dicParas["openFee"].ToString() : string.Empty;
            string freePay = dicParas.ContainsKey("freePay") ? dicParas["freePay"].ToString() : string.Empty;
            string note = dicParas.ContainsKey("note") ? dicParas["note"].ToString() : string.Empty;
            string orderSource = dicParas.ContainsKey("orderSource") ? dicParas["orderSource"].ToString() : string.Empty;
            string authorId = dicParas.ContainsKey("authorId") ? dicParas["authorId"].ToString() : string.Empty;
            string saleCoinType = dicParas.ContainsKey("saleCoinType") ? dicParas["saleCoinType"].ToString() : string.Empty;

            XCCloudUserTokenModel userTokenModel = (XCCloudUserTokenModel)(dicParas[Constant.XCCloudUserTokenModel]);
            TokenDataModel userTokenDataModel = (TokenDataModel)(userTokenModel.DataModel);

            if (!CheckUpdateOrderParams(dicParas, out errMsg))
            {
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, errMsg);
            }

            string storedProcedure = "updateOrder";
            SqlParameter[] sqlParameter = new SqlParameter[18];
            sqlParameter[0] = new SqlParameter("@FoodSaleDetailList", SqlDbType.Structured);
            sqlParameter[0].Value = FlwFoodOrderBusiness.GetOrderBuyDetailList(userTokenDataModel.StoreID, foodSaleDetailListJson);
            sqlParameter[1] = new SqlParameter("@CouponsList", SqlDbType.Structured);
            sqlParameter[1].Value = FlwFoodOrderBusiness.GetCouponList(userTokenDataModel.StoreID, couponsListJson);
            sqlParameter[2] = new SqlParameter("@StoreId", SqlDbType.VarChar,15);
            sqlParameter[2].Value = userTokenDataModel.StoreID;
            sqlParameter[3] = new SqlParameter("@FlwOrderId", SqlDbType.VarChar,32);
            sqlParameter[3].Value = flwOrderId;
            sqlParameter[4] = new SqlParameter("@NewFlwSaleId", SqlDbType.VarChar, 32);
            sqlParameter[4].Value = RedisCacheHelper.CreateCloudSerialNo(userTokenDataModel.StoreID, true);
            sqlParameter[5] = new SqlParameter("@Mobile", SqlDbType.VarChar,11);
            sqlParameter[5].Value = mobile;
            sqlParameter[6] = new SqlParameter("@DistinctRuleId", SqlDbType.Int);
            sqlParameter[6].Value = distinctRuleId;
            sqlParameter[7] = new SqlParameter("@PayCount", SqlDbType.Decimal);
            sqlParameter[7].Value = payCount;
            sqlParameter[8] = new SqlParameter("@FreePay", SqlDbType.Decimal);
            sqlParameter[8].Value = freePay;
            sqlParameter[9] = new SqlParameter("@UserId", SqlDbType.Int);
            sqlParameter[9].Value = userTokenDataModel.CreateUserID;
            sqlParameter[10] = new SqlParameter("@WorkStation", SqlDbType.VarChar,50);
            sqlParameter[10].Value = workStation;
            sqlParameter[11] = new SqlParameter("@Note", SqlDbType.VarChar,200);
            sqlParameter[11].Value = note;
            sqlParameter[12] = new SqlParameter("@OrderSource", SqlDbType.Int);
            sqlParameter[12].Value = orderSource;
            sqlParameter[13] = new SqlParameter("@AuthorId", SqlDbType.Int);
            sqlParameter[13].Value = authorId;
            sqlParameter[14] = new SqlParameter("@SaleCoinType", SqlDbType.Int);
            sqlParameter[14].Value = saleCoinType;
            sqlParameter[15] = new SqlParameter("@NewFlwOrderId", SqlDbType.VarChar,32);
            sqlParameter[15].Direction = ParameterDirection.Output;
            sqlParameter[16] = new SqlParameter("@ErrMsg", SqlDbType.VarChar,200);
            sqlParameter[16].Direction = ParameterDirection.Output;
            sqlParameter[17] = new SqlParameter("@Return", SqlDbType.Int);
            sqlParameter[17].Direction = ParameterDirection.ReturnValue;

            System.Data.DataSet ds = XCCloudBLL.GetStoredProcedureSentence(storedProcedure, sqlParameter);

            if (sqlParameter[17].Value.ToString() == "1")
            {
                var obj = new
                {
                    orderFlwId = sqlParameter[17].Value.ToString()
                };
                NewOrderDataSync(ds.Tables[0]);
                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, obj);
            }
            else
            {
                return new ResponseModel(Return_Code.T, "", Result_Code.F, sqlParameter[16].Value.ToString());
            }

            if (string.IsNullOrEmpty(flwOrderId))
            {
                return new ResponseModel(Return_Code.T, "", Result_Code.F, "订单Id参数无效");
            }

            return null;
        }

        private bool CheckUpdateOrderParams(Dictionary<string, object> dicParas, out string errMsg)
        {
            errMsg = string.Empty;
            string flwOrderId = dicParas.ContainsKey("flwOrderId") ? dicParas["flwOrderId"].ToString() : string.Empty;
            string mobile = dicParas.ContainsKey("mobile") ? dicParas["mobile"].ToString() : string.Empty;
            string icCardId = dicParas.ContainsKey("icCardId") ? dicParas["icCardId"].ToString() : string.Empty;
            string distinctRuleId = dicParas.ContainsKey("distinctRuleId") ? dicParas["distinctRuleId"].ToString() : string.Empty;
            string realPay = dicParas.ContainsKey("realPay") ? dicParas["realPay"].ToString() : string.Empty;
            string freePay = dicParas.ContainsKey("freePay") ? dicParas["freePay"].ToString() : string.Empty;
            string userId = dicParas.ContainsKey("userId") ? dicParas["userId"].ToString() : string.Empty;
            string workStation = dicParas.ContainsKey("workStation") ? dicParas["workStation"].ToString() : string.Empty;
            string deposit = dicParas.ContainsKey("deposit") ? dicParas["deposit"].ToString() : string.Empty;
            string openFee = dicParas.ContainsKey("openFee") ? dicParas["openFee"].ToString() : string.Empty;
            string note = dicParas.ContainsKey("note") ? dicParas["note"].ToString() : string.Empty;
            string orderSource = dicParas.ContainsKey("orderSource") ? dicParas["orderSource"].ToString() : string.Empty;
            string authorId = dicParas.ContainsKey("authorId") ? dicParas["authorId"].ToString() : string.Empty;
            string saleCoinType = dicParas.ContainsKey("saleCoinType") ? dicParas["saleCoinType"].ToString() : string.Empty;

            if (string.IsNullOrEmpty(flwOrderId))
            {
                errMsg = "订单流水号无效";
                return false;
            }

            //if (!Utils.IsNumeric(newFlwOrderId))
            //{
            //    errMsg = "新的订单流水号无效";
            //    return false;
            //}

            if (string.IsNullOrEmpty(mobile))
            {
                errMsg = "手机号码无效";
                return false;
            }

            if (string.IsNullOrEmpty(distinctRuleId))
            {
                errMsg = "满减规则无效";
                return false;
            }

            if (string.IsNullOrEmpty(realPay) || !Utils.IsDecimal(realPay))
            {
                errMsg = "实付金额无效";
                return false;
            }

            if (string.IsNullOrEmpty(freePay))
            {
                errMsg = "免费金额无效";
                return false;
            }

            string orderSourceDefineStr = "0,1,2,3,4";
            string[] orderSourceArr = orderSource.Split(',');
            for (int i = 0; i < orderSourceArr.Length; i++)
            {
                if (!Utils.IsNumeric(orderSourceArr[i]))
                {
                    errMsg = "订单来源无效";
                    return false;
                }

                if (!orderSourceDefineStr.Contains(orderSourceArr[i]))
                {
                    errMsg = "订单来源无效";
                    return false;
                }
            }

            //:1电子币、2实物币手工、3、实物币售币机
            string saleCoinTypeStr = "1,2,3";
            string[] saleCoinTypeArr = saleCoinTypeStr.Split(',');
            for (int i = 0; i < saleCoinTypeArr.Length; i++)
            {
                if (!Utils.IsNumeric(saleCoinTypeArr[i]))
                {
                    errMsg = "售币类型无效";
                    return false;
                }

                if (!saleCoinTypeStr.Contains(saleCoinTypeArr[i]))
                {
                    errMsg = "售币类型无效";
                    return false;
                }
            }

            return true;
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object removeOrder(Dictionary<string, object> dicParas)
        {
            XCCloudUserTokenModel userTokenModel = (XCCloudUserTokenModel)(dicParas[Constant.XCCloudUserTokenModel]);
            TokenDataModel userTokenDataModel = (TokenDataModel)(userTokenModel.DataModel);
            string orderId = dicParas.ContainsKey("orderId") ? dicParas["orderId"].ToString() : string.Empty;

            if (FlwFoodOrderBusiness.Exist(orderId))
            {
                FlwFoodOrderBusiness.Remove(orderId);
                return new ResponseModel(Return_Code.T, "", Result_Code.T, "");
            }
            else
            {
                return new ResponseModel(Return_Code.T, "", Result_Code.F, "订单不存在");
            }
        }

        /// <summary>
        /// 完成订单支付
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [Authorize(Roles = "XcUser,XcAdmin")]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object payOrder(Dictionary<string, object> dicParas)
        {
            string errMsg = string.Empty;
            string registerMemberJson = dicParas.ContainsKey("registerMemberJson") ? dicParas["registerMemberJson"].ToString() : string.Empty;
            string workStationId = dicParas.ContainsKey("workStationId") ? dicParas["workStationId"].ToString() : string.Empty;
            string flwOrderId = dicParas.ContainsKey("flwOrderId") ? dicParas["flwOrderId"].ToString() : string.Empty;
            string mobile = dicParas.ContainsKey("mobile") ? dicParas["mobile"].ToString() : string.Empty;
            string realPay = dicParas.ContainsKey("realPay") ? dicParas["realPay"].ToString() : string.Empty;
            string payType = dicParas.ContainsKey("payType") ? dicParas["payType"].ToString() : string.Empty;
            

            XCCloudUserTokenModel userTokenModel = (XCCloudUserTokenModel)(dicParas[Constant.XCCloudUserTokenModel]);
            TokenDataModel userTokenDataModel = (TokenDataModel)(userTokenModel.DataModel);

            string flwSendId = RedisCacheHelper.CreateCloudSerialNo(userTokenDataModel.StoreID, true);

            if (string.IsNullOrEmpty(flwOrderId))
            {
                return new ResponseModel(Return_Code.T, "", Result_Code.F, "订单Id参数无效");
            }

            if (string.IsNullOrEmpty(workStationId))
            {
                return new ResponseModel(Return_Code.T, "", Result_Code.F, "工作站参数无效");
            }

            if (!Utils.IsNumeric(realPay) && decimal.Parse(realPay) <= 0)
            {
                return new ResponseModel(Return_Code.T, "", Result_Code.F, "实付金额无效");
            }

            RegisterMember registerMember = Utils.DataContractJsonDeserializer<RegisterMember>(registerMemberJson);
            string storedProcedure = "payOrder";
            SqlParameter[] sqlParameter = new SqlParameter[11];
            sqlParameter[0] = new SqlParameter("@RegisterMember", SqlDbType.Structured);
            sqlParameter[0].Value = FlwFoodOrderBusiness.GetRegisterMember(registerMember);
            sqlParameter[1] = new SqlParameter("@FlwSeedId", SqlDbType.VarChar, 29);
            sqlParameter[1].Value = flwSendId;
            sqlParameter[2] = new SqlParameter("@MerchId", SqlDbType.VarChar, 15);
            sqlParameter[2].Value = userTokenDataModel.MerchID;
            sqlParameter[3] = new SqlParameter("@StoreId", SqlDbType.VarChar, 15);
            sqlParameter[3].Value = userTokenDataModel.StoreID;
            sqlParameter[4] = new SqlParameter("@WorkStationId", SqlDbType.Int);
            sqlParameter[4].Value = userTokenDataModel.WorkStationID;
            sqlParameter[5] = new SqlParameter("@FlwOrderId", SqlDbType.VarChar,32);
            sqlParameter[5].Value = flwOrderId;
            sqlParameter[6] = new SqlParameter("@Mobile", SqlDbType.VarChar, 11);
            sqlParameter[6].Value = mobile;
            sqlParameter[7] = new SqlParameter("@RealPay", SqlDbType.Decimal);
            sqlParameter[7].Value = realPay;
            sqlParameter[8] = new SqlParameter("@PayType", SqlDbType.Int);
            sqlParameter[8].Value = payType;
            sqlParameter[9] = new SqlParameter("@ErrMsg", SqlDbType.VarChar, 200);
            sqlParameter[9].Direction = ParameterDirection.Output;
            sqlParameter[10] = new SqlParameter("@Return", SqlDbType.Int);
            sqlParameter[10].Direction = ParameterDirection.ReturnValue;

            System.Data.DataSet ds = XCCloudBLL.GetStoredProcedureSentence(storedProcedure, sqlParameter);

            if (sqlParameter[10].Value.ToString() == "1")
            {
                return new ResponseModel(Return_Code.T, "", Result_Code.T, "");
                NewOrderDataSync(ds.Tables[0]);
            }
            else
            {
                return new ResponseModel(Return_Code.T, "", Result_Code.F, sqlParameter[9].Value.ToString());
            }
        }

        private bool CheckAddOrderParams(Dictionary<string, object> dicParas,out string errMsg)
        {
            errMsg = string.Empty;
            string icCardId = dicParas.ContainsKey("icCardId") ? dicParas["icCardId"].ToString() : string.Empty;
            string payCount = dicParas.ContainsKey("payCount") ? dicParas["payCount"].ToString() : string.Empty;
            string workStation = dicParas.ContainsKey("workStation") ? dicParas["workStation"].ToString() : string.Empty;
            string authorId = dicParas.ContainsKey("authorId") ? dicParas["authorId"].ToString() : string.Empty;
            string note = dicParas.ContainsKey("note") ? dicParas["note"].ToString() : string.Empty;
            string orderSource = dicParas.ContainsKey("orderSource") ? dicParas["orderSource"].ToString() : string.Empty;
            string saleCoinType = dicParas.ContainsKey("saleCoinType") ? dicParas["saleCoinType"].ToString() : string.Empty;

            if (!Utils.IsDecimal(payCount))
            {
                errMsg = "应付金额无效";
                return false;
            }

            if (string.IsNullOrEmpty(workStation))
            {
                errMsg = "工作站无效";
                return false;
            }

            if (!Utils.IsNumeric(authorId))
            {
                errMsg = "授权员工Id无效";
                return false;
            }

            if (string.IsNullOrEmpty(orderSource))
            {
                errMsg = "订单来源无效";
                return false;
            }

            string orderSourceDefineStr = "0,1,2,3,4";
            string[] orderSourceArr = orderSource.Split(',');
            for (int i = 0; i < orderSourceArr.Length; i++)
            {
                if (!Utils.IsNumeric(orderSourceArr[i]))
                {
                    errMsg = "订单来源无效";
                    return false;
                }

                if (!orderSourceDefineStr.Contains(orderSourceArr[i]))
                {
                    errMsg = "订单来源无效";
                    return false;
                }
            }

            //:1电子币、2实物币手工、3、实物币售币机
            string saleCoinTypeStr = "1,2,3";
            string[] saleCoinTypeArr = saleCoinTypeStr.Split(',');
            for (int i = 0; i < saleCoinTypeArr.Length; i++)
            {
                if (!Utils.IsNumeric(saleCoinTypeArr[i]))
                {
                    errMsg = "售币类型无效";
                    return false;
                }

                if (!saleCoinTypeStr.Contains(saleCoinTypeArr[i]))
                {
                    errMsg = "售币类型无效";
                    return false;
                }
            }

            return true;
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetOrders(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                object[] conditions = dicParas.ContainsKey("conditions") ? (object[])dicParas["conditions"] : null;
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

                SqlParameter[] parameters = new SqlParameter[1];
                parameters[0] = new SqlParameter("@merchId", merchId);
                string sqlWhere = string.Empty;
                
                if (conditions != null && conditions.Length > 0)
                {
                    if (!QueryBLL.GenDynamicSql(conditions, "b.", ref sqlWhere, ref parameters, out errMsg))
                    {
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                }

                string sql = @"select b.ID, a.StoreID, a.StoreName, b.OrderID, b.FoodCount, b.OrderSource, b.CreateTime, b.PayType, b.OrderStatus," +
                    " c.DictKey as OrderSourceStr, d.DictKey as PayTypeStr, e.DictKey as OrderStatusStr, f.FoodName from Base_StoreInfo a " +
                    " inner join Flw_Order b on a.StoreID=b.StoreID " +
                    " left join (select b.* from Dict_System a inner join Dict_System b on a.ID=b.PID where a.DictKey='订单来源' and a.PID=0) c on convert(varchar, b.OrderSource)=c.DictValue " +
                    " left join (select b.* from Dict_System a inner join Dict_System b on a.ID=b.PID where a.DictKey='支付通道' and a.PID=0) d on convert(varchar, b.PayType)=d.DictValue " +
                    " left join (select b.* from Dict_System a inner join Dict_System b on a.ID=b.PID where a.DictKey='订单状态' and a.PID=0) e on convert(varchar, b.OrderStatus)=e.DictValue " +
                    " left join (select top 1 a.ID as OrderFlwID, d.FoodName from Flw_Order a inner join Flw_Order_Detail b on a.ID=b.OrderFlwID " +
                    " inner join Flw_Food_Sale c on b.FoodFlwID=c.ID " +
                    " inner join Data_FoodInfo d on c.FoodID=d.FoodID) f on b.ID=f.OrderFlwID " +
                    " where a.MerchID=@merchId ";
                sql = sql + sqlWhere;
                var dbContext = DbContextFactory.CreateByModelNamespace(typeof(Flw_Order).Namespace);
                var flw_Orders = dbContext.Database.SqlQuery<Flw_OrdersModel>(sql, parameters).ToList();

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, flw_Orders);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetOrdersDetails(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;                
                var orderFlwId = dicParas.Get("id");

                IFlw_OrderService flw_OrderService = BLLContainer.Resolve<IFlw_OrderService>();
                if (!flw_OrderService.Any(p => p.ID.Equals(orderFlwId, StringComparison.OrdinalIgnoreCase)))
                {
                    errMsg = "该订单不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                SqlParameter[] parameters = new SqlParameter[1];
                parameters[0] = new SqlParameter("@id", orderFlwId);

                string sql = @"select d.*, e.StoreName, f.DictKey as FoodTypeStr, g.DictKey as RechargeTypeStr, h.DictKey as FoodStateStr from Flw_Order a" +
                    " inner join Flw_Order_Detail b on a.ID=b.OrderFlwID " +
                    " inner join Flw_Food_Sale c on b.FoodFlwID=c.ID " +
                    " inner join Data_FoodInfo d on c.FoodID=d.FoodID " +
                    " left join Base_StoreInfo e on d.StoreID=e.StoreID " +
                    " left join Dict_System f on d.FoodType=f.ID " +
                    " left join (select b.* from Dict_System a inner join Dict_System b on a.ID=b.PID where a.DictKey='充值方式' and a.PID=0) g on convert(varchar, d.RechargeType)=g.DictValue " +
                    " left join (select b.* from Dict_System a inner join Dict_System b on a.ID=b.PID where a.DictKey='套餐状态' and a.PID=0) h on convert(varchar, d.FoodState)=h.DictValue " +
                    " where a.ID=@id ";
                var dbContext = DbContextFactory.CreateByModelNamespace(typeof(Data_FoodInfo).Namespace);
                var data_FoodInfo = dbContext.Database.SqlQuery<Data_FoodInfoModel>(sql, parameters).ToList();

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, data_FoodInfo);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetOrdersChart(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

                DateTime now = DateTime.Now;
                DateTime start = new DateTime(now.Year, now.Month, 1);
                DateTime end = start.AddMonths(1).AddDays(-1);
                TimeSpan ts = end.Subtract(start);
                int num = ts.Days;

                List<Store_CheckDateChartModel> store_CheckDateChart = new List<Store_CheckDateChartModel>();
                var base_StoreInfo = BLLContainer.Resolve<IBase_StoreInfoService>().GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase)).Select(o => o.ID);
                var store_CheckDate = BLLContainer.Resolve<IStore_CheckDateService>().GetModels(p => base_StoreInfo.Contains(p.StoreID) && System.Data.Entity.DbFunctions.DiffMonths(p.CheckDate, DateTime.Now) == 0);
                for (int i = 0; i <= num; i++)
                {
                    DateTime da = start.AddDays(i).Date;
                    var query = store_CheckDate.Where(p => System.Data.Entity.DbFunctions.DiffDays(p.CheckDate, da) == 0);
                    Store_CheckDateChartModel store_CheckDateModel = new Store_CheckDateChartModel();
                    store_CheckDateModel.CheckDate = da;
                    store_CheckDateModel.AliPay = (decimal)query.Sum(s => s.AliPay).GetValueOrDefault();
                    store_CheckDateModel.Wechat = (decimal)query.Sum(s => s.Wechat).GetValueOrDefault();
                    store_CheckDateChart.Add(store_CheckDateModel);
                }

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, store_CheckDateChart);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetOrdersCheck(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string checkDate = dicParas.ContainsKey("checkDate") ? dicParas["checkDate"].ToString() : string.Empty;
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

                string sql = " exec  GetOrdersCheck @CheckDate,@MerchId ";
                var parameters = new SqlParameter[2];
                parameters[0] = new SqlParameter("@CheckDate", checkDate);
                parameters[1] = new SqlParameter("@MerchId", merchId);

                var dbContext = DbContextFactory.CreateByModelNamespace(typeof(Flw_CheckDate).Namespace);
                var flw_OrderCheck = dbContext.Database.SqlQuery<Flw_OrderCheckModel>(sql, parameters).ToList();
                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, flw_OrderCheck);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object CheckOrders(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                int id = (dicParas.ContainsKey("id") && Utils.isNumber(dicParas["id"])) ? Convert.ToInt32(dicParas["id"]) : 0;
                string checkDate = dicParas.ContainsKey("checkDate") ? dicParas["checkDate"].ToString() : string.Empty;
                string merchId = string.Empty;
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

                if (string.IsNullOrEmpty(checkDate))
                {
                    errMsg = "营业日期不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }
                
                string sql = " exec  CheckOrders @CheckDate,@MerchId,@ID,@Return output ";
                var parameters = new SqlParameter[4];
                parameters[0] = new SqlParameter("@CheckDate", checkDate);
                parameters[1] = new SqlParameter("@MerchId", merchId);
                parameters[2] = new SqlParameter("@ID", id);
                parameters[3] = new SqlParameter("@Return", 0);
                parameters[3].Direction = System.Data.ParameterDirection.Output;
                
                XCCloudBLL.ExecuteQuerySentence(sql, parameters);
                if (parameters[3].Value.ToString() == "1")
                {
                    return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn);
                }
                else
                {
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, "账目审核失败");
                }                
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 获取订单支付状态
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MethodToken)]
        public object getOrderPayState(Dictionary<string, object> dicParas)
        {
            try
            {
                string OrderId = dicParas.ContainsKey("orderId") ? dicParas["orderId"].ToString().Trim() : string.Empty;
                string UserToken = dicParas.ContainsKey("userToken") ? dicParas["userToken"].ToString() : string.Empty;
                if (string.IsNullOrEmpty(UserToken))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "用户token无效");
                }

                var list = XCCloudUserTokenBusiness.GetUserTokenModel(UserToken);
                if (list == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "用户token无效");
                }

                OrderPayCacheModel model = new OrderPayCacheModel();
                if(OrderPayCache.IsExist(OrderId))
                {
                    model = OrderPayCache.GetValue(OrderId) as OrderPayCacheModel;
                }
                else
                {
                    Flw_Order order = Flw_OrderBusiness.GetOrderModel(OrderId);
                    if(order != null)
                    {
                        model.OrderId = OrderId;
                        model.PayAmount = order.RealPay == null ? 0 : order.RealPay.Value;
                        model.PayTime = order.PayTime == null ? "" : order.PayTime.Value.ToString("yyyy-MM-dd HH:mm:ss");
                        model.PayState = (OrderState)order.OrderStatus.Value;
                    }
                }

                return ResponseModelFactory<OrderPayCacheModel>.CreateModel(isSignKeyReturn, model);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}