using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudWebBar.CacheService;
using XCCloudWebBar.Common;

namespace XCCloudWebBar.Business.WeiXin
{
    public class WeiXinAccessTokenBusiness
    {
        public static void AddAccessToken(string accessToken,int expires)
        {
            WeiXinAccessTokenCache.Add(Constant.WeiXinAccessToken,accessToken,expires);
        }

        public static bool GetAccessToken(out string accessToken)
        {
            accessToken = string.Empty;
            object obj = WeiXinAccessTokenCache.GetValue(Constant.WeiXinAccessToken);
            if (obj == null)
            {
                return false;
            }
            else
            {
                accessToken = obj.ToString();
                return true;
            }
        }

        public static void AddJsapiTicket(string ticket, int expires)
        {
            WeiXinAccessTokenCache.Add(CommonConfig.WxPubApiTicket, ticket, expires);
        }

        public static bool GetJsapiTicket(out string ticket)
        {
            ticket = string.Empty;
            object obj = WeiXinAccessTokenCache.GetValue(CommonConfig.WxPubApiTicket);
            if (obj == null)
            {
                return false;
            }
            else
            {
                ticket = obj.ToString();
                return true;
            }
        }
    }
}
