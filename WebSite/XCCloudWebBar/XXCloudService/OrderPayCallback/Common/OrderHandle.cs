﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Transactions;
using System.Web;
using XCCloudWebBar.BLL.Container;
using XCCloudWebBar.Business.XCGameMana;
using XCCloudWebBar.Common;
using XCCloudWebBar.Common.Enum;
using XCCloudWebBar.Model.CustomModel.XCGameManager;
using XCCloudWebBar.Model.WeiXin.Message;
using XCCloudWebBar.Model.XCGameManager;
using XCCloudWebBar.Pay.PPosPay;
using XCCloudWebBar.RadarService;
using XCCloudWebBar.WeiXin.Message;
using XXCloudService.Utility;

namespace XCCloudWebBar.OrderPayCallback.Common
{
    public class OrderHandle
    {
        private static readonly Object OrderHandleLock = new Object();

        #region 支付订单处理
        /// <summary>
        /// 支付订单处理
        /// </summary>
        /// <param name="order"></param>
        public static bool FlwFoodSaleOrderHandle(Data_Order order, string paymentOrderNo)
        {
            bool flag = false;
            lock (OrderHandleLock)
            {
                if (MPOrderBusiness.UpdateOrderForPaySuccess(order.OrderID, paymentOrderNo))
                {
                    XCCloudWebBar.Common.LogHelper.SaveLog(TxtLogType.PPosPay, TxtLogContentType.Debug, TxtLogFileType.Day, "应用：莘拍档 订单号：" + order.OrderID + " 支付成功！");

                    OrderUnpaidModel unpaiOrder = null;
                    UnpaidOrderList.GetItem(order.OrderID, out unpaiOrder);
                    DateTime d = DateTime.Now;

                    if (unpaiOrder != null && unpaiOrder.CreateTime.AddMinutes(1) < d)
                    {
                        //对一分钟以内支付的订单进行处理
                        flag = FoodSale(order, unpaiOrder);
                        if (flag)
                        {
                            UnpaidOrderList.RemoveItem(unpaiOrder);
                            XCCloudWebBar.Common.LogHelper.SaveLog(TxtLogType.WeiXin, TxtLogContentType.Debug, TxtLogFileType.Day, "应用：莘拍档 订单号：" + order.OrderID + " 回调业务处理成功！");
                        }
                        else
                        {
                            XCCloudWebBar.Common.LogHelper.SaveLog(TxtLogType.WeiXin, TxtLogContentType.Debug, TxtLogFileType.Day, "应用：莘拍档 订单号：" + order.OrderID + " 回调业务处理失败！！！");
                        }
                    }
                    else
                    {
                        //超过一分钟支付的订单进行退款
                        PPosPayData.Refund pay = new PPosPayData.Refund();
                        string error = string.Empty;
                        pay.orderNo = order.TradeNo;
                        pay.tradeNo = DateTime.Now.ToString("yyyyMMddHHmmssffff") + order.StoreID;
                        pay.txnAmt = "";
                        PPosPayApi ppos = new PPosPayApi();
                        PPosPayData.RefundACK result = ppos.RefundPay(pay, out error);
                        if (result != null && result.returnCode == "000000")
                        {
                            XCCloudWebBar.Common.LogHelper.SaveLog(TxtLogType.WeiXin, TxtLogContentType.Debug, TxtLogFileType.Day, "应用：莘拍档 订单号：" + order.OrderID + " 退款成功！");

                            //退款成功后修改订单状态为已退款
                            MPOrderBusiness.UpdateOrderForRefund(order.OrderID, order.TradeNo);
                            UnpaidOrderList.RemoveItem(unpaiOrder);
                            flag = true;
                        }
                        else
                        {
                            XCCloudWebBar.Common.LogHelper.SaveLog(TxtLogType.WeiXin, TxtLogContentType.Debug, TxtLogFileType.Day, "应用：莘拍档 订单号：" + order.OrderID + " 退款失败！！！ 原因：" + error);
                            flag = false;
                        }
                    }
                }
                else
                {
                    XCCloudWebBar.Common.LogHelper.SaveLog(TxtLogType.PPosPay, TxtLogContentType.Debug, TxtLogFileType.Day, "应用：莘拍档 订单号：" + order.OrderID + " 已支付订单更新失败！！！");
                }
            }
            
            return flag;
        } 
        #endregion

        #region 订单处理
        public static bool FoodSale(Data_Order order, OrderUnpaidModel orderCache)
        {
            string errMsg = string.Empty;
            string segment = string.Empty;
            string mcuId = string.Empty;
            string xcGameDBName = string.Empty;
            int deviceIdentityId = 0;
            string storePassword = string.Empty;
            string storeName = string.Empty;
            string foodName = string.Empty;
            string deviceStoreId = string.Empty; //设备门店号
            string deviceId = string.Empty;

            string mobile = orderCache.MemberTokenModel.Mobile;

            int icCardId = 0;//会员
            int balance = 0;//币余额
            int memberLevelId = 0;//会员级别

            string paymentype = order.OrderType == 0 ? "微信" : "支付宝";

            XCGameManaDeviceStoreType deviceStoreType;

            if (order.BuyType == "购币")
            {
                //查询终端号是否存在         
                if (!ExtendBusiness.checkXCGameManaDeviceInfo(orderCache.DeviceToken, out deviceStoreType, out deviceStoreId, out deviceId))
                {
                    return false;
                }
                //验证订单的门店号和设备门店号
                if (!orderCache.MemberTokenModel.StoreId.Equals(deviceStoreId))
                {
                    return false;
                }
                //验证门店信息和设备状态是否为启用状态
                if (!ExtendBusiness.checkStoreDeviceInfo(deviceStoreType, deviceStoreId, deviceId, out segment, out mcuId, out xcGameDBName, out deviceIdentityId, out storePassword, out storeName, out errMsg))
                {
                    return false;
                }
                //验证雷达设备缓存状态
                if (!ExtendBusiness.checkRadarDeviceState(deviceStoreType, deviceStoreId, deviceId, out errMsg))
                {
                    return false;
                }
                //获取会员信息
                if (!ExtendBusiness.GetMemberInfo(deviceStoreType, mobile, xcGameDBName, out balance, out icCardId, out memberLevelId, out errMsg))
                {
                    return false;
                }
                //购币
                if (!BuyCoin(deviceStoreType, xcGameDBName, deviceStoreId, icCardId, memberLevelId, orderCache.FoodId, orderCache.OrderId, order.Price.ToString("0.00"), (int)order.Coins, balance, paymentype, deviceId, deviceIdentityId, out errMsg))
                {
                    return false;
                }

                //请求雷达处理出币
                if (!IConUtiltiy.DeviceOutputCoin(deviceStoreType, DevieControlTypeEnum.出币, deviceStoreId, mobile, icCardId, orderCache.OrderId, segment, mcuId, storePassword, orderCache.FoodId, (int)order.Coins, string.Empty, out errMsg))
                {
                    return false;
                }

                //设置推送消息的缓存结构
                //string form_id = dicParas.ContainsKey("form_id") ? dicParas["form_id"].ToString() : string.Empty;
                //MemberFoodSaleNotifyDataModel dataModel = new MemberFoodSaleNotifyDataModel("购币", storeName, mobile, foodName, foodNum, icCardId, decimal.Parse(money), coins);
                //SAppMessageMana.SetMemberCoinsMsgCacheData(SAppMessageType.MemberFoodSaleNotify, orderId, form_id, mobile, dataModel, out errMsg);
            }
            else if (order.BuyType == "充值")
            {
                StoreCacheModel storeModel = null;
                //验证门店
                StoreBusiness store = new StoreBusiness();
                if (!store.IsEffectiveStore(orderCache.MemberTokenModel.StoreId, out deviceStoreType, ref storeModel, out errMsg))
                {
                    return false;
                }
                //获取会员信息
                if (!ExtendBusiness.GetMemberInfo(deviceStoreType, mobile, storeModel.StoreDBName, out balance, out icCardId, out memberLevelId, out errMsg))
                {
                    return false;
                }
                //充值
                //LogHelper.SaveLog(TxtLogType.Api, TxtLogContentType.Debug, TxtLogFileType.Day, "Recharge:" + errMsg);
                if (!Recharge(deviceStoreType, mobile, storeModel.StoreDBName, orderCache.MemberTokenModel.StoreId, icCardId, memberLevelId, orderCache.FoodId, orderCache.OrderId, order.Price.ToString("0.00"), (int)order.Coins, balance, paymentype, deviceId, deviceIdentityId, out foodName, out errMsg))
                {
                    return false;
                }

                if (order.OrderType == 0 && !string.IsNullOrEmpty(orderCache.OpenId))
                {
                    MemberRechargeNotifyDataModel dataModel = new MemberRechargeNotifyDataModel();
                    dataModel.AccountType = "充值套餐";
                    dataModel.Account = order.Descript;
                    dataModel.Amount = order.Price.ToString("0.00");
                    dataModel.Status = "成功";
                    dataModel.Remark = "充值成功，祝您生活愉快！";
                    MessageMana.PushMessage(WeiXinMesageType.MemberRechargeNotify, orderCache.OpenId, dataModel, out errMsg);
                }
            }
            else
            {
                return false;
            }
            return true;
        } 
        #endregion

        #region 购币
        //购币
        static bool BuyCoin(XCGameManaDeviceStoreType deviceStoreType, string xcGameDBName, string storeId, int icCardId, int memberLevelId, int foodId, string orderId, string money, int coins, int balance, string paymentype, string deviceId, int deviceIdentityId, out string errMsg)
        {
            errMsg = string.Empty;
            if (deviceStoreType == XCGameManaDeviceStoreType.Store)
            {
                //验证套餐信息
                XCCloudWebBar.BLL.IBLL.XCGame.IFoodsService foodservice = BLLContainer.Resolve<XCCloudWebBar.BLL.IBLL.XCGame.IFoodsService>(xcGameDBName);
                var foodlist = foodservice.GetModels(p => p.FoodID == foodId).FirstOrDefault<XCCloudWebBar.Model.XCGame.t_foods>();
                if (foodlist == null)
                {
                    errMsg = "套餐不存在";
                    return false;
                }
                //验证班次信息
                XCCloudWebBar.BLL.IBLL.XCGame.IScheduleService scheduleService = BLLContainer.Resolve<XCCloudWebBar.BLL.IBLL.XCGame.IScheduleService>(xcGameDBName);
                var scheduleModel = scheduleService.GetModels(p => p.State.Equals("0", StringComparison.OrdinalIgnoreCase)).FirstOrDefault<XCCloudWebBar.Model.XCGame.flw_schedule>();
                if (scheduleModel == null)
                {
                    errMsg = "相关班次不存在";
                    return false;
                }

                int userID = Convert.ToInt32(scheduleModel.UserID);
                string scheduleId = scheduleModel.ID.ToString();
                string workStation = scheduleModel.WorkStation;
                string foodName = foodlist.FoodName;
                //获取套餐名称
                ///向数据库子表的food_sale插入数据
                string sql = "exec InsertFood @Balance,@ICCardID,@FoodID,@CoinQuantity,@Point,@MemberLevelID,@UserID,@ScheduleID,@WorkStation,@MacAddress,@OrderID,@FoodName,@Money,@Paymentype,@Return output ";
                SqlParameter[] parameters = new SqlParameter[15];
                parameters[0] = new SqlParameter("@Balance", balance);
                parameters[1] = new SqlParameter("@ICCardID", icCardId);
                parameters[2] = new SqlParameter("@FoodID", foodId);
                parameters[3] = new SqlParameter("@CoinQuantity", coins);
                parameters[4] = new SqlParameter("@Point", "0");
                parameters[5] = new SqlParameter("@MemberLevelID", memberLevelId);
                parameters[6] = new SqlParameter("@UserID", userID);
                parameters[7] = new SqlParameter("@ScheduleID", scheduleId);
                parameters[8] = new SqlParameter("@WorkStation", workStation);
                parameters[9] = new SqlParameter("@MacAddress", deviceId);
                parameters[10] = new SqlParameter("@OrderID", orderId);
                parameters[11] = new SqlParameter("@FoodName", foodName);
                parameters[12] = new SqlParameter("@Money", money);
                parameters[13] = new SqlParameter("@Paymentype", paymentype);
                parameters[14] = new SqlParameter("@Return", 0);
                parameters[14].Direction = System.Data.ParameterDirection.Output;
                XCCloudWebBar.BLL.IBLL.XCGame.IFoodsaleService foodsale = BLLContainer.Resolve<XCCloudWebBar.BLL.IBLL.XCGame.IFoodsaleService>(xcGameDBName);
                XCCloudWebBar.Model.XCGame.flw_food_sale member = foodsale.SqlQuery(sql, parameters).FirstOrDefault<XCCloudWebBar.Model.XCGame.flw_food_sale>();
                return true;
            }
            else if (deviceStoreType == XCGameManaDeviceStoreType.Merch)
            {
                XCCloudWebBar.BLL.IBLL.XCCloudRS232.IFoodSaleService foodsale = BLLContainer.Resolve<XCCloudWebBar.BLL.IBLL.XCCloudRS232.IFoodSaleService>();
                XCCloudWebBar.Model.XCCloudRS232.flw_food_sale flwFood = new XCCloudWebBar.Model.XCCloudRS232.flw_food_sale();
                flwFood.OrderID = orderId;
                flwFood.MerchID = int.Parse(storeId);
                flwFood.ICCardID = icCardId;
                flwFood.DeviceID = deviceIdentityId;
                flwFood.FlowType = 1;
                flwFood.CoinQuantity = coins;
                flwFood.TotalMoney = decimal.Parse(money);
                flwFood.Point = 0;
                flwFood.Balance = balance;
                flwFood.Note = string.Empty;
                flwFood.PayType = PayBusiness.GetPaymentTypeId(paymentype);
                flwFood.PayTime = System.DateTime.Now;
                flwFood.PayState = 1;
                flwFood.PayTotal = 0;
                foodsale.Add(flwFood);
                return true;
            }
            else
            {
                errMsg = "门店类型不正确";
                return false;
            }
        } 
        #endregion

        #region 充值
        /// <summary>
        /// 充值
        /// </summary>
        /// <param name="deviceStoreType"></param>
        /// <param name="mobile"></param>
        /// <param name="xcGameDBName"></param>
        /// <param name="storeId"></param>
        /// <param name="icCardId"></param>
        /// <param name="memberLevelId"></param>
        /// <param name="foodId"></param>
        /// <param name="orderId"></param>
        /// <param name="money"></param>
        /// <param name="coins"></param>
        /// <param name="balance"></param>
        /// <param name="paymentype"></param>
        /// <param name="deviceId"></param>
        /// <param name="deviceIdentityId"></param>
        /// <param name="foodName"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        private static bool Recharge(XCGameManaDeviceStoreType deviceStoreType, string mobile, string xcGameDBName, string storeId, int icCardId, int memberLevelId, int foodId, string orderId, string money, int coins, int balance, string paymentype, string deviceId, int deviceIdentityId, out string foodName, out string errMsg)
        {
            foodName = string.Empty;
            errMsg = string.Empty;
            balance += coins;
            if (deviceStoreType == XCGameManaDeviceStoreType.Store)
            {
                //验证套餐信息

                XCCloudWebBar.BLL.IBLL.XCGame.IFoodsService foodservice = BLLContainer.Resolve<XCCloudWebBar.BLL.IBLL.XCGame.IFoodsService>(xcGameDBName);
                var foodModel = foodservice.GetModels(p => p.FoodID == foodId).FirstOrDefault<XCCloudWebBar.Model.XCGame.t_foods>();
                if (foodModel == null)
                {
                    errMsg = "套餐明细不存在";
                    return false;
                }
                foodName = foodModel.FoodName;
                //验证班次信息
                XCCloudWebBar.BLL.IBLL.XCGame.IScheduleService scheduleService = BLLContainer.Resolve<XCCloudWebBar.BLL.IBLL.XCGame.IScheduleService>(xcGameDBName);
                var schedulelist = scheduleService.GetModels(p => p.State.Equals("0", StringComparison.OrdinalIgnoreCase)).FirstOrDefault<XCCloudWebBar.Model.XCGame.flw_schedule>();
                if (schedulelist == null)
                {
                    errMsg = "相关班次不存在";
                    return false;
                }

                string sql = "exec RechargeFood @Balance,@ICCardID,@FoodID,@CoinQuantity,@Point,@MemberLevelID,@UserID,@ScheduleID,@WorkStation,@MacAddress,@OrderID,@FoodName,@Money,@Paymentype,@Return output ";
                SqlParameter[] parameters = new SqlParameter[15];
                parameters[0] = new SqlParameter("@Balance", balance);
                parameters[1] = new SqlParameter("@ICCardID", icCardId);
                parameters[2] = new SqlParameter("@FoodID", foodId);
                parameters[3] = new SqlParameter("@CoinQuantity", coins);
                parameters[4] = new SqlParameter("@Point", "0");
                parameters[5] = new SqlParameter("@MemberLevelID", memberLevelId);
                parameters[6] = new SqlParameter("@UserID", Convert.ToInt32(schedulelist.UserID));
                parameters[7] = new SqlParameter("@ScheduleID", schedulelist.ID.ToString());
                parameters[8] = new SqlParameter("@WorkStation", schedulelist.WorkStation);
                parameters[9] = new SqlParameter("@MacAddress", "");
                parameters[10] = new SqlParameter("@OrderID", orderId);
                parameters[11] = new SqlParameter("@FoodName", foodModel.FoodName);
                parameters[12] = new SqlParameter("@Money", money);
                parameters[13] = new SqlParameter("@Paymentype", paymentype);
                parameters[14] = new SqlParameter("@Return", 0);
                parameters[14].Direction = System.Data.ParameterDirection.Output;
                XCCloudWebBar.BLL.IBLL.XCGame.IFoodsaleService foodsale = BLLContainer.Resolve<XCCloudWebBar.BLL.IBLL.XCGame.IFoodsaleService>(xcGameDBName);
                XCCloudWebBar.Model.XCGame.flw_food_sale member = foodsale.SqlQuery(sql, parameters).FirstOrDefault<XCCloudWebBar.Model.XCGame.flw_food_sale>();
            }
            else if (deviceStoreType == XCGameManaDeviceStoreType.Merch)
            {
                XCCloudWebBar.BLL.IBLL.XCCloudRS232.IFoodSaleService foodsale = BLLContainer.Resolve<XCCloudWebBar.BLL.IBLL.XCCloudRS232.IFoodSaleService>();
                XCCloudWebBar.Model.XCCloudRS232.flw_food_sale flwFood = new XCCloudWebBar.Model.XCCloudRS232.flw_food_sale();
                flwFood.OrderID = orderId;
                flwFood.MerchID = int.Parse(storeId);
                flwFood.ICCardID = icCardId;
                flwFood.DeviceID = deviceIdentityId;
                flwFood.FlowType = 1;
                flwFood.CoinQuantity = coins;
                flwFood.TotalMoney = decimal.Parse(money);
                flwFood.Point = 0;
                flwFood.Balance = balance;
                flwFood.Note = string.Empty;
                flwFood.PayType = PayBusiness.GetPaymentTypeId(paymentype);
                flwFood.PayTime = System.DateTime.Now;
                flwFood.PayState = 1;
                flwFood.PayTotal = 0;

                XCCloudWebBar.BLL.IBLL.XCCloudRS232.IMemberService memberService = BLLContainer.Resolve<XCCloudWebBar.BLL.IBLL.XCCloudRS232.IMemberService>();
                var memberModel = memberService.GetModels(p => p.Mobile.Equals(mobile, StringComparison.OrdinalIgnoreCase)).FirstOrDefault<XCCloudWebBar.Model.XCCloudRS232.t_member>();
                memberModel.Balance = Convert.ToInt32(memberModel.Balance) + coins;

                using (var transactionScope = new System.Transactions.TransactionScope(TransactionScopeOption.RequiresNew))
                {
                    foodsale.Add(flwFood);
                    memberService.Update(memberModel);
                    transactionScope.Complete();
                }

                return true;
            }
            else
            {
                errMsg = "门店类型不正确";
            }
            return true;
        }

        #endregion
    }
}