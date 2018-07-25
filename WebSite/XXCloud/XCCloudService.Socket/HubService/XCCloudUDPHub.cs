﻿using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using XCCloudService.Business.Common;
using XCCloudService.Business.XCGameMana;
using XCCloudService.CacheService;
using XCCloudService.Common;
using XCCloudService.Common.Enum;
using XCCloudService.Model.CustomModel.XCCloud;
using XCCloudService.Model.CustomModel.XCGameManager;

namespace XCCloudService.SocketService.TCP.HubService
{


    [HubName("XCCloudUDPHub")]
    public class XCCloudUDPHub : Hub
    {
        public class CurrentUser
        {
            public string ConnectionId { set; get; }

            public string UserID { set; get; }

            public List<int> InsList { set; get; }

            public List<string> RadarTokenList { set; get; }

            public List<string> StoreList { set; get; }

            public List<string> SegmentList { set; get; }
        }

        public static List<XCCloudUDPHub.CurrentUser> ConnectedUsers = new List<XCCloudUDPHub.CurrentUser>();
        public void Connect(string userID, string insStr)
        {
            var id = Context.ConnectionId;

            XCCloudUserTokenModel tokenModel = XCCloudUserTokenCache.GetModel(userID);
            if (tokenModel == null)
            {
                var model = new { callType = "checkUserRole", result_code = 0, result_msg = "用户没有授权" };
                Clients.Client(id).HubCall(model);
                return;
            }

            bool isNewUser = false;
            XCCloudUDPHub.CurrentUser currentUser = null;
            UpdateUser(id, userID, out isNewUser, ref currentUser);
            if (isNewUser)
            {
                UpdateSettings(currentUser, "", "", insStr);
                Clients.Caller.onConnected(id, userID, insStr);
            }
            else
            {
                UpdateSettings(currentUser, "", "", insStr);
                Clients.Client(id).onExistUserConnected(id, userID);
            }
        }


        private void UpdateUser(string connectionId, string userId, out bool isNewUser, ref XCCloudUDPHub.CurrentUser currentUser)
        {
            XCCloudUDPHub.CurrentUser user = GetCurrentUserByConnectedId(connectionId);
            if (user == null)
            {
                XCCloudUDPHub.CurrentUser newUser = CreateUser(connectionId, userId);
                isNewUser = true;
                currentUser = newUser;
            }
            else
            {
                user.UserID = userId;
                isNewUser = false;
                currentUser = user;
            }
        }

        /// <summary>
        /// 登出
        /// </summary>
        public void Exit(string userID)
        {
            var id = Context.ConnectionId;
            XCCloudUDPHub.CurrentUser user = GetCurrentUserByUserId(userID);
            if (user == null)
            {
                Clients.Client(id).onExit(id, userID, 0, "监听未启动");
                return;
            }
            RemoveCurrentUserByUserId(userID);
            OnDisconnected(true);
            Clients.Client(id).onExit(id, userID,1,"");
        }

        /// <summary>
        /// 断开
        /// </summary>
        /// <returns></returns>
        public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled)
        {
            var item = ConnectedUsers.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            if (item != null)
            {
                ConnectedUsers.Remove(item);

                var id = Context.ConnectionId;
                Clients.All.onUserDisconnected(id, item.UserID);

            }
            return base.OnDisconnected(stopCalled);
        }


        private void UpdateSettings(XCCloudUDPHub.CurrentUser user, string storeIdStr, string segmentStr, string insStr)
        {
            user.StoreList.Clear();
            user.SegmentList.Clear();
            user.InsList.Clear();
            if (!string.IsNullOrEmpty(segmentStr))
            { 
                string[] segmentArr = Utils.SplitString(segmentStr, ",");
                for (int i = 0; i < segmentArr.Length; i++)
                {
                    if (!string.IsNullOrEmpty(segmentArr[i]))
                    {
                        user.SegmentList.Add(segmentArr[i]);
                    }
                }
            }

            if (!string.IsNullOrEmpty(storeIdStr))
            {
                string[] storeIdArr = Utils.SplitString(storeIdStr, ",");
                for (int i = 0; i < storeIdArr.Length; i++)
                {
                    if (!string.IsNullOrEmpty(storeIdArr[i]))
                    {
                        user.StoreList.Add(storeIdArr[i]);
                    }
                }
            }

            if (!string.IsNullOrEmpty(insStr))
            {
                string[] insArr = Utils.SplitString(insStr, ",");

                for (int i = 0; i < insArr.Length; i++)
                {
                    user.InsList.Add(Convert.ToInt32(insArr[i], 16));
                }
            }

            List<XCGameManaRadarMonitor> monitorList = null;
            GetRadarList(user, ref monitorList);
        }


        private XCCloudUDPHub.CurrentUser CreateUser(string connectionId, string userID)
        {
            XCCloudUDPHub.CurrentUser user = new XCCloudUDPHub.CurrentUser
            {
                ConnectionId = connectionId,
                UserID = userID,
                InsList = new List<int>(),
                RadarTokenList = new List<string>(),
                StoreList = new List<string>(),
                SegmentList = new List<string>()
            };
            ConnectedUsers.Add(user);
            return user;
        }

        private void GetRadarList(XCCloudUDPHub.CurrentUser user, ref List<XCGameManaRadarMonitor> monitorList, int type = 0)
        {
            string errMsg = string.Empty;
            string storeName = string.Empty;
            
            List<UDPClientItemBusiness.ClientItem> storeFilterClientList = null;
            List<UDPClientItemBusiness.ClientItem> segmentFilterClientList = null;
            
            if (user.StoreList.Count > 0)
            {
                storeFilterClientList = new List<UDPClientItemBusiness.ClientItem>();
                for (int i = 0; i < UDPClientItemBusiness.ClientList.Count; i++)
                {
                    for (int j = 0; j < user.StoreList.Count; j++)
                    {
                        if (UDPClientItemBusiness.ClientList[i].StoreID.Equals(user.StoreList[j]))
                        {
                            storeFilterClientList.Add(UDPClientItemBusiness.ClientList[i]);
                        }
                    }
                }
            }
            else
            {
                storeFilterClientList = UDPClientItemBusiness.ClientList;
            }

            if (user.SegmentList.Count > 0)
            {
                segmentFilterClientList = new List<UDPClientItemBusiness.ClientItem>();
                for (int i = 0; i < storeFilterClientList.Count; i++)
                {
                    for (int j = 0; j < user.SegmentList.Count; j++)
                    {
                        if (storeFilterClientList[i].Segment.Equals(user.SegmentList[j]))
                        {
                            segmentFilterClientList.Add(storeFilterClientList[i]);
                        }
                    }
                }
            }
            else
            {
                segmentFilterClientList = storeFilterClientList;
            }

            if (type == 1)
            {
                StoreBusiness storeBusiness = new StoreBusiness();
                monitorList = new List<XCGameManaRadarMonitor>();
                for (int i = 0; i < segmentFilterClientList.Count; i++)
                {
                    XCGameManaRadarMonitor monitor = new XCGameManaRadarMonitor();
                    monitor.StoreId = segmentFilterClientList[i].StoreID;
                    if (storeBusiness.GetStoreName(monitor.StoreId, out storeName, out errMsg))
                    {
                        monitor.StoreName = storeName;
                    }
                    else
                    {
                        monitor.StoreName = "门店不存在";
                    }
                    monitor.Segment = segmentFilterClientList[i].Segment;
                    monitor.Token = segmentFilterClientList[i].gID;
                    monitor.RegisterTime = segmentFilterClientList[i].curTime.ToString("yyyy-MM-dd HH:mm:ss");
                    monitor.HeatTime = segmentFilterClientList[i].HeatTime.ToString("yyyy-MM-dd HH:mm:ss");
                    monitor.Address = ((IPEndPoint)(segmentFilterClientList[i].remotePoint)).Address.ToString();
                    monitor.Port = ((IPEndPoint)(segmentFilterClientList[i].remotePoint)).Port;
                    monitor.StateName = getStateName(segmentFilterClientList[i].HeatTime);
                    monitorList.Add(monitor);
                }
            }

            for (int i = 0; i < segmentFilterClientList.Count; i++)
            {
                user.RadarTokenList.Add(segmentFilterClientList[i].gID);
            }
        }

        public void GetRadarList(string userId)
        {
            string connectionId = Context.ConnectionId;

            XCCloudUserTokenModel tokenModel = XCCloudUserTokenCache.GetModel(userId);
            if (tokenModel == null)
            {
                var model = new { callType = "getRadarList", result_code = 0, result_msg = "用户没有授权" };
                Clients.Client(connectionId).HubCall(model);
                return;
            }

            bool isNewUser = false;
            XCCloudUDPHub.CurrentUser currentUser = null;
            UpdateUser(connectionId, userId, out isNewUser, ref currentUser);

            string errMsg = string.Empty;
            string storeName = string.Empty;

            List<XCGameManaRadarMonitor> monitorList = null;
            GetRadarList(currentUser, ref monitorList, 1);

            var obj = new { callType = "getRadarList", result_code = 1, result_msg = "", result_data = monitorList };
            Clients.Client(currentUser.ConnectionId).HubCall(obj);
        }

        private string getStateName(DateTime heatTime)
        {
            if (heatTime < DateTime.Now.AddSeconds(0 - XCCloudService.Common.CommonConfig.RadarOffLineTimeLong))
            {
                return "离线";
            }
            else
            {
                return "在线";
            }
        }

        private XCCloudUDPHub.CurrentUser GetCurrentUserByUserId(string userId)
        {
            return ConnectedUsers.Where<XCCloudUDPHub.CurrentUser>(p => p.UserID.Equals(userId)).FirstOrDefault<XCCloudUDPHub.CurrentUser>();
        }

        private CurrentUser GetCurrentUserByConnectedId(string connectedId)
        {
            return ConnectedUsers.Where<XCCloudUDPHub.CurrentUser>(p => p.ConnectionId.Equals(connectedId)).FirstOrDefault<XCCloudUDPHub.CurrentUser>();
        }

        private void RemoveCurrentUserByUserId(string userId)
        {
            var user = ConnectedUsers.Where<XCCloudUDPHub.CurrentUser>(p => p.UserID.Equals(userId)).FirstOrDefault<XCCloudUDPHub.CurrentUser>();
            ConnectedUsers.Remove(user);
        }

        public class SignalrServerToClient
        {
            static readonly IHubContext _myHubContext = GlobalHost.ConnectionManager.GetHubContext<XCCloudUDPHub>();
            public static void BroadcastMessage(int insId, string name, string radar, string message, DateTime time)
            {
                try
                {
                    foreach (var user in XCCloudUDPHub.ConnectedUsers)
                    {
                        if (user.InsList.Where(p => p == insId).Count() > 0)
                        {
                            if (insId == 240)
                            {
                                //如果是雷达注册
                                _myHubContext.Clients.Client(user.ConnectionId).broadcastMessage(name, message, System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                            }
                            else if (user.RadarTokenList.Where(p => p == radar).Count() > 0)
                            {
                                if (user.InsList.Count > 0 && user.InsList.Where(p => p == insId).Count() == 0)
                                {
                                    return;
                                }

                                _myHubContext.Clients.Client(user.ConnectionId).broadcastMessage(name, message, System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                            }
                        }
                    }
                }
                catch (Exception e)
                {

                }
            }

            public static void BroadcastMessageByRadarRegister(string name, string storeId, string segment, string message, DateTime time)
            {
                try
                {
                    foreach (var user in XCCloudUDPHub.ConnectedUsers)
                    {
                        if (user.StoreList.Count > 0 && user.StoreList.Where(p => p == storeId).Count() == 0)
                        {
                            return;
                        }
                        if (user.SegmentList.Count > 0 && user.SegmentList.Where(p => p == segment).Count() == 0)
                        {
                            return;
                        }

                        if (user.InsList.Count > 0 && user.InsList.Where(p => p == 240).Count() == 0)
                        {
                            return;
                        }

                        _myHubContext.Clients.Client(user.ConnectionId).broadcastMessage(240, storeId, System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));

                    }
                }
                catch (Exception e)
                {

                }
            }


            public static void BroadcastMessage(int insId, string name, string radar, string message)
            {
                try
                {
                    foreach (var user in XCCloudUDPHub.ConnectedUsers)
                    {
                        if (user.InsList.Count > 0 && user.InsList.Where(p => p == insId).Count() == 0)
                        {
                            return;
                        }

                        if (user.RadarTokenList.Where(p => p == radar).Count() > 0)
                        {
                            _myHubContext.Clients.Client(user.ConnectionId).broadcastMessage(name, message, System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                        }
                    }
                }
                catch (Exception e)
                {

                }
            }
        }
    }


    
}