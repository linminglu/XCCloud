using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.CommonBLL;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
using XCCloudService.BLL.XCCloud;
using XCCloudService.CacheService;
using XCCloudService.CacheService.XCCloud;
using XCCloudService.Common.Enum;
using XCCloudService.Common.Extensions;
using XCCloudService.Model.CustomModel.XCCloud;
using XCCloudService.Model.XCCloud;

namespace XCCloudService.Business.XCCloud
{
    public partial class Flw_OrderBusiness
    {
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

                string StoreID = order.StoreID;
                decimal PayCount = order.PayCount.HasValue ? order.PayCount.Value : 0; //应付金额
                decimal FreePay = order.FreePay.HasValue ? order.FreePay.Value : 0;   //减免金额
                decimal payAmount = PayCount - FreePay; //实际应支付金额

                order.PayTime = DateTime.Now;
                order.PayType = (int)payment;
                order.RealPay = amount;
                order.OrderNumber = channelOrderNo;

                if (payAmount == amount)
                {
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

        #region 订单退款处理
        /// <summary>
        /// 订单退款处理
        /// </summary>
        /// <param name="orderId">订单号</param>
        /// <param name="amount">实际支付金额</param>
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

                if(queryOrderDetail.Count > 0)
                {
                    foreach (var item in queryOrderDetail)
                    {
                        Flw_Food_Sale foodSale = Flw_Food_SaleService.I.GetModels(t => t.ID == item.FoodSaleId).FirstOrDefault();
                        if (foodSale != null)
                        {
                            decimal returnFee = 0m;
                            ReturnTaxModel returnModel = getRefundTax(foodSale.SingleType, foodSale.FoodID);
                            if(returnModel != null)
                            {
                                returnFee = returnModel.ReturnFee.HasValue ? returnModel.ReturnFee.Value : 0m;
                                if(returnModel.ReturnType == 1)
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
                            foodExit.FoodID = Convert.ToInt32(foodSale.FoodID);
                            foodExit.CardID = order.CardID;
                            foodExit.ExitFee = returnFee;
                            foodExit.TotalMoney = amount - returnFee;
                            if (!Flw_Food_ExitService.I.Add(foodExit))
                            {
                                return false;
                            }
                        }
                    }
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
