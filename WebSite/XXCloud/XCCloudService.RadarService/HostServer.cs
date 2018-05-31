using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XCCloudService.RadarService
{
    public class HostServer
    {
        public static int 当前总指令数;
        public static int 当前查询指令数;
        public static int 当前币业务指令数;
        public static int 当前IC卡查询重复指令数;
        public static int 当前IC卡进出币指令重复数;
        public static int 当前小票指令数;
        public static int 当前错误指令数;
        public static int 当前返还指令数;

        public static string LogPath = "C:\\xclog";
        public static string SQLConnectString = "";
        public static string MerchID = "";
        public static string StoreID = "";
        int ServerPort = 0;
        UDPServer udp = new UDPServer();
        Queue<byte[]> RecvList = new Queue<byte[]>();

        Thread recvProcT1 = null;
        bool _initSuccess = false;
        public bool InitSuccess { get { return _initSuccess; } }

        public HostServer(string merchID, string storeID, string DBIP, string DBPwd, int udpPort)
        {
            SQLConnectString = string.Format("Data Source ={0};Initial Catalog = XCCloud;User Id = sa;Password = {1};", DBIP, DBPwd);
            MerchID = merchID;
            StoreID = storeID;
            ServerPort = udpPort;
        }

        #region 路由器设备列表

        Dictionary<string, EndPoint> 路由器通讯列表 = new Dictionary<string, EndPoint>();

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
        public bool StartServer(string logPath = "")
        {
            if (InitSuccess) return false;

            if (logPath.Trim() != "")
                LogPath = logPath;

            try
            {
                udp.Init(ServerPort);
                udp.OnInternetDataRecv += udp_OnInternetDataRecv;
                if (recvProcT1 == null)
                {
                    recvProcT1 = new Thread(new ThreadStart(tRunProcess));
                    recvProcT1.IsBackground = true;
                    recvProcT1.Name = "后台接收队列处理线程1";
                    recvProcT1.Start();
                }
                _initSuccess = true;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex);
            }
            return false;
        }

        void udp_OnInternetDataRecv(byte[] data, EndPoint p)
        {
            byte[] recvData = data.Skip(16).Take(data.Length - 16).ToArray();
            string code = Coding.ConvertData.BytesToString(data.Take(16).ToArray()).Replace(" ","").ToLower();
            UpdateRoute(code, p);
            RecvList.Enqueue(recvData);
        }

        void tRunProcess()
        {
            while (true)
            {
                try
                {
                    if (RecvList.Count > 0)
                    {
                        byte[] data = RecvList.Dequeue();
                        ProcessCommad(data);
                    }
                    Thread.Sleep(5);
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog(ex);
                    Thread.Sleep(50);
                }
            }
        }

        void ProcessCommad(byte[] data)
        {
            //byte[] data = p as byte[];
            try
            {
                FrameData f = new FrameData(data);
                DateTime processTime = DateTime.Now;
                Console.WriteLine(string.Format("接收：{0}", Coding.ConvertData.BytesToString(f.recvData)));
                当前总指令数++;
                switch (f.commandType)
                {
                    case CommandType.设置机头长地址应答:
                        {
                            SetMCUSuccess();
                        }
                        break;
                    //        case CommandType.液晶卡头扩展信息下发应答指令:
                    //            {
                    //                StringBuilder sb = new StringBuilder();
                    //                sb.Append("=============================================\r\n");
                    //                sb.AppendFormat("{0:yyyy-MM-dd HH:mm:ss.fff}  收到数据\r\n", processTime);
                    //                sb.Append(PubLib.BytesToString(f.recvData) + Environment.NewLine);
                    //                string headAddress = PubLib.Hex2String(f.commandData[0]);
                    //                int SN = BitConverter.ToUInt16(f.commandData, 1);
                    //                sb.AppendFormat("指令类别：{0}\r\n", f.commandType);
                    //                sb.AppendFormat("流水号：{0}\r\n", SN);
                    //                UIClass.接收内容 += sb.ToString();
                    //                ClearBodyItem(f.commandType, null, SN, headAddress);
                    //            }
                    //            break;
                    //        case CommandType.液晶卡头扩展信息请求指令:
                    //            {
                    //                Command.Recv.Recv液晶卡头扩展信息请求指令 function = new Command.Recv.Recv液晶卡头扩展信息请求指令(f, processTime);
                    //                function.AskData();
                    //                if (!function.isRepeat)
                    //                {
                    //                    Command.Ask.Ask液晶卡头扩展信息下发指令 ask = new Command.Ask.Ask液晶卡头扩展信息下发指令(function.机头地址, function.参数);
                    //                }
                    //                UIClass.接收内容 = function.GetRecvData(processTime);
                    //                UIClass.发送内容 = function.GetSendData();
                    //            }
                    //            break;
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
                    //        case CommandType.机头卡片报警指令:
                    //            {
                    //                Command.Recv.Recv机头卡片报警指令 function = new Command.Recv.Recv机头卡片报警指令(f, processTime);
                    //                LogHelper.WriteSNLog("接收", function.机头地址, PubLib.路由器段号, function.流水号.ToString(), f.recvData.ToArray());
                    //                LogHelper.WriteSNLog("发送", function.机头地址, PubLib.路由器段号, function.流水号.ToString(), function.SendData);
                    //                UIClass.接收内容 = function.GetRecvData(processTime);
                    //                UIClass.发送内容 = function.GetSendData();
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
                    //        case CommandType.液晶卡头读卡指令:
                    //            {
                    //                PubLib.当前查询指令数++;
                    //                Command.Recv.Recv液晶卡头读卡指令 function = new Command.Recv.Recv液晶卡头读卡指令(f, processTime);
                    //                LogHelper.WriteSNLog("接收", function.机头地址, PubLib.路由器段号, function.流水号.ToString(), f.recvData.ToArray());
                    //                LogHelper.WriteSNLog("发送", function.机头地址, PubLib.路由器段号, function.流水号.ToString(), function.SendData);
                    //                function.AskData();
                    //                if (UIClass.是否显示IC卡查询数据 && ((UIClass.显示指定机头信息 && UIClass.显示机头号 == function.机头地址) || !UIClass.显示指定机头信息))
                    //                {
                    //                    UIClass.接收内容 = function.GetRecvData(processTime);
                    //                    UIClass.发送内容 = function.GetSendData();
                    //                }
                    //            }
                    //            break;
                    //        case CommandType.液晶卡头投币指令:
                    //        case CommandType.液晶卡头退币指令:
                    //            {
                    //                PubLib.当前币业务指令数++;
                    //                Command.Recv.Recv液晶卡头进出币指令 function = new Command.Recv.Recv液晶卡头进出币指令(f, processTime);
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
                    //        case CommandType.IC卡模式会员余额查询:
                    //            {
                    //                PubLib.当前查询指令数++;
                    //                Command.Recv.RecvIC卡模式会员余额查询 function = new Command.Recv.RecvIC卡模式会员余额查询(f, processTime);
                    //                LogHelper.WriteSNLog("接收", function.机头地址, PubLib.路由器段号, function.流水号.ToString(), f.recvData.ToArray());
                    //                LogHelper.WriteSNLog("发送", function.机头地址, PubLib.路由器段号, function.流水号.ToString(), function.SendData);
                    //                function.AskData();
                    //                if (UIClass.是否显示IC卡查询数据 && ((UIClass.显示指定机头信息 && UIClass.显示机头号 == function.机头地址) || !UIClass.显示指定机头信息))
                    //                {
                    //                    UIClass.接收内容 = function.GetRecvData(processTime);
                    //                    UIClass.发送内容 = function.GetSendData();
                    //                }
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
                    //        case CommandType.游戏机参数申请:
                    //            {
                    //                Command.Recv.Recv游戏机参数申请 function = new Command.Recv.Recv游戏机参数申请(f);
                    //                function.AskData();
                    //                if (UIClass.是否显示游戏机参数申请数据 && ((UIClass.显示指定机头信息 && UIClass.显示机头号 == function.机头地址) || !UIClass.显示指定机头信息))
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
                    //        case CommandType.机头网络状态报告:
                    //            {
                    //                Command.Recv.Recv机头网络状态报告 function = new Command.Recv.Recv机头网络状态报告(f);
                    //                if (UIClass.是否显示机头网络状态)
                    //                {
                    //                    StringBuilder sb = new StringBuilder();
                    //                    sb.Append("=============================================\r\n");
                    //                    sb.AppendFormat("{0:yyyy-MM-dd HH:mm:ss.fff}  收到数据\r\n", processTime);
                    //                    sb.Append(PubLib.BytesToString(f.recvData) + Environment.NewLine);
                    //                    sb.AppendFormat("指令类别：{0}\r\n", f.commandType);
                    //                    UIClass.接收内容 = sb.ToString();
                    //                    sb = new StringBuilder();
                    //                    sb.Append("=============================================\r\n");
                    //                    sb.AppendFormat("{0:yyyy-MM-dd HH:mm:ss.fff}  发送数据\r\n", function.SendDate);
                    //                    sb.Append(PubLib.BytesToString(function.SendData) + Environment.NewLine);
                    //                    sb.AppendFormat("指令类别：{0}\r\n", function.RecvData.commandType);
                    //                    sb.AppendFormat("路由器地址：{0}\r\n", function.RecvData.routeAddress);
                    //                    UIClass.发送内容 = sb.ToString();
                    //                }
                    //                else
                    //                {
                    //                    //StringBuilder sb = new StringBuilder();
                    //                    //sb = new StringBuilder();
                    //                    //sb.Append("=============================================\r\n");
                    //                    //sb.AppendFormat("{0}  不在线机头列表\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                    //                    //sb.Append(Info.HeadInfo.获取不在线机头信息());
                    //                    //UIClass.接收内容 = sb.ToString();
                    //                }
                    //            }
                    //            break;
                    //        case CommandType.机头地址动态分配:
                    //            {
                    //                Command.Recv.Recv机头地址动态分配 function = new Command.Recv.Recv机头地址动态分配(f);
                    //                if (UIClass.是否显示机头地址动态分配数据 && ((UIClass.显示指定机头信息 && UIClass.显示机头号 == function.机头地址) || !UIClass.显示指定机头信息))
                    //                {
                    //                    UIClass.接收内容 = function.GetRecvData(processTime);
                    //                    if (function.SendData != null)
                    //                    {
                    //                        UIClass.发送内容 = function.GetSendData();
                    //                    }
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
                    //        case CommandType.设置路由器地址应答:
                    //            {
                    //                string routeVer = Encoding.ASCII.GetString(f.commandData);
                    //                ClearBodyItem(f.commandType, f, "FF");
                    //                StringBuilder sb = new StringBuilder();
                    //                sb.Append("=============================================\r\n");
                    //                sb.AppendFormat("{0}  收到数据\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                    //                sb.Append(PubLib.BytesToString(f.recvData) + Environment.NewLine);
                    //                sb.AppendFormat("指令类别：{0}\r\n", f.commandType);
                    //                sb.AppendFormat("路由器地址：{0}\r\n", f.routeAddress);
                    //                sb.AppendFormat("版本号：{0}\r\n", routeVer);
                    //                PubLib.硬件版本号 = routeVer;
                    //                UIClass.接收内容 = sb.ToString();
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
                    //        case CommandType.机头锁定解锁指令应答:
                    //            {
                    //                StringBuilder sb = new StringBuilder();
                    //                sb.Append("=============================================\r\n");
                    //                sb.AppendFormat("{0:yyyy-MM-dd HH:mm:ss.fff}  收到数据\r\n", processTime);
                    //                sb.Append(PubLib.BytesToString(f.recvData) + Environment.NewLine);
                    //                sb.AppendFormat("指令类别：{0}\r\n", f.commandType);
                    //                UIClass.接收内容 = sb.ToString();
                    //                ClearBodyItem(f.commandType, null);
                    //            }
                    //            break;
                    //        case CommandType.远程投币上分指令应答:
                    //            {
                    //                StringBuilder sb = new StringBuilder();
                    //                sb.Append("=============================================\r\n");
                    //                sb.AppendFormat("{0:yyyy-MM-dd HH:mm:ss.fff}  收到数据\r\n", processTime);
                    //                sb.Append(PubLib.BytesToString(f.recvData) + Environment.NewLine);
                    //                int SN = BitConverter.ToUInt16(f.recvData, 7);
                    //                sb.AppendFormat("指令类别：{0}\r\n", f.commandType);
                    //                sb.AppendFormat("流水号：{0}\r\n", SN);
                    //                UIClass.接收内容 += sb.ToString();
                    //                string headAddress = PubLib.Hex2String(f.commandData[0]);
                    //                ClearBodyItem(f.commandType, null, SN, headAddress);
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
                LogHelper.WriteLog(ex, data);
            }
        }
    }
}
