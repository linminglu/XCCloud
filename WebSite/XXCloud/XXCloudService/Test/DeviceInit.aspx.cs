using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using XCCloudService.CacheService;
using XXCloudService.Utility;

namespace XXCloudService.Test
{
    public partial class DeviceInit : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            string merchId = txtMerchId.Text.Trim();
            string storeId = txtStoreId.Text.Trim();
            string dbip = txtDBIP.Text.Trim();
            string dbPwd = txtDBPwd.Text.Trim();
            int port = Convert.ToInt32(txtUdpPort.Text.Trim());

            //DeviceRadarServer server = new DeviceRadarServer(merchId, storeId, dbip, dbPwd, port);
            //server.Init();
            DeviceRadarServer.Init(merchId, storeId, dbip, dbPwd, port);
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            string routeToken = "4952B360B3A4475FB62E09FE3D320FBA".ToLower();
            string mcuId = RedisCacheHelper.CreateDeviceMCUID(1);
            bool ret = DeviceRadarServer.SendMCUFunction(mcuId, routeToken);
            Response.Write(ret);
        }
    }
}