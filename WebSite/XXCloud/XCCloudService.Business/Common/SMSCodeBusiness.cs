using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.CacheService;

namespace XCCloudService.Business.Common
{
    public class SMSCodeBusiness
    {
        public static bool SendSMSCode(string mobile, string templateId, out string errMsg)
        {
            errMsg = string.Empty;
            string key = string.Empty;
            //验证请求次数
            if (!FilterMobileBusiness.IsTestSMS && !FilterMobileBusiness.ExistMobile(mobile))
            { 
                if (!RequestTotalCache.CanRequest(mobile, ApiRequestType.SendSMSCode))
                {
                    errMsg = "已超过单日最大请求次数";
                    return false;
                }
                else
                {
                    RequestTotalCache.Add(mobile, ApiRequestType.SendSMSCode);
                }

                string smsCode = string.Empty;
                if (SMSBusiness.GetSMSCode(out smsCode))
                {
                    key = mobile + "_" + smsCode;
                    SMSCodeCache.Add(key, mobile, CacheExpires.SMSCodeExpires);
                    if (SMSBusiness.SendSMSCode(templateId, mobile, smsCode, out errMsg))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    errMsg = "发送验证码出错"; 
                    return false;
                }
            }
            else
            {
                key = mobile + "_" + "123456";
                SMSCodeCache.Add(key, mobile, CacheExpires.SMSCodeExpires);
            }

            return true;
        }
    }
}
