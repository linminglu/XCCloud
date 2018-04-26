using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using XCCloudService.BLL.CommonBLL;
using XCCloudService.Common;
using XCCloudService.Model.CustomModel.XCCloud;

namespace XXCloudService.Api.HaoKu
{
    public partial class charge : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            SortedDictionary<string, string> sPara = GetRequestPost();

            Response.Write(Request.QueryString.ToString());

            string shopId = Request["shopId"];
            string cardId = Request["cardId"];
            string amount = Request["amount"];
            string source = Request["source"];
            string t = Request["t"];
            string sign = Request["sign"];

            Response.HeaderEncoding = Encoding.UTF8;
            Response.ContentType = "application/json";

            XmlDocument doc = new XmlDocument();
            doc.Load(Server.MapPath("/Api/HaoKu/ShopList.xml"));

            foreach (XmlNode node in doc.SelectNodes("List/ShopID"))
            {
                if (node.Attributes["HKID"].InnerText == shopId)
                {
                    //找到对应的门店ID
                    string storeId = node.Attributes["XCID"].Value;
                    //通过门店ID和IC卡ID查找会员及余额信息
                    string storedProcedure = "GetMember";
                    SqlParameter[] parameters = new SqlParameter[4];
                    parameters[0] = new SqlParameter("@ICCardID", cardId);
                    parameters[1] = new SqlParameter("@StoreID", storeId);
                    parameters[2] = new SqlParameter("@Result", SqlDbType.Int);
                    parameters[2].Direction = System.Data.ParameterDirection.Output;
                    parameters[3] = new SqlParameter("@ErrMsg", SqlDbType.VarChar, 200);
                    parameters[3].Direction = System.Data.ParameterDirection.Output;
                    System.Data.DataSet ds = XCCloudBLL.GetStoredProcedureSentence(storedProcedure, parameters);
                    if (parameters[2].Value.ToString() == "1" && ds.Tables.Count > 0)
                    {
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            var baseMemberModel = Utils.GetModelList<MemberBaseModel>(ds.Tables[0]).ToList()[0];
                        }
                        if (ds.Tables[1].Rows.Count > 0)
                        {
                            var baseMemberModel = Utils.GetModelList<MemberBaseModel>(ds.Tables[0]).ToList()[0];
                        }
                        //增加币余额

                    }

                    


                    //找到对应的店铺信息
                    //TransmiteObject.互联网充值结构 recharge = new TransmiteObject.互联网充值结构()
                    //{
                    //    ICCardID = cardId,
                    //    Amount = Convert.ToUInt16(amount),
                    //    OrderID = source,
                    //    OrderTime = Convert.ToUInt64(t)
                    //};
                    //object response = new object();
                    //string msg = "";
                    //if (ClientList.RequestCommand(node.Attributes["XCID"].InnerText, TransmiteEnum.互联网充值, recharge.ToArray(), out response, out msg))
                    //{
                    //    Response.Write("{\"return_code\":\"01\",\"return_msg\":\"充值成功\"}");
                    //}
                    //else
                    //{
                    //    Response.Write("{\"return_code\":\"02\",\"return_msg\":\"" + msg + "\"}");
                    //}
                    return;
                }
            }

            ////没有找到对应店铺信息

            //Response.Write("{\"return_code\":\"02\",\"return_msg\":\"没有找到对应店铺信息\"}");
        }

        #region 获取好酷GET过来的请求消息，并以“参数名=参数值”的形式组成数组
        /// <summary>
        /// 获取好酷GET过来的请求消息，并以“参数名=参数值”的形式组成数组
        /// </summary>
        /// <returns>request回来的信息组成的数组</returns>
        public SortedDictionary<string, string> GetRequestPost()
        {
            int i = 0;
            SortedDictionary<string, string> sArray = new SortedDictionary<string, string>();
            NameValueCollection coll;
            //Load Form variables into NameValueCollection variable.
            coll = Request.QueryString;

            // Get names of all forms into a string array.
            String[] requestItem = coll.AllKeys;

            for (i = 0; i < requestItem.Length; i++)
            {
                sArray.Add(requestItem[i], Request.QueryString[requestItem[i]]);
            }

            return sArray;
        }
        #endregion
    }
}