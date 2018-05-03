using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using XCCloudService.Common.Enum;
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
                    if (node.Attributes["HKID"].Value == shopId)
                    {
                        //找到对应的门店ID
                        string storeId = node.Attributes["XCStoreId"].Value;
                        string errMsg = string.Empty;

                        ////请求雷达处理出币
                        //if (!IConUtiltiy.DeviceOutputCoin(XCGameManaDeviceStoreType.Store, DevieControlTypeEnum.投币, storeId, mobile, icCardId, orderId, segment, mcuId, storePassword, 0, coins, gameRuleId, out errMsg))
                        //{
                        //    Response.Write(ReturnModel.ReturnInfo(ReturnCode.F, errMsg));
                        //}
                        //else
                        //{
                        //    Response.Write(ReturnModel.ReturnInfo(ReturnCode.T, ""));
                        //}                        
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