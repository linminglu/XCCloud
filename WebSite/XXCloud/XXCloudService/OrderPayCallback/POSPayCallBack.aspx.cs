using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using XCCloudService.BLL.CommonBLL;
using XCCloudService.BLL.XCCloud;
using XCCloudService.Business.XCCloud;
using XCCloudService.Business.XCGameMana;
using XCCloudService.CacheService;
using XCCloudService.CacheService.XCCloud;
using XCCloudService.Common;
using XCCloudService.Common.Enum;
using XCCloudService.Model.CustomModel.XCCloud;
using XCCloudService.Model.CustomModel.XCGameManager;
using XCCloudService.Model.XCCloud;
using XCCloudService.Model.XCGameManager;
using XCCloudService.OrderPayCallback.Common;
using XCCloudService.Pay.PPosPay;

namespace XXCloudService.PayChannel
{
    public partial class POSPayCallBack : System.Web.UI.Page
    {
        class PosStarCallback
        {
            /// <summary>
            /// 平台流水号
            /// </summary>
            public string logNo { get; set; }

            public string TxnLogId { get; set; }

            /// <summary>
            /// 商户订单号
            /// </summary>
            public string ChannelId { get; set; }

            /// <summary>
            /// 商户号
            /// </summary>
            public string BusinessId { get; set; }

            /// <summary>
            /// 交易码
            /// </summary>
            public string TxnCode { get; set; }

            /// <summary>
            /// 交易状态
            /// </summary>
            public string TxnStatus { get; set; }

            /// <summary>
            /// 交易金额
            /// </summary>
            public string TxnAmt { get; set; }

            /// <summary>
            /// 支付通道订单号(微信/支付宝订单号)
            /// </summary>
            public string OfficeId { get; set; }

            /// <summary>
            /// 支付通道
            /// </summary>
            public string PayChannel { get; set; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            System.IO.Stream s = Request.InputStream;
            int count = 0;
            byte[] buffer = new byte[1024];
            StringBuilder builder = new StringBuilder();
            while ((count = s.Read(buffer, 0, 1024)) > 0)
            {
                builder.Append(Encoding.GetEncoding("GBK").GetString(buffer, 0, count));
            }
            s.Flush();
            s.Close();
            s.Dispose();

            PayLogHelper.WriteEvent(builder.ToString(), "新大陆支付");

            string r = "{\"RspCode\":\"000000\",\"RspDes\":\"\"}";

            JavaScriptSerializer jsonSerialize = new JavaScriptSerializer();
            PosStarCallback callback = jsonSerialize.Deserialize<PosStarCallback>(builder.ToString());

            if (callback != null)
            {
                //判断商户号是莘宸还是其他商户号
                if (callback.TxnStatus == "1" && callback.BusinessId == PPosPayConfig.MerchNo) //莘宸商户号
                {
                    string out_trade_no = callback.ChannelId; //商户单号
                    decimal payAmount = Convert.ToDecimal(callback.TxnAmt);//交易金额

                    Flw_OrderBusiness orderBusiness = new Flw_OrderBusiness();

                    if (callback.TxnCode.Trim() == "L005") //退款
                    {
                        //订单退款                     
                        orderBusiness.OrderRefundPay(out_trade_no, payAmount);
                    }
                    else  //扫码支付、公众号支付
                    {                       
                        string payChannel = callback.PayChannel;
                        PaymentChannel payment = PaymentChannel.WXPAY;
                        if (payChannel == "1")
                        {
                            payment = PaymentChannel.ALIPAY;
                        }

                        string errMsg = string.Empty;
                        //云平台订单处理                        
                        orderBusiness.OrderPay(out_trade_no, payAmount, callback.OfficeId, SelttleType.StarPos, payment, out errMsg);

                        OrderCacheModel orderCache = RedisCacheHelper.HashGet<OrderCacheModel>(CommonConfig.UdpOrderSNCache, out_trade_no);
                        if (orderCache != null)
                        {
                            XCCloudService.SocketService.UDP.Server.AskScanPayResult("1", errMsg, orderCache.SN);
                        }
                    }
                }
            }
            else
            {
                r = "{\"RspCode\":\"999999\",\"RspDes\":\"\"}";
            }

            Response.ContentType = "application/json";
            Response.HeaderEncoding = Encoding.GetEncoding("GBK");            
            Response.Write(r);
        }
    }
}