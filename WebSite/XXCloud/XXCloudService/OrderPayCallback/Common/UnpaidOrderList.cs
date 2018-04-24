using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using XCCloudService.Business.XCGameMana;
using XCCloudService.Common;
using XCCloudService.Common.Enum;
using XCCloudService.Model.XCGameManager;
using XCCloudService.Pay.PPosPay;

namespace XXCloudService.OrderPayCallback.Common
{
    public class UnpaidOrderList
    {
        private static readonly Object UnpaidOrderLock = new Object();

        static List<OrderUnpaidModel> unpaidOrderList = new List<OrderUnpaidModel>();

        #region 添加
        /// <summary>
        /// 收到新的未支付订单
        /// </summary>
        /// <param name="item">订单</param>
        public static void AddNewItem(OrderUnpaidModel item)
        {
            unpaidOrderList.Add(item);

            ClearTimeout();
        } 
        #endregion

        #region 清除过期数据
        static void ClearTimeout()
        {
            lock (UnpaidOrderLock)
            {
                DateTime d = DateTime.Now;
            continueline:
                foreach (OrderUnpaidModel item in unpaidOrderList)
                {
                    if (item.CreateTime.AddMinutes(1) < d)
                    {
                        unpaidOrderList.Remove(item);
                        goto continueline;
                    }
                }
            }
        } 
        #endregion

        #region 获取元素
        public static bool GetItem(string orderId, out OrderUnpaidModel item)
        {
            item = null;
            lock (UnpaidOrderLock)
            {
                item = unpaidOrderList.Where(p => p.OrderId == orderId).FirstOrDefault();
                if (item != null)
                {
                    unpaidOrderList.Remove(item);
                    return true;
                }
                else
                    return false;
            }
        } 
        #endregion

        static Thread thRun1, thRun2;
        public static void Init()
        {
            thRun1 = new Thread(new ThreadStart(Run));
            thRun1.Name = "一分钟内未支付订单查询线程";
            thRun1.IsBackground = true;
            thRun1.Start();

            thRun2 = new Thread(new ThreadStart(Query));
            thRun2.Name = "超过一分钟未支付订单查询线程";
            thRun2.IsBackground = true;
            thRun2.Start();
        }

        private static void Run()
        {
            //每10秒查询1分钟以内数据
            while (true)
            {
                try
                {
                    DateTime d = DateTime.Now;
                    List<OrderUnpaidModel> list = unpaidOrderList.Where(t => t.CreateTime.AddMinutes(1) <= d).ToList();
                    if(list.Count > 0)
                    {
                        PPosPayData.QueryOrder pay = new PPosPayData.QueryOrder();
                        PPosPayApi ppos = null;
                        foreach (OrderUnpaidModel item in list)
                        {
                            string error = string.Empty;
                            //pay.tradeNo = item.CreateTime.ToString("yyyyMMddHHmmssffff");
                            pay.tradeNo = item.OrderId;
                            pay.qryNo = item.OrderId;
                            ppos = new PPosPayApi();
                            PPosPayData.QueryOrderACK ack = ppos.QueryOrderPay(pay, out error);
                            if (ack.result == "s")
                            {
                                Data_Order order = MPOrderBusiness.GetOrderModel(item.OrderId);
                                OrderHandle.FlwFoodSaleOrderHandle(order, ack.orderNo);

                                //移除已查询完成的缓存项
                                unpaidOrderList.Remove(item);
                            }
                        }                        
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.SaveLog(TxtLogType.PPosPay, TxtLogContentType.Debug, TxtLogFileType.Day, "未支付订单查询错误！ " + ex.Message);
                }

                Thread.Sleep(10 * 1000);
            }
        }

        private static void Query()
        {
            //每10秒查询1分钟以内数据
            while (true)
            {
                try
                {
                    DateTime d = DateTime.Now;
                    List<OrderUnpaidModel> list = unpaidOrderList.Where(t => t.CreateTime.AddMinutes(1) > d).ToList();
                    if (list.Count > 0)
                    {
                        PPosPayData.QueryOrder pay = new PPosPayData.QueryOrder();
                        PPosPayApi ppos = null;
                        foreach (OrderUnpaidModel item in list)
                        {
                            string error = string.Empty;
                            //pay.tradeNo = item.CreateTime.ToString("yyyyMMddHHmmssffff");
                            pay.tradeNo = item.OrderId;
                            pay.qryNo = item.OrderId;
                            ppos = new PPosPayApi();
                            PPosPayData.QueryOrderACK ack = ppos.QueryOrderPay(pay, out error);
                            if (ack != null && ack.result == "s")
                            {
                                PPosPayData.Refund refund = new PPosPayData.Refund();
                                refund.orderNo = ack.orderNo;
                                refund.tradeNo = item.OrderId;
                                refund.txnAmt = "";
                                PPosPayData.RefundACK result = ppos.RefundPay(refund, out error);
                                if(result!= null && result.returnCode == "000000")
                                {
                                    //退款成功后修改订单状态为已退款
                                    MPOrderBusiness.UpdateOrderForRefund(item.OrderId, ack.orderNo);
                                }
                                //移除已查询完成的缓存项
                                unpaidOrderList.Remove(item);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.SaveLog(TxtLogType.PPosPay, TxtLogContentType.Debug, TxtLogFileType.Day, "未支付订单查询错误！ " + ex.Message);
                }

                Thread.Sleep(10 * 1000);
            }
        }
    }

    #region 待支付订单model
    /// <summary>
    /// 待支付订单元素
    /// </summary>
    public class OrderUnpaidModel
    {
        /// <summary>
        /// 订单ID
        /// </summary>
        public string OrderId { get; set; }

        /// <summary>
        /// 设备token
        /// </summary>
        public string DeviceToken { get; set; }

        /// <summary>
        /// 套餐ID
        /// </summary>
        public int FoodId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    } 
    #endregion
}