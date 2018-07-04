using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudWebBar.Common;
using XCCloudWebBar.Common.Enum;
using XCCloudWebBar.SocketService.TCP.Business;

namespace XCCloudWebBar.Utility
{
    public class ApplicationEnd
    {
        public static void End()
        {
            LogHelper.SaveLog(TxtLogType.SystemInit, "********************************Application End********************************");
            //TCPSocketEnd();
            UDPSocketEnd();
        }

        private static void TCPSocketEnd()
        {
            try
            { 
                TCPServiceBusiness.Server.End();
                LogHelper.SaveLog(TxtLogType.SystemInit, "TCP End Success...");
            }
            catch (Exception ex)
            {
                LogHelper.SaveLog(TxtLogType.SystemInit, "TCP End Fail..." + Utils.GetException(ex));
            }      
        }

        private static void UDPSocketEnd()
        {
            try
            {
                XCCloudWebBar.SocketService.UDP.Server.End();
                LogHelper.SaveLog(TxtLogType.SystemInit, "UDP End Success...");
            }
            catch (Exception ex)
            {
                LogHelper.SaveLog(TxtLogType.SystemInit, "UDP End Fail..." + Utils.GetException(ex));
            }
        }
    }
}
