﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web;
using XCCloudWebBar.Business.XCCloudRS232;
using XCCloudWebBar.Common;
using XCCloudWebBar.Common.Extensions;
using XCCloudWebBar.Common.Enum;
using XXCloudService.Utility.Info;
using XCCloudWebBar.Model.XCCloudRS232;
using RadarService;
using Microsoft.AspNet.SignalR;
using XXCloudService;

namespace XCCloudWebBar.Utility
{
    public class UDPServer
    {
        private static readonly Object thisMessageLock = new Object();

        delegate void DelegShowMsg(string msg);

        //public static UDPServerHelper server = new UDPServerHelper();

        public static HostServer server;

        public static List<string> 当前消息列表 = new List<string>();
        public static string NetworkRate = "0.0";
        public static int PerMinuteCount = 0;
        static bool isReckon = false;

        public static void Init()
        {
            string merchID = System.Configuration.ConfigurationManager.AppSettings["MerchID"];
            string storeID = System.Configuration.ConfigurationManager.AppSettings["StoreID"];
            string dbConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["XCCloudDB"].ConnectionString;
            server = new HostServer(merchID, storeID, dbConnectString, 6066, "192.168.1.73", 12888, "192.168.1.73", 5753);
            server.StartServer();
            server.OnRadarDataShow += server_OnRadarDataShow;
            server.OnTransferSpeed += server_OnTransferSpeed;
            ////server.Init(6066);
            ////server.OnShowMsg += new UDPServerHelper.消息显示(server_OnShowMsg);
            ////server.OnChangeState += new UDPServerHelper.状态更新(server_OnChangeState);

            //InitRouterDevices();

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
            XXCloudService.MyHub.InstMessage message = new MyHub.InstMessage() { MsgContent = ShowText.Replace("=", "").Replace("\r\n", "<br />").Trim("<br />".ToCharArray()) };
            IHubContext _myHubContext = GlobalHost.ConnectionManager.GetHubContext<MyHub>();
            _myHubContext.Clients.All.ShowMessage(message);
        }

        class TransferSpeedModel
        {
            public long RecvSpeed { get; set; }
            public long SendSpeed { get; set; }
            public string Step { get; set; }
        }

        //static void server_OnChangeState(Recv机头网络状态报告 cmd)
        //{
        //    string routeToken = cmd.RecvData.routeAddress;
        //    string headAddress = cmd.机头地址;
        //    int? deviceId = null;

        //    //获取控制器实体
        //    Base_DeviceInfo router = DeviceBusiness.GetDeviceModel(routeToken);
        //    Data_MerchDevice merchDevice = MerchDeviceBusiness.GetMerchModel(router.ID, headAddress);
        //    if (merchDevice != null)
        //    {
        //        deviceId = merchDevice.DeviceID;
        //    }

        //    if (deviceId == null)
        //    {
        //        Data_MerchSegment merchSegment = MerchSegmentBusiness.GetMerchSegmentModel(router.ID, headAddress);
        //        if (merchSegment == null)
        //        {
        //            deviceId = merchDevice.DeviceID;
        //        }
        //    }

        //    if (deviceId != null)
        //    {
        //        Base_DeviceInfo device = DeviceBusiness.GetDeviceModelById((int)deviceId);
        //        if (device != null)
        //        {
        //            List<HeadInfo.机头信息> headList = HeadInfo.GetAllHead();
        //            HeadInfo.机头信息 head = headList.FirstOrDefault(t => t.令牌 == device.Token);
        //            string strDeviceState = GetDeviceState(head);

        //            string state = DeviceStatusBusiness.GetDeviceState(device.Token);
        //            if (state != strDeviceState)
        //            {
        //                DeviceStatusBusiness.SetDeviceState(device.Token, strDeviceState);
        //            }
        //        }
        //    }
        //}

        /// <summary>
        /// 控制器列表
        /// </summary>
        public static List<RouterInfo> RouterList = new List<RouterInfo>();
        /// <summary>
        /// 分组列表
        /// </summary>
        public static List<GroupInfo> GroupList = new List<GroupInfo>();
        /// <summary>
        /// 外设列表
        /// </summary>
        public static List<DeviceModel> CoinDeviceList = new List<DeviceModel>();
        /// <summary>
        /// 终端列表
        /// </summary>
        public static List<DeviceModel> TerminalList = new List<DeviceModel>();

        public static void InitRouterDevices()
        {
            RouterList.Clear();
            GroupList.Clear();
            CoinDeviceList.Clear();
            TerminalList.Clear();

            DataTable table = DeviceBusiness.GetRouterDetail();
            if (table.Rows.Count > 0)
            {
                foreach (DataRow row in table.Rows)
                {
                    RouterInfo router = new RouterInfo();
                    router.RouterId = Convert.ToInt32(row["ID"].ToString());
                    router.RouterToken = row["DeviceToken"].ToString();
                    router.DeviceInfo.Token = row["DeviceToken"].ToString();
                    router.DeviceInfo.DeviceName = row["DeviceName"].ToString();
                    router.DeviceInfo.DeviceType = Convert.ToInt32(row["DeviceType"]);
                    router.DeviceInfo.SN = row["SN"].ToString();
                    router.DeviceInfo.Status = 1;

                    router.MerchInfo.MerchName = row["MerchName"].ToString();
                    router.MerchInfo.Mobile = row["Mobile"].ToString();
                    router.MerchInfo.State = Convert.ToInt32(row["State"]);

                    RouterList.Add(router);
                }
            }

            GroupList = GameBusiness.GetGameList().ToList().Select(g => new GroupInfo
            {
                RouterToken = g.DeviceID.IsNull() ? "" : RouterList.FirstOrDefault(r => r.RouterId == g.DeviceID).RouterToken,
                GroupId = g.GroupID,
                GroupName = g.GroupName,
                GroupType = ((GroupTypeEnum)g.GroupType).ToDescription()
            }).ToList();

            DataTable coinDeviceTable = DeviceBusiness.GetCoinDeviceList();
            if (coinDeviceTable.Rows.Count > 0)
            {
                foreach (DataRow row in coinDeviceTable.Rows)
                {
                    DeviceModel device = new DeviceModel();
                    device.RouterToken = row["RouterToken"].ToString();
                    device.DeviceId = Convert.ToInt32(row["DeviceId"].ToString());
                    device.DeviceToken = row["DeviceToken"].ToString();
                    device.DeviceName = row["DeviceName"].ToString();
                    device.DeviceType = ((GroupTypeEnum)Convert.ToInt32(row["DeviceType"].ToString())).ToDescription();
                    device.State = row["State"].ToString();
                    device.SN = row["SN"].ToString();
                    device.HeadAddress = row["HeadAddress"].ToString();

                    CoinDeviceList.Add(device);
                }
            }

            DataTable TerminalTable = DeviceBusiness.GetTerminalDeviceList();
            if (TerminalTable.Rows.Count > 0)
            {
                foreach (DataRow row in TerminalTable.Rows)
                {
                    DeviceModel device = new DeviceModel();
                    device.RouterToken = row["RouterToken"].ToString();
                    device.DeviceId = Convert.ToInt32(row["DeviceId"].ToString());
                    device.DeviceToken = row["DeviceToken"].ToString();
                    device.DeviceName = row["DeviceName"].ToString();
                    device.DeviceType = ((GroupTypeEnum)Convert.ToInt32(row["DeviceType"].ToString())).ToDescription();
                    device.State = row["State"].ToString();
                    device.SN = row["SN"].ToString();
                    device.HeadAddress = row["HeadAddress"].ToString();
                    device.GroupId = Convert.ToInt32(row["GroupID"].ToString());

                    TerminalList.Add(device);
                }
            }
        }

        //public static string GetDeviceState(HeadInfo.机头信息 head)
        //{
        //    if (head.状态.在线状态)
        //    {
        //        if (head.状态.锁定机头)
        //        {
        //            return DeviceStatusEnum.锁定.ToDescription();
        //        }
        //        if (head.状态.出币机或存币机正在数币)
        //        {
        //            return DeviceStatusEnum.工作中.ToDescription();
        //        }
        //        if (head.状态.读币器故障)
        //        {
        //            return DeviceStatusEnum.报警.ToDescription();
        //        }
        //        return DeviceStatusEnum.在线.ToDescription();
        //    }
        //    return DeviceStatusEnum.离线.ToDescription();
        //}
    }
}