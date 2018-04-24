using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using XCCloudService.Pay.PPosPay;

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
            pay.txnAmt = "";
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
    }
}