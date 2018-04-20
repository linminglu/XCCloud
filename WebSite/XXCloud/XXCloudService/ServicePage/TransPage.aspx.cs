using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using XCCloudService.Common;

namespace XXCloudService.ServicePage
{
    public partial class TransPage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Request["action"]))
            {
                switch (Request["action"])
                {
                    case "m": M_Redirect(); break;
                    default: break;
                }
            }
        }

        private void M_Redirect()
        {
            string deviceNo = Request["deviceNo"];
            string url = CommonConfig.H5_M_WeiXinAuthRedirectUrl + "?deviceNo=" + deviceNo;
            Response.Redirect(url);
        }
    }
}