using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using XCCloudService.Base;
using XCCloudService.BLL.CommonBLL;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCGameManager;
using XCCloudService.Business.Common;
using XCCloudService.Business.XCGameMana;
using XCCloudService.CacheService;
using XCCloudService.Common;
using XCCloudService.Model.CustomModel.XCGameManager;

namespace XXCloudService.Api.XCGameMana
{
    /// <summary>
    /// DataOrder 的摘要说明
    /// </summary>
    public class DataOrder : ApiBase
    {

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MobileToken)]
        public object getDataOrder(Dictionary<string, object> dicParas)
        {
            try
            {
                string mobile = string.Empty;
                string PageSize = CommonConfig.DataOrderPageSize;//显示数量
                string PageIndex = dicParas.ContainsKey("pageIndex") ? dicParas["pageIndex"].ToString() : string.Empty;//页码
                string MobileToken = dicParas.ContainsKey("mobileToken") ? dicParas["mobileToken"].ToString() : string.Empty;//手机token
                string StoreName = dicParas.ContainsKey("storename") ? dicParas["storename"].ToString() : string.Empty;//门店名称
                string StartCoins = dicParas.ContainsKey("startcoins") ? dicParas["startcoins"].ToString() : string.Empty;//开始币数
                string EndCoins = dicParas.ContainsKey("endcoins") ? dicParas["endcoins"].ToString() : string.Empty;//结束币数
                string Buytype = dicParas.ContainsKey("buytype") ? dicParas["buytype"].ToString() : string.Empty;//购买类型
                string Startcreatetime = dicParas.ContainsKey("startcreatetime") ? dicParas["startcreatetime"].ToString() : string.Empty;//创建时间
                string Endcreatetime = dicParas.ContainsKey("endcreatetime") ? dicParas["endcreatetime"].ToString() : string.Empty;//创建时间
                string StartPrice = dicParas.ContainsKey("startprice") ? dicParas["startprice"].ToString() : string.Empty;//开始金额
                string EndPrice = dicParas.ContainsKey("endprice") ? dicParas["endprice"].ToString() : string.Empty;//结束金额
                if (!MobileTokenBusiness.ExistToken(MobileToken, out mobile))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "手机token无效");
                }
                string name = "";
                string CreateTime = "1";
                if (Startcreatetime != "" && Endcreatetime == "")
                {
                    Endcreatetime = Startcreatetime + " 23:59";

                }
                else if (Endcreatetime != "" && Startcreatetime == "")
                {
                    Startcreatetime = Endcreatetime + " 00:00";
                    Endcreatetime += " 23:59";
                }
                else if (Startcreatetime != "" && Endcreatetime != "")
                {
                    Endcreatetime += " 23:59";

                }
                else
                {
                    CreateTime = "";
                }
                string sql = "select row_number() over(order by CreateTime desc) as [No], OrderID,StoreID,Price,Fee,OrderType,PayStatus,CONVERT(varchar(100), PayTime, 23)as PayTime,CreateTime,Descript,Mobile,BuyType,StoreName,Coins from Data_Order where Mobile='" + mobile + "' and PayStatus='1' ";
                if (StartCoins != "")
                {
                    sql += " and Coins>=" + StartCoins + "";
                }
                if (EndCoins != "")
                {
                    sql += " and Coins<=" + EndCoins + "";
                }
                if (StartPrice != "")
                {
                    sql += " and Price>=" + StartPrice + "";
                }
                if (EndPrice != "")
                {
                    sql += " and Price<=" + EndPrice + "";
                }
                if (CreateTime != "")
                {
                    sql += " and CreateTime >='" + Convert.ToDateTime(Startcreatetime) + "' and CreateTime<='" + Convert.ToDateTime(Endcreatetime) + "'";
                }
                if (Buytype != "")
                {
                    sql += " and BuyType='" + Buytype + "'";
                }
                if (StoreName != "")
                {
                    sql += " and StoreName like '%" + StoreName + "%'";
                }
                sql += " order by CreateTime desc";
                System.Data.DataSet ds = XCGameManabll.ExecuteQuerySentence(sql, null);
                DataTable dt = ds.Tables[0];
                dt.Columns.Add("CreateTimes");
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DateTime time1 = Convert.ToDateTime(dt.Rows[i]["CreateTime"]);
                        string CreateTimes = time1.ToString("yyyy-MM-dd H:mm");
                        dt.Rows[i]["CreateTimes"] = CreateTimes;
                    }
                    var list = Utils.GetModelList<DataOrderModel>(ds.Tables[0]).ToList();
                    int PageCout = 0;
                    List<DataOrderModel> list1 = null;
                    if (Utils.GetPageList<DataOrderModel>(list, Convert.ToInt32(PageIndex), Convert.ToInt32(PageSize), out PageCout, ref list1))
                    {
                        string Totalcoins = list.Where<DataOrderModel>(p => p.PayStatus == 1).Sum<DataOrderModel>(p => p.Coins).ToString();
                        string Totalmoney = list.Where<DataOrderModel>(p => p.PayStatus == 1).Sum<DataOrderModel>(p => p.Price).ToString("0.00");
                        string Buycoins = list.Where<DataOrderModel>(p => p.PayStatus == 1 && p.BuyType == "购币").Sum<DataOrderModel>(p => p.Coins).ToString();
                        var TmpList = list.Where<DataOrderModel>(p => p.PayStatus == 1).ToList<DataOrderModel>();
                        List<string> BuyTypelist = new List<string>();
                        foreach (var model in TmpList)
                        {
                            if (BuyTypelist.Where<string>(p => p == model.BuyType).Count() == 0)
                            { 
                                BuyTypelist.Add(model.BuyType);
                            }
                        }
                        List<string> StoreNamelist = new List<string>();
                        foreach (var model in TmpList)
                        {
                            if (StoreNamelist.Where<string>(p => p == model.StoreName).Count() == 0)
                            {
                                StoreNamelist.Add(model.StoreName);
                            }
                        }
                        List<string> PayTypeIdlist = new List<string>();
                        List<string> PayTypeNamelist = new List<string>();
                        foreach (var model in TmpList)
                        {
                            if (PayTypeIdlist.Where<string>(p => p == model.OrderType.ToString()).Count() == 0)
                            {
                                PayTypeIdlist.Add(model.OrderType.ToString());
                                PayTypeNamelist.Add(GetPayTypeName(model.OrderType));
                            }
                        }

                        foreach(var model in list1)
                        {
                            model.PayStatusName = GetPayStatusName(model.PayStatus);
                            model.OrderTypeName = GetPayTypeName(model.OrderType);
                        }

                        DataOrderPageModel dataOrder = new DataOrderPageModel();
                        dataOrder.Lists = list1;
                        dataOrder.Page = PageCout.ToString();
                        dataOrder.Count = list.Count.ToString();
                        dataOrder.Totalcoins = Totalcoins;
                        dataOrder.Buycoins = Buycoins;
                        dataOrder.Totalmoney = Totalmoney;
                        dataOrder.BuyTypelist = BuyTypelist;
                        dataOrder.StoreNamelist = StoreNamelist;
                        dataOrder.PayTypelist = PayTypeNamelist;
                        return ResponseModelFactory<DataOrderPageModel>.CreateModel(isSignKeyReturn, dataOrder);
                    }
                }
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "未查询数据");
            }
            catch(Exception e)
            {
                throw e;
            }
        }


        private string GetPayTypeName(int payType)
        {
            switch (payType)
            {
                case 0: return "微信";
                case 1: return "支付宝";
                default: return "未知";
            }
        }

        private string GetPayStatusName(int payStatus)
        {
            switch (payStatus)
            {
                case 0: return "未支付";
                case 1: return "已支付";
                case 2: return "已退款";
                default: return "未知";
            }
        }
    }
}