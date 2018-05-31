using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XCCloudService.BLL.XCCloud;
using XCCloudService.Model.XCCloud;

namespace XXCloudService.Utility
{
    public class DeviceRadarServer
    {
        public static XCCloudService.RadarService.HostServer hostServer;

        public static void Init(string merchID, string storeID, string DBIP, string DBPwd, int udpPort)
        {
            hostServer = new XCCloudService.RadarService.HostServer(merchID, storeID, DBIP, DBPwd, udpPort);
            hostServer.StartServer();

            hostServer.OnSetMCUSuccess += hostServer_OnSetMCUSuccess;
        }

        public static bool SendMCUFunction(string MCUID, string RouteToken)
        {
            return hostServer.SendMCUFunction(MCUID, RouteToken);
        }

        static void hostServer_OnSetMCUSuccess()
        {
            Base_DeviceInfo device = new Base_DeviceInfo();
            device.type = 0;
            device.Token = Guid.NewGuid().ToString("N");
            device.CmdType = 2;
            device.MCUID = "20180530600001";
            device.create_time = DateTime.Now;
            device.BarCode = "";
            Base_DeviceInfoService.I.Update(device);
        }
    }
}