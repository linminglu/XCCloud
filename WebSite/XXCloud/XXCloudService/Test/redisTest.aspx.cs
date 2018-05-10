using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using XCCloudService.CacheService;

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
                string serialNo = RedisCacheHelper.CreateSerialNo("100016360103001");
                Response.Write(serialNo);
            }
            catch
            { 
            }
            
        }
    }
}