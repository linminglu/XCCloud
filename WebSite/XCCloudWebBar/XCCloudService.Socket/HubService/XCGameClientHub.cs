using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using XCCloudWebBar.Business.Common;
using XCCloudWebBar.Business.XCGame;
using XCCloudWebBar.Business.XCGameMana;
using XCCloudWebBar.CacheService;
using XCCloudWebBar.Common;
using XCCloudWebBar.Common.Enum;
using XCCloudWebBar.Model.CustomModel.XCGame;
using XCCloudWebBar.Model.CustomModel.XCGameManager;

namespace XCCloudWebBar.SocketService.TCP.HubService
{


    [HubName("XCGameClientHub")]
    public class XCGameClientHub : Hub
    {
        public class CurrentUser
        {
            public string ConnectionId { set; get; }

            public string UserID { set; get; }

            public string Mobile { set; get; }
        }

        public static List<XCGameClientHub.CurrentUser> ConnectedUsers = new List<XCGameClientHub.CurrentUser>();
        /// <summary>
        /// 连接服务端
        /// </summary>
        /// <param name="token"></param>
        /// <param name="tokenType">0-mobileToken,1-memberToken</param>
        public void Connect(string token,string tokenType)
        {
            string mobile = string.Empty;
            var id = Context.ConnectionId;

            if (!CheckRole(token, tokenType,out mobile))
            {
                var model = new { callType = "connect", result_code = 0, result_msg = "用户没有授权" };
                Clients.Client(id).HubCall(model);
            }
            else
            {
                bool isNewUser = false;
                XCGameClientHub.CurrentUser currentUser = null;
                UpdateUser(id, token,mobile, out isNewUser, ref currentUser);
                var model = new { callType = "connect", result_code = 1, result_msg = "" };
                Clients.Client(id).HubCall(model); 
            }
        }

        private bool CheckRole(string token, string tokenType,out string mobile)
        {
            mobile = string.Empty;
            if (tokenType == "0")
            {
                if (MobileTokenBusiness.ExistToken(token,out mobile))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (tokenType == "1")
            {
                XCGameMemberTokenModel memberTokenModel = null;
                if (MemberTokenBusiness.ExistToken(token, ref memberTokenModel))
                {
                    mobile = memberTokenModel.Mobile;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private void UpdateUser(string connectionId, string userId,string mobile, out bool isNewUser, ref XCGameClientHub.CurrentUser currentUser)
        {
            XCGameClientHub.CurrentUser user = GetCurrentUserByConnectedId(connectionId);
            if (user == null)
            {
                XCGameClientHub.CurrentUser newUser = CreateUser(connectionId, userId, mobile);
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
            XCGameClientHub.CurrentUser user = GetCurrentUserByUserId(userID);
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


        private XCGameClientHub.CurrentUser CreateUser(string connectionId, string userID, string mobile)
        {
            XCGameClientHub.CurrentUser user = new XCGameClientHub.CurrentUser
            {
                ConnectionId = connectionId,
                UserID = userID,
                Mobile = mobile
            };
            ConnectedUsers.Add(user);
            return user;
        }


        private XCGameClientHub.CurrentUser GetCurrentUserByUserId(string userId)
        {
            return ConnectedUsers.Where<XCGameClientHub.CurrentUser>(p => p.UserID.Equals(userId)).FirstOrDefault<XCGameClientHub.CurrentUser>();
        }

        private CurrentUser GetCurrentUserByConnectedId(string connectedId)
        {
            return ConnectedUsers.Where<XCGameClientHub.CurrentUser>(p => p.ConnectionId.Equals(connectedId)).FirstOrDefault<XCGameClientHub.CurrentUser>();
        }

        private void RemoveCurrentUserByUserId(string userId)
        {
            var user = ConnectedUsers.Where<XCGameClientHub.CurrentUser>(p => p.UserID.Equals(userId)).FirstOrDefault<XCGameClientHub.CurrentUser>();
            ConnectedUsers.Remove(user);
        }

        public class SignalrServerToClient
        {
            static readonly IHubContext _myHubContext = GlobalHost.ConnectionManager.GetHubContext<XCGameClientHub>();
            public static void BroadcastMessage(object model)
            {

            }

            public static void Notify(string mobile,object model)
            {
                XCGameClientHub.CurrentUser currentUser = XCGameClientHub.ConnectedUsers.Where<XCGameClientHub.CurrentUser>(p => p.Mobile == mobile).FirstOrDefault<XCGameClientHub.CurrentUser>();
                if (currentUser != null)
                {
                    var obj = new { callType = "notify", result_code = 1, result_msg = "", result_data = model };
                    _myHubContext.Clients.Client(currentUser.ConnectionId).HubCall(obj); 
                }
            }
        }
    }   
}