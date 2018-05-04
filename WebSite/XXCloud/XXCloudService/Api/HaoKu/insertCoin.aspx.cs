using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using XCCloudService.BLL.XCCloud;
using XCCloudService.Business.Common;
using XCCloudService.Common.Enum;
using XCCloudService.Model.Socket.UDP;
using XCCloudService.Model.XCCloud;
using XCCloudService.SocketService.UDP.Factory;
using XXCloudService.Api.HaoKu.Com;
using XXCloudService.Utility;

namespace XXCloudService.Api.HaoKu
{
    public partial class insertCoin : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                HaokuAPI api = new HaokuAPI();
                SortedDictionary<string, string> sPara = api.GetRequestQuerys(this.Context);

                string shopId = Request["shopId"];
                string deviceSN = Request["device"];
                string count = Request["count"];
                string source = Request["source"];
                string time = Request["t"];
                string sign = Request["sign"];

                string currSign = api.GetSign(sPara);
                if (currSign != sign)
                {
                    Response.Write(ReturnModel.ReturnInfo(ReturnCode.F, "签名错误"));
                    return;
                }

                if (string.IsNullOrEmpty(shopId))
                {
                    Response.Write(ReturnModel.ReturnInfo(ReturnCode.F, "shopId错误"));
                    return;
                }

                int raiseCount = 0;
                if (!int.TryParse(count, out raiseCount))
                {
                    Response.Write(ReturnModel.ReturnInfo(ReturnCode.F, "投币数错误"));
                    return;
                }

                //LogHelper.WriteLog("收到接口调用 URL:" + Request.Url.Query);

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
                        if(store == null)
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

                        //获取当前设备所在雷达设备
                        Base_DeviceInfo radar = Base_DeviceInfoService.I.GetModels(t => t.ID == device.BindDeviceID).FirstOrDefault();
                        if (radar == null)
                        {
                            Response.Write(ReturnModel.ReturnInfo(ReturnCode.F, "没有找到对应控制器设备"));
                            return;
                        }

                        //根据门店ID和段地址获取雷达
                        UDPClientItemBusiness.ClientItem item = XCCloudService.SocketService.UDP.ClientList.ClientListObj.Where(p => p.StoreID.Equals(storeId) && p.Segment == radar.segment).FirstOrDefault();
                        if (item == null)
                        {
                            Response.Write(ReturnModel.ReturnInfo(ReturnCode.F, "没有找到控制器设备"));
                            return;
                        }

                        //获取设备状态
                        string state = DeviceStateBusiness.GetDeviceState(storeId, device.MCUID);

                        if (state != "1")
                        {
                            Response.Write(ReturnModel.ReturnInfo(ReturnCode.F, "设备不在线"));
                            return;
                        }

                        string errMsg = string.Empty;

                        //请求雷达处理投币
                        string action = ((int)(DevieControlTypeEnum.投币)).ToString();
                        string sn = UDPSocketAnswerBusiness.GetSN();
                        string orderId = System.Guid.NewGuid().ToString("N");
                        DeviceControlRequestDataModel deviceControlModel = new DeviceControlRequestDataModel(storeId, orderId, source, device.segment, device.MCUID, action, 0, sn, orderId, store.Password, 0, "");
                        //MPOrderBusiness.AddTCPAnswerOrder(orderId, mobileTokenModel.Mobile, 0, action, memberTokenModel.ICCardId, deviceModel.StoreId);
                        //IconOutLockBusiness.AddByNoTimeLimit(mobileTokenModel.Mobile);
                        if (!DataFactory.SendDataToRadar(deviceControlModel, out errMsg))
                        {
                            Response.Write(ReturnModel.ReturnInfo(ReturnCode.F, errMsg));
                        }      
                        return;
                    }
                }

                Response.Write(ReturnModel.ReturnInfo(ReturnCode.F, "没有找到对应店铺信息"));
            }
            catch (Exception ex)
            {
                Response.Write(ReturnModel.ReturnInfo(ReturnCode.F, "服务调用出错"));
            }
        }
    }
}