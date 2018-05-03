using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using XXCloudService.Api.HaoKu.Com;

namespace XXCloudService.Api.HaoKu
{
    public partial class ping : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                string shopId = Request["shopId"];
                string device = Request["device"];
                string sign = Request["sign"];

                //LogHelper.WriteLog("收到接口调用");

                Response.HeaderEncoding = Encoding.UTF8;
                Response.ContentType = "application/json";

                XmlDocument doc = new XmlDocument();
                doc.Load(Server.MapPath("/ShopList.xml"));

                foreach (XmlNode node in doc.SelectNodes("List/ShopID"))
                {
                    if (node.Attributes["HKID"].Value == shopId)
                    {
                        //找到对应的门店ID
                        string storeId = node.Attributes["XCStoreId"].Value;
                        //LogHelper.WriteLog("找到调用店铺信息：" + node.Attributes["HKID"].InnerText);
                        ////找到对应的店铺信息
                        //TransmiteObject.请求设备状态结构 status = new TransmiteObject.请求设备状态结构()
                        //{
                        //    DeviceID = device
                        //};
                        //object response = new object();
                        //string msg = "";
                        //if (ClientList.RequestCommand(node.Attributes["XCID"].InnerText, TransmiteEnum.互联网状态查询, status.ToArray(), out response, out msg))
                        //{
                        //    TransmiteObject.请求设备状态应答结构 o = response as TransmiteObject.请求设备状态应答结构;
                        //    if (o != null)
                        //    {
                        //        Response.Write("{\"return_code\":\"" + o.Result_Code + "\",\"deviceid\":\"" + o.DeviceID + "\",\"status\":\"" + o.DeviceStatus + "\",\"return_msg\":\"" + o.Result_Msg + "\"}");
                        //    }
                        //    else
                        //    {
                        //        Response.Write("{\"return_code\":\"02\",\"return_msg\":\"对象转换出错\"}");
                        //    }
                        //}
                        //else
                        //{
                        //    Response.Write("{\"return_code\":\"02\",\"return_msg\":\"" + msg + "\"}");
                        //}
                        //return;
                    }
                }

                //没有找到对应店铺信息

                Response.Write(ReturnModel.ReturnInfo(ReturnCode.F, "没有找到对应店铺信息"));
            }
            catch (Exception ex)
            {
                Response.Write(ReturnModel.ReturnInfo(ReturnCode.F, "服务调用出错"));
            }
        }
    }
}