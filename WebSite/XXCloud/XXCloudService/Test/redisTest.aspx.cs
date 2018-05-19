using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using XCCloudService.Business.XCGameMana;
using XCCloudService.CacheService;
using XCCloudService.Model.CustomModel.XCCloud;
using XCCloudService.OrderPayCallback.Common;

namespace XXCloudService.Test
{
    public partial class redisTest : System.Web.UI.Page
    {
        //protected override void OnInit(EventArgs e)
        //{
        //    this.Button1
        //    base.OnInit(e);
        //}

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            try
            {
                string serialNo = RedisCacheHelper.CreateCloudSerialNo("100016360103001");
                Response.Write(serialNo);
            }
            catch
            { 
            }
            
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            string conn = Config.EncryptDES("Data Source=192.168.1.119;Initial Catalog=XinchenPay;User Id=sa;Password=xinchen;Connection Timeout=10;", "Xinchen1");
            Response.Write(conn);
        }

        protected void Button3_Click(object sender, EventArgs e)
        {
            string token = txtToken.Text.Trim();

            XCCloudUserTokenModel model = XCCloudUserTokenBusiness.GetUserTokenModel(token);
        }
    }
}