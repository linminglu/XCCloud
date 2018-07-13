﻿using Com.Alipay;
using Com.Alipay.Business;
using Com.Alipay.Domain;
using Com.Alipay.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml.XPath;
using System.Xml.Linq;
using XCCloudWebBar.Base;
using XCCloudWebBar.BLL.XCCloud;
using XCCloudWebBar.Common;
using XCCloudWebBar.Common.Enum;
using XCCloudWebBar.Common.Extensions;
using XCCloudWebBar.Model.CustomModel.XCCloud;
using XCCloudWebBar.Model.XCCloud;
using XCCloudWebBar.Pay.Alipay;
using XCCloudWebBar.Pay.DinPay;
using XCCloudWebBar.Pay.LcswPay;
using XCCloudWebBar.Pay.PPosPay;
using XCCloudWebBar.Pay.WeiXinPay.Business;
using XCCloudWebBar.Pay.WeiXinPay.Lib;
using XXCloudService.Api;
using Aop.Api;
using XCCloudWebBar.Business.XCGameMana;
using XCCloudWebBar.Model.XCGameManager;
using XCCloudWebBar.CacheService;
using XCCloudWebBar.Pay;
using Aop.Api.Domain;
using Aop.Api.Request;
using Aop.Api.Response;
using XCCloudWebBar.Model.CustomModel.XCGameManager;
using XCCloudWebBar.CacheService.XCCloud;
using XCCloudWebBar.OrderPayCallback.Common;
using XCCloudWebBar.BLL.IBLL.XCCloud;
using XCCloudWebBar.BLL.Container;
using XCCloudWebBar.Model.CustomModel.XCGame;
using XCCloudWebBar.Business.XCCloud;
using XCCloudWebBar.Model.WeiXin;

namespace XXCloudService.Api
{
    /// <summary>
    /// Payment 的摘要说明
    /// </summary>
    public class Payment : ApiBase
    {
        #region 获取支付二维码
        /// <summary>
        /// 获取支付二维码
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MethodToken)]
        public object getPayQRcode(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string orderId = dicParas.ContainsKey("orderId") ? dicParas["orderId"].ToString() : string.Empty;
                string strPayChannel = dicParas.ContainsKey("payChannel") ? dicParas["payChannel"].ToString() : string.Empty;
                string payType = dicParas.ContainsKey("payType") ? dicParas["payType"].ToString() : string.Empty;
                string subject = dicParas.ContainsKey("subject") ? dicParas["subject"].ToString() : string.Empty;

                if (string.IsNullOrWhiteSpace(orderId))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "订单号无效");
                }

                if (string.IsNullOrWhiteSpace(payType))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "支付方式为空");
                }
                //支付方式
                PaymentChannel PayChannel = (PaymentChannel)Convert.ToInt32(payType);

                Flw_Order order = Flw_OrderBusiness.GetOrderModel(orderId);
                if (order == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "订单号无效");
                }

                var storeId = order.StoreID;
                IBase_StoreInfoService base_StoreInfoService = BLLContainer.Resolve<IBase_StoreInfoService>();
                if (!base_StoreInfoService.Any(p => p.ID.Equals(storeId, StringComparison.OrdinalIgnoreCase)))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "订单所属门店无效");
                }

                //订单减免金额
                decimal freePay = order.FreePay == null ? 0 : order.FreePay.Value;
                //计算订单实付金额，单位：元 
                decimal amount = (decimal)order.PayCount - freePay;

                PayQRcodeModel model = new PayQRcodeModel();
                model.OrderId = orderId;

                //SelttleType selttleType = (SelttleType)store.SelttleType.Value;
                SelttleType selttleType = (SelttleType)Convert.ToInt32(strPayChannel);
                switch (selttleType)
                {
                    case SelttleType.NotThird:
                        break;
                    case SelttleType.AliWxPay: //微信支付宝官方通道
                        {
                            #region 支付宝、微信
                            if (PayChannel == PaymentChannel.ALIPAY)//支付宝
                            {
                                try
                                {
                                    IAlipayTradeService serviceClient = F2FBiz.CreateClientInstance(AliPayConfig.serverUrl, AliPayConfig.appId, AliPayConfig.merchant_private_key, AliPayConfig.version,
                                AliPayConfig.sign_type, AliPayConfig.alipay_public_key, AliPayConfig.charset);

                                    AliPayCommon alipay = new AliPayCommon();
                                    AlipayTradePrecreateContentBuilder builder = alipay.BuildPrecreateContent(order, amount, subject);
                                    //如果需要接收扫码支付异步通知，那么请把下面两行注释代替本行。
                                    //推荐使用轮询撤销机制，不推荐使用异步通知,避免单边账问题发生。
                                    //AlipayF2FPrecreateResult precreateResult = serviceClient.tradePrecreate(builder);
                                    //AliPayConfig.notify_url  //商户接收异步通知的地址
                                    AlipayF2FPrecreateResult precreateResult = serviceClient.tradePrecreate(builder, AliPayConfig.notify_url);

                                    switch (precreateResult.Status)
                                    {
                                        case ResultEnum.SUCCESS:
                                            model.QRcode = precreateResult.response.QrCode;
                                            break;
                                        case ResultEnum.FAILED:
                                            LogHelper.SaveLog(TxtLogType.AliPay, precreateResult.response.Body);
                                            return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, precreateResult.response.Body);
                                        case ResultEnum.UNKNOWN:
                                            if (precreateResult.response == null)
                                            {
                                                LogHelper.SaveLog(TxtLogType.AliPay, "配置或网络异常，请检查后重试");
                                                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "获取支付二维码失败");
                                            }
                                            else
                                            {
                                                LogHelper.SaveLog(TxtLogType.AliPay, "系统异常，请更新外部订单后重新发起请求");
                                                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "获取支付二维码失败");
                                            }
                                    }
                                }
                                catch (Exception e)
                                {
                                    LogHelper.SaveLog(TxtLogType.AliPay, e.Message);
                                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "获取支付二维码失败");
                                }
                            }
                            else if (PayChannel == PaymentChannel.WXPAY)//微信
                            {
                                NativePay native = new NativePay();
                                try
                                {
                                    WxPayData resultData = native.GetPayUrl(order, amount, subject);
                                    if (resultData.GetValue("result_code") != null)
                                    {
                                        string resule = resultData.GetValue("result_code").ToString();
                                        if (resule == "SUCCESS")
                                        {
                                            model.QRcode = resultData.GetValue("code_url").ToString();//获得统一下单接口返回的二维码链接
                                        }
                                        else
                                        {
                                            LogHelper.SaveLog(TxtLogType.WeiXinPay, resultData.GetValue("err_code_des").ToString());
                                            return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "获取支付二维码失败，" + resultData.GetValue("err_code_des").ToString());
                                        }
                                    }
                                    else
                                    {
                                        LogHelper.SaveLog(TxtLogType.WeiXinPay, resultData.GetValue("return_msg").ToString());
                                        return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "获取支付二维码失败，" + resultData.GetValue("return_msg").ToString());
                                    }
                                }
                                catch (WxPayException ex)
                                {
                                    LogHelper.SaveLog(TxtLogType.WeiXinPay, ex.Message);
                                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "获取支付二维码失败");
                                }
                                catch (Exception e)
                                {
                                    LogHelper.SaveLog(TxtLogType.WeiXinPay, e.Message);
                                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "获取支付二维码失败");
                                }
                            }
                            #endregion
                        }
                        break;
                    case SelttleType.StarPos: //新大陆
                        #region 新大陆
                        {
                            string error = "";
                            PPosPayData.OrderPay pposOrder = new PPosPayData.OrderPay();
                            pposOrder.txnTime = System.DateTime.Now.ToString("yyyyMMddHHmmss");
                            pposOrder.tradeNo = order.ID;
                            //pposOrder.tradeNo = Guid.NewGuid().ToString().Replace("-", "");
                            pposOrder.amount = Convert.ToInt32(amount * 100).ToString();//实际付款
                            pposOrder.total_amount = Convert.ToInt32(amount * 100).ToString();//订单总金额
                            pposOrder.subject = subject;
                            pposOrder.payChannel = PayChannel.ToString();

                            PPosPayApi ppos = new PPosPayApi();
                            PPosPayData.OrderPayACK result = ppos.OrderPay(pposOrder, out error);
                            if (result == null)
                            {
                                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "获取支付二维码失败，" + error);
                            }
                            model.QRcode = result.payCode;
                        }
                        break;
                        #endregion
                    case SelttleType.LcswPay: //扫呗
                        #region 扫呗
                        {
                            string qrcode = "";
                            LcswPayDate.OrderPay payInfo = new LcswPayDate.OrderPay();
                            //payInfo.terminal_trace = Guid.NewGuid().ToString().Replace("-", "");
                            payInfo.terminal_trace = order.ID;
                            payInfo.pay_type = PayChannel.ToDescription();
                            payInfo.order_body = subject;
                            payInfo.total_fee = Convert.ToInt32(amount * 100).ToString();
                            LcswPayAPI api = new LcswPayAPI();
                            if (api.OrderPay(payInfo, out qrcode))
                            {
                                model.QRcode = qrcode;
                            }
                            else
                            {
                                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "获取支付二维码失败");
                            }
                        }
                        break;
                        #endregion
                    case SelttleType.DinPay: //智付
                        #region 智付
                        string errmsg = "";
                        DinPayData.ScanPay scanPay = new DinPayData.ScanPay();
                        //scanPay.order_no = order.OrderID;
                        scanPay.order_no = Guid.NewGuid().ToString().Replace("-", "");
                        scanPay.order_time = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        scanPay.order_amount = amount.ToString("0.01");
                        scanPay.service_type = PayChannel == PaymentChannel.WXPAY ? "weixin_scan" : "alipay_scan";
                        scanPay.product_name = subject;
                        scanPay.product_desc = subject;

                        DinPayApi payApi = new DinPayApi();
                        string payCode = payApi.GetQRcode(scanPay, out errmsg);
                        if (payCode == "")
                        {
                            return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "获取支付二维码失败，" + errmsg);
                        }
                        model.QRcode = payCode;
                        break;
                        #endregion
                }

                return ResponseModelFactory<PayQRcodeModel>.CreateModel(isSignKeyReturn, model);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion

        #region 条码支付--商户扫客户付款码
        /// <summary>
        /// 条码支付
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MethodToken)]
        public object BarcodePay(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string orderId = dicParas.ContainsKey("orderId") ? dicParas["orderId"].ToString() : string.Empty;
                string strPayChannel = dicParas.ContainsKey("payChannel") ? dicParas["payChannel"].ToString() : string.Empty;
                string subject = dicParas.ContainsKey("subject") ? dicParas["subject"].ToString() : string.Empty;
                string payType = dicParas.ContainsKey("payType") ? dicParas["payType"].ToString() : string.Empty;
                string authCode = dicParas.ContainsKey("authCode") ? dicParas["authCode"].ToString() : string.Empty;

                if (string.IsNullOrWhiteSpace(orderId))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "订单号无效");
                }

                if (string.IsNullOrWhiteSpace(payType))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "支付方式为空");
                }
                //支付方式
                PaymentChannel PayChannel = (PaymentChannel)Convert.ToInt32(payType);

                Flw_Order order = Flw_OrderBusiness.GetOrderModel(orderId);
                if (order == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "订单号无效");
                }

                var storeId = order.StoreID;
                IBase_StoreInfoService base_StoreInfoService = BLLContainer.Resolve<IBase_StoreInfoService>();
                if (!base_StoreInfoService.Any(p => p.ID.Equals(storeId, StringComparison.OrdinalIgnoreCase)))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "订单所属门店无效");
                }

                //订单减免金额
                decimal freePay = order.FreePay == null ? 0 : order.FreePay.Value;
                //计算订单实付金额，单位：元
                decimal amount = (decimal)order.PayCount - freePay;

                BarcodePayModel model = new BarcodePayModel();
                model.OrderId = orderId;

                //SelttleType selttleType = (SelttleType)store.SelttleType.Value;
                SelttleType selttleType = (SelttleType)Convert.ToInt32(strPayChannel);
                switch (selttleType)
                {
                    case SelttleType.NotThird:
                        break;
                    case SelttleType.AliWxPay: //微信支付宝官方通道
                        {
                            #region 微信支付宝官方通道
                            if (PayChannel == PaymentChannel.ALIPAY)//支付宝
                            {
                                try
                                {
                                    IAlipayTradeService serviceClient = F2FBiz.CreateClientInstance(AliPayConfig.serverUrl, AliPayConfig.appId, AliPayConfig.merchant_private_key, AliPayConfig.version,
                                AliPayConfig.sign_type, AliPayConfig.alipay_public_key, AliPayConfig.charset);

                                    AliPayCommon alipay = new AliPayCommon();
                                    AlipayTradePayContentBuilder builder = alipay.BuildPayContent(order, amount, subject, authCode);
                                    //string out_trade_no = builder.out_trade_no;

                                    AlipayF2FPayResult payResult = serviceClient.tradePay(builder);

                                    if (payResult.Status == ResultEnum.SUCCESS)
                                    {
                                        decimal payAmount = Convert.ToDecimal(payResult.response.TotalAmount);

                                        //支付成功后的处理
                                        BarcodePayModel callbackModel = Flw_OrderBusiness.OrderPay(payResult.response.OutTradeNo, payAmount, selttleType);

                                        model.OrderStatus = callbackModel.OrderStatus;
                                        model.PayAmount = payAmount.ToString("0.00");
                                    }
                                    else
                                    {
                                        LogHelper.SaveLog(TxtLogType.AliPay, payResult.response.SubMsg);
                                        return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "支付失败");
                                    }
                                }
                                catch (Exception e)
                                {
                                    LogHelper.SaveLog(TxtLogType.AliPay, e.Message);
                                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "支付失败");
                                }
                            }
                            else if (PayChannel == PaymentChannel.WXPAY)
                            {
                                try
                                {
                                    MicroPay pay = new MicroPay();
                                    WxPayData resultData = pay.BarcodePay(orderId, subject, amount, authCode);
                                    string resule = resultData.GetValue("result_code").ToString();
                                    if (resule == "SUCCESS")
                                    {
                                        string out_trade_no = resultData.GetValue("out_trade_no").ToString();
                                        decimal total_fee = Convert.ToDecimal(resultData.GetValue("total_fee"));
                                        decimal payAmount = total_fee / 100;

                                        //支付成功后的处理
                                        BarcodePayModel callbackModel = Flw_OrderBusiness.OrderPay(out_trade_no, payAmount, selttleType);

                                        model.OrderStatus = callbackModel.OrderStatus;
                                        model.PayAmount = payAmount.ToString("0.00");
                                    }
                                    else
                                    {
                                        LogHelper.SaveLog(TxtLogType.WeiXinPay, resultData.GetValue("result_code").ToString());
                                        return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "支付失败");
                                    }
                                }
                                catch (WxPayException ex)
                                {
                                    LogHelper.SaveLog(TxtLogType.WeiXinPay, ex.Message);
                                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "支付失败");
                                }
                                catch (Exception e)
                                {
                                    LogHelper.SaveLog(TxtLogType.WeiXinPay, e.Message);
                                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "支付失败");
                                }
                            }
                            #endregion
                        }
                        break;
                    case SelttleType.StarPos: //新大陆
                        #region 新大陆
                        PPosPayData.MicroPay pposOrder = new PPosPayData.MicroPay();
                        //string tradeNo = Guid.NewGuid().ToString().Replace("-", "");
                        string tradeNo = order.ID;

                        pposOrder.tradeNo = tradeNo;
                        pposOrder.amount = Convert.ToInt32(amount * 100).ToString();
                        pposOrder.total_amount = Convert.ToInt32(amount * 100).ToString();
                        pposOrder.authCode = authCode;
                        pposOrder.payChannel = PayChannel.ToString();
                        pposOrder.subject = subject;
                        pposOrder.selOrderNo = tradeNo;
                        pposOrder.txnTime = System.DateTime.Now.ToString("yyyyMMddHHmmss");
                        pposOrder.signValue = "";

                        string error = "";
                        PPosPayApi ppos = new PPosPayApi();
                        PPosPayData.MicroPayACK result = ppos.ScanPay(pposOrder, out error);
                        if (result != null)
                        {
                            //SUCCESS
                            string out_trade_no = result.tradeNo;
                            decimal total_fee = Convert.ToDecimal(result.total_amount);
                            decimal payAmount = total_fee / 100;

                            //支付成功后的处理
                            BarcodePayModel callbackModel = Flw_OrderBusiness.OrderPay(out_trade_no, payAmount, selttleType);

                            model.OrderStatus = callbackModel.OrderStatus;
                            model.PayAmount = payAmount.ToString("0.00");
                        }
                        else
                        {
                            LogHelper.SaveLog(TxtLogType.PPosPay, error);
                            return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "支付失败，" + error);
                        }
                        break;
                        #endregion
                    case SelttleType.LcswPay: //扫呗
                        #region 扫呗
                        LcswPayDate.TradePay tradePay = new LcswPayDate.TradePay();
                        tradePay.terminal_trace = order.ID;
                        tradePay.pay_type = PayChannel.ToDescription();
                        tradePay.order_body = subject;
                        tradePay.total_fee = Convert.ToInt32(amount * 100).ToString();
                        tradePay.auth_no = authCode;
                        LcswPayAPI api = new LcswPayAPI();
                        LcswPayDate.OrderPayACK ack = api.BarcodePay(tradePay);
                        if (ack != null)
                        {
                            if (ack.return_code == "01" && ack.result_code == "01")
                            {
                                string out_trade_no = ack.out_trade_no;
                                decimal total_fee = Convert.ToDecimal(ack.total_fee);
                                decimal payAmount = total_fee / 100;

                                //支付成功后的处理
                                BarcodePayModel callbackModel = Flw_OrderBusiness.OrderPay(out_trade_no, payAmount, SelttleType.LcswPay);

                                model.OrderStatus = callbackModel.OrderStatus;
                                model.PayAmount = payAmount.ToString("0.00");
                            }
                        }
                        else
                        {
                            LogHelper.SaveLog(TxtLogType.LcswPay, "条码支付失败");
                            return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "支付失败");
                        }
                        break;
                        #endregion
                    case SelttleType.DinPay: //智付
                        #region 智付
                        string errmsg = "";

                        DinPayData.MicroPay Pay = new DinPayData.MicroPay();
                        //scanPay.order_no = order.OrderID;
                        Pay.order_no = Guid.NewGuid().ToString().Replace("-", "");
                        Pay.order_time = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        Pay.order_amount = amount.ToString("0.01");
                        Pay.service_type = PayChannel == PaymentChannel.WXPAY ? "weixin_micropay" : "alipay_micropay";
                        Pay.product_name = subject;
                        Pay.auth_code = authCode;
                        //Pay.return_url = "http://106.14.174.131/returnurl.aspx";

                        DinPayApi payApi = new DinPayApi();
                        string resultXml = payApi.MicroPay(Pay);

                        //将同步返回的xml中的参数提取出来
                        var el = XElement.Load(new StringReader(resultXml));
                        //处理码
                        string resp_code = el.XPathSelectElement("/response/resp_code").Value;
                        //业务结果
                        string result_code = el.XPathSelectElement("/response/result_code").Value;
                        if (resp_code == "SUCCESS" && result_code == "0")
                        {
                            //支付成功后的处理
                            string out_trade_no = el.XPathSelectElement("/response/order_no").Value;
                            decimal total_fee = Convert.ToDecimal(el.XPathSelectElement("/response/order_amount").Value);
                            decimal payAmount = total_fee / 100;

                            BarcodePayModel callbackModel = Flw_OrderBusiness.OrderPay(out_trade_no, payAmount, SelttleType.DinPay);

                            model.OrderStatus = callbackModel.OrderStatus;
                            model.PayAmount = payAmount.ToString("0.00");
                        }
                        else
                        {
                            errmsg = el.XPathSelectElement("/response/result_desc").Value;
                            LogHelper.SaveLog(TxtLogType.DinPay, errmsg);
                            return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "支付失败，" + errmsg);
                        }
                        break;
                        #endregion
                }

                return ResponseModelFactory<BarcodePayModel>.CreateModel(isSignKeyReturn, model);
            }
            catch (WxPayException ex)
            {
                LogHelper.SaveLog(TxtLogType.WeiXinPay, ex.Message);
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "支付失败");
            }
            catch (Exception e)
            {
                LogHelper.SaveLog(TxtLogType.Api, e.Message);
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "支付失败");
            }
        }
        #endregion

        #region 智付转账
        /// <summary>
        /// 智付转账
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MethodToken)]
        public object getTransferPay(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string mer_transfer_no = dicParas.ContainsKey("mer_transfer_no") ? dicParas["mer_transfer_no"].ToString() : string.Empty;
                string tran_amount = dicParas.ContainsKey("tran_amount") ? dicParas["tran_amount"].ToString() : string.Empty;
                string recv_bank_code = dicParas.ContainsKey("recv_bank_code") ? dicParas["recv_bank_code"].ToString() : string.Empty;
                string recv_accno = dicParas.ContainsKey("recv_accno") ? dicParas["recv_accno"].ToString() : string.Empty;
                string recv_name = dicParas.ContainsKey("recv_name") ? dicParas["recv_name"].ToString() : string.Empty;
                string recv_province = dicParas.ContainsKey("recv_province") ? dicParas["recv_province"].ToString() : string.Empty;
                string recv_city = dicParas.ContainsKey("recv_city") ? dicParas["recv_city"].ToString() : string.Empty;
                string tran_fee_type = dicParas.ContainsKey("tran_fee_type") ? dicParas["tran_fee_type"].ToString() : string.Empty;
                string tran_type = dicParas.ContainsKey("tran_type") ? dicParas["tran_type"].ToString() : string.Empty;

                PayQRcodeModel model = new PayQRcodeModel();
                model.OrderId = mer_transfer_no;

                #region 智付
                string errmsg = "";
                DinPayData.TransferData payOrder = new DinPayData.TransferData();
                payOrder.mer_transfer_no = mer_transfer_no;
                payOrder.tran_amount = tran_amount;
                payOrder.recv_bank_code = recv_bank_code;
                payOrder.recv_accno = recv_accno;
                payOrder.recv_name = recv_name;
                payOrder.recv_province = recv_province;
                payOrder.recv_city = recv_city;
                payOrder.tran_fee_type = tran_fee_type;
                payOrder.tran_type = tran_type;

                DinPayApi payApi = new DinPayApi();
                string payCode = payApi.TransferPay(payOrder);

                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "" + payCode);
                //if (payCode == "")
                //{
                //    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "" + payCode);
                //}
                #endregion

                //return ResponseModelFactory<PayQRcodeModel>.CreateModel(isSignKeyReturn, model);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion

        #region 莘拍档支付宝小程序支付
        /// <summary>
        /// 莘拍档支付宝小程序支付
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MobileToken)]
        public object getAliMiniAppPaySign(Dictionary<string, object> dicParas)
        {
            try
            {
                int coins = 0;
                string orderNo = string.Empty;
                string errMsg = string.Empty;
                string mobileToken = dicParas.ContainsKey("mobileToken") ? dicParas["mobileToken"].ToString() : string.Empty;
                string storeId = dicParas.ContainsKey("storeId") ? dicParas["storeId"].ToString() : string.Empty;
                string productName = dicParas.ContainsKey("productName") ? dicParas["productName"].ToString() : string.Empty;
                string payPriceStr = dicParas.ContainsKey("payPrice") ? dicParas["payPrice"].ToString() : string.Empty;
                string buyType = dicParas.ContainsKey("buyType") ? dicParas["buyType"].ToString() : string.Empty;
                string coinsStr = dicParas.ContainsKey("coins") ? dicParas["coins"].ToString() : string.Empty;

                decimal payPrice = 0;
                if (!decimal.TryParse(payPriceStr, out payPrice))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "支付金额不正确");
                }

                if (!int.TryParse(coinsStr, out coins))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "购买币数不正确");
                }
                MobileTokenModel mobileTokenModel = (MobileTokenModel)(dicParas[Constant.MobileTokenModel]);

                //生成服务器订单号
                orderNo = PayOrderHelper.CreateXCGameOrderNo(storeId, payPrice, 0, (int)(OrderType.Ali), productName, mobileTokenModel.Mobile, buyType, coins);

                IAopClient client = new DefaultAopClient(AliPayConfig.serverUrl, AliPayConfig.miniAppId, AliPayConfig.merchant_miniapp_private_key, "json", "1.0", "RSA2", AliPayConfig.alipay_miniapp_public_key, AliPayConfig.charset, false);
                AlipayTradeAppPayModel builder = new AlipayTradeAppPayModel();
                builder.Body = CommonConfig.PayTitle + "-" + buyType;
                builder.Subject = productName;
                builder.OutTradeNo = orderNo;
                builder.TotalAmount = payPrice.ToString("0.00");
                builder.ProductCode = "QUICK_MSECURITY_PAY";
                builder.TimeoutExpress = "10m";

                AlipayTradeAppPayRequest request = new AlipayTradeAppPayRequest();

                request.SetBizModel(builder);
                request.SetNotifyUrl(AliPayConfig.AliMiniAppNotify_url);

                AlipayTradeAppPayResponse response = client.SdkExecute(request);
                //string strSign = HttpUtility.HtmlEncode(response.Body);

                AlipayMiniAppSignModel model = new AlipayMiniAppSignModel();
                model.OrderId = orderNo;
                model.PaySign = response.Body;

                return ResponseModelFactory<AlipayMiniAppSignModel>.CreateModel(isSignKeyReturn, model);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion

        #region 微信H5支付 -- 新大陆支付通道
        /// <summary>
        /// 微信H5支付 -- 新大陆支付通道
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MethodToken)]
        public object getPposWxPay(Dictionary<string, object> dicParas)
        {
            try
            {
                string token = dicParas.ContainsKey("token") ? dicParas["token"].ToString().Trim() : string.Empty;
                string deviceToken = dicParas.ContainsKey("deviceToken") ? dicParas["deviceToken"].ToString().Trim() : "";
                string coinType = dicParas.ContainsKey("coinType") ? dicParas["coinType"].ToString().Trim() : string.Empty;//投币方式 0 散客 1 会员
                string coinRuleId = dicParas.ContainsKey("coinRuleId") ? dicParas["coinRuleId"].ToString().Trim() : string.Empty;

                if (string.IsNullOrEmpty(deviceToken))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "设备令牌无效");
                }

                MemberTokenModel memberTokenModel = MemberTokenCache.GetModel(token);
                if (memberTokenModel == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "用户令牌无效，请重新登陆");
                }

                //Base_MemberInfo member = Base_MemberInfoService.I.GetModels(t => t.ID == memberTokenModel.MemberId).FirstOrDefault();
                //if (member == null)
                //{
                //    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "用户令牌无效，请重新登陆");
                //}

                Base_DeviceInfo device = Base_DeviceInfoService.I.GetModels().FirstOrDefault(t => t.Token == deviceToken);
                if (device == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "设备令牌无效");
                }
                if (!string.IsNullOrEmpty(memberTokenModel.CurrStoreId) && memberTokenModel.CurrStoreId != device.StoreID)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "当前门店与设备所属门店不符，请先选择门店");
                }

                Data_GameInfo game = Data_GameInfoService.I.GetModels(t => t.ID == device.GameIndexID).FirstOrDefault();
                if (game == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "游戏机不存在");
                }

                //付款方式 0 现金 1 微信 2 支付宝 3 银联 4 储值金
                int PayType = 1;
                int OrderSource = 1; //订单来源 1 自助机
                decimal PayCount = 0m; //应付金额
                decimal FreePay = 0m; //减免金额

                string coinNote = string.Empty;

                int currCoinRuleId = 0;
                if (!int.TryParse(coinRuleId, out currCoinRuleId))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "参数错误，请重试");
                }
                if(coinType == "0")
                {
                    //散客扫码支付
                    Data_GameAPP_Rule coinRule = Data_GameAPP_RuleService.I.GetModels(t => t.ID == currCoinRuleId).FirstOrDefault();
                    PayCount = coinRule.PayCount.Value;
                    coinNote = string.Format("{0}元{1}局", PayCount, coinRule.PlayCount);
                }
                else
                {
                    #region 会员投币
                    //散客扫码支付
                    Data_GameAPP_MemberRule coinRule = Data_GameAPP_MemberRuleService.I.GetModels(t => t.ID == currCoinRuleId).FirstOrDefault();
                    if (coinRule.PushCoin1 > 0)
                    {
                        CardBalance currBalance = memberTokenModel.CurrentCardInfo.CardBalanceList.FirstOrDefault(t => t.BalanceIndex == coinRule.PushBalanceIndex1);
                        if (currBalance.Quantity < coinRule.PushCoin1)
                        {
                            return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, string.Format("{0}余额不足，请充值", currBalance.BalanceName));
                        }
                    }
                    if (coinRule.PushCoin2 > 0)
                    {
                        CardBalance currBalance = memberTokenModel.CurrentCardInfo.CardBalanceList.FirstOrDefault(t => t.BalanceIndex == coinRule.PushBalanceIndex2);
                        if (currBalance.Quantity < coinRule.PushCoin2)
                        {
                            return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, string.Format("{0}余额不足，请充值", currBalance.BalanceName));
                        }
                    }
                    //会员投币 
                    #endregion
                }

                Flw_Order order = new Flw_Order();
                order.ID = RedisCacheHelper.CreateCloudSerialNo(device.StoreID);
                order.StoreID = device.StoreID;
                order.FoodCount = 1;
                order.GoodCount = 0;
                order.MemberID = memberTokenModel.MemberId;
                order.CardID = memberTokenModel.CurrentCardInfo.CardId;
                order.OrderSource = OrderSource;
                order.CreateTime = DateTime.Now;
                order.PayType = PayType;
                order.PayCount = PayCount;
                order.FreePay = FreePay;
                order.OrderStatus = 1; //待支付
                order.Note = "手机扫码直接支付";

                bool ret = Flw_OrderService.I.Add(order, false);
                if(!ret)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "生成订单失败");
                }



                #region 新大陆微信公众号支付
                string error = "";
                PPosPayData.WeiXinPubPay pay = new PPosPayData.WeiXinPubPay();

                pay.openid = token;//在授权回调页面中获取到的授权code或者openid

                pay.amount = ((int)(PayCount * 100)).ToString();//实际付款
                pay.total_amount = pay.amount;//订单总金额
                pay.subject = game.GameName + "-投币";
                pay.selOrderNo = order.ID;
                pay.goods_tag = coinNote;

                PPosPayApi ppos = new PPosPayApi();
                PPosPayData.WeiXinPubPayACK ack = new PPosPayData.WeiXinPubPayACK();
                PPosPayData.WeiXinPubPayACK result = ppos.PubPay(pay, ref ack, out error);
                if (result == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "生成订单失败，" + error);
                }
                #endregion

                PayLogHelper.WriteEvent(result.orderNo, "新大陆支付");

                PposPubSigPay model = new PposPubSigPay();
                model.appId = result.apiAppid;
                model.timeStamp = result.apiTimestamp;
                model.nonceStr = result.apiNoncestr;
                model.package = result.apiPackage;
                model.paySign = result.apiPaysign;

                ////将订单相关的设备、套餐信息加入到缓存，用于在收不到回调的时候发起主动查询
                //OrderUnpaidModel UnpaidOrder = new OrderUnpaidModel();
                //UnpaidOrder.OrderId = orderNo;
                //UnpaidOrder.DeviceToken = deviceToken;
                //UnpaidOrder.FoodId = foodId;
                //UnpaidOrder.MemberTokenModel = memberTokenModel;
                //UnpaidOrder.OpenId = openId;
                //UnpaidOrder.CreateTime = DateTime.Now;
                //UnpaidOrderList.AddNewItem(UnpaidOrder);

                return ResponseModelFactory<PposPubSigPay>.CreateModel(isSignKeyReturn, model);
            }
            catch (Exception ex)
            {
                LogHelper.SaveLog(TxtLogType.PPosPay, TxtLogContentType.Debug, TxtLogFileType.Day, "新大陆微信公众号支付失败，原因：" + ex.InnerException != null ? ex.InnerException.Message : ex.Message);
                throw ex;
            }
        }
        #endregion

        #region 支付宝H5支付 -- 创建支付订单
        /// <summary>
        /// 支付宝H5支付 -- 创建支付订单
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCGameMemberOrMobileToken)]
        public object createAlipayOrder(Dictionary<string, object> dicParas)
        {
            try
            {
                int coins = 0;
                string orderNo = string.Empty;
                string errMsg = string.Empty;
                string aliId = dicParas.ContainsKey("aliId") ? dicParas["aliId"].ToString() : string.Empty;
                string storeId = dicParas.ContainsKey("storeId") ? dicParas["storeId"].ToString() : string.Empty;
                string productName = dicParas.ContainsKey("productName") ? dicParas["productName"].ToString() : string.Empty;
                string payPriceStr = dicParas.ContainsKey("payPrice") ? dicParas["payPrice"].ToString() : string.Empty;
                string buyType = dicParas.ContainsKey("buyType") ? dicParas["buyType"].ToString() : string.Empty;
                string coinsStr = dicParas.ContainsKey("coins") ? dicParas["coins"].ToString() : string.Empty;

                string memberToken = dicParas.ContainsKey("membertoken") ? dicParas["membertoken"].ToString() : string.Empty;
                string deviceToken = dicParas.ContainsKey("deviceToken") ? dicParas["deviceToken"].ToString() : string.Empty;
                string strFoodId = dicParas.ContainsKey("foodid") ? dicParas["foodid"].ToString() : string.Empty;

                decimal payPrice = 0;
                if (!decimal.TryParse(payPriceStr, out payPrice))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "支付金额不正确");
                }

                if (!int.TryParse(coinsStr, out coins))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "购买币数不正确");
                }

                int foodId = 0;
                if (!int.TryParse(strFoodId, out foodId))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "套餐编号不正确");
                }

                //MobileTokenModel mobileTokenModel = (MobileTokenModel)(dicParas[Constant.MobileTokenModel]);
                XCGameMemberTokenModel memberTokenModel = (XCGameMemberTokenModel)(dicParas[Constant.XCGameMemberTokenModel]);
                string mobile = memberTokenModel.Mobile;

                //生成服务器订单号
                orderNo = PayOrderHelper.CreateXCGameOrderNo(storeId, payPrice, 0, (int)(OrderType.Ali), productName, mobile, buyType, coins);

                #region 支付宝下单创建
                IAopClient client = new DefaultAopClient(AliPayConfig.serverUrl, AliPayConfig.authAppId, AliPayConfig.merchant_auth_private_key, "json", AliPayConfig.version, AliPayConfig.sign_type, AliPayConfig.alipay_auth_public_key, AliPayConfig.charset, false);
                AlipayTradeCreateRequest request = new AlipayTradeCreateRequest();
                AlipayTradeCreateModel builder = new AlipayTradeCreateModel();
                builder.Body = productName;
                builder.Subject = CommonConfig.PayTitle + "-" + buyType + "-" + productName;
                builder.OutTradeNo = orderNo;
                builder.TotalAmount = payPrice.ToString("0.00");
                builder.BuyerId = aliId;
                builder.TimeoutExpress = "1m";

                request.SetBizModel(builder);
                request.SetNotifyUrl(AliPayConfig.AliH5NotifyUrl);
                AlipayTradeCreateResponse response = client.Execute(request);

                AliCreateOrderResModel model = new AliCreateOrderResModel();
                model.tradeNO = response.TradeNo;
                model.orderId = response.OutTradeNo;

                OrderUnpaidModel UnpaidOrder = new OrderUnpaidModel();
                UnpaidOrder.OrderId = orderNo;
                UnpaidOrder.DeviceToken = deviceToken;
                UnpaidOrder.FoodId = foodId;
                UnpaidOrder.MemberTokenModel = memberTokenModel;
                UnpaidOrder.CreateTime = DateTime.Now;
                UnpaidOrderList.AddNewItem(UnpaidOrder);

                #endregion
                return ResponseModelFactory<AliCreateOrderResModel>.CreateModel(isSignKeyReturn, model);
            }
            catch (Exception ex)
            {
                LogHelper.SaveLog(TxtLogType.AliPay, TxtLogContentType.Debug, TxtLogFileType.Day, "支付宝H5订单创建失败，原因：" + ex.InnerException != null ? ex.InnerException.Message : ex.Message);
                throw ex;
            }
        }
        #endregion
    }
}