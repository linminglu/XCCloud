using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Web.Script.Serialization;
using System.Diagnostics;

namespace RadarService.Notify
{
    public class XCHelper
    {
        Thread tRun;
        Thread tTick;
        Socket client;
        string StoreID = "";
        string PwdRead = "";
        string ServerIP = "";
        int ServerPort = 0;
        string Segment = "";
        public string CurToken = "";
        int SN = 0;

        int GetSN()
        {
            SN++;
            if (SN > 40000000)
                SN = 1;

            return SN;
        }
        /// <summary>
        /// 系统初始化标志
        /// </summary>
        bool InitFlag = false;

        Queue<FrameData> bufQueue = new Queue<FrameData>();

        ManualResetEvent allDone = new ManualResetEvent(false);
        bool isRun = false;
        /// <summary>
        /// 最后发送时间
        /// </summary>
        DateTime ConnectSendTime;
        /// <summary>
        /// 最后接收时间
        /// </summary>
        DateTime ConnectRecvTime;

        public delegate void 设备控制指令(JsonObject.DeviceControl contorl);
        public event 设备控制指令 OnDeviceControl;
        public void DeviceControlHandler(JsonObject.DeviceControl contorl)
        {
            if (OnDeviceControl != null)
            {
                OnDeviceControl(contorl);
            }
        }

        public delegate void 游戏机信息变更(JsonObject.GameInfoChangeRequest gameChange);
        public event 游戏机信息变更 OnGameInfoChange;
        public void GameInfoChange(JsonObject.GameInfoChangeRequest gameChange)
        {
            if (OnGameInfoChange != null)
            {
                OnGameInfoChange(gameChange);
            }
        }

        public delegate void 卡头复位指令(JsonObject.DeviceResetRequest device);
        public event 卡头复位指令 OnDeviceReset;
        public void DeviceReset(JsonObject.DeviceResetRequest device)
        {
            if (OnDeviceReset != null)
            {
                OnDeviceReset(device);
            }
        }

        public delegate void 数据同步指令(JsonObject.DataSyncRequest data);
        public event 数据同步指令 OnDataSync;
        public void DataSync(JsonObject.DataSyncRequest data)
        {
            if (OnDataSync != null)
            {
                OnDataSync(data);
            }
        }

        public delegate void 支付请求应答(JsonObject.ScanPayDataResponse data);
        public event 支付请求应答 OnPayRequest;
        public void PayRequest(JsonObject.ScanPayDataResponse data)
        {
            if (OnPayRequest != null)
            {
                OnPayRequest(data);
            }
        }

        public delegate void 支付结果应答(JsonObject.ScanPayResultRequest data);
        public event 支付结果应答 OnPayResult;
        public void PayResult(JsonObject.ScanPayResultRequest data)
        {
            if (OnPayResult != null)
            {
                OnPayResult(data);
            }
        }

        /// <summary>
        /// 构造函数初始化必要参数
        /// </summary>
        /// <param name="IP">服务器IP</param>
        /// <param name="port">服务器端口</param>
        /// <param name="_StoreID">门店编号</param>
        /// <param name="_PwdRead">读卡密码</param>
        /// <param name="_Segment">路由器段号</param>
        public XCHelper(string IP, int port, string _StoreID, string _PwdRead)
        {
            ServerIP = IP;
            ServerPort = port;
            StoreID = _StoreID;
            PwdRead = _PwdRead;
        }

        public bool Init()
        {
            try
            {
                //return true;

                ConnectSendTime = DateTime.Now;
                ConnectRecvTime = DateTime.Now;

                for (int i = 0; i < 100; i++)
                {
                    BodySendList[i] = new RepeatDataItem();
                    BodySendList[i].SN = "";
                    BodySendList[i].SendFlag = false;
                }

                if (isRun) return false;
                tRun = new Thread(new ThreadStart(CommandProcess)) { IsBackground = true, Name = "客户端处理线程" };
                tRun.Start();

                tTick = new Thread(new ThreadStart(TickProcess)) { IsBackground = true, Name = "心跳处理线程" };
                tTick.Start();

                tRepeat = new Thread(new ThreadStart(SendBodyProcess)) { IsBackground = true, Name = "指令重发线程" };
                //tRepeat = new Thread(new ThreadStart(RepeatTick)) { IsBackground = true, Name = "指令重发线程" };
                tRepeat.Start();

                client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                byte[] optionInValue = { Convert.ToByte(false) };
                byte[] optionOutValue = new byte[4];
                uint IOC_IN = 0x80000000;
                uint IOC_VENDOR = 0x18000000;
                uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
                client.IOControl((int)SIO_UDP_CONNRESET, optionInValue, optionOutValue);
                IPEndPoint serverPoint = new IPEndPoint(IPAddress.Parse(ServerIP), ServerPort);
                client.Connect((EndPoint)serverPoint);

                IPEndPoint sendP = new IPEndPoint(IPAddress.Any, 0);
                EndPoint tempRemoteEP = (EndPoint)sendP;

                StateObject so = new StateObject();
                so.socket = client;
                client.BeginReceiveFrom(so.buffer, 0, StateObject.BUF_SIZE, SocketFlags.None, ref tempRemoteEP, new AsyncCallback(RecvCallBack), so);

                isRun = true;
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return false;
        }
        /// <summary>
        /// socket数据接收异步回调
        /// </summary>
        /// <param name="ar"></param>
        void RecvCallBack(IAsyncResult ar)
        {
            allDone.Set();

            StateObject so = (StateObject)ar.AsyncState;
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint tempRemoteEP = (EndPoint)sender;

            int readBytes = 0;
            try
            {
                readBytes = so.socket.EndReceiveFrom(ar, ref tempRemoteEP);
            }
            catch (ObjectDisposedException oe)
            {
                Console.WriteLine(oe);
                throw oe;
            }
            catch (SocketException se)
            {
                Console.WriteLine(se);
                throw se;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                // 获得接收失败信息 
                throw e;
            }

            if (readBytes > 0)
            {
                byte[] mybytes = new byte[readBytes];
                Array.Copy(so.buffer, mybytes, readBytes);
                FrameData f = new FrameData(mybytes);
                if (f.IsSuccess)
                {
                    lock (bufQueue)
                    {
                        bufQueue.Enqueue(f);
                    }
                }
                //收到数据处理
                so.socket.BeginReceiveFrom(so.buffer, 0, StateObject.BUF_SIZE, SocketFlags.None, ref tempRemoteEP, new AsyncCallback(RecvCallBack), so);
            }
        }


        /// <summary>
        /// 数据处理函数
        /// </summary>
        void CommandProcess()
        {
            while (true)
            {
                bool isSuccess = false;
                FrameData f = null;
                try
                {
                    //从队列中取数据，防止集合被改变用异常处理
                    lock (bufQueue)
                    {
                        if (bufQueue.Count > 0)
                        {
                            f = bufQueue.Dequeue();
                            isSuccess = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                if (isSuccess)
                {
                    //Console.WriteLine("莘宸服务接收【" + f.FrameType.ToString() + "】：" + f.FrameJsontxt);
                    //取数据成功
                    switch (f.FrameType)
                    {
                        case 0x90:  //收到雷达注册应答
                            {
                                JavaScriptSerializer jss = new JavaScriptSerializer();
                                JsonObject.RegistResponse regist = jss.Deserialize<JsonObject.RegistResponse>(f.FrameJsontxt);
                                JsonObject json = new JsonObject();
                                string key = json.GetSignKey(regist, PwdRead);
                                if (key == regist.signkey)
                                {
                                    //验签成功，保存令牌
                                    CurToken = regist.token;
                                    InitFlag = true;
                                    ConnectRecvTime = DateTime.Now;
                                }
                                else
                                {
                                    Console.WriteLine("莘宸服务接收【签名错误】");
                                }
                            }
                            break;
                        case 0x91:  //收到雷达心跳指令
                            {
                                JavaScriptSerializer jss = new JavaScriptSerializer();
                                JsonObject.TickResponse tick = jss.Deserialize<JsonObject.TickResponse>(f.FrameJsontxt);
                                JsonObject json = new JsonObject();
                                string key = json.GetSignKey(tick, PwdRead);
                                if (key == tick.signkey)
                                {
                                    //验签成功，保存令牌
                                    ConnectRecvTime = DateTime.Now;
                                    if (tick.result_code != "1")
                                        isRun = false;  //心跳发送错误，必须重新请求令牌
                                }
                                else
                                {
                                    Console.WriteLine("莘宸服务接收【签名错误】");
                                }
                            }
                            break;
                        case 0x80:  //设备状态变更通知
                            break;
                        case 0x81:  //收到设备控制指令
                            {
                                JavaScriptSerializer jss = new JavaScriptSerializer();
                                JsonObject.DeviceControl control = jss.Deserialize<JsonObject.DeviceControl>(f.FrameJsontxt);
                                JsonObject json = new JsonObject();
                                string key = json.GetSignKey(control, PwdRead);
                                if (key == control.signkey && CurToken == control.token)
                                {
                                    DeviceControlHandler(control);
                                }
                                else
                                {
                                    Console.WriteLine("莘宸服务接收【签名错误】");
                                }
                            }
                            break;
                        case 0x82:  //收到通知应答
                            {
                                JavaScriptSerializer jss = new JavaScriptSerializer();
                                JsonObject.NotifyResponse notify = jss.Deserialize<JsonObject.NotifyResponse>(f.FrameJsontxt);
                                ClearBodySend(notify.sn);
                            }
                            break;
                        case 0x60:  //游戏机变更通知
                            {
                                JavaScriptSerializer jss = new JavaScriptSerializer();
                                JsonObject.GameInfoChangeRequest notify = jss.Deserialize<JsonObject.GameInfoChangeRequest>(f.FrameJsontxt);
                                Console.WriteLine("收到游戏机变更通知：" + f.FrameJsontxt);
                                GameInfoChange(notify);
                            }
                            break;
                        case 0x62:  //设备复位通知指令
                            {
                                JavaScriptSerializer jss = new JavaScriptSerializer();
                                JsonObject.DeviceResetRequest device = jss.Deserialize<JsonObject.DeviceResetRequest>(f.FrameJsontxt);
                                Console.WriteLine("收到设备复位通知：" + f.FrameJsontxt);
                                DeviceReset(device);
                            }
                            break;
                        case 0x63:  //收到支付请求应答
                            {
                                JavaScriptSerializer jss = new JavaScriptSerializer();
                                JsonObject.ScanPayDataResponse response = jss.Deserialize<JsonObject.ScanPayDataResponse>(f.FrameJsontxt);
                                JsonObject json = new JsonObject();
                                string key = json.GetSignKey(response, PwdRead);
                                if (key != response.signkey)
                                {
                                    Debug.WriteLine("应答签名错误");
                                }
                                else
                                {
                                    ClearBodySend(response.sn);
                                    PayRequest(response);
                                }
                                Debug.WriteLine("收到支付请求应答：" + f.FrameJsontxt);
                            }
                            break;
                        case 0x64:  //收到支付结果请求
                            {
                                JavaScriptSerializer jss = new JavaScriptSerializer();
                                JsonObject.ScanPayResultRequest request = jss.Deserialize<JsonObject.ScanPayResultRequest>(f.FrameJsontxt);
                                JsonObject json = new JsonObject();
                                string key = json.GetSignKey(request, PwdRead);
                                string msg = "", code = "";
                                if (key != request.signkey)
                                {
                                    msg = "应答签名错误";
                                    code = "0";
                                    Debug.WriteLine(msg);
                                }
                                else
                                { PayResult(request); code = "1"; }

                                JsonObject.ScanPayResultResponse response = new JsonObject.ScanPayResultResponse();
                                response.result_code = code;
                                response.result_msg = msg;
                                response.sn = request.sn;
                                response.signkey = json.GetSignKey(response, PwdRead);
                                string jsonString = jss.Serialize(response);
                                SendData(Encoding.UTF8.GetBytes(jsonString), 0x34);

                                Debug.WriteLine("收到支付结果请求：" + f.FrameJsontxt);
                            }
                            break;
                    }
                }
                Thread.Sleep(50);
            }
        }

        public void SendData(byte[] data, byte cmdType)
        {
            if (!isRun) return;

            List<byte> cmd = new List<byte>();
            cmd.Add(0xfe);
            cmd.Add(0xfe);
            cmd.Add(0x68);
            cmd.AddRange(BitConverter.GetBytes((UInt16)data.Length));
            cmd.Add(0x01);
            cmd.Add(0x01);
            cmd.Add(cmdType);
            cmd.AddRange(data);
            cmd.Add(0x16);
            cmd.Add(0xfe);
            cmd.Add(0xfe);

            IPEndPoint clients = new IPEndPoint(IPAddress.Parse(ServerIP), ServerPort);
            EndPoint epSender = (EndPoint)clients;
            client.BeginSendTo(cmd.ToArray(), 0, cmd.Count(), SocketFlags.None, epSender, new AsyncCallback(SendCallBack), client);
        }

        void SendCallBack(IAsyncResult ar)
        {
            try
            {
                Socket Client = (Socket)ar.AsyncState;
                Client.EndSend(ar);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        /// <summary>
        /// 心跳处理
        /// </summary>
        void TickProcess()
        {
            int i = 0;
            while (true)
            {
                if (InitFlag)
                {
                    if (ConnectRecvTime.AddSeconds(60) < ConnectSendTime)
                    {
                        //服务器断开
                        InitFlag = false;
                        //CallBackEvent.ServerDisconnect();
                    }
                    if (i > 10)
                    {
                        i = 0;
                        TickConnectServer();
                    }
                    i++;
                }
                else
                {
                    RegistRouteDevice();
                }
                Thread.Sleep(5000);
            }
        }
        #region 主发队列缓存

        Thread tRepeat = null;

        class RepeatDataItem
        {
            public byte CmdType { get; set; }
            public byte[] DataBuf { get; set; }
            public string SN { get; set; }
            public DateTime LastTime { get; set; }
            public object SendOjbect { get; set; }
            public bool SendFlag { get; set; }
            public int ReSendTimes { get; set; }
        }
        RepeatDataItem[] BodySendList = new RepeatDataItem[100];
        /// <summary>
        /// 设置主发队列
        /// 序列号为空的队列可以占用
        /// </summary>
        /// <param name="item"></param>
        void SetBodySend(RepeatDataItem item)
        {
            for (int i = 0; i < 100; i++)
            {
                if (BodySendList[i].SN == "")
                {
                    BodySendList[i].CmdType = item.CmdType;
                    BodySendList[i].DataBuf = new byte[item.DataBuf.Length];
                    Array.Copy(item.DataBuf, BodySendList[i].DataBuf, item.DataBuf.Length);
                    BodySendList[i].LastTime = DateTime.Now;
                    BodySendList[i].SendFlag = false;
                    BodySendList[i].SendOjbect = item.SendOjbect;
                    BodySendList[i].SN = item.SN;
                    BodySendList[i].ReSendTimes = 0;
                    return;
                }
            }
        }
        /// <summary>
        /// 清除主发队列，将该SN清除
        /// </summary>
        /// <param name="sn"></param>
        void ClearBodySend(string sn)
        {
            for (int i = 0; i < 100; i++)
            {
                if (BodySendList[i].SN == sn)
                {
                    BodySendList[i].SN = "";
                    return;
                }
            }
        }
        /// <summary>
        /// 定时刷新主发队列
        /// </summary>
        void RefreshBodySend()
        {
            DateTime d = DateTime.Now;
            for (int i = 0; i < 100; i++)
            {
                if (BodySendList[i].SN != "" && BodySendList[i].SendFlag)
                {
                    if (BodySendList[i].LastTime.AddSeconds(3) < d)
                    {
                        //主发超时没有应答需要重发
                        BodySendList[i].SendFlag = false;
                        if (BodySendList[i].ReSendTimes >= 30)
                            BodySendList[i].SN = "";    //重发次数超过30次则清空
                    }
                    return;
                }
            }
        }

        void SendBodyProcess()
        {
            while (true)
            {
                try
                {
                    DateTime d = DateTime.Now;
                    for (int i = 0; i < 100; i++)
                    {
                        if (BodySendList[i].SN != "" && !BodySendList[i].SendFlag)
                        {
                            //需要发送则发送
                            SendData(BodySendList[i].DataBuf, BodySendList[i].CmdType);
                            BodySendList[i].ReSendTimes++;
                            BodySendList[i].LastTime = d;
                            BodySendList[i].SendFlag = true;
                        }
                    }
                    RefreshBodySend();
                    Thread.Sleep(2000);
                }
                catch (Exception e)
                {
                    LogHelper.LogHelper.WriteLog(e);
                }
            }
        }

        //class RepeatBody
        //{
        //    public byte CmdType { get; set; }
        //    public byte[] DataBuf { get; set; }
        //    public int SN { get; set; }
        //    public DateTime LastTime { get; set; }
        //    public object SendOjbect { get; set; }
        //    public bool SendFlag { get; set; }
        //}
        ///// <summary>
        ///// 主发队列
        ///// </summary>
        //List<RepeatBody> RepeatList = new List<RepeatBody>();
        //void InsertRepeat(RepeatBody item)
        //{
        //    lock (RepeatList)
        //    {
        //        RepeatList.Add(item);
        //    }
        //}

        //bool CheckRepeat(int curSN)
        //{
        //    lock (RepeatList)
        //    {
        //        foreach (RepeatBody item in RepeatList)
        //        {
        //            if (item.SN == curSN)
        //            {
        //                RepeatList.Remove(item);
        //                return true;
        //            }
        //        }
        //    }
        //    return false;
        //}
        //void RepeatTick()
        //{
        //    while (true)
        //    {
        //        bool isBreak = false;

        //        DateTime d = DateTime.Now;
        //        lock (RepeatList)
        //        {
        //            foreach (RepeatBody item in RepeatList)
        //            {
        //                if (item.LastTime.AddHours(2) < d)
        //                {
        //                    //2小时没有处理的数据则移除
        //                    RepeatList.Remove(item);
        //                    isBreak = true;
        //                    break;
        //                }
        //                else
        //                {
        //                    if (item.SendFlag)
        //                        item.SendFlag = false;
        //                    else
        //                    {
        //                        if (item.CmdType == 0x12)
        //                        {
        //                            JsonObject.ControlResultNotify o = item.SendOjbect as JsonObject.ControlResultNotify;
        //                            if (o != null)
        //                            {
        //                                o.token = CurToken;
        //                                JsonObject json = new JsonObject();
        //                                o.signkey = json.GetSignKey(o, PwdRead);

        //                                JavaScriptSerializer jss = new JavaScriptSerializer();
        //                                string jsonString = jss.Serialize(o);

        //                                byte[] datalist = Encoding.UTF8.GetBytes(jsonString);

        //                                //Console.WriteLine("重发指令【" + item.CmdType.ToString() + "】 action=" + o.action);
        //                                SendData(datalist, 0x12);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            Console.WriteLine("重发指令【" + item.CmdType.ToString() + "】");
        //                            SendData(item.DataBuf, item.CmdType);   //指令重发
        //                        }
        //                        item.SendFlag = true;
        //                    }
        //                }
        //            }
        //        }
        //        if (isBreak)
        //            Thread.Sleep(10);       //如果有队列被移除则短空闲
        //        else
        //            Thread.Sleep(2000);
        //    }
        //}
        #endregion
        #region 应答事件



        #endregion
        #region 公共方法
        /// <summary>
        /// 路由器注册
        /// </summary>
        public void RegistRouteDevice()
        {
            JsonObject.RegistDevice regist = new JsonObject.RegistDevice()
            {
                storeid = StoreID,
                segment = Segment,
                signkey = ""
            };

            JsonObject json = new JsonObject();
            regist.signkey = json.GetSignKey(regist, PwdRead);



            JavaScriptSerializer jss = new JavaScriptSerializer();
            string jsonString = jss.Serialize(regist);
            //Console.WriteLine("发送 注册信息[" + CurToken + "]：" + jsonString);
            SendData(Encoding.UTF8.GetBytes(jsonString), 0xf0);
            ConnectSendTime = DateTime.Now;
        }
        /// <summary>
        /// 路由器心跳
        /// </summary>
        public void TickConnectServer()
        {
            if (!isRun) return;
            JsonObject.DeviceTick tick = new JsonObject.DeviceTick()
            {
                token = CurToken
            };

            JavaScriptSerializer jss = new JavaScriptSerializer();
            string jsonString = jss.Serialize(tick);
            //Console.WriteLine("发送 心跳[" + CurToken + "]：" + jsonString);
            SendData(Encoding.UTF8.GetBytes(jsonString), 0xf1);
            ConnectSendTime = DateTime.Now;
        }

        /// <summary>
        /// 设备状态变更通知
        /// </summary>
        /// <param name="mcuid">当前设备序列号</param>
        /// <param name="action">设备状态</param>
        public void ChangeDeviceStatus(List<JsonObject.StatusItem> changeList)
        {
            if (!isRun || !InitFlag) return;

            JsonObject.ChangeStatus o = new JsonObject.ChangeStatus()
            {
                devicelist = changeList,
                token = CurToken,
                signkey = ""
            };

            JsonObject json = new JsonObject();
            o.signkey = json.GetSignKey(o, PwdRead);

            JavaScriptSerializer jss = new JavaScriptSerializer();
            string jsonString = jss.Serialize(o);
            //Console.WriteLine("发送 设备状态变更通知[" + CurToken + "]：" + jsonString);
            SendData(Encoding.UTF8.GetBytes(jsonString), 0x10);
            ConnectSendTime = DateTime.Now;
            //Console.WriteLine("设备状态变更通知：" + jsonString);
        }
        /// <summary>
        /// 设备控制应答
        /// </summary>
        /// <param name="sn"></param>
        /// <param name="code"></param>
        /// <param name="msg"></param>
        public void AskControl(string sn, string code, string msg)
        {
            if (!isRun || !InitFlag) return;

            JsonObject.ControlResponse o = new JsonObject.ControlResponse()
            {
                result_code = code,
                result_msg = msg,
                sn = sn,
                signkey = ""
            };

            JsonObject json = new JsonObject();
            o.signkey = json.GetSignKey(o, PwdRead);

            JavaScriptSerializer jss = new JavaScriptSerializer();
            string jsonString = jss.Serialize(o);
            Console.WriteLine("发送 设备控制应答[" + CurToken + "]：" + jsonString);
            SendData(Encoding.UTF8.GetBytes(jsonString), 0x11);
            ConnectSendTime = DateTime.Now;
            Console.WriteLine("设备控制应答：" + jsonString);
        }

        public void AskGameInfoChange(string code, string msg, string sn)
        {
            JsonObject.GameInfoChangeResponse o = new JsonObject.GameInfoChangeResponse()
            {
                result_code = code,
                result_msg = msg,
                signkey = "",
                sn = sn
            };
            JsonObject json = new JsonObject();
            o.signkey = json.GetSignKey(o, PwdRead);

            JavaScriptSerializer jss = new JavaScriptSerializer();
            string jsonString = jss.Serialize(o);
            Console.WriteLine("发送 游戏机变更同步应答[" + CurToken + "]：" + jsonString);
            SendData(Encoding.UTF8.GetBytes(jsonString), 0x30);
            ConnectSendTime = DateTime.Now;
            Console.WriteLine("游戏机变更同步应答：" + jsonString);
        }

        public void AskDeviceReset(string code, string msg, string sn)
        {
            JsonObject.GameInfoChangeResponse o = new JsonObject.GameInfoChangeResponse()
            {
                result_code = code,
                result_msg = msg,
                signkey = "",
                sn = sn
            };
            JsonObject json = new JsonObject();
            o.signkey = json.GetSignKey(o, PwdRead);

            JavaScriptSerializer jss = new JavaScriptSerializer();
            string jsonString = jss.Serialize(o);
            Console.WriteLine("发送 卡头复位指令[" + CurToken + "]：" + jsonString);
            SendData(Encoding.UTF8.GetBytes(jsonString), 0x32);
            ConnectSendTime = DateTime.Now;
            Console.WriteLine("卡头复位指令：" + jsonString);
        }
        /// <summary>
        /// 托盘应用程序专用发起调用支付接口
        /// 吧台扫玩家付款码
        /// </summary>
        /// <param name="orderID">当前订单号</param>
        /// <param name="authCode">付款码</param>
        /// <returns></returns>
        public bool ScanPayRequest(string orderID, string authCode)
        {
            if (!InitFlag) return false;    //服务器未初始化

            //请求结构
            JsonObject.ScanPayDataRequest o = new JsonObject.ScanPayDataRequest()
            {
                authcode = authCode,
                orderid = orderID,
                signkey = "",
                sn = Guid.NewGuid().ToString().Replace("-", ""),
                token = CurToken
            };
            JsonObject json = new JsonObject();
            o.signkey = json.GetSignKey(o, PwdRead);

            JavaScriptSerializer jss = new JavaScriptSerializer();
            string jsonString = jss.Serialize(o);
            Console.WriteLine("发送 支付请求[" + CurToken + "]：" + jsonString);
            RepeatDataItem item = new RepeatDataItem();
            item.CmdType = 0x33;
            item.DataBuf = Encoding.UTF8.GetBytes(jsonString);
            item.SendOjbect = o;
            SetBodySend(item);
            ConnectSendTime = DateTime.Now;
            return true;
        }
        #endregion
    }
}
