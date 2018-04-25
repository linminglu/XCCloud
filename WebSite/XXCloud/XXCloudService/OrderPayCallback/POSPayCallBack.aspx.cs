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
                    string out_trade_no = callback.ChannelId;
                    decimal payAmount = Convert.ToDecimal(callback.TxnAmt);

                    Data_Order order = MPOrderBusiness.GetOrderModel(out_trade_no);
                    //判断是莘拍档订单还是云平台订单
                    if (order != null)
                    {
                        //莘拍档订单处理
                        // 判断payAmount是否确实为该订单的实际金额
                        if (order.Price != payAmount)
                        {
                            LogHelper.SaveLog(TxtLogType.PPosPay, TxtLogContentType.Debug, TxtLogFileType.Day, "应用：莘拍档 订单号：" + out_trade_no + " 警告：支付支付金额验证失败！！！");
                        }
                        else
                        {
                            //如果订单状态为未支付，就更新订单状态
                            if (order.PayStatus == 0)
                            {
                                OrderHandle.FlwFoodSaleOrderHandle(order, callback.OfficeId);
                                //if (MPOrderBusiness.UpdateOrderForPaySuccess(out_trade_no, callback.OfficeId))
                                //{
                                //    LogHelper.SaveLog(TxtLogType.PPosPay, TxtLogContentType.Debug, TxtLogFileType.Day, "应用：莘拍档 订单号：" + out_trade_no + " 支付成功！");

                                //    OrderHandle.FlwFoodSaleOrderHandle(order);
                                //}
                                //else
                                //{
                                //    LogHelper.SaveLog(TxtLogType.PPosPay, TxtLogContentType.Debug, TxtLogFileType.Day, "应用：莘拍档 订单号：" + out_trade_no + " 已支付订单更新失败！！！");
                                //}
                            }
                        }
                    }
                    else
                    {
                        //云平台订单处理
                        Flw_OrderBusiness.OrderPay(out_trade_no, payAmount, SelttleType.StarPos);
                    }
                }
                else //其他商户号
                {
                    PayList.AddNewItem(callback.TxnLogId, callback.TxnAmt);

                    DataAccess ac = new DataAccess();
                    string out_trade_no = callback.TxnLogId;
                    string sql = "";

                    sql = "update data_order set PayStatus='1',PayTime=GETDATE() where OrderID='" + out_trade_no + "' and PayStatus='0'"; //支付成功
                    ac.Execute(sql);

                    sql = "select o.Price, s.* from Data_Order o,Base_StoreInfo s where o.StoreID=s.StoreID and o.OrderID='" + out_trade_no + "'";
                    DataTable dt = ac.GetTable(sql);
                    if (dt.Rows.Count > 0)
                    {
                        //获取当前结算费率，计算手续费
                        double fee = Convert.ToDouble(dt.Rows[0]["POSStarFee"]);
                        double d = Math.Round(Convert.ToDouble(dt.Rows[0]["Price"]) * fee, 2, MidpointRounding.AwayFromZero);   //最小单位为0.01元
                        if (d < 0.01) d = 0.01;
                        sql = "update data_order set Fee='" + d + "' where OrderID='" + out_trade_no + "'";
                        ac.Execute(sql);
                    }

                    ac.Dispose();
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