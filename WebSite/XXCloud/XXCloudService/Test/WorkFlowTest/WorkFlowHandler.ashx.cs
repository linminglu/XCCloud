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

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}