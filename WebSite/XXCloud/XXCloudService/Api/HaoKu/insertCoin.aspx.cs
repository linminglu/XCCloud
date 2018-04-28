using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

namespace XXCloudService.Api.HaoKu
{
    public partial class insertCoin : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                string shopId = Request["shopId"];
                string device = Request["device"];
                string count = Request["count"];
                string source = Request["source"];
                string t = Request["t"];
                string sign = Request["sign"];

                //LogHelper.WriteLog("收到接口调用 URL:" + Request.Url.Query);

                Response.HeaderEncoding = Encoding.UTF8;
                Response.ContentType = "application/json";

                XmlDocument doc = new XmlDocument();
                doc.Load(Server.MapPath("/ShopList.xml"));

                foreach (XmlNode node in doc.SelectNodes("List/ShopID"))
                {
                    if (node.Attributes["HKID"].InnerText == shopId)
                    {
                        //LogHelper.WriteLog("找到投币调用店铺信息：" + node.Attributes["HKID"].InnerText);
                        ////找到对应的店铺信息
                        //TransmiteObject.互联网投币结构 recharge = new TransmiteObject.互联网投币结构()
                        //{
                        //    DeviceID = device,
                        //    Coin = Convert.ToUInt16(count),
                        //    OrderID = source,
                        //    OrderTime = Convert.ToUInt64(t)
                        //};
                        //object response = new object();
                        //string msg = "";
                        //if (ClientList.RequestCommand(node.Attributes["XCID"].InnerText, TransmiteEnum.互联网上分, recharge.ToArray(), out response, out msg))
                        //{
                        //    Response.Write("{\"return_code\":\"01\",\"return_msg\":\"投币成功\"}");
                        //}
                        //else
                        //{
                        //    Response.Write("{\"return_code\":\"02\",\"return_msg\":\"" + msg + "\"}");
                        //}
                        //return;
                    }
                }

                //没有找到对应店铺信息

                Response.Write("{\"return_code\":\"02\",\"return_msg\":\"没有找到对应店铺信息\"}");
            }
            catch (Exception ex)
            {
                //LogHelper.WriteLog(ex);
                Response.Write("{\"return_code\":\"02\",\"return_msg\":\"服务调用出错\"}");
            }

            Response.Write("{\"return_code\":\"02\",\"return_msg\":\"没有找到对应店铺信息\"}");
        }
    }
}