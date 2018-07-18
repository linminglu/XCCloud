using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Newtonsoft.Json;
using System.Net;
using XCCloudWebBar.Business.XCCloudRS232;
using System.Data;
using XCCloudWebBar.Utility;
using XXCloudService.Utility.Info;
using XCCloudWebBar.Common.Enum;
using XCCloudWebBar.Common.Extensions;

namespace XXCloudService
{
    [HubName("MyHub")]
    public class MyHub : Hub
    {
        private readonly static List<Connect> _connections = new List<Connect>();

        #region 推送指令数和控制器集合
        /// <summary>
        /// 推送指令
        /// </summary>
        public void PushRoutersAndInst(string clientId, int level = 1, int routeId = 0)
        {
            Connect connect = _connections.FirstOrDefault(c => c.ClientId == clientId);
            if (connect == null)
            {
                if (_connections.Count > 10)
                {
                    _connections.RemoveAt(0);
                }
                connect = new Connect() { ClientId = clientId, ConnectionId = Context.ConnectionId };
                _connections.Add(connect);
            }

            PushRouteInstModel model = new PushRouteInstModel();
            try
            {
                InstructionsModel inst = new InstructionsModel();

                inst.Total = RadarService.HostServer.当前总指令数;
                inst.Querys = RadarService.HostServer.当前查询指令数;
                inst.Coins = RadarService.HostServer.当前币业务指令数;
                inst.ICCardCoinRepeats = RadarService.HostServer.当前IC卡进出币指令重复数;
                inst.ICCardQueryRepeats = RadarService.HostServer.当前IC卡查询重复指令数;
                inst.Receipts = RadarService.HostServer.当前小票指令数;
                inst.Errors = RadarService.HostServer.当前错误指令数;
                inst.Returns = RadarService.HostServer.当前返还指令数;
                model.Instructions = inst;

                //model.CurrVar = new CurrVar() { Level = level, CurrRouteId = routeId };

                if (level == 1)
                {
                    model.RouteList = UDPServer.GetRouteList();
                }
                else
                {
                    model.DeviceList = UDPServer.GetDeviceList(routeId);
                }

                Clients.Client(connect.ConnectionId).PullInst(model);
            }
            catch (Exception ex)
            {
                model.status = 0;
                model.msg = ex.Message;
                Clients.Client(connect.ConnectionId).PullInst(model);
            }
        }
        #endregion

        #region 远程事件

        #region 锁定/解锁(OK)
        /// <summary>
        /// 锁定/解锁
        /// </summary>
        /// <param name="mcuid"></param>
        /// <param name="isLock"></param>
        public void LockDevice(string mcuid, bool isLock)
        {
            string errMsg = string.Empty;
            ResultModel model = new ResultModel();
            if (!UDPServer.server.远程锁定与解锁指令(mcuid, isLock, out errMsg))
            {
                model.status = 0;
                model.msg = errMsg;
            }
            Clients.All.HubCall(model);
        } 
        #endregion

        #region 远程投币
        /// <summary>
        /// 远程投币
        /// </summary>
        /// <param name="routerToken"></param>
        /// <param name="headAddress"></param>
        /// <param name="icCard"></param>
        /// <param name="coins"></param>
        public void InCoins(string routerToken, string headAddress, string icCard, int coins)
        {
            ResultModel model = new ResultModel();
            string errMsg = string.Empty;
            UDPServer.server.远程投币指令("mcuid", 1, "pushTypeName", true, out errMsg);

            string jsonData = JsonConvert.SerializeObject(model);
            Clients.All.HubCall(model);
        } 
        #endregion

        #region 退币
        /// <summary>
        /// 退币
        /// </summary>
        /// <param name="routerToken"></param>
        /// <param name="headAddress"></param>
        public void OutCoins(string routerToken, string headAddress)
        {
            ResultModel model = new ResultModel();
            //UDPServer.server.远程退分指令(routerToken, headAddress);

            string jsonData = JsonConvert.SerializeObject(model);
            Clients.All.HubCall(model);

        } 
        #endregion

        #region 设置机头长地址
        /// <summary>
        /// 设置机头长地址
        /// </summary>
        /// <param name="routerToken"></param>
        /// <param name="mcuid"></param>
        public void SetDeviceSN(string routerToken, string mcuid)
        {
            ResultModel model = new ResultModel();
            //UDPServer.server.设置机头长地址(routerToken, mcuid);

            string jsonData = JsonConvert.SerializeObject(model);
            Clients.All.HubCall(model);
        } 
        #endregion

        #region 指定路由器复位(OK)
        /// <summary>
        /// 指定路由器复位
        /// </summary>
        /// <param name="routerToken"></param>
        /// <param name="mcuid"></param>
        public void ResetRouter(string segment)
        {
            string errMsg = string.Empty;
            ResultModel model = new ResultModel();
            if (!UDPServer.server.路由器复位(segment, out errMsg))
            {
                model.status = 0;
                model.msg = errMsg;
            }
            Clients.All.InitDeviceCall(model);
        } 
        #endregion

        public void SetMessageCommandType(int type, bool check)
        {
            RadarService.CommandType commandType = (RadarService.CommandType)type;
            if (check)
            {
                UDPServer.server.CommandTypeList.Add(commandType);
            }
            else
            {
                UDPServer.server.CommandTypeList.Remove(commandType);
            }
        }

        //public void ClearMessageCommand()
        //{
        //    UDPServer.server.CommandTypeList.Clear();
        //    UDPServer.server.ListenRouterToken = string.Empty;
        //    UDPServer.server.ListenDeviceAddress = string.Empty;
        //}

        public void SetListenRouter(string routerToken, bool isListen)
        {
            if (isListen)
            {
                UDPServer.server.ListenRouteAddress = routerToken;
            }
            else
            {
                UDPServer.server.ListenRouteAddress = string.Empty;
            }
        }

        public void SetListenDevice(string routerToken, string deiveceToken, bool isListen)
        {
            //if (isListen)
            //{
            //    UDPServer.server.ListenRouterToken = routerToken;
            //    UDPServer.server.ListenDeviceAddress = deiveceToken;
            //}
            //else
            //{
            //    UDPServer.server.ListenDeviceAddress = string.Empty;
            //}
        }
        #endregion

        #region 基础状态码
        public class ResultModel
        {
            public ResultModel(int _status = 200, string _msg = "")
            {
                this.status = _status;
                this.msg = _msg;
            }
            public int status { get; set; }

            public string msg { get; set; }
        }
        #endregion

        #region 控制器及指令数据
        public class PushRouteInstModel : ResultModel
        {
            public PushRouteInstModel()
            {
                this.Instructions = new InstructionsModel();
                this.RouteList = new List<RouterInfo>();
            }

            public InstructionsModel Instructions { get; set; }

            public List<RouterInfo> RouteList { get; set; }

            public List<DeviceModel> DeviceList { get; set; }
        }

        #region 指令
        public class InstructionsModel
        {
            public InstructionsModel()
            {
                Total = Querys = Coins = ICCardCoinRepeats = ICCardQueryRepeats = Receipts = Errors = Returns = 0;
            }
            /// <summary>
            /// 总指令数
            /// </summary>
            public int Total { get; set; }

            /// <summary>
            /// 查询指令数
            /// </summary>
            public int Querys { get; set; }

            /// <summary>
            /// 币业务指令数
            /// </summary>
            public int Coins { get; set; }

            /// <summary>
            /// IC卡查询重复指令数
            /// </summary>
            public int ICCardQueryRepeats { get; set; }

            /// <summary>
            /// IC卡进出币指令重复数
            /// </summary>
            public int ICCardCoinRepeats { get; set; }

            /// <summary>
            /// 小票指令数
            /// </summary>
            public int Receipts { get; set; }

            /// <summary>
            /// 错误指令数
            /// </summary>
            public int Errors { get; set; }

            /// <summary>
            /// 返还指令数
            /// </summary>
            public int Returns { get; set; }
        }
        #endregion

        #endregion

        #region 客户端连接对象
        public class Connect
        {
            public string ClientId { get; set; }

            public string ConnectionId { get; set; }
        } 
        #endregion
    }
}