using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using XCCloudService.BLL.CommonBLL;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
using XCCloudService.BLL.XCCloud;
using XCCloudService.CacheService;
using XCCloudService.CacheService.XCCloud;
using XCCloudService.Common;
using XCCloudService.Common.Enum;
using XCCloudService.Common.Extensions;
using XCCloudService.Model.CustomModel.XCCloud;
using XCCloudService.Model.XCCloud;

namespace XCCloudService.Business.XCCloud
{
    public partial class Flw_OrderBusiness
    {
        /// <summary>
        /// 订单支付业务处理异步方法
        /// </summary>
        /// <param name="orderId">订单号</param>
        /// <param name="authCode">授权码</param>
        /// <param name="callback">回调</param>
        public void OrderPayAsync(string orderId, decimal amount, string channelOrderNo, SelttleType selttleType, PaymentChannel payment, Action<OrderPayResultModel> callback)
        {
            Func<OrderPayResultModel> func = () =>
            {
                try
                {
                    return OrderPayByCall(orderId, amount, channelOrderNo, selttleType, payment);
                }
                catch
                {
                    return null;
                }
            };//声明异步方法实现方式
            func.BeginInvoke((ar) =>
            {
                var result = func.EndInvoke(ar);//调用完毕执行的结果 
                callback.Invoke(result);//委托执行，回传结果值
            }, null);
        }

        public OrderPayResultModel OrderPayByCall(string orderId, decimal amount, string channelOrderNo, SelttleType selttleType, PaymentChannel payment)
        {
            OrderPayResultModel resultModel = new OrderPayResultModel() { PayResult = PayResultEnum.交易成功, Result = false, Message = string.Empty };
            string errMsg = string.Empty;
            resultModel.Result = OrderPay(orderId, amount, channelOrderNo, selttleType, payment, out errMsg);
            resultModel.Message = errMsg;
            return resultModel;
        }

        #region 订单支付成功处理
        /// <summary>
        /// 订单支付成功处理
        /// </summary>
        /// <param name="orderId">订单号</param>
        /// <param name="amount">实际支付金额</param>
        /// <param name="channelOrderNo">支付渠道订单号</param>
        /// <param name="selttleType">结算类型</param>
        /// <param name="selttleType">支付方式</param>
        /// <returns></returns>
        public bool OrderPay(string orderId, decimal amount, string channelOrderNo, SelttleType selttleType, PaymentChannel payment, out string errMsg)
        {
            errMsg = string.Empty;
            try
            {
                Flw_Order order = Flw_OrderService.I.GetModels(t => t.ID == orderId).FirstOrDefault();
                if (order == null)
                {
                    errMsg = "查询订单失败";
                    return false;
                }

                if(order.OrderStatus == 2 || order.OrderStatus == 3)
                {
                    return true;
                }

                string StoreID = order.StoreID;
                decimal PayCount = order.PayCount.HasValue ? order.PayCount.Value : 0; //应付金额
                decimal FreePay = order.FreePay.HasValue ? order.FreePay.Value : 0;   //减免金额
                decimal payAmount = PayCount - FreePay; //实际应支付金额

                DateTime now = DateTime.Now;

                order.PayTime = now;
                order.PayType = (int)payment;
                order.RealPay = amount;
                order.OrderNumber = channelOrderNo;

                if (payAmount == amount)
                {
                    if (order.CreateTime.Value.AddMinutes(1) < now)
                    {
                        order.OrderStatus = (int)OrderState.Alarm;
                        order.Note += " 支付超时";
                        errMsg = "支付超时";
                        Flw_OrderService.I.Update(order);
                        return false;
                    }

                    order.OrderStatus = (int)OrderState.AlreadyPaid;

                    decimal? fee = 0;
                    if (selttleType == SelttleType.StarPos)
                    {
                        var QuerySettleFee = from a in Base_StoreInfoService.N.GetModels(t => t.ID == StoreID)
                                             join b in Base_SettlePPOSService.N.GetModels() on a.SettleID equals b.ID
                                             select new
                                             {
                                                 StoreId = a.ID,
                                                 SettleFee = b.SettleFee
                                             };
                        var settleFee = QuerySettleFee.FirstOrDefault();
                        if (settleFee != null)
                        {
                            fee = settleFee.SettleFee;
                        }
                    }
                    else if (selttleType == SelttleType.LcswPay)
                    {
                        var QuerySettleFee = from a in Base_StoreInfoService.N.GetModels(t => t.ID == StoreID)
                                             join b in Base_SettleLCPayService.N.GetModels() on a.SettleID equals b.ID
                                             select new
                                             {
                                                 StoreId = a.ID,
                                                 SettleFee = b.SettleFee
                                             };
                        var settleFee = QuerySettleFee.FirstOrDefault();
                        if (settleFee != null)
                        {
                            fee = settleFee.SettleFee;
                        }
                    }
                    else
                    {
                        var QuerySettleFee = from a in Base_StoreInfoService.N.GetModels(t => t.ID == StoreID)
                                             join b in Base_SettleOrgService.N.GetModels() on a.SettleID equals b.ID
                                             select new
                                             {
                                                 StoreId = a.ID,
                                                 SettleFee = b.SettleFee
                                             };
                        var settleFee = QuerySettleFee.FirstOrDefault();
                        if (settleFee != null)
                        {
                            fee = settleFee.SettleFee;
                        }
                    }

                    decimal payFee = 0m;
                    if (fee > 0)
                    {
                        payFee = Math.Round((decimal)(payAmount * fee), 2, MidpointRounding.AwayFromZero);
                        if (payFee < 0.01m) payFee = 0.01m; //最小单位为0.01元
                    }
                    order.PayFee = payFee;
                }
                else
                {
                    errMsg = "支付金额异常";
                    //支付异常
                    order.OrderStatus = (int)OrderState.Alarm;
                }

                if (!Flw_OrderService.I.Update(order))
                {
                    errMsg = "订单更新失败";
                    return false;
                }
            }
            catch(Exception ex)
            {
                errMsg = ex.Message;
                return false;
            }
            return true;
        } 
        #endregion

        #region H5订单支付成功处理
        /// <summary>
        /// H5订单支付成功处理
        /// </summary>
        /// <param name="orderId">订单号</param>
        /// <param name="amount">实际支付金额</param>
        /// <param name="channelOrderNo">支付渠道订单号</param>
        /// <param name="selttleType">结算类型</param>
        /// <param name="selttleType">支付方式</param>
        /// <returns></returns>
        public bool H5OrderPay(string orderId, decimal amount, string channelOrderNo, SelttleType selttleType, PaymentChannel payment, out OrderHandleEnum handle, out string coinRuleId, out string errMsg)
        {
            handle = OrderHandleEnum.充值;
            coinRuleId = string.Empty;
            errMsg = string.Empty;
            try
            {
                Flw_Order order = Flw_OrderService.I.GetModels(t => t.ID == orderId).FirstOrDefault();
                if (order == null)
                {
                    errMsg = "查询订单失败";
                    return false;
                }

                if (order.OrderStatus == 2 || order.OrderStatus == 3)
                {
                    return false;
                }

                string StoreID = order.StoreID;
                decimal PayCount = order.PayCount.HasValue ? order.PayCount.Value : 0; //应付金额
                decimal FreePay = order.FreePay.HasValue ? order.FreePay.Value : 0;   //减免金额
                decimal payAmount = PayCount - FreePay; //实际应支付金额

                DateTime now = DateTime.Now;

                order.PayTime = now;
                order.PayType = (int)payment;
                order.RealPay = amount;
                order.OrderNumber = channelOrderNo;

                Flw_Order_Detail orderDetail = null;

                if (payAmount == amount)
                {
                    if (order.CreateTime.Value.AddMinutes(1) < now)
                    {
                        order.OrderStatus = (int)OrderState.Alarm;
                        order.Note += " 支付超时";
                        errMsg = "支付超时";
                        Flw_OrderService.I.Update(order);
                        return false;
                    }

                    order.OrderStatus = (int)OrderState.AlreadyPaid;

                    decimal? fee = 0;
                    #region 计算手续费
                    if (selttleType == SelttleType.StarPos)
                    {
                        var QuerySettleFee = from a in Base_StoreInfoService.N.GetModels(t => t.ID == StoreID)
                                             join b in Base_SettlePPOSService.N.GetModels() on a.SettleID equals b.ID
                                             select new
                                             {
                                                 StoreId = a.ID,
                                                 SettleFee = b.SettleFee
                                             };
                        var settleFee = QuerySettleFee.FirstOrDefault();
                        if (settleFee != null)
                        {
                            fee = settleFee.SettleFee;
                        }
                    }
                    else if (selttleType == SelttleType.LcswPay)
                    {
                        var QuerySettleFee = from a in Base_StoreInfoService.N.GetModels(t => t.ID == StoreID)
                                             join b in Base_SettleLCPayService.N.GetModels() on a.SettleID equals b.ID
                                             select new
                                             {
                                                 StoreId = a.ID,
                                                 SettleFee = b.SettleFee
                                             };
                        var settleFee = QuerySettleFee.FirstOrDefault();
                        if (settleFee != null)
                        {
                            fee = settleFee.SettleFee;
                        }
                    }
                    else
                    {
                        var QuerySettleFee = from a in Base_StoreInfoService.N.GetModels(t => t.ID == StoreID)
                                             join b in Base_SettleOrgService.N.GetModels() on a.SettleID equals b.ID
                                             select new
                                             {
                                                 StoreId = a.ID,
                                                 SettleFee = b.SettleFee
                                             };
                        var settleFee = QuerySettleFee.FirstOrDefault();
                        if (settleFee != null)
                        {
                            fee = settleFee.SettleFee;
                        }
                    }

                    decimal payFee = 0m;
                    if (fee > 0)
                    {
                        payFee = Math.Round((decimal)(payAmount * fee), 2, MidpointRounding.AwayFromZero);
                        if (payFee < 0.01m) payFee = 0.01m; //最小单位为0.01元
                    } 
                    #endregion
                    order.PayFee = payFee;

                    //订单详情
                    orderDetail = Flw_Order_DetailService.I.GetModels(t => t.OrderFlwID == orderId).FirstOrDefault();
                }
                else
                {
                    errMsg = "支付金额异常";
                    //支付异常
                    order.OrderStatus = (int)OrderState.Alarm;
                }

                Base_MemberInfo member = Base_MemberInfoService.I.GetModels(t => t.ID == order.MemberID).FirstOrDefault();
                Data_Member_Card memberCard = Data_Member_CardService.I.GetModels(t => t.ID == order.CardID).FirstOrDefault();
                Data_MemberLevel level = Data_MemberLevelService.I.GetModels(t => t.ID == memberCard.MemberLevelID).FirstOrDefault();
                //当前班次
                Flw_Schedule schedule = Flw_ScheduleService.I.GetModels(t => t.StoreID == order.StoreID && t.State == 1).FirstOrDefault();

                using (TransactionScope ts = new TransactionScope(TransactionScopeOption.RequiresNew))
                {
                    if (!Flw_OrderService.I.Update(order))
                    {
                        errMsg = "订单更新失败";
                        return false;
                    }

                    if (orderDetail != null)
                    {
                        Flw_Food_Sale sale = Flw_Food_SaleService.I.GetModels(t => t.ID == orderDetail.FoodFlwID).FirstOrDefault();
                        if (sale != null)
                        {
                            switch (sale.SingleType)
                            {
                                case 0://套餐
                                    int foodId = sale.FoodID.Toint(0);
                                    var foodDetail = Data_Food_DetialService.I.GetModels(t => t.FoodID == foodId).ToList();
                                    foreach (var detail in foodDetail)
                                    {
                                        if (detail.OperateType == 1)
                                        {
                                            switch (detail.FoodType)
                                            {
                                                case 0://余额
                                                    var balance = Data_Card_BalanceService.I.GetModels(t => t.CardIndex == order.CardID && t.BalanceIndex == detail.ContainID).FirstOrDefault();
                                                    int? quantity = sale.SaleCount * detail.ContainCount;
                                                    balance.Balance += quantity;
                                                    if (!Data_Card_BalanceService.I.Update(balance))
                                                    {
                                                        return false;
                                                    }

                                                    var balanceFree = Data_Card_Balance_FreeService.I.GetModels(t => t.CardIndex == order.CardID && t.BalanceIndex == detail.ContainID).FirstOrDefault();

                                                    //记录余额变化流水
                                                    Flw_MemberData fmd = new Flw_MemberData();
                                                    fmd.ID = RedisCacheHelper.CreateCloudSerialNo(order.StoreID);
                                                    fmd.MerchID = order.MerchID;
                                                    fmd.StoreID = order.StoreID;
                                                    fmd.MemberID = order.MemberID;
                                                    fmd.MemberName = member.UserName;
                                                    fmd.CardIndex = memberCard.ID;
                                                    fmd.ICCardID = memberCard.ICCardID;
                                                    fmd.MemberLevelName = level.MemberLevelName;
                                                    fmd.ChannelType = (int)MemberDataChannelType.莘宸云;
                                                    fmd.OperationType = (int)MemberDataOperationType.售币;
                                                    fmd.OPTime = DateTime.Now;
                                                    fmd.SourceType = 10;
                                                    fmd.SourceID = order.ID;
                                                    fmd.BalanceIndex = balance.BalanceIndex;
                                                    fmd.ChangeValue = 0 + quantity;
                                                    fmd.Balance = balance.Balance;
                                                    fmd.FreeChangeValue = 0;
                                                    fmd.FreeBalance = balanceFree == null ? 0 : balanceFree.Balance.Todecimal(0);
                                                    fmd.BalanceTotal = fmd.Balance + fmd.FreeBalance;
                                                    fmd.Note = "充值套餐";
                                                    fmd.ScheduleID = schedule.ID;
                                                    fmd.WorkStation = "手机自助";
                                                    fmd.CheckDate = schedule.CheckDate;
                                                    if (!Flw_MemberDataService.I.Add(fmd))
                                                    {
                                                        return false;
                                                    }
                                                    break;
                                                case 4:
                                                    var couponList = Data_CouponListService.I.GetModels(t => t.CouponID == detail.ContainID && t.StoreID == order.StoreID && t.State == 2 && t.IsLock == 0 && t.JackpotID == 0);
                                                    foreach (var coupon in couponList)
                                                    {
                                                        if (coupon.CheckVerifiction())
                                                        {
                                                            coupon.MemberID = member.ID;
                                                            if (!Data_CouponListService.I.Update(coupon))
                                                            {
                                                                return false;
                                                            }
                                                            break;
                                                        }
                                                    }
                                                    break;
                                            }
                                        }
                                    }
                                    break;
                                case 7:
                                    //散客投币
                                    handle = OrderHandleEnum.散客投币;
                                    coinRuleId = sale.FoodID;
                                    break;
                                case 8:
                                    //会员投币
                                    handle = OrderHandleEnum.会员投币;
                                    coinRuleId = sale.FoodID;
                                    break;
                            }
                        }
                    }
                    ts.Complete();
                }
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
                return false;
            }
            return true;
        }
        #endregion

        #region 订单退款处理
        /// <summary>
        /// 订单退款处理
        /// </summary>
        /// <param name="orderId">订单号</param>
        /// <param name="amount">退款金额</param>
        /// <returns></returns>
        public bool OrderRefundPay(string orderId, decimal amount)
        {
            try
            {
                Flw_Order order = Flw_OrderService.I.GetModels(t => t.ID == orderId).FirstOrDefault();
                if (order == null)
                {
                    return false;
                }

                Base_StoreInfo store = Base_StoreInfoService.I.GetModels(t=>t.ID == order.StoreID).FirstOrDefault();
                if(store == null)
                {
                    return false;
                }

                var queryOrderDetail = Flw_Order_DetailService.I.GetModels(t=>t.OrderFlwID == orderId).GroupBy(t=> t.FoodFlwID).Select(t=> new 
                    {
                        FoodSaleId = t.Key
                    }).ToList();

                using (TransactionScope ts = new TransactionScope(TransactionScopeOption.RequiresNew))
                {
                    if (queryOrderDetail.Count > 0)
                    {
                        foreach (var item in queryOrderDetail)
                        {
                            Flw_Food_Sale foodSale = Flw_Food_SaleService.I.GetModels(t => t.ID == item.FoodSaleId).FirstOrDefault();
                            if (foodSale != null)
                            {
                                decimal returnFee = 0m;
                                ReturnTaxModel returnModel = getRefundTax(foodSale.SingleType, foodSale.FoodID);
                                if (returnModel != null)
                                {
                                    returnFee = returnModel.ReturnFee.HasValue ? returnModel.ReturnFee.Value : 0m;
                                    if (returnModel.ReturnType == 1)
                                    {
                                        returnFee = amount * returnModel.ReturnFee.Value / 100;
                                        returnFee = Math.Round(returnFee, 2, MidpointRounding.AwayFromZero);
                                    }
                                }

                                Flw_Food_Exit foodExit = new Flw_Food_Exit();
                                foodExit.ID = RedisCacheHelper.CreateCloudSerialNo(order.StoreID);
                                foodExit.MerchID = store.MerchID;
                                foodExit.StoreID = store.ID;
                                foodExit.OrderID = orderId;
                                foodExit.FoodID = foodSale.FoodID;
                                foodExit.CardID = order.CardID;
                                foodExit.ExitFee = returnFee;
                                foodExit.TotalMoney = amount - returnFee;
                                foodExit.Note = "订单退款";
                                foodExit.RealTime = DateTime.Now;
                                if (!Flw_Food_ExitService.I.Add(foodExit))
                                {
                                    LogHelper.SaveLog(TxtLogType.PPosPay, "添加退款记录失败");
                                    return false;
                                }
                            }
                        }
                    }

                    order.ExitPrice = amount;
                    order.ExitFee = 0;
                    order.ExitTime = DateTime.Now;
                    if (!Flw_OrderService.I.Update(order))
                    {
                        LogHelper.SaveLog(TxtLogType.PPosPay, string.Format("新大陆退款：订单更新失败，订单号：{0}，退款金额：{1}", orderId, amount));
                        return false;
                    }
                    ts.Complete();
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        ReturnTaxModel getRefundTax(int? singleType, string singleId)
        {
            ReturnTaxModel model = null;
            switch (singleType)
            {
                case 0:
                    int foodId = singleId.Toint(0);
                    Data_FoodInfo foodInfo = Data_FoodInfoService.I.GetModels(t => t.ID == foodId).FirstOrDefault();
                    if (foodInfo != null)
                    {
                        model = new ReturnTaxModel();
                        model.ReturnFee = foodInfo.ReturnFee;
                        model.ReturnType = foodInfo.FeeType;
                    }
                    break;
                case 1:
                    int ticketId = singleId.Toint(0);
                    Data_ProjectTicket ticket = Data_ProjectTicketService.I.GetModels(t => t.ID == ticketId).FirstOrDefault();
                    if (ticket != null)
                    {
                        model = new ReturnTaxModel();
                        model.ReturnFee = ticket.ExitTicketValue;
                        model.ReturnType = ticket.ExitTicketType;
                    }
                    break;
                case 2:
                    int goodId = singleId.Toint(0);
                    Base_GoodsInfo goodInfo = Base_GoodsInfoService.I.GetModels(t => t.ID == goodId).FirstOrDefault();
                    if (goodInfo != null)
                    {
                        model = new ReturnTaxModel();
                        model.ReturnFee = goodInfo.ReturnFee;
                        model.ReturnType = goodInfo.FeeType;
                    }
                    break;
            }
            return model;
        }
        #endregion
    }

    public class ReturnTaxModel
    {
        public decimal? ReturnFee { get; set; }
        public int? ReturnType { get; set; }
    }
}
