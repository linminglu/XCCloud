using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web;
using XCCloudWebBar.Common;
using XCCloudWebBar.Common.Extensions;
using XCCloudWebBar.Common.Enum;
using XXCloudService.Utility.Info;
using RadarService;
using Microsoft.AspNet.SignalR;
using XXCloudService;
using XCCloudWebBar.Business.XCCloud;
using Newtonsoft.Json;
using RadarService.Info;

namespace XCCloudWebBar.Utility
{
    public class UDPServer
    {
        public static HostServer server;

        static string merchID = System.Configuration.ConfigurationManager.AppSettings["MerchID"];
        static string storeID = System.Configuration.ConfigurationManager.AppSettings["StoreID"];
        public static void Init()
        {            
            string dbConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["XCCloudDB"].ConnectionString;
            server = new HostServer(merchID, storeID, dbConnectString, 6066, "192.168.1.73", 12888, "192.168.1.73", 5753);
            server.StartServer();
            server.OnRadarDataShow += server_OnRadarDataShow;
            server.OnTransferSpeed += server_OnTransferSpeed;
        }

        static void server_OnTransferSpeed(int RecvSpeed, int SendSpeed)
        {
            string date = DateTime.Now.ToString("HH:mm:ss");

            TransferSpeedModel speed = new TransferSpeedModel() { RecvSpeed = RecvSpeed, SendSpeed = SendSpeed, Step = date };

            IHubContext _myHubContext = GlobalHost.ConnectionManager.GetHubContext<MyHub>();
            _myHubContext.Clients.All.NetworkSpeed(speed);
        }

        static void server_OnRadarDataShow(string ShowText)
        {
            MessageModel message = new MessageModel() { MsgContent = ShowText.Replace("=", "").Replace("\r\n", "<br />").Trim("<br />".ToCharArray()) };
            IHubContext _myHubContext = GlobalHost.ConnectionManager.GetHubContext<MyHub>();
            _myHubContext.Clients.All.ShowMessage(message);
        }

        class MessageModel
        {
            public string MsgContent { get; set; }
        }

        class TransferSpeedModel
        {
            public long RecvSpeed { get; set; }
            public long SendSpeed { get; set; }
            public string Step { get; set; }
        }

        #region 获取路由器列表
        public static List<RouterInfo> GetRouteList()
        {
            List<RouterInfo> RouteList = DeviceBusiness.GetDeviceList(merchID, storeID).Where(t => t.type == 8).Select(t => new RouterInfo
            {
                RouteId = t.ID,
                RouteName = t.DeviceName,
                RouteToken = t.Token,
                Segment = t.segment
            }).ToList();

            foreach (var route in RouteList)
            {
                var radar = server.GetRadarStatusByToken(route.RouteToken.ToLower());
                route.RemotePoint = GetEndPoint(radar.RemotePoint);
                route.UpdateTime = radar.UpdateTime.ToString("yyyy-MM-dd HH:mm:ss");
                route.Online = radar.Online;
                DateTime now = DateTime.Now;
                if (radar.UpdateTime.AddSeconds(10) < now)
                {
                    route.Online = false;
                }
            }
            RouteList = RouteList.OrderByDescending(t => t.Online).ToList();
            return RouteList;
        }

        static string GetEndPoint(string remotePoint)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(remotePoint))
                {
                    RemotePointModel endPoint = JsonConvert.DeserializeObject<RemotePointModel>(remotePoint);
                    string strPoint = endPoint.Address + ":" + endPoint.Port;
                    return strPoint;
                }
            }
            catch
            {
            }
            return "";
        }

        class RemotePointModel
        {
            public string Address { get; set; }
            public int Port { get; set; }
        } 
        #endregion

        #region 获取路由器绑定的设备列表
        public static List<DeviceModel> GetDeviceList(int routeId)
        {
            var queryDevices = DeviceBusiness.GetDeviceList(merchID, storeID).Where(t => t.BindDeviceID == routeId && t.type != 8).Select(t => new
            {
                DeviceId = t.ID,
                DeviceName = t.DeviceName,
                DeviceType = t.type,
                SiteName = t.SiteName,
                MCUID = t.MCUID,
                Address = t.Address
            }).ToList();

            List<DeviceModel> DeviceList = new List<DeviceModel>();
            foreach (var item in queryDevices)
            {
                DeviceModel model = new DeviceModel();
                var device = DeviceInfo.GetBufMCUIDDeviceInfo(item.MCUID);
                model.RouterId = routeId;
                model.DeviceId = item.DeviceId;
                model.DeviceName = item.DeviceName;
                model.SiteName = item.SiteName;
                model.DeviceType = device.类型.ToString();
                model.MCUID = item.MCUID;
                model.Address = device.机头短地址;
                //model.State = device.状态.在线状态 ? "在线" : "离线";
                model.State = GetDeviceState(device.状态);

                DeviceList.Add(model);
            }

            DeviceList = DeviceList.OrderBy(t => t.Address).ToList();
            return DeviceList;
        }

        public static string GetDeviceState(DeviceInfo.状态值 status)
        {
            if (status.在线状态)
            {
                if (status.非法币或卡报警)
                {
                    return DeviceStatusEnum.报警.ToDescription();
                }
                if (status.锁定机头)
                {
                    return DeviceStatusEnum.锁定.ToDescription();
                }
                if (status.读币器故障)
                {
                    return "读币器故障";
                }
                return DeviceStatusEnum.在线.ToDescription();
            }
            return DeviceStatusEnum.离线.ToDescription();
        }
        #endregion        
    }
}