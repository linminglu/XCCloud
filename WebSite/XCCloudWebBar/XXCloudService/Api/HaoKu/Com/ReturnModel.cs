using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace XXCloudService.Api.HaoKu.Com
{
    public class ReturnModel
    {
        public ReturnModel(int returnCode, string returnMsg)
        {
            this.return_code = returnCode;
            this.return_msg = returnMsg;
        }


        public static string ReturnInfo(int returnCode, string returnMsg)
        {
            ReturnModel model = new ReturnModel(returnCode, returnMsg);
            return JsonConvert.SerializeObject(model); 
        }
        public int return_code { set; get; }

        public string return_msg { get; set; }
    }

    public class ReturnCode
    {
        public static int W { get { return 0; } }
        public static int T { get { return 1; } }
        public static int F { get { return 2; } }
    }

    
}