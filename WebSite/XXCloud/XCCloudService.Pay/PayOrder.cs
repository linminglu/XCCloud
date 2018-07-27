using Com.Alipay;
using Com.Alipay.Business;
using Com.Alipay.Domain;
using Com.Alipay.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.XCCloud;
using XCCloudService.Business.XCCloud;
using XCCloudService.Business.XCGameMana;
using XCCloudService.Common;
using XCCloudService.Common.Enum;
using XCCloudService.Model.CustomModel.XCCloud;
using XCCloudService.Model.XCCloud;
using XCCloudService.Pay.Alipay;
using XCCloudService.Pay.PPosPay;
using XCCloudService.Pay.WeiXinPay.Business;
using XCCloudService.Pay.WeiXinPay.Lib;

namespace XCCloudService.Pay
{
    public class PayOrderHelper
    {
        public static string CreateXCGameOrderNo(string storeId, decimal price, decimal fee, int orderType, string productName, string mobile, string buyType, int coins)
        {
            string orderNo = MPOrderBusiness.GetOrderNo(storeId, price, fee, orderType, productName, mobile, buyType, coins);
            return orderNo;
        }

        /// <summary>
        /// 订单支付业务处理异步方法
        /// </summary>
        /// <param name="orderId">订单号</param>
        /// <param name="authCode">授权码</param>
        /// <param name="callback">回调</param>
        public void BarcodePayAsync(string orderId, string authCode, Action<OrderPayResultModel> callback)
        {
            Func<OrderPayResultModel> func = () =>
            {
                try
                {
                    return BarcodePay(orderId, authCode);
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

        public OrderPayResultModel BarcodePay(string orderId, string authCode)
        {
            OrderPayResultModel resultModel = new OrderPayResultModel() { Result = false, PayResult = PayResultEnum.交易未知 };

            Flw_Order order = Flw_OrderService.I.GetModels(t => t.ID == orderId).FirstOrDefault();
            if (order == null)
            {
                resultModel.Message = "查询不到该订单";
                return resultModel;
            }

            if(!order.CheckVerifiction())
            {
                resultModel.Message = "订单有效性校验失败";
                return resultModel;
            }

            Base_StoreInfo store = Base_StoreInfoService.I.GetModels(t => t.ID == order.StoreID).FirstOrDefault();
            if(store == null)
            {
                resultModel.Message = "订单所属的门店不存在";
                return resultModel;
            }

            PaymentChannel payChannel;
            string head = authCode.Substring(0, 2);
            if (authCode.Length != 18)
            {
                resultModel.Message = "条码长度异常";
                return resultModel;
            }
            else
            {
                switch (head)
                {
                    case "10":
                    case "11":
                    case "12":
                    case "13":
                    case "14":
                    case "15":
                        //启动微信支付
                        payChannel = PaymentChannel.WXPAY;
                        break;
                    case "28":
                        //启动支付宝支付
                        payChannel = PaymentChannel.ALIPAY;
                        break;
                    default:
                        resultModel.Message = "条码识别错误";
                        return resultModel;
                }
            }

            if(store.SelttleType == null)
            {
                resultModel.Message = "门店支付结算方式为空";
                return resultModel;
            }

            SelttleType selttleType = (SelttleType)store.SelttleType;

            //订单减免金额
            decimal freePay = order.FreePay == null ? 0 : order.FreePay.Value;
            //计算订单实付金额，单位：元
            decimal amount = (decimal)order.PayCount - freePay;
            //订单描述
            string subject = !string.IsNullOrWhiteSpace(order.Note) ? order.Note : order.ID;

            Flw_OrderBusiness orderBusiness = new Flw_OrderBusiness();

            switch (selttleType)
            {
                case SelttleType.NotThird:
                    break;
                case SelttleType.AliWxPay: //微信支付宝官方通道
                    #region 微信支付宝官方通道
                    //if (payChannel == PaymentChannel.ALIPAY)//支付宝
                    //{
                    //    try
                    //    {
                    //        IAlipayTradeService serviceClient = F2FBiz.CreateClientInstance(AliPayConfig.serverUrl, AliPayConfig.appId, AliPayConfig.merchant_private_key, AliPayConfig.version,
                    //    AliPayConfig.sign_type, AliPayConfig.alipay_public_key, AliPayConfig.charset);

                    //        AliPayCommon alipay = new AliPayCommon();
                    //        AlipayTradePayContentBuilder builder = alipay.BuildPayContent(order, amount, subject, authCode);
                    //        //string out_trade_no = builder.out_trade_no;

                    //        AlipayF2FPayResult payResult = serviceClient.tradePay(builder);

                    //        if (payResult.Status == ResultEnum.SUCCESS)
                    //        {
                    //            decimal payAmount = Convert.ToDecimal(payResult.response.TotalAmount);

                    //            ////支付成功后的处理
                    //            bool ret = orderBusiness.OrderPay(payResult.response.OutTradeNo, payAmount, "", selttleType, PaymentChannel.ALIPAY, out errMsg);
                    //            return true;
                    //        }
                    //        else
                    //        {
                    //            LogHelper.SaveLog(TxtLogType.AliPay, payResult.response.SubMsg);
                    //        }
                    //    }
                    //    catch (Exception e)
                    //    {
                    //        LogHelper.SaveLog(TxtLogType.AliPay, e.Message);
                    //    }
                    //}
                    //else if (payChannel == PaymentChannel.WXPAY)
                    //{
                    //    try
                    //    {
                    //        MicroPay pay = new MicroPay();
                    //        WxPayData resultData = pay.BarcodePay(orderId, subject, amount, authCode);
                    //        string resule = resultData.GetValue("result_code").ToString();
                    //        if (resule == "SUCCESS")
                    //        {
                    //            string out_trade_no = resultData.GetValue("out_trade_no").ToString();
                    //            decimal total_fee = Convert.ToDecimal(resultData.GetValue("total_fee"));
                    //            decimal payAmount = total_fee / 100;

                    //            //支付成功后的处理
                    //            bool ret = orderBusiness.OrderPay(out_trade_no, payAmount, "", selttleType, PaymentChannel.WXPAY, out errMsg);

                    //            return true;
                    //        }
                    //        else
                    //        {
                    //            LogHelper.SaveLog(TxtLogType.WeiXinPay, resultData.GetValue("result_code").ToString());
                    //        }
                    //    }
                    //    catch (WxPayException ex)
                    //    {
                    //        LogHelper.SaveLog(TxtLogType.WeiXinPay, ex.Message);
                    //    }
                    //    catch (Exception e)
                    //    {
                    //        LogHelper.SaveLog(TxtLogType.WeiXinPay, e.Message);
                    //    }
                    //}
                    #endregion
                    break;
                case SelttleType.StarPos: //新大陆
                    #region 新大陆
                    PPosPayData.MicroPay pposOrder = new PPosPayData.MicroPay();
                    string tradeNo = order.ID;

                    pposOrder.tradeNo = tradeNo;
                    pposOrder.amount = Convert.ToInt32(amount * 100).ToString();
                    pposOrder.total_amount = Convert.ToInt32(amount * 100).ToString();
                    pposOrder.authCode = authCode;
                    pposOrder.payChannel = payChannel.ToString();
                    pposOrder.subject = subject;
                    pposOrder.selOrderNo = tradeNo;
                    pposOrder.txnTime = DateTime.Now.ToString("yyyyMMddHHmmss");
                    pposOrder.signValue = "";

                    string errMsg = string.Empty;
                    PPosPayApi ppos = new PPosPayApi();
                    PPosPayData.MicroPayACK result = ppos.ScanPay(pposOrder, out errMsg);
                    if (result != null)
                    {
                        if(result.result.ToUpper() == "S") 
                        {
                            //交易成功
                            //string out_trade_no = result.tradeNo;
                            //string channelOrderNo = result.orderNo;
                            //decimal total_fee = Convert.ToDecimal(result.total_amount);
                            //decimal payAmount = total_fee / 100;

                            //支付成功后的处理（订单处理业务在吧台完成）
                            //resultModel.Result = orderBusiness.OrderPay(out_trade_no, payAmount, channelOrderNo, selttleType, payChannel, out errMsg);

                            resultModel.Result = true;
                            resultModel.PayResult = PayResultEnum.交易成功;
                            resultModel.Message = errMsg;
                            return resultModel;
                        }
                        else if(result.result.ToUpper() == "F")
                        {
                            //交易失败
                            resultModel.PayResult = PayResultEnum.交易失败;
                            resultModel.Message = "交易失败";
                            return resultModel;
                        }
                        else
                        {
                            //交易结果未知
                            resultModel.Message = "等待支付";
                            return resultModel;
                        }
                    }
                    else
                    {
                        LogHelper.SaveLog(TxtLogType.PPosPay, errMsg);
                    }
                    #endregion
                    break;
            }

            resultModel.Message = "门店支付结算方式错误";
            return resultModel;
        }
    }
}
