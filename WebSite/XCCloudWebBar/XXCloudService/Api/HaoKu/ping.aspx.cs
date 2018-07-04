using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using XCCloudWebBar.BLL.XCCloud;
using XCCloudWebBar.Business.Common;
using XCCloudWebBar.Model.XCCloud;
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
                string deviceSN = Request["device"];
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

                        Base_StoreInfo store = Base_StoreInfoService.I.GetModels(t => t.StoreID == storeId).FirstOrDefault();
                        if (store == null)
                        {
                            Response.Write(ReturnModel.ReturnInfo(ReturnCode.F, "没有找到对应店铺信息"));
                            return;
                        }

                        Base_DeviceInfo device = Base_DeviceInfoService.I.GetModels(t => t.MCUID == deviceSN).FirstOrDefault();
                        if (device == null)
                        {
                            Response.Write(ReturnModel.ReturnInfo(ReturnCode.F, "没有找到对应设备"));
                            return;
                        }

                        //获取设备状态
                        string state = DeviceStateBusiness.GetDeviceState(storeId, device.MCUID);

                        if (state == "1")
                        {
                            Response.Write(ReturnModel.ReturnInfo(ReturnCode.T, "设备在线"));
                        }
                        else
                        {
                            Response.Write(ReturnModel.ReturnInfo(ReturnCode.F, "设备离线"));
                        }
                        
                        return;
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