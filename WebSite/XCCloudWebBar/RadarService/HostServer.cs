using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Data;
using RadarService.PublicHelper;
using RadarService.Notify;
using DSS;

namespace RadarService
{
    public class HostServer
    {
        public static int 当前总指令数 = 0;
        public static int 当前查询指令数 = 0;
        public static int 当前币业务指令数 = 0;
        public static int 当前IC卡查询重复指令数 = 0;
        public static int 当前IC卡进出币指令重复数 = 0;
        public static int 当前小票指令数 = 0;
        public static int 当前错误指令数 = 0;
        public static int 当前返还指令数 = 0;

        static XCHelper xcClient = null;
        DSS.Client.Client dsClient = null;
        class RouteData
        {
            public string Token { get; set; }
            public EndPoint RemotePoint { get; set; }
            public byte[] RecvData { get; set; }
        }
        string CloudServerIP = "";
        int CloudServerPort = 0;

        public delegate void 雷达数据显示(string ShowText);
        public event 雷达数据显示 OnRadarDataShow;
        void RadarDataShow(string ShowText)
        {
            if (OnRadarDataShow != null)
            {
                OnRadarDataShow(ShowText);
            }
        }

        public delegate void 雷达通讯速率(int RecvSpeed, int SendSpeed);
        public event 雷达通讯速率 OnTransferSpeed;
        void TransferSpeed(int RecvSpeed, int SendSpeed)
        {
            if (OnTransferSpeed != null)
            {
                OnTransferSpeed(RecvSpeed, SendSpeed);
            }
        }

        UDPServer udp = new UDPServer();
        Queue<RouteData> RecvList = new Queue<RouteData>();
        Thread recvProcT1 = null;
        int ServerPort = 0;
        string DSServerIP = "";
        int DSServerPort = 0;
        bool _initSuccess = false;
        string GetSystemParameter(string key)
        {
            DataAccess ac = new DataAccess();
            DataTable dt = ac.ExecuteQueryReturnTable("select * from Data_Parameters where system='chkPrintBarcode' and StoreID='" + PublicHelper.SystemDefiner.StoreID + "'");
            if (dt.Rows.Count > 0)
                return dt.Rows[0]["IsAllow"].ToString();
            return "";
        }

        public HostServer(string merchID, string storeID, string dbConnect, int udpPort, string cloudIP, int cloudPort, string dsServerIP, int dsServerPort)
        {
            DataAccess.SQLConnectString = dbConnect;
            PublicHelper.SystemDefiner.MerchID = merchID;
            PublicHelper.SystemDefiner.StoreID = storeID;
            ServerPort = udpPort;
            CloudServerIP = cloudIP;
            CloudServerPort = cloudPort;
            DSServerIP = dsServerIP;
            DSServerPort = dsServerPort;

            DataAccess ac = new DataAccess();
            DataTable dt = ac.ExecuteQueryReturnTable("select MerchSecret,ProxyType from Base_MerchantInfo where MerchID='" + merchID + "'");
            if (dt.Rows.Count > 0)
            {
                PublicHelper.SystemDefiner.AppSecret = dt.Rows[0]["MerchSecret"].ToString();
                PublicHelper.SystemDefiner.ProxyType = Convert.ToInt32(dt.Rows[0]["ProxyType"]);
            }
            dt = ac.ExecuteQueryReturnTable("select Password,StoreName from Base_StoreInfo where StoreID='" + storeID + "'");
            if (dt.Rows.Count > 0)
            {
                PublicHelper.SystemDefiner.StorePassword = dt.Rows[0]["Password"].ToString();
                PublicHelper.SystemDefiner.StoreName = dt.Rows[0]["StoreName"].ToString();
            }
            PublicHelper.SystemDefiner.ElecTicketValidDay = Convert.ToInt32(GetSystemParameter("txtTicketDate"));
            PublicHelper.SystemDefiner.PrintBarcode = (GetSystemParameter("chkPrintBarcode") == "1");
            PublicHelper.SystemDefiner.AllowProjectAddup = (GetSystemParameter("chkAddLeaveTime") == "1");
        }

        #region 路由器设备列表
        /// <summary>
        /// key->code,value->udp
        /// </summary>
        Dictionary<string, EndPoint> 路由器通讯列表 = new Dictionary<string, EndPoint>();
        /// <summary>
        /// key->code,value->segment
        /// </summary>
        Dictionary<string, string> 路由器段号校验列表 = new Dictionary<string, string>();
        /// <summary>
        /// key->segment,value->code
        /// </summary>
        Dictionary<string, string> 路由器段码对照表 = new Dictionary<string, string>();

        public void LoadAllRoute()
        {
            DataAccess ac = new DataAccess();
            DataTable dt = ac.ExecuteQueryReturnTable("select Token,segment from Base_DeviceInfo where type='8' and token <> ''");
            路由器段号校验列表.Clear();
            路由器段码对照表.Clear();
            foreach (DataRow row in dt.Rows)
            {
                路由器段号校验列表.Add(row["Token"].ToString().ToLower(), row["segment"].ToString().ToLower());
                路由器段码对照表.Add(row["segment"].ToString().ToLower(), row["Token"].ToString().ToLower());
            }
        }

        void UpdateRouteSegment(string code, string segment)
        {
            if (路由器段号校验列表.ContainsKey(code))
            {
                路由器段号校验列表[code] = segment;
            }
        }

        void UpdateRoute(string code, EndPoint p)
        {
            if (路由器通讯列表.ContainsKey(code))
            {
                路由器通讯列表[code] = p;
            }
            else
            {
                路由器通讯列表.Add(code, p);
            }
        }

        #endregion

        #region 重复性检查
        //重复性检查需要的结构
        class CRepeat
        {
            /// <summary>
            /// 路由器地址
            /// </summary>
            public string rAddress;
            /// <summary>
            /// 机头地址
            /// </summary>
            public string hAddress;
            /// <summary>
            /// 应答对象
            /// </summary>
            public object askObject;
            /// <summary>
            /// 应答类别
            /// </summary>
            public CommandType askType;
            /// <summary>
            /// 接收指令类别
            /// </summary>
            public CommandType recvType;
            /// <summary>
            /// 重复性流水号
            /// </summary>
            public UInt16 repeatID;
            /// <summary>
            /// 记录产生时间
            /// </summary>
            public DateTime createTime;
            /// <summary>
            /// 最后检查时间
            /// </summary>
            public DateTime checkTime;
            /// <summary>
            /// 发送记录时间
            /// </summary>
            public List<DateTime> SendDateTimeList = new List<DateTime>();
            /// <summary>
            /// IC卡号码
            /// </summary>
            public string ICCard;
        }
        //重复性检查超时检查线程
        static Thread repeatProcT = null;
        /// <summary>
        /// 重复性检查队列
        /// </summary>
        static List<CRepeat> repeatQueue = new List<CRepeat>();
        /// <summary>
        /// 重复性队列超时时间，秒
        /// </summary>
        static int repeatTimeoutSecond = 30;
        /// <summary>
        /// 指令重复性检查
        /// </summary>
        /// <param name="rAddress">路由器段号</param>
        /// <param name="hAddress">机头地址</param>
        /// <param name="SN">流水号</param>
        /// <returns>查找到重复性指令返回1,否则返回0,出错返回-1</returns>
        public static int CheckRepeat(string rAddress, string hAddress, string ICCard, CommandType ctype, ref object askObj, UInt16 SN)
        {
            try
            {
                lock (repeatQueue)
                {
                    //Console.WriteLine("rAddress：" + rAddress + "    hAddress：" + hAddress + "     ICCard：" + ICCard + "   SN：" + SN);
                    foreach (CRepeat item in repeatQueue)
                    {
                        if (item.repeatID == SN && item.rAddress == rAddress && item.hAddress == hAddress && item.ICCard == ICCard && item.recvType == ctype)
                        {
                            //switch (item.recvType)
                            //{
                            //    case CommandType.IC卡模式投币数据:
                            //        Command.Ask.AskIC卡模式进出币数据 ask = item.askObject as Command.Ask.AskIC卡模式进出币数据;
                            //        if (ask == null) continue;
                            //        break;
                            //}
                            askObj = item.askObject;
                            item.checkTime = DateTime.Now;
                            return 1;
                        }
                    }
                    return 0;
                }
            }
            catch (Exception ex)
            {
                //goto continueline;
                throw ex;
            }
        }

        public static void UpdateRepeatTime(string rAddress, string hAddress, UInt16 SN)
        {
            try
            {
                lock (repeatQueue)
                {
                    foreach (CRepeat item in repeatQueue)
                    {
                        if (item.repeatID == SN && item.rAddress == rAddress && item.hAddress == hAddress)
                        {
                            DateTime curDate = DateTime.Now;
                            item.SendDateTimeList.Add(curDate);
                        }
                    }
                }
            }
            catch
            {
                throw;
            }
        }
        public static void InsertRepeat(string rAddress, string hAddress, string ICCard, CommandType recvType, CommandType askType, object askObj, UInt16 SN, DateTime recvDate)
        {
            CRepeat c = new CRepeat();
            c.askObject = askObj;
            c.askType = askType;
            c.checkTime = DateTime.Now;
            c.createTime = recvDate;
            c.hAddress = hAddress;
            c.rAddress = rAddress;
            c.repeatID = SN;
            c.ICCard = ICCard;
            c.recvType = recvType;

            lock (repeatQueue)
            {
                //Console.WriteLine("正常指令 机头：" + hAddress + " 卡号：" + ICCard + " 流水号：" + SN + " 类型：" + askObj + " 接收时间：" + c.createTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                repeatQueue.Add(c);

            }
        }

        static void CheckRepeatTimeOut()
        {
            while (true)
            {
                lock (repeatQueue)
                {
                    if (repeatQueue.Count > 0)
                    {
                        try
                        {
                            foreach (CRepeat item in repeatQueue)
                            {
                                if (item.checkTime.AddSeconds(repeatTimeoutSecond) < DateTime.Now)
                                {
                                    repeatQueue.Remove(item);
                                    break;
                                }
                            }
                            Thread.Sleep(2);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }
                    else
                    {
                        Thread.Sleep(10);
                    }
                }
            }
        }
        #endregion

        #region 路由器设置基础指令集，不重发
        #region 设置长地址处理

        static string curMCUID = "";
        public bool SendMCUFunction(string MCUID, string RouteToken)
        {
            if (!路由器通讯列表.ContainsKey(RouteToken)) return false;
            curMCUID = MCUID;
            List<byte> send = new List<byte>();
            send.AddRange(Coding.ConvertData.StringToByte(MCUID));
            List<byte> sendRe = new List<byte>();
            //位置调换，低位在前
            for (int i = send.Count - 1; i >= 0; i--)
            {
                sendRe.Add(send[i]);
            }
            FrameData data = new FrameData();
            data.routeAddress = "FFFF";
            byte[] Senddata = Coding.ConvertData.GetFrameDataBytes(data, sendRe.ToArray(), CommandType.设置机头长地址);
            udp.SendData(Senddata, 路由器通讯列表[RouteToken]);
            return true;
        }

        public delegate void 长地址设置成功();
        public event 长地址设置成功 OnSetMCUSuccess;
        public void SetMCUSuccess()
        {
            if (OnSetMCUSuccess != null)
            {
                OnSetMCUSuccess();
            }
        }

        #endregion
        #region 路由器时间控制
        public bool 路由器时间控制(string date, int IsSet, string RouteToken)
        {
            if (!路由器通讯列表.ContainsKey(RouteToken)) return false;
            FrameData data = new FrameData();
            data.commandType = CommandType.路由器时间控制;
            data.routeAddress = "FFFF";
            List<byte> dataList = new List<byte>();
            dataList.Add((byte)IsSet);
            DateTime d;

            if (IsSet == 1)
            {
                byte[] dateBytes;
                d = Convert.ToDateTime(date);
                dateBytes = Coding.ConvertData.DateTimeBCD(d);
                dataList.AddRange(dateBytes);
            }

            byte[] Send = Coding.ConvertData.GetFrameDataBytes(data, dataList.ToArray(), CommandType.路由器时间控制);
            udp.SendData(Send, 路由器通讯列表[RouteToken]);
            return true;
        }
        #endregion
        #endregion

        #region 主动下发指令，带重发机制
        //主动发送队列缓存
        public static int BodySendBUFCount { get { return BodySendQueue.Count; } }
        static Queue<FrameBody> BodySendQueue = new Queue<FrameBody>();
        static Dictionary<string, List<FrameBody>> BodySendBUF = new Dictionary<string, List<FrameBody>>();
        FrameBody curBody = new FrameBody();
        Thread bodySendT = null;
        const int SendCountOff = 10;    //主发指令发送30次后未响应则强行清除队列

        /// <summary>
        /// 主动发数据结构
        /// </summary>
        class FrameBody
        {
            //发送数据结构
            public object sendObj = null;
            //数据类容
            public byte[] data;
            //发送类别
            public CommandType SendType;
            //应答类别
            public CommandType AskType;
            //发送时间，用于判定超时
            public DateTime SendTime;
            //发送次数  0 未发送 每次发送数量累加1
            public int SendCount = 0;
            //发送显示文字
            public string SendShow = "";
            /// <summary>
            /// 指令流水号号，不是每个指令都有
            /// </summary>
            public int SN = 0;
            //远程发送端点
            public EndPoint remoteP;
        }

        /// <summary>
        /// 清除当前主动发送数据
        /// </summary>
        public void ClearBodyCommand()
        {
            lock (BodySendBUF)
            {
                BodySendBUF.Clear();
            }
            lock (BodySendQueue)
            {
                BodySendQueue.Clear();
            }
        }

        /// <summary>
        /// 主动发送数据压栈
        /// </summary>
        /// <param name="data">数据包</param>
        /// <param name="sendType">主动发送类别</param>
        /// <param name="askType">期待应答类别</param>
        public static void BodySend(object sendObj, byte[] data, CommandType sendType, CommandType askType, string SendShowString, int SN, string headAddress, EndPoint p)
        {
            headAddress = headAddress.ToLower();
            FrameBody f = new FrameBody()
            {
                sendObj = sendObj,
                AskType = askType,
                data = data,
                SendType = sendType,
                SendShow = SendShowString,
                SN = SN,
                remoteP = p
            };
            if (!BodySendBUF.ContainsKey(headAddress))
            {
                BodySendBUF.Add(headAddress, new List<FrameBody>());
            }
            BodySendBUF[headAddress].Add(f);
        }

        /// <summary>
        /// 主动发送数据压栈
        /// </summary>
        /// <param name="data">数据包</param>
        /// <param name="sendType">主动发送类别</param>
        /// <param name="askType">期待应答类别</param>
        public static void BodySendEx(object sendObj, byte[] data, CommandType sendType, CommandType askType, string SendShowString)
        {
            FrameBody f = new FrameBody()
            {
                sendObj = sendObj,
                AskType = askType,
                data = data,
                SendType = sendType,
                SendShow = SendShowString
            };

            BodySendQueue.Enqueue(f);
        }

        static object ClearBodyItem(CommandType cType, object data, int SN, string headAddress)
        {
            //Console.WriteLine("清除");
            headAddress = headAddress.ToLower();
            object res = null;
            lock (BodySendBUF)
            {
                if (BodySendBUF.ContainsKey(headAddress))
                {
                    var bodys = BodySendBUF[headAddress].Where(p => p.AskType == cType && p.SN == SN);
                    foreach (FrameBody f in bodys)
                    {
                        BodySendBUF[headAddress].Remove(f);
                        return f;
                    }
                }
            }
            return res;
        }
        static object ClearBodyItem(CommandType cType, object data, string headAddress)
        {
            headAddress = headAddress.ToLower();
            //Console.WriteLine("清除");
            object res = null;
            if (BodySendBUF.ContainsKey(headAddress))
            {
                var bodys = BodySendBUF[headAddress].Where(p => p.AskType == cType);
                foreach (FrameBody f in bodys)
                {
                    BodySendBUF[headAddress].Remove(f);
                    return f;
                }
            }
            return res;
        }

        static object ClearBodyItem(CommandType cType, object data)
        {
            //Console.WriteLine("清除");
            object res = null;
            //lock (BodySendBUF)
            {
            continueline:
                foreach (string headAddress in BodySendBUF.Keys)
                {
                    var bodys = BodySendBUF[headAddress].Where(p => p.AskType == cType);
                    foreach (FrameBody f in bodys)
                    {
                        BodySendBUF[headAddress].Remove(f);
                        goto continueline;
                    }
                }
            }
            return res;
        }
        void BodySendFun()
        {
            while (true)
            {
            continueline:
                //所有未发送的指令
                try
                {
                    foreach (string hAddress in BodySendBUF.Keys)
                    {
                        if (BodySendBUF[hAddress].Count > 0)
                        {
                            FrameBody f = BodySendBUF[hAddress][0];
                            if ((f.SendType != CommandType.投币机投币指令 && f.SendCount > SendCountOff) || (f.SendType == CommandType.投币机投币指令 && f.SendCount > 100))
                            {
                                BodySendBUF[hAddress].Remove(f);
                                goto continueline;
                            }

                            udp.SendData(f.data, f.remoteP);
                            f.SendCount++;
                            f.SendTime = DateTime.Now;

                            //Console.WriteLine("发送：  " + f.SendCount);
                            Thread.Sleep(20);
                        }
                    }
                }
                catch (Exception ex)
                {
                    //LogHelper.WriteLog(ex);
                    Thread.Sleep(10);
                }
                Thread.Sleep(1000);
            }
        }
        #endregion
        public bool StartServer(string logPath = "")
        {
            if (PublicHelper.SystemDefiner.InitSuccess) return false;
            string checkdate = XCCloudSerialNo.SerialNoHelper.StringGet("营业日期");
            if (checkdate == null)
                XCCloudSerialNo.SerialNoHelper.StringSet("营业日期", DateTime.Now.ToString("yyyy-MM-dd"));

            if (logPath.Trim() != "")
                PublicHelper.SystemDefiner.LogPath = logPath;

            try
            {
                LoadAllRoute();
                Info.DeviceInfo.Init();
                Info.GameInfo.Init();
                udp.Init(ServerPort);
                udp.OnInternetDataRecv += udp_OnInternetDataRecv;
                udp.OnTransferSpeed += udp_OnTransferSpeed;
                if (recvProcT1 == null)
                {
                    recvProcT1 = new Thread(new ThreadStart(tRunProcess));
                    recvProcT1.IsBackground = true;
                    recvProcT1.Name = "后台接收队列处理线程1";
                    recvProcT1.Start();
                }
                if (bodySendT == null)
                {
                    bodySendT = new Thread(new ThreadStart(BodySendFun));
                    bodySendT.IsBackground = true;
                    bodySendT.Name = "主动发送内容线程";
                    bodySendT.Start();
                }

                xcClient = new XCHelper(CloudServerIP, CloudServerPort, PublicHelper.SystemDefiner.StoreID, PublicHelper.SystemDefiner.StorePassword);
                xcClient.OnDeviceControl += xcClient_OnDeviceControl;   //设备远程控制
                xcClient.OnGameInfoChange += xcClient_OnGameInfoChange; //游戏机变更同步
                xcClient.OnDeviceReset += xcClient_OnDeviceReset;       //卡头复位同步
                xcClient.Init();    //初始化
                xcClient.RegistRouteDevice();   //服务在互联网服务上注册

                dsClient = new DSS.Client.Client(DSServerIP, DSServerPort, PublicHelper.SystemDefiner.StoreID, PublicHelper.SystemDefiner.MerchID, PublicHelper.SystemDefiner.AppSecret);
                dsClient.Init();
                _initSuccess = true;
            }
            catch (Exception ex)
            {
                LogHelper.LogHelper.WriteLog(ex);
            }
            return false;
        }

        void udp_OnTransferSpeed(int RecvSpeed, int SendSpeed)
        {
            TransferSpeed(RecvSpeed, SendSpeed);
        }

        void xcClient_OnDeviceReset(JsonObject.DeviceResetRequest device)
        {
            修改机头地址(device.mcuid, "ff");
            xcClient.AskDeviceReset("1", "", device.sn);
        }

        void xcClient_OnGameInfoChange(JsonObject.GameInfoChangeRequest gameChange)
        {
            int gameIndex = Convert.ToInt32(gameChange.gameindex);
            修改游戏机参数(gameIndex);
            xcClient.AskGameInfoChange("1", "", gameChange.sn);
        }
        /// <summary>
        /// 调用汇报设备状态
        /// </summary>
        /// <param name="head"></param>
        /// <param name="status"></param>
        public static void ChangeDeviceStatus(List<JsonObject.StatusItem> changeList)
        { xcClient.ChangeDeviceStatus(changeList); }
        /// <summary>
        /// 设备远程控制
        /// </summary>
        /// <param name="contorl"></param>
        void xcClient_OnDeviceControl(JsonObject.DeviceControl contorl)
        {
            Info.DeviceInfo.机头信息 head = XCCloudSerialNo.SerialNoHelper.StringGet<Info.DeviceInfo.机头信息>(contorl.mcuid);
            if (head != null)
            {
                if (head.状态.在线状态)
                {
                    if (contorl.action == "1" || contorl.action == "2" || contorl.action == "6")
                    {
                        int zkzyvalue = 0;
                        int.TryParse(contorl.zkzy, out zkzyvalue);
                        head.状态.出币机或存币机正在数币 = true;
                        head.订单编号 = contorl.orderid;
                        if (contorl.iccardid != null)
                            head.当前卡片号 = contorl.iccardid;
                        string info = "";
                        if (contorl.action == "1") info = "远程提币";
                        else if (contorl.action == "2") info = "远程存币";
                        else if (contorl.action == "6") info = "远程投币";

                        Console.WriteLine("莘宸服务器接收【" + info + "】");

                        Command.Ask.Ask远程投币上分数据 a = new Command.Ask.Ask远程投币上分数据(head, Convert.ToInt32(contorl.count), info, SystemDefiner.雷达主发流水号, (zkzyvalue == 1), 路由器通讯列表[路由器段码对照表[head.路由器段号]]);

                        List<JsonObject.StatusItem> changeList = new List<JsonObject.StatusItem>();
                        JsonObject.StatusItem item = new JsonObject.StatusItem();
                        item.mcuid = head.设备序列号;
                        item.status = "出币中";
                        changeList.Add(item);
                        xcClient.ChangeDeviceStatus(changeList);
                        xcClient.AskControl(contorl.sn, "1", "");
                    }
                    else if (contorl.action == "7")
                    {
                        Console.WriteLine("莘宸服务器接收【远程退币】");
                        Command.Ask.Ask远程被动退分解锁指令 function = new Command.Ask.Ask远程被动退分解锁指令(head, 路由器通讯列表[路由器段码对照表[head.路由器段号]]);
                    }
                    else
                    {
                        xcClient.AskControl(contorl.sn, "0", "未知操作");
                    }
                }
                else
                {
                    xcClient.AskControl(contorl.sn, "0", "设备不在线");
                }
            }
            else
            {
                xcClient.AskControl(contorl.sn, "0", "未知设备");
            }
        }
        void udp_OnInternetDataRecv(byte[] data, EndPoint p)
        {
            byte[] recvData = data.Skip(16).Take(data.Length - 16).ToArray();
            string code = Coding.ConvertData.BytesToString(data.Take(16).ToArray()).Replace(" ", "").ToLower();
            UpdateRoute(code, p);
            if (路由器段号校验列表.ContainsKey(code))
            {
                RouteData r = new RouteData();
                r.RecvData = recvData;
                r.RemotePoint = p;
                r.Token = code;
                RecvList.Enqueue(r);
            }
        }
        void tRunProcess()
        {
            while (true)
            {
                try
                {
                    if (RecvList.Count > 0)
                    {
                        RouteData route = RecvList.Dequeue();
                        ProcessCommad(route);
                    }
                    Thread.Sleep(5);
                }
                catch (Exception ex)
                {
                    LogHelper.LogHelper.WriteLog(ex);
                    Thread.Sleep(50);
                }
            }
        }

        void ProcessCommad(RouteData route)
        {
            try
            {
                byte[] data = route.RecvData;
                FrameData f = new FrameData(data);
                if (路由器段号校验列表.ContainsKey(route.Token))
                {
                    //if (f.routeAddress != 路由器段号校验列表[route.Token]) return;   //如果收到的路由器编号不一致为非法数据，直接退出
                }
                DateTime processTime = DateTime.Now;
                Console.WriteLine(string.Format("接收：{0}", Coding.ConvertData.BytesToString(f.recvData)));
                当前总指令数++;
                switch (f.commandType)
                {
                    case CommandType.机头网络状态报告:
                        {
                            Command.Recv.Recv机头网络状态报告 function = new Command.Recv.Recv机头网络状态报告(f);
                            udp.SendData(function.SendData, route.RemotePoint);
                            StringBuilder sb = new StringBuilder();
                            sb.Append("=============================================\r\n");
                            sb.AppendFormat("{0:yyyy-MM-dd HH:mm:ss.fff}  收到数据\r\n", processTime);
                            sb.Append(Coding.ConvertData.BytesToString(f.recvData) + Environment.NewLine);
                            sb.AppendFormat("指令类别：{0}\r\n", f.commandType);
                            RadarDataShow(sb.ToString());
                            function.SendDate = DateTime.Now;
                            sb = new StringBuilder();
                            sb.Append("=============================================\r\n");
                            sb.AppendFormat("{0:yyyy-MM-dd HH:mm:ss.fff}  发送数据\r\n", function.SendDate);
                            sb.Append(Coding.ConvertData.BytesToString(function.SendData) + Environment.NewLine);
                            sb.AppendFormat("指令类别：{0}\r\n", function.RecvData.commandType);
                            sb.AppendFormat("路由器地址：{0}\r\n", function.RecvData.routeAddress);
                            RadarDataShow(sb.ToString());
                        }
                        break;
                    case CommandType.设置机头长地址应答:
                        {
                            SetMCUSuccess();
                            StringBuilder sb = new StringBuilder();
                            sb.Append("=============================================\r\n");
                            sb.AppendFormat("{0:yyyy-MM-dd HH:mm:ss.fff}  收到数据\r\n", DateTime.Now);
                            sb.Append(Coding.ConvertData.BytesToString(f.recvData) + Environment.NewLine);
                            sb.AppendFormat("指令类别：{0}\r\n", f.commandType);
                            sb.AppendFormat("路由器地址：{0}\r\n", f.routeAddress);
                            RadarDataShow(sb.ToString());
                        }
                        break;
                    case CommandType.设置路由器地址应答:
                        {
                            string routeVer = Encoding.ASCII.GetString(f.commandData);
                            UpdateRouteSegment(route.Token, f.routeAddress);
                            ClearBodyItem(CommandType.设置路由器地址应答, null);
                            StringBuilder sb = new StringBuilder();
                            sb.Append("=============================================\r\n");
                            sb.AppendFormat("{0}  收到数据\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                            sb.Append(Coding.ConvertData.BytesToString(f.recvData) + Environment.NewLine);
                            sb.AppendFormat("指令类别：{0}\r\n", f.commandType);
                            sb.AppendFormat("路由器地址：{0}\r\n", f.routeAddress);
                            sb.AppendFormat("版本号：{0}\r\n", routeVer);
                            RadarDataShow(sb.ToString());
                        }
                        break;
                    case CommandType.机头地址动态分配:
                        {
                            Command.Recv.Recv机头地址动态分配 function = new Command.Recv.Recv机头地址动态分配(f);
                            RadarDataShow(function.GetRecvData(processTime));
                            if (function.SendData != null)
                            {
                                udp.SendData(function.SendData, route.RemotePoint);
                                function.SendDataTime = DateTime.Now;
                                RadarDataShow(function.GetSendData());
                            }
                        }
                        break;
                    case CommandType.游戏机参数申请:
                        {
                            Command.Recv.Recv游戏机参数申请 function = new Command.Recv.Recv游戏机参数申请(f);
                            RadarDataShow(function.GetRecvData(processTime));
                            if (function.SendData != null)
                            {
                                udp.SendData(function.SendData, route.RemotePoint);
                                function.SendDataTime = DateTime.Now;
                                RadarDataShow(function.GetSendData());
                            }
                        }
                        break;
                    case CommandType.液晶卡头扩展信息下发应答指令:
                        {
                            string headAddress = Coding.ConvertData.Hex2String(f.commandData[0]);
                            int SN = BitConverter.ToUInt16(f.commandData, 1);
                            ClearBodyItem(f.commandType, null, SN, headAddress);
                            StringBuilder sb = new StringBuilder();
                            sb.Append("=============================================\r\n");
                            sb.AppendFormat("{0:yyyy-MM-dd HH:mm:ss.fff}  收到数据\r\n", processTime);
                            sb.Append(Coding.ConvertData.BytesToString(f.recvData) + Environment.NewLine);
                            sb.AppendFormat("指令类别：{0}\r\n", f.commandType);
                            sb.AppendFormat("流水号：{0}\r\n", SN);
                            RadarDataShow(sb.ToString());
                        }
                        break;
                    case CommandType.液晶卡头扩展信息请求指令:
                        {
                            Command.Recv.Recv液晶卡头扩展信息请求指令 function = new Command.Recv.Recv液晶卡头扩展信息请求指令(f, processTime);
                            udp.SendData(function.SendData, route.RemotePoint);
                            function.SendDataTime = DateTime.Now;
                            Console.WriteLine(function.GetRecvData(processTime));
                            Console.WriteLine(function.GetSendData());
                            RadarDataShow(function.GetRecvData(processTime));
                            RadarDataShow(function.GetSendData());
                            if (!function.isRepeat)
                            {
                                Command.Ask.Ask液晶卡头扩展信息下发指令 ask = new Command.Ask.Ask液晶卡头扩展信息下发指令(function.head, function.参数, route.RemotePoint);
                            }
                        }
                        break;
                    case CommandType.液晶卡头读卡指令:
                        {
                            Command.Recv.Recv液晶卡头读卡指令 function = new Command.Recv.Recv液晶卡头读卡指令(f, processTime);
                            udp.SendData(function.SendData, route.RemotePoint);
                            function.SendDataTime = DateTime.Now;
                            Console.WriteLine(function.GetRecvData(processTime));
                            Console.WriteLine(function.GetSendData());
                            RadarDataShow(function.GetRecvData(processTime));
                            RadarDataShow(function.GetSendData());
                        }
                        break;
                    case CommandType.液晶卡头投币指令:
                    case CommandType.液晶卡头退币指令:
                        {
                            Command.Recv.Recv液晶卡头进出币指令 function = new Command.Recv.Recv液晶卡头进出币指令(f, processTime);
                            udp.SendData(function.SendData, route.RemotePoint);
                            function.SendDataTime = DateTime.Now;
                            LogHelper.LogHelper.WriteSNLog("接收", function.机头地址, f.routeAddress, function.流水号.ToString(), f.recvData.ToArray());
                            LogHelper.LogHelper.WriteSNLog("发送", function.机头地址, f.routeAddress, function.流水号.ToString(), function.SendData);
                            Console.WriteLine(function.GetRecvData(processTime));
                            Console.WriteLine(function.GetSendData());
                            RadarDataShow(function.GetRecvData(processTime));
                            RadarDataShow(function.GetSendData());
                        }
                        break;
                    case CommandType.IC卡模式会员余额查询:
                        {
                            Command.Recv.RecvIC卡模式会员余额查询 function = new Command.Recv.RecvIC卡模式会员余额查询(f, processTime);
                            udp.SendData(function.SendData, route.RemotePoint);
                            function.SendDataTime = DateTime.Now;
                            LogHelper.LogHelper.WriteSNLog("接收", function.机头地址, f.routeAddress, function.流水号.ToString(), f.recvData.ToArray());
                            LogHelper.LogHelper.WriteSNLog("发送", function.机头地址, f.routeAddress, function.流水号.ToString(), function.SendData);
                            Console.WriteLine(function.GetRecvData(processTime));
                            Console.WriteLine(function.GetSendData());
                            RadarDataShow(function.GetRecvData(processTime));
                            RadarDataShow(function.GetSendData());
                        }
                        break;
                    case CommandType.IC卡模式退币数据:
                    case CommandType.IC卡模式投币数据:
                        {
                            Command.Recv.RecvIC卡模式进出币数据 function = new Command.Recv.RecvIC卡模式进出币数据(f, processTime, route.RemotePoint);
                            udp.SendData(function.SendData, route.RemotePoint);
                            function.SendDataTime = DateTime.Now;
                            LogHelper.LogHelper.WriteSNLog("接收", function.机头地址, f.routeAddress, function.流水号.ToString(), f.recvData.ToArray());
                            LogHelper.LogHelper.WriteSNLog("发送", function.机头地址, f.routeAddress, function.流水号.ToString(), function.SendData);
                            Console.WriteLine(function.GetRecvData(processTime));
                            Console.WriteLine(function.GetSendData());
                            RadarDataShow(function.GetRecvData(processTime));
                            RadarDataShow(function.GetSendData());
                        }
                        break;
                    case CommandType.机头锁定解锁指令应答:
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.Append("=============================================\r\n");
                            sb.AppendFormat("{0:yyyy-MM-dd HH:mm:ss.fff}  收到数据\r\n", processTime);
                            sb.Append(Coding.ConvertData.BytesToString(f.recvData) + Environment.NewLine);
                            sb.AppendFormat("指令类别：{0}\r\n", f.commandType);
                            Console.WriteLine(sb.ToString());
                            RadarDataShow(sb.ToString());
                            ClearBodyItem(f.commandType, null);
                        }
                        break;
                    case CommandType.机头卡片报警指令:
                        {
                            Command.Recv.Recv机头卡片报警指令 function = new Command.Recv.Recv机头卡片报警指令(f, processTime);
                            udp.SendData(function.SendData, route.RemotePoint);
                            function.SendDataTime = DateTime.Now;
                            LogHelper.LogHelper.WriteSNLog("接收", function.机头地址, f.routeAddress, function.流水号.ToString(), f.recvData.ToArray());
                            LogHelper.LogHelper.WriteSNLog("发送", function.机头地址, f.routeAddress, function.流水号.ToString(), function.SendData);
                            Console.WriteLine(function.GetRecvData(processTime));
                            Console.WriteLine(function.GetSendData());
                            RadarDataShow(function.GetRecvData(processTime));
                            RadarDataShow(function.GetSendData());
                        }
                        break;
                    case CommandType.远程投币上分指令应答:
                        {
                            string headAddress = Coding.ConvertData.Hex2String(f.commandData[0]);
                            int SN = BitConverter.ToUInt16(f.recvData, 7);
                            StringBuilder sb = new StringBuilder();
                            sb.Append("=============================================\r\n");
                            sb.AppendFormat("{0:yyyy-MM-dd HH:mm:ss.fff}  收到数据\r\n", processTime);
                            sb.Append(Coding.ConvertData.BytesToString(f.recvData) + Environment.NewLine);
                            sb.AppendFormat("指令类别：{0}\r\n", f.commandType);
                            sb.AppendFormat("流水号：{0}\r\n", SN);
                            sb.AppendFormat("机头地址：{0}\r\n", headAddress);
                            Console.WriteLine(sb.ToString());
                            RadarDataShow(sb.ToString());
                            ClearBodyItem(f.commandType, null, SN, headAddress);
                        }
                        break;
                    case CommandType.远程被动退分解锁指令应答:
                        {
                            Command.Recv.Recv远程被动退分解锁应答 function = new Command.Recv.Recv远程被动退分解锁应答(f);
                            string headAddress = Coding.ConvertData.Hex2String(f.commandData[0]);
                            ClearBodyItem(f.commandType, function, headAddress);
                        }
                        break;
                    //        case CommandType.退币信号延时检测指令应答:
                    //            {
                    //                string headAddress = PubLib.Hex2String(f.commandData[0]);
                    //                ClearBodyItem(f.commandType, f, headAddress);
                    //                StringBuilder sb = new StringBuilder();
                    //                sb.Append("=============================================\r\n");
                    //                sb.AppendFormat("{0:yyyy-MM-dd HH:mm:ss.fff}  收到数据\r\n", processTime);
                    //                sb.Append(PubLib.BytesToString(f.recvData) + Environment.NewLine);
                    //                sb.AppendFormat("指令类别：{0}\r\n", f.commandType);
                    //                UIClass.接收内容 = sb.ToString();
                    //                PubLib.是否允许继续发送机头指令 = true;
                    //            }
                    //            break;
                    //        case CommandType.退币信号延时应答指令:
                    //            {
                    //                Command.Recv.Recv退币信号延时应答指令 function = new Command.Recv.Recv退币信号延时应答指令(f);
                    //                LogHelper.WriteSNLog("接收", function.机头地址, PubLib.路由器段号, function.流水号.ToString(), f.recvData.ToArray());
                    //                LogHelper.WriteSNLog("发送", function.机头地址, PubLib.路由器段号, function.流水号.ToString(), function.SendData.ToArray());
                    //            }
                    //            break;

                    //        case CommandType.远程被动退分解锁指令应答:
                    //            {
                    //                PubLib.当前返还指令数++;
                    //                Command.Recv.Recv远程被动退分解锁应答 function = new Command.Recv.Recv远程被动退分解锁应答(f);
                    //                string headAddress = PubLib.Hex2String(f.commandData[0]);
                    //                ClearBodyItem(f.commandType, function, headAddress);
                    //            }
                    //            break;

                    //        case CommandType.IC卡模式退币数据:
                    //        case CommandType.IC卡模式投币数据:
                    //            {
                    //                PubLib.当前币业务指令数++;
                    //                Command.Recv.RecvIC卡模式进出币数据 function = new Command.Recv.RecvIC卡模式进出币数据(f, processTime);
                    //                LogHelper.WriteSNLog("接收", function.机头地址, PubLib.路由器段号, function.流水号.ToString(), f.recvData.ToArray());
                    //                LogHelper.WriteSNLog("发送", function.机头地址, PubLib.路由器段号, function.流水号.ToString(), function.SendData);
                    //                function.AskData();

                    //                if (UIClass.是否显示IC卡进出币数据 && ((UIClass.显示指定机头信息 && UIClass.显示机头号 == function.机头地址) || !UIClass.显示指定机头信息))
                    //                {
                    //                    UIClass.接收内容 = function.GetRecvData(processTime);
                    //                    UIClass.发送内容 = function.GetSendData();
                    //                }
                    //            }
                    //            break;

                    //        case CommandType.游戏机参数修改应答:
                    //            {
                    //                Command.Recv.Recv游戏机参数修改 function = new Command.Recv.Recv游戏机参数修改(f);
                    //                ClearBodyItem(f.commandType, function, function.机头地址);
                    //                StringBuilder sb = new StringBuilder();
                    //                sb.Append("=============================================\r\n");
                    //                sb.AppendFormat("{0:yyyy-MM-dd HH:mm:ss.fff}  收到数据\r\n", processTime);
                    //                sb.Append(PubLib.BytesToString(f.recvData) + Environment.NewLine);
                    //                sb.AppendFormat("指令类别：{0}\r\n", f.commandType);
                    //                UIClass.接收内容 = sb.ToString();
                    //            }
                    //            break;
                    //        case CommandType.电子币模式投币数据:
                    //            {
                    //                PubLib.当前币业务指令数++;
                    //                Command.Recv.Recv电子币模式投币数据 function = new Command.Recv.Recv电子币模式投币数据(f, processTime);
                    //                LogHelper.WriteSNLog("接收", function.机头地址, PubLib.路由器段号, function.流水号.ToString(), f.recvData.ToArray());
                    //                LogHelper.WriteSNLog("发送", function.机头地址, PubLib.路由器段号, function.流水号.ToString(), function.SendData);
                    //                function.AskData();
                    //                if (UIClass.是否显示数字币投币数据 && ((UIClass.显示指定机头信息 && UIClass.显示机头号 == function.机头地址) || !UIClass.显示指定机头信息))
                    //                {
                    //                    UIClass.接收内容 = function.GetRecvData(processTime);
                    //                    UIClass.发送内容 = function.GetSendData();
                    //                }
                    //            }
                    //            break;
                    //        case CommandType.电子币模式退币出票数据:
                    //            {
                    //                PubLib.当前币业务指令数++;
                    //                PubLib.当前小票指令数++;
                    //                Command.Recv.Recv电子币模式退币出票数据 function = new Command.Recv.Recv电子币模式退币出票数据(f, processTime);
                    //                LogHelper.WriteSNLog("接收", function.机头地址, PubLib.路由器段号, function.流水号.ToString(), f.recvData.ToArray());
                    //                LogHelper.WriteSNLog("发送", function.机头地址, PubLib.路由器段号, function.流水号.ToString(), function.SendData);
                    //                function.AskData();
                    //                if (UIClass.是否显示数字币出票数据 && ((UIClass.显示指定机头信息 && UIClass.显示机头号 == function.机头地址) || !UIClass.显示指定机头信息))
                    //                {
                    //                    UIClass.接收内容 = function.GetRecvData(processTime);
                    //                    UIClass.发送内容 = function.GetSendData();
                    //                }
                    //            }
                    //            break;


                    //        case CommandType.机头地址修改应答:
                    //            {
                    //                Command.Recv.Recv机头地址修改 function = new Command.Recv.Recv机头地址修改(f);
                    //                Command.Ask.Ask机头地址动态分配 ask = (Command.Ask.Ask机头地址动态分配)ClearBodyItem(f.commandType, function, function.机头地址);
                    //                string hadr = Info.HeadInfo.GetHeadAddress(Convert.ToString((long)ask.MCUID, 16));
                    //                DataAccess.Execute(string.Format("update t_head set mcuid='' where mcuid='{0}'; update t_head set mcuid='{0}' where headaddress='{1}';", Convert.ToString((long)ask.MCUID, 16), PubLib.Hex2String(ask.机头地址)));

                    //                Info.HeadInfo.机头信息 机头 = Info.HeadInfo.GetHeadInfo(PubLib.路由器段号, function.机头地址);
                    //                机头.常规.机头长地址 = Convert.ToString((long)ask.MCUID, 16);
                    //                机头 = Info.HeadInfo.GetHeadInfo(PubLib.路由器段号, hadr);
                    //                机头.常规.机头长地址 = "";
                    //            }
                    //            break;

                    //        case CommandType.设置机头长地址应答:
                    //            {
                    //                //ClearBodyItem(f.commandType, f);
                    //                StringBuilder sb = new StringBuilder();
                    //                sb.Append("=============================================\r\n");
                    //                sb.AppendFormat("{0:yyyy-MM-dd HH:mm:ss.fff}  收到数据\r\n", DateTime.Now);
                    //                sb.Append(PubLib.BytesToString(f.recvData) + Environment.NewLine);
                    //                sb.AppendFormat("指令类别：{0}\r\n", f.commandType);
                    //                sb.AppendFormat("路由器地址：{0}\r\n", f.routeAddress);
                    //                UIClass.接收内容 = sb.ToString();
                    //                FrmMain.GetInterface.ShowSetMCUIDResult();
                    //            }
                    //            break;
                    //        case CommandType.数据透传广播指令应答:
                    //            {
                    //                FrameBody curf = (FrameBody)ClearBodyItem(f.commandType, f, "FF");
                    //                if (curf.data[10] == curf.data[11] && curf.data[12] == 0x00)
                    //                {
                    //                    PubLib.intSetPrint = 0;
                    //                    PubLib.isSetPrint = true;
                    //                }
                    //                StringBuilder sb = new StringBuilder();
                    //                sb.Append("=============================================\r\n");
                    //                sb.AppendFormat("{0:yyyy-MM-dd HH:mm:ss.fff}  收到数据\r\n", processTime);
                    //                sb.Append(PubLib.BytesToString(f.recvData) + Environment.NewLine);
                    //                sb.AppendFormat("指令类别：{0}\r\n", f.commandType);
                    //                UIClass.接收内容 = sb.ToString();
                    //            }
                    //            break;
                    //        case CommandType.游戏机参数查询应答:
                    //            {
                    //                Command.Recv.Recv游戏机参数查询应答 function = new Command.Recv.Recv游戏机参数查询应答(f);
                    //                ClearBodyItem(f.commandType, function, function.机头地址);
                    //            }
                    //            break;
                    //        case CommandType.路由器时间控制应答:
                    //            {
                    //                Command.Recv.Recv路由器时间控制 function = new Command.Recv.Recv路由器时间控制(f);
                    //                ClearBodyItem(f.commandType, function, "FF");
                    //            }
                    //            break;


                    //        case CommandType.投币机投币指令:
                    //            {
                    //                PubLib.当前币业务指令数++;
                    //                Command.Recv.Recv投币机投币指令 function = new Command.Recv.Recv投币机投币指令(f, processTime);
                    //                function.AskData();
                    //                UIClass.接收内容 = function.GetRecvData(processTime);
                    //                UIClass.发送内容 = function.GetSendData();
                    //                LogHelper.WriteSNLog("接收", function.投币机地址, PubLib.路由器段号, function.流水号.ToString(), f.recvData.ToArray());
                    //                LogHelper.WriteSNLog("发送", function.投币机地址, PubLib.路由器段号, function.流水号.ToString(), function.SendData.ToArray());
                    //            }
                    //            break;
                    //        case CommandType.卡头程序更新通知应答:
                    //            {
                    //                StringBuilder sb = new StringBuilder();
                    //                sb.Append("=============================================\r\n");
                    //                sb.AppendFormat("{0:yyyy-MM-dd HH:mm:ss.fff}  收到数据\r\n", processTime);
                    //                sb.Append(PubLib.BytesToString(f.recvData) + Environment.NewLine);
                    //                sb.AppendFormat("指令类别：{0}\r\n", f.commandType);
                    //                UIClass.接收内容 += sb.ToString();
                    //                ClearBodyItem(f.commandType, null, "FF");

                    //            }
                    //            break;
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogHelper.WriteLog(ex, route.RecvData);
            }
        }

        public void Test()
        {
            Info.DeviceInfo.机头信息 head = XCCloudSerialNo.SerialNoHelper.StringGet<Info.DeviceInfo.机头信息>("20180328000001");
            IPEndPoint p = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12345);
            Command.Ask.Ask液晶卡头扩展信息下发指令 a = new Command.Ask.Ask液晶卡头扩展信息下发指令(head, Command.Recv.参数类别.显示参数, p);
            a.SendShowParamet(head, p);
        }

        public void 终端同步()
        {
            Info.DeviceInfo.Init();
            Info.GameInfo.Init();
        }
        public bool 路由器复位(string segment, out string errMsg)
        {
            errMsg = "";
            if (路由器段码对照表.ContainsKey(segment))
            {
                if (路由器通讯列表.ContainsKey(路由器段码对照表[segment]))
                {
                    Command.Ask.Ask设置路由器地址 ask = new Command.Ask.Ask设置路由器地址(segment, 路由器通讯列表[路由器段码对照表[segment]]);
                    return true;
                }
                else
                {
                    errMsg = "非法路由器设备";
                }
            }
            else
            {
                errMsg = "路由器段号错误";
            }
            return false;
        }
        public bool 远程退分指令(string mcuid, out string errMsg)
        {
            errMsg = "";
            try
            {
                Info.DeviceInfo.机头信息 head = Info.DeviceInfo.GetBufMCUIDDeviceInfo(mcuid);
                if (head != null)
                {
                    if (路由器段码对照表.ContainsKey(head.路由器段号))
                    {
                        if (路由器通讯列表.ContainsKey(路由器段码对照表[head.路由器段号]))
                        {
                            Command.Ask.Ask远程被动退分解锁指令 ask = new Command.Ask.Ask远程被动退分解锁指令(head, 路由器通讯列表[路由器段码对照表[head.路由器段号]]);
                            return true;
                        }
                        else
                        {
                            errMsg = "非法路由器设备";
                        }
                    }
                    else
                    {
                        errMsg = "路由器段号错误";
                    }
                }
                else
                {
                    errMsg = "未知设备";
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogHelper.WriteLog(ex);
                errMsg = ex.Message;
            }
            return false;
        }
        public bool 远程投币指令(string mcuid, int coin, string pushTypeName, bool ZKZY, out string errMsg)
        {
            errMsg = "";
            try
            {
                Info.DeviceInfo.机头信息 head = Info.DeviceInfo.GetBufMCUIDDeviceInfo(mcuid);
                if (head != null)
                {
                    if (路由器段码对照表.ContainsKey(head.路由器段号))
                    {
                        if (路由器通讯列表.ContainsKey(路由器段码对照表[head.路由器段号]))
                        {
                            Command.Ask.Ask远程投币上分数据 ask = new Command.Ask.Ask远程投币上分数据(head, coin, pushTypeName, PublicHelper.SystemDefiner.雷达主发流水号, ZKZY, 路由器通讯列表[路由器段码对照表[head.路由器段号]]);
                            return true;
                        }
                        else
                        {
                            errMsg = "非法路由器设备";
                        }
                    }
                    else
                    {
                        errMsg = "路由器段号错误";
                    }
                }
                else
                {
                    errMsg = "未知设备";
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogHelper.WriteLog(ex);
                errMsg = ex.Message;
            }
            return false;
        }
        public bool 远程锁定与解锁指令(string mcuid, bool isLock, out string errMsg)
        {
            errMsg = "";
            try
            {
                Info.DeviceInfo.机头信息 head = Info.DeviceInfo.GetBufMCUIDDeviceInfo(mcuid);
                if (head != null)
                {
                    if (路由器段码对照表.ContainsKey(head.路由器段号))
                    {
                        if (路由器通讯列表.ContainsKey(路由器段码对照表[head.路由器段号]))
                        {
                            Command.Ask.Ask机头锁定解锁指令 ask = new Command.Ask.Ask机头锁定解锁指令(head, isLock, 路由器通讯列表[路由器段码对照表[head.路由器段号]]);
                            if (!ask.IsSuccess)
                                errMsg = "执行错误";
                            else
                                return true;
                        }
                        else
                        {
                            errMsg = "非法路由器设备";
                        }
                    }
                    else
                    {
                        errMsg = "路由器段号错误";
                    }
                }
                else
                {
                    errMsg = "未知设备";
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogHelper.WriteLog(ex);
                errMsg = ex.Message;
            }
            return false;
        }

        public void 修改游戏机参数(int gameIndex)
        {
            Info.GameInfo.ReloadGame(gameIndex);
            List<string> deviceList = Info.GameInfo.GetDeviceList(gameIndex);
            foreach (string mcuid in deviceList)
            {
                Info.DeviceInfo.ReloadDeviceInfo(mcuid);
                Info.DeviceInfo.机头信息 head = Info.DeviceInfo.GetBufMCUIDDeviceInfo(mcuid);
                if (head.状态.在线状态)
                {
                    Command.Ask.Ask机头地址修改 a = new Command.Ask.Ask机头地址修改(mcuid, head.机头短地址, head.路由器段号, 路由器通讯列表[路由器段码对照表[head.路由器段号]]);
                }
            }
        }
        public void 修改机头地址(string mcuid, string address)
        {
            Info.DeviceInfo.ReloadDeviceInfo(mcuid);
            Info.DeviceInfo.机头信息 head = Info.DeviceInfo.GetBufMCUIDDeviceInfo(mcuid);
            if (head != null)
            {
                if (head.状态.在线状态)
                {
                    Command.Ask.Ask机头地址修改 a = new Command.Ask.Ask机头地址修改(mcuid, address, head.路由器段号, 路由器通讯列表[路由器段码对照表[head.路由器段号]]);
                }
            }
        }
        /// <summary>
        /// 门店数据同步请求
        /// 当门店操作发生数据变更时，请求同步
        /// 每条记录为一个请求
        /// </summary>
        /// <param name="tableName">数据表名</param>
        /// <param name="idValue">主键值</param>
        /// <param name="action">同步方式 0 新增 1 修改 2 删除</param>
        public void 门店数据同步请求(string tableName, string idValue, int action)
        {
            dsClient.StoreDataSync(PublicHelper.SystemDefiner.StoreID, tableName, idValue, action);
        }
    }
}
