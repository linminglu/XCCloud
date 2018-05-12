using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using XCCloudService.WorkFlow;
using System.Web.SessionState;
using XCCloudService.Business.XCCloud;

namespace XXCloudService.Test.WorkFlowTest
{
    /// <summary>
    /// WorkFlowHandler 的摘要说明
    /// </summary>
    public class WorkFlowHandler : IHttpHandler, IRequiresSessionState
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            context.Response.ContentEncoding = Encoding.UTF8;

            Stream inputStream = context.Request.InputStream;
            Encoding encoding = context.Request.ContentEncoding;
            StreamReader streamReader = new StreamReader(inputStream, encoding);

            string strJson = streamReader.ReadToEnd();
            var p = JsonConvert.DeserializeObject<Dictionary<string, string>>(strJson);

            string acton = context.Request["method"];
            switch (acton)
            {
                case "fireRequest": fireRequest(p); break;
                case "fireStore": fireStore(p); break;


                case "fireGoodRequest": fireGoodRequest(p); break;
                case "fireGoodRequestV": fireGoodRequestV(p); break;
                case "fireGoodOut": fireGoodOut(p); break;
                case "fireGoodOutV": fireGoodOutV(p); break;
                case "fireGoodIn": fireGoodIn(p); break;
                case "fireGoodInV": fireGoodInV(p); break;
                case "fireCancel": fireCancel(p); break;
                case "fireClose": fireClose(p); break;
            }
        }

        private void fireRequest(Dictionary<string, string> p)
        {
            string workFlowToken = HttpContext.Current.Session["workflowtoken"].ToString();
            string handler = p["handler"];
            int count = Convert.ToInt32(p["count"]);

            StockWorkFlow stockWorkFlow = null;
            WorkFlowBusiness.Get<StockWorkFlow>(workFlowToken, out stockWorkFlow);
            stockWorkFlow.Request(handler, count);
        }

        private void fireStore(Dictionary<string, string> p)
        {
            string workFlowToken = HttpContext.Current.Session["workflowtoken"].ToString();
            StockWorkFlow stockWorkFlow = null;
            WorkFlowBusiness.Get<StockWorkFlow>(workFlowToken, out stockWorkFlow);
            stockWorkFlow.Store();
        }

        private void fireGoodRequest(Dictionary<string, string> p)
        {
            int eventId = Convert.ToInt32(p["eventId"]);
            int userId = Convert.ToInt32(p["userId"]);
            int requestType = Convert.ToInt32(p["requestType"]);

            var wf = new GoodReqWorkFlow(eventId, requestType);
            var errMsg = string.Empty;
            var ret = wf.Request(userId, out errMsg);
        }
        private void fireGoodRequestV(Dictionary<string, string> p)
        {
            int eventId = Convert.ToInt32(p["eventId"]);
            int userId = Convert.ToInt32(p["userId"]);
            int requestType = Convert.ToInt32(p["requestType"]);

            var wf = new GoodReqWorkFlow(eventId, requestType);
            var errMsg = string.Empty;
            var ret = wf.RequestVerify(userId, 1, string.Empty, out errMsg);
        }
        private void fireGoodOut(Dictionary<string, string> p)
        {
            int eventId = Convert.ToInt32(p["eventId"]);
            int userId = Convert.ToInt32(p["userId"]);
            int requestType = Convert.ToInt32(p["requestType"]);

            var wf = new GoodReqWorkFlow(eventId, requestType);
            var errMsg = string.Empty;
            var ret = wf.SendDeal(userId, out errMsg);
        }
        private void fireGoodOutV(Dictionary<string, string> p)
        {
            int eventId = Convert.ToInt32(p["eventId"]);
            int userId = Convert.ToInt32(p["userId"]);
            int requestType = Convert.ToInt32(p["requestType"]);

            var wf = new GoodReqWorkFlow(eventId, requestType);
            var errMsg = string.Empty;
            var ret = wf.SendDealVerify(userId, 1, string.Empty, out errMsg);
        }
        private void fireGoodIn(Dictionary<string, string> p)
        {
            int eventId = Convert.ToInt32(p["eventId"]);
            int userId = Convert.ToInt32(p["userId"]);
            int requestType = Convert.ToInt32(p["requestType"]);

            var wf = new GoodReqWorkFlow(eventId, requestType);
            var errMsg = string.Empty;
            var ret = wf.RequestDeal(userId, out errMsg);
        }
        private void fireGoodInV(Dictionary<string, string> p)
        {
            int eventId = Convert.ToInt32(p["eventId"]);
            int userId = Convert.ToInt32(p["userId"]);
            int requestType = Convert.ToInt32(p["requestType"]);

            var wf = new GoodReqWorkFlow(eventId, requestType);
            var errMsg = string.Empty;
            var ret = wf.RequestDealVerify(userId, 1, string.Empty, out errMsg);
        }
        private void fireCancel(Dictionary<string, string> p)
        {
            int eventId = Convert.ToInt32(p["eventId"]);
            int userId = Convert.ToInt32(p["userId"]);
            int requestType = Convert.ToInt32(p["requestType"]);

            var wf = new GoodReqWorkFlow(eventId, requestType);
            var errMsg = string.Empty;
            var ret = wf.Cancel(userId, out errMsg);
        }
        private void fireClose(Dictionary<string, string> p)
        {
            int eventId = Convert.ToInt32(p["eventId"]);
            int userId = Convert.ToInt32(p["userId"]);
            int requestType = Convert.ToInt32(p["requestType"]);

            var wf = new GoodReqWorkFlow(eventId, requestType);
            var errMsg = string.Empty;
            var ret = wf.Close(userId, out errMsg);
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}