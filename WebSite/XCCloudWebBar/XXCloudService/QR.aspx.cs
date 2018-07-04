using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using XCCloudWebBar.WeiXin.WeixinOAuth;

namespace XXCloudService
{
    public partial class QR : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Redirect(WeiXinQR.GetRQImageByMerch_Login(), false);
            return;
        }
    }
}