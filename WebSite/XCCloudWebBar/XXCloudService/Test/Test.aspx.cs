﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using XCCloudWebBar.Business.Common;
using XCCloudWebBar.Business.XCCloud;
using XCCloudWebBar.Business.XCGame;
using XCCloudWebBar.Business.XCGameMana;
using XCCloudWebBar.CacheService;
using XCCloudWebBar.Common;
using XCCloudWebBar.Common.Enum;
using XCCloudWebBar.Model.CustomModel.Common;
using XCCloudWebBar.Model.Socket.UDP;
using XCCloudWebBar.Model.WeiXin.Message;
using XCCloudWebBar.Model.WeiXin.SAppMessage;
using XCCloudWebBar.SocketService.TCP.Model;
using XCCloudWebBar.SocketService.UDP;
using XCCloudWebBar.SocketService.UDP.Common;
using XCCloudWebBar.SocketService.UDP.Factory;
using XCCloudWebBar.SocketService.UDP.Security;
using XCCloudWebBar.WeiXin.Message;
using XCCloudWebBar.WeiXin.WeixinOAuth;



namespace XCCloudWebBar.Test
{
    public partial class Test : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string errMsg = string.Empty;
            string openId = "";
            OrderAuditDataModel dataModel = new OrderAuditDataModel();
            dataModel.AuditId = "10";
            dataModel.Remark = "Remark";
            dataModel.SendTime = System.DateTime.Now.ToString("yyyyMMddHHmmss");
            dataModel.SendUserName = "李俊杰";
            dataModel.SendUserId = "1001";
            MessageMana.PushMessage(WeiXinMesageType.XCCloudOrderAuditRequest,openId,dataModel,out errMsg);
        }


        protected void Button1_Click(object sender, EventArgs e)
        {
            //var entity = new
            //{
            //    userid = 0,
            //    pagename = "tokenStorage",
            //    processname = "tokenStorage",
            //    token = "fewfewf32432432432432423",
            //    signkey = "fewfewfewfewfewfewfewfwefwe"
            //};
            //System.Web.Script.Serialization.JavaScriptSerializer jss = new System.Web.Script.Serialization.JavaScriptSerializer();
            //string json = jss.Serialize(entity);

            //string url = "http://localhost:2860/Api/query.ashx?action=init";
            //string txt = Utils.HttpPost(url, json);
            //Response.Write(txt);

            string errMsg = string.Empty;
            //DataFactory.SendDataStoreQuery("100027", "10000", "778852013146", out errMsg);
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            RequestTestEntity entity = new RequestTestEntity();
            entity.shopId = "000001";
            entity.cardId = "123432432";
            entity.cardName = "ffefwewfewfew";
            entity.realName = "32432432432432";
            entity.idCard = "3243243243243223";
            entity.phone = "15618920033";
            entity.token = "fewfewf32432432432432423";
            entity.signKey = "fewfewfewfewfewfewfewfwefwe";
            System.Web.Script.Serialization.JavaScriptSerializer jss = new System.Web.Script.Serialization.JavaScriptSerializer();
            string json = jss.Serialize(entity);

            string url = "http://localhost:2860/Api/dashboard.ashx?action=todayRevenue";
            string txt = Utils.HttpPost(url, json);
            Response.Write(txt);
        }



        protected void Button4_Click(object sender, EventArgs e)
        {
            var entity = new
            {
                userid = 1,
                pagename = "index",
                processname = "test",
                token = "fewfewf32432432432432423",
                signKey = "fewfewfewfewfewfewfewfwefwe"
            };

            System.Web.Script.Serialization.JavaScriptSerializer jss = new System.Web.Script.Serialization.JavaScriptSerializer();
            string json = jss.Serialize(entity);

            string url = "http://192.168.1.119:8889/query?action=init";
            string txt = Utils.HttpPost(url, json);
            Response.Write(txt);
        }

    }

    public class RequestTestEntity
    {
        public string shopId {set;get;}
        public string cardId {set;get;}
        public string cardName {set;get;}
        public string realName {set;get;}
        public string idCard {set;get;}
        public string phone {set;get;}
        public string token {set;get;}
        public string signKey {set;get;}
    }
}