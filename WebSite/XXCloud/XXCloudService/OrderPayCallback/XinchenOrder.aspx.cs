using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using XCCloudService.OrderPayCallback.Common;

namespace XXCloudService.OrderPayCallback
{
    public partial class XinchenOrder : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string order_id = Request.QueryString["OrderID"];
            string store_id = Request.QueryString["StoreID"];
            string workstation = Request.QueryString["WorkStation"];
            string price = Request.QueryString["Price"];
            string order_type = Request.QueryString["OrderType"];
            string descript = Request.QueryString["Descript"];

            string sql = string.Format("INSERT INTO Data_Order (OrderID,StoreID,WorkStation,Price,OrderType,CreateTime,Descript) VALUES ('{0}','{1}','{2}','{3}','{4}',GetDate(),'{5}')",
                    order_id, store_id, workstation, price, order_type, descript
                );
            DataAccess ac = new DataAccess();
            if (ac.Execute(sql) > 0)
            {
                Response.Write("SUCCESS");
            }
            else
            {
                Response.Write("Fail");
            }
            ac.Dispose();
        }
    }
}