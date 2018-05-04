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
using XCCloudService.BLL.XCCloud;
using XCCloudService.Common;
using XCCloudService.Model.CustomModel.XCCloud;
using XCCloudService.Model.XCCloud;
using XXCloudService.Api.HaoKu.Com;

namespace XXCloudService.Api.HaoKu
{
    public partial class charge : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                HaokuAPI api = new HaokuAPI();
                SortedDictionary<string, string> sPara = api.GetRequestQuerys(this.Context);

                string shopId = Request["shopId"];
                string cardId = Request["cardId"];
                string amount = Request["amount"];
                string source = Request["source"];
                string t = Request["t"];
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

                int raiseBalance = 0;
                if (!int.TryParse(amount, out raiseBalance))
                {
                    Response.Write(ReturnModel.ReturnInfo(ReturnCode.F, "转币数错误"));
                    return;
                }

                Response.HeaderEncoding = Encoding.UTF8;
                Response.ContentType = "application/json";

                XmlDocument doc = new XmlDocument();
                doc.Load(Server.MapPath("/Api/HaoKu/ShopList.xml"));

                foreach (XmlNode node in doc.SelectNodes("List/ShopID"))
                {
                    if (node.Attributes["HKID"].Value == shopId)
                    {
                        //找到对应的门店ID
                        string storeId = node.Attributes["XCStoreId"].Value;
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
                            MemberBaseModel member = null;
                            MemberBalancesModel memberBalance = null;
                            if (ds.Tables[0].Rows.Count > 0)
                            {
                                member = Utils.GetModelList<MemberBaseModel>(ds.Tables[0]).FirstOrDefault();
                            }
                            if (ds.Tables[1].Rows.Count > 0)
                            {
                                memberBalance = Utils.GetModelList<MemberBalancesModel>(ds.Tables[1]).FirstOrDefault(b => b.HKType == 1);
                            }
                            //增加币余额
                            if (member != null && memberBalance != null)
                            {
                                Data_Card_Balance model = Data_Card_BalanceService.I.GetModels(b => b.BalanceIndex == memberBalance.BalanceIndex).FirstOrDefault();
                                model.Banlance = memberBalance.Banlance + raiseBalance;
                                bool ret = Data_Card_BalanceService.I.Update(model);
                                if (ret)
                                {
                                    Response.Write(ReturnModel.ReturnInfo(ReturnCode.T, "转换成功"));
                                }
                                else
                                {
                                    Response.Write(ReturnModel.ReturnInfo(ReturnCode.F, "转换失败"));
                                }

                                return;
                            }
                        }
                    }
                }

                //没有找到对应店铺信息
                Response.Write(ReturnModel.ReturnInfo(ReturnCode.F, "没有找到对应店铺信息"));
            }
            catch
            {
                Response.Write(ReturnModel.ReturnInfo(ReturnCode.F, "服务调用出错"));
            }
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