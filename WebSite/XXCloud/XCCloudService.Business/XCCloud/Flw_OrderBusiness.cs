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
        /// <param name="selttleType">支付方式</param>
        /// <returns></returns>
        public static BarcodePayModel OrderPay(string orderId, decimal amount, SelttleType selttleType)
        {
            BarcodePayModel model = null;
            try
            {
                Flw_Order order = Flw_OrderService.I.GetModels(t => t.ID == orderId).FirstOrDefault();
                if (order != null)
                {
                    model = new BarcodePayModel();
                    model.OrderId = orderId;
                    model.PayAmount = amount.ToString("0.00");

                    string payTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    string StoreID = order.StoreID;
                    decimal PayCount = order.PayCount != null ? (decimal)order.PayCount : 0; //应付金额
                    decimal FreePay = order.FreePay != null ? (decimal)order.FreePay : 0;   //减免金额
                    decimal payAmount = PayCount - FreePay; //实际应支付金额

                    if (payAmount == amount)
                    {
                        string sql = string.Format("update Flw_Order set OrderStatus={3}, RealPay='{0}', PayTime='{1}' where ID='{2}'", payAmount.ToString("0.00"), payTime, orderId, OrderState.AlreadyPaid);
                        XCCloudBLL.ExecuteSql(sql);

                        model.OrderStatus = OrderState.AlreadyPaid;

                        if (selttleType == SelttleType.StarPos)
                        {
                            sql = @"SELECT a.ID as StoreID, SettleFee FROM Base_StoreInfo a
                                INNER JOIN Flw_Order b ON b.StoreID = a.ID
                                INNER JOIN Base_SettlePPOS c ON c.ID = a.SettleID
                                WHERE b.ID = '" + orderId + "'";
                        }
                        else if (selttleType == SelttleType.LcswPay)
                        {
                            sql = @"SELECT a.ID as StoreID, SettleFee FROM Base_StoreInfo a
                                INNER JOIN Flw_Order b ON b.StoreID = a.ID
                                INNER JOIN Base_SettleLCPay c ON c.ID = a.SettleID
                                WHERE b.ID = '" + orderId + "'";
                        }
                        else
                        {
                            sql = "SELECT SettleFee FROM Base_StoreInfo a, Base_SettleOrg b WHERE a.SettleID = b.ID AND a.ID = '" + StoreID + "'";
                        }

                        DataTable dt = XCCloudBLL.ExecuterSqlToTable(sql);
                        if (dt.Rows.Count > 0)
                        {
                            //获取当前结算费率，计算手续费
                            double fee = Convert.ToDouble(dt.Rows[0]["SettleFee"]);
                            double d = Math.Round(Convert.ToDouble(payAmount) * fee, 2, MidpointRounding.AwayFromZero);   //最小单位为0.01元
                            if (d < 0.01) d = 0.01;
                            sql = "update Flw_Order set PayFee='" + d + "' where ID='" + orderId + "'";
                            XCCloudBLL.ExecuteSql(sql);
                        }
                    }
                    else
                    {
                        //支付异常
                        //PayList.AddNewItem(out_trade_no, amount);
                        string sql = string.Format("update Flw_Order set OrderStatus={3}, RealPay='{0}', PayTime='{1}' where ID='{2}'", amount.ToString("0.00"), payTime, orderId, OrderState.Alarm);
                        XCCloudBLL.ExecuteSql(sql);

                        model.OrderStatus = OrderState.Alarm;
                    }

                    AddOrderPayCache(orderId, amount, payTime, model.OrderStatus, selttleType);
                }
            }
            catch
            {
                return model;
            }
            return model;
        } 
        #endregion

        public static void AddOrderPayCache(string orderId, decimal amount, string payTime, OrderState payState, SelttleType payType)
        {
            OrderPayCacheModel orderPay = new OrderPayCacheModel();
            orderPay.OrderId = orderId;
            orderPay.PayAmount = amount;
            orderPay.PayTime = payTime;
            orderPay.PayState = payState;
            orderPay.PayType = payType;
            OrderPayCache.Add<OrderPayCacheModel>(orderId, orderPay, CacheExpires.OrderPayCacheExpiresTime);
        }        
    }
}
