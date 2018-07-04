using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using XCCloudWebBar.Business.XCGameMana;
using XCCloudWebBar.Model.XCGameManager;
using XCCloudWebBar.Pay.PPosPay;
using XCCloudWebBar.OrderPayCallback.Common;
using XCCloudWebBar.Model.WeiXin.Message;
using XCCloudWebBar.WeiXin.Message;
using XCCloudWebBar.Common.Enum;

namespace XXCloudService.Test
{
    public partial class pposTest : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            PPosPayData.WeiXinPubQuery query = new PPosPayData.WeiXinPubQuery();
            PPosPayData.WeiXinPubQueryACK ack = new PPosPayData.WeiXinPubQueryACK();
            string error= string.Empty;

            PPosPayApi ppos = new PPosPayApi();
            bool result = ppos.WeiXinPubQuery(query, ref ack, out error);
            Response.Write(error);
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            PPosPayData.WeiXinPubPay pay = new PPosPayData.WeiXinPubPay();
            PPosPayData.WeiXinPubPayACK ack = new PPosPayData.WeiXinPubPayACK();
            string error = string.Empty;

            PPosPayApi ppos = new PPosPayApi();
            PPosPayData.WeiXinPubPayACK result = ppos.PubPay(pay, ref ack, out error);
            Response.Write(error);
        }

        protected void Button3_Click(object sender, EventArgs e)
        {
            PPosPayData.Refund pay = new PPosPayData.Refund();
            //PPosPayData.RefundACK ack = new PPosPayData.RefundACK();
            string error = string.Empty;
            //pay.orderNo = "000000031078505";
            ////pay.tradeNo = DateTime.Now.ToString("yyyyMMddHHmmssffff");
            //pay.tradeNo = "2018042020014810002700000004";
            pay.orderNo = TextBox1.Text.Trim();
            pay.tradeNo = TextBox2.Text.Trim();
            pay.txnAmt = TextBox3.Text.Trim();
            PPosPayApi ppos = new PPosPayApi();
            PPosPayData.RefundACK result = ppos.RefundPay(pay, out error);
            Response.Write(error);
        }

        protected void Button4_Click(object sender, EventArgs e)
        {
            PPosPayData.QueryOrder pay = new PPosPayData.QueryOrder();
            string error = string.Empty;
            pay.tradeNo = DateTime.Now.ToString("yyyyMMddHHmmssffff");
            pay.qryNo = "2018042020110110002700000005";
            PPosPayApi ppos = new PPosPayApi();
            PPosPayData.QueryOrderACK result = ppos.QueryOrderPay(pay, out error);
            Response.Write(error);
        }

        protected void Button5_Click(object sender, EventArgs e)
        {
            string orderId = "2018041717554610000000000005";
            //string deviceToken = "000001";
            string deviceToken = "000003";

            OrderUnpaidModel UnpaidOrder = new OrderUnpaidModel();
            UnpaidOrder.OrderId = orderId;
            UnpaidOrder.DeviceToken = deviceToken;
            UnpaidOrder.FoodId = 40;
            UnpaidOrder.CreateTime = DateTime.Now;
            UnpaidOrderList.AddNewItem(UnpaidOrder);

            Data_Order order = MPOrderBusiness.GetOrderModel(orderId);

            OrderUnpaidModel unpaiOrder = null;
            UnpaidOrderList.GetItem(order.OrderID, out unpaiOrder);

            bool flag = OrderHandle.FoodSale(order, unpaiOrder);
        }

        protected void Button6_Click(object sender, EventArgs e)
        {
            string errMsg = string.Empty;
            MemberRechargeNotifyDataModel dataModel = new MemberRechargeNotifyDataModel();
            dataModel.AccountType = "充值套餐";
            dataModel.Account = "5元10币";
            dataModel.Amount = "99.9元";
            dataModel.Status = "成功";
            dataModel.Remark = "充值成功，祝您玩的愉快！";
            bool flag = MessageMana.PushMessage(WeiXinMesageType.MemberRechargeNotify, "oNWocwSC_GFO8n_8mtZ0iV9tL0WI", dataModel, out errMsg);

            Response.Write(errMsg);
        }
    }
}