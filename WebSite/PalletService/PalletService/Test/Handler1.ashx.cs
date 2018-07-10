using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace Test
{
    /// <summary>
    /// Handler1 的摘要说明
    /// </summary>
    public class Handler1 : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            string action = context.Request["action"] ?? "";

            switch (action)
            {
                case "userLogin": barLogin(context); break;
            }
        }

        private void barLogin(HttpContext context)
        {
            string url = string.Format("http://localhost:3288/XCCloud/userinfo?action=barLogin&loginName={0}&password={1}&workStation={2}&dogId={3}",
                "lijunjie", "123456", "lijunjie", "1234");
            WebClient webClient = new WebClient();
            string json = webClient.DownloadString(url);
            context.Response.Write(json);
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