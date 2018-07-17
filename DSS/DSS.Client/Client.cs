using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace DSS.Client
{
    public class Client
    {
        Socket client;
        string ServerIP = "";
        int ServerPort = 0;
        string StoreID = "";
        string MerchID = "";
        string AppSecret = "";
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
        /// <summary>
        /// 待处理数据缓存
        /// </summary>
        Queue<FrameData> queueRecv = new Queue<FrameData>();
        EndPoint serverP;
        Thread tRun = null;
        Thread tTick = null;
        /// <summary>
        /// 初始化成功标志
        /// </summary>
        bool InitFlag = false;
        string CurToken = "";
        public ServerDataItem[] ServerRecvDataList = new ServerDataItem[500];   //最后一条数据
        public class ServerDataItem
        {
            /// <summary>
            /// 指令流水号
            /// </summary>
            public string SN { get; set; }
            /// <summary>
            /// 重发次数，每次覆盖查找重发次数大于10次的或者空的
            /// </summary>
            public int ReSendTimes { get; set; }
            /// <summary>
            /// 主键值
            /// </summary>
            public string IDValue { get; set; }
            /// <summary>
            /// 数据表名
            /// </summary>
            public string TableName { get; set; }
            /// <summary>
            /// 数据内容JSON格式
            /// </summary>
            public string JsonData { get; set; }
            /// <summary>
            /// 同步类别 
            /// 0 新增
            /// 1 修改
            /// 2 删除
            /// </summary>
            public int SyncType { get; set; }
            /// <summary>
            /// 是否已处理过指令
            /// </summary>
            public bool IsProcess { get; set; }
            /// <summary>
            /// 最后发送时间
            /// </summary>
            public DateTime SendTime { get; set; }
        }
        class RecvSyncItem
        {
            public string StoreID { get; set; }
            public string TableName { get; set; }
            public string IdValue { get; set; }
            public string JsonText { get; set; }
            public int Action { get; set; }
            public DateTime CreateTime { get; set; }
        }

        Dictionary<string, RecvSyncItem> StoreSyncOrderList = new Dictionary<string, RecvSyncItem>();
        public Client(string serverIP, int serverPort, string storeID, string merchID, string secret)
        {
            ServerIP = serverIP;
            ServerPort = serverPort;
            StoreID = storeID;
            AppSecret = secret;
            MerchID = merchID;
        }

        public bool Init()
        {
            try
            {
                //return true;

                //ConnectSendTime = DateTime.Now;
                //ConnectRecvTime = DateTime.Now;
                InitCommandSend();

                if (isRun) return false;
                tRun = new Thread(new ThreadStart(CommandProcess)) { IsBackground = true, Name = "客户端处理线程" };
                tRun.Start();

                tTick = new Thread(new ThreadStart(TickProcess)) { IsBackground = true, Name = "心跳处理线程" };
                tTick.Start();

                //tRepeat = new Thread(new ThreadStart(RepeatTick)) { IsBackground = true, Name = "指令重发线程" };
                //tRepeat.Start();

                client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                byte[] optionInValue = { Convert.ToByte(false) };
                byte[] optionOutValue = new byte[4];
                uint IOC_IN = 0x80000000;
                uint IOC_VENDOR = 0x18000000;
                uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
                client.IOControl((int)SIO_UDP_CONNRESET, optionInValue, optionOutValue);
                IPEndPoint serverPoint = new IPEndPoint(IPAddress.Parse(ServerIP), ServerPort);
                serverP = serverPoint;
                client.Connect(serverP);

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
                FrameData f = new FrameData(mybytes, tempRemoteEP);
                if (f.CheckSuccess)
                    queueRecv.Enqueue(f);
                //收到数据处理
                so.socket.BeginReceiveFrom(so.buffer, 0, StateObject.BUF_SIZE, SocketFlags.None, ref tempRemoteEP, new AsyncCallback(RecvCallBack), so);
            }
        }
        void CommandProcess()
        {
            while (true)
            {
                try
                {
                    if (queueRecv.Count > 0)
                    {
                        FrameData f = queueRecv.Dequeue();
                        if (f != null)
                        {
                            switch (f.CommandType)
                            {
                                case TransmiteEnum.通知服务器上线应答:
                                    {
                                        //通知服务器上线
                                        JavaScriptSerializer jss = new JavaScriptSerializer();
                                        //JSON数据格式转换
                                        JsonObject.StoreRegistResponse regist = jss.Deserialize<JsonObject.StoreRegistResponse>(f.FrameJsontxt);
                                        JsonObject jo = new JsonObject();
                                        //计算签名
                                        if (jo.GetSignKey(regist, AppSecret) == regist.SignKey)
                                        {
                                            //签名验证通过
                                            CurToken = regist.Token;
                                            ConnectRecvTime = DateTime.Now;
                                            InitFlag = true;
                                            Console.WriteLine("注册成功，当前令牌：" + CurToken);
                                        }
                                        else
                                        {
                                            Console.WriteLine("注册失败：签名错误");
                                        }
                                    }
                                    break;
                                case TransmiteEnum.心跳:
                                    {
                                        JavaScriptSerializer jss = new JavaScriptSerializer();
                                        //JSON数据格式转换
                                        JsonObject.StoreTick request = jss.Deserialize<JsonObject.StoreTick>(f.FrameJsontxt);
                                        JsonObject jo = new JsonObject();
                                        //计算签名
                                        if (jo.GetSignKey(request, AppSecret) == request.SignKey)
                                        {
                                            //签名验证通过
                                            ConnectRecvTime = DateTime.Now;
                                            InitFlag = true;
                                            Console.WriteLine("收到门店心跳");
                                        }
                                        else
                                        {
                                            Console.WriteLine("门店心跳：签名错误");
                                        }
                                    }
                                    break;
                                case TransmiteEnum.门店数据变更同步应答:
                                    {
                                        Console.WriteLine("收到门店数据同步应答：" + f.FrameJsontxt);
                                        JavaScriptSerializer jss = new JavaScriptSerializer();
                                        //JSON数据格式转换
                                        JsonObject.DataSyncResponse response = jss.Deserialize<JsonObject.DataSyncResponse>(f.FrameJsontxt);
                                        JsonObject jo = new JsonObject();
                                        //计算签名
                                        if (jo.GetSignKey(response, AppSecret) == response.SignKey)
                                        {
                                            //签名验证通过
                                            ClearCommandSend(response.SN);
                                        }
                                        else
                                            Console.WriteLine("门店数据同步应答：签名错误");
                                    }
                                    break;
                                case TransmiteEnum.云端数据变更同步请求:
                                    {
                                        Console.WriteLine("收到云端数据同步应答：" + f.FrameJsontxt);
                                        JavaScriptSerializer jss = new JavaScriptSerializer();
                                        //JSON数据格式转换
                                        JsonObject.DataSyncRequest request = jss.Deserialize<JsonObject.DataSyncRequest>(f.FrameJsontxt);
                                        JsonObject jo = new JsonObject();
                                        //计算签名
                                        if (jo.GetSignKey(request, AppSecret) == request.SignKey)
                                        {
                                            //签名验证通过
                                            //执行数据同步
                                            if (!StoreSyncOrderList.ContainsKey(request.SN))
                                            {
                                                //令牌校验成功
                                                DSS.DataAccess ac = new DSS.DataAccess();
                                                if (request.Action == 0)
                                                    ac.SyncAddData(request.TableName, request.JsonText, AppSecret);//新增同步
                                                else if (request.Action == 1)
                                                    ac.SyncUpdateData(request.TableName, request.IdValue, request.JsonText, AppSecret); //修改同步
                                                else if (request.Action == 2)
                                                    ac.SyncDeleteData(request.TableName, request.IdValue);    //删除同步
                                                //添加缓存，避免重复执行
                                                RecvSyncItem item = new RecvSyncItem();
                                                item.Action = request.Action;
                                                item.CreateTime = DateTime.Now;
                                                item.IdValue = request.IdValue;
                                                item.JsonText = request.JsonText;
                                                item.StoreID = request.StoreID;
                                                item.TableName = request.TableName;
                                                StoreSyncOrderList.Add(request.SN, item);
                                            }
                                            JsonObject.DataSyncResponse response = new JsonObject.DataSyncResponse();
                                            response.SN = request.SN;
                                            response.StoreID = StoreID;
                                            response.Token = request.Token;
                                            response.SignKey = jo.GetSignKey(response, AppSecret);
                                            string jsonString = jss.Serialize(response);
                                            SendData(Encoding.UTF8.GetBytes(jsonString), (byte)TransmiteEnum.云端数据变更同步应答);
                                            ConnectSendTime = DateTime.Now;
                                        }
                                        else
                                            Console.WriteLine("云端数据同步应答：签名错误");
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    else
                    {
                        Thread.Sleep(50);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                finally
                { }
            }
        }
        void InitCommandSend()
        {
            for (int i = 0; i < 500; i++)
            {
                ServerRecvDataList[i] = new ServerDataItem();
                ServerRecvDataList[i].SN = "";
                ServerRecvDataList[i].IsProcess = false;
            }
        }
        void SetCommandSend(ServerDataItem item)
        {
            for (int i = 0; i < 500; i++)
            {
                if (!ServerRecvDataList[i].IsProcess)
                {
                    //查找到有效空队列
                    ServerRecvDataList[i].IDValue = item.IDValue;
                    ServerRecvDataList[i].IsProcess = true;
                    ServerRecvDataList[i].JsonData = item.JsonData;
                    ServerRecvDataList[i].ReSendTimes = 0;
                    ServerRecvDataList[i].SN = item.SN;
                    ServerRecvDataList[i].SyncType = item.SyncType;
                    ServerRecvDataList[i].TableName = item.TableName;
                    break;
                }
            }
        }
        /// <summary>
        /// 清除应答数据
        /// </summary>
        /// <param name="storeID"></param>
        /// <param name="sn"></param>
        void ClearCommandSend(string sn)
        {
            for (int i = 0; i < 500; i++)
            {
                if (ServerRecvDataList[i].SN == sn)
                {
                    //查找到有效空队列
                    ServerRecvDataList[i].IsProcess = false;
                    ServerRecvDataList[i].SN = "";
                    break;
                }
            }
        }
        /// <summary>
        /// 获取当前需要发送的队列
        /// </summary>
        /// <param name="storeID"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        bool GetCommandSend(out int num)
        {
            num = 0;
            for (int i = 0; i < 500; i++)
            {
                if (!ServerRecvDataList[i].IsProcess && ServerRecvDataList[i].SN == "")
                {
                    //查找到有效空队列
                    num = i;
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 发送需要发送的队列
        /// </summary>
        void SendCommandSend()
        {
            for (int i = 0; i < 500; i++)
            {
                if (ServerRecvDataList[i].IsProcess && ServerRecvDataList[i].SN != "")
                {
                    //查找到有效空队列
                    JsonObject.DataSyncRequest request = new JsonObject.DataSyncRequest();
                    request.Action = ServerRecvDataList[i].SyncType;
                    request.IdValue = ServerRecvDataList[i].IDValue;
                    request.JsonText = ServerRecvDataList[i].JsonData;
                    request.SN = ServerRecvDataList[i].SN;
                    request.StoreID = StoreID;
                    request.TableName = ServerRecvDataList[i].TableName;
                    request.Token = CurToken;

                    JsonObject json = new JsonObject();
                    request.SignKey = json.GetSignKey(request, AppSecret);

                    JavaScriptSerializer jss = new JavaScriptSerializer();
                    string jsonString = jss.Serialize(request);
                    SendData(Encoding.UTF8.GetBytes(jsonString), (byte)TransmiteEnum.门店数据变更同步请求);
                    ConnectSendTime = DateTime.Now;
                }
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

            client.BeginSendTo(cmd.ToArray(), 0, cmd.Count(), SocketFlags.None, serverP, new AsyncCallback(SendCallBack), client);
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
        /// 保持心跳和服务器重连
        /// </summary>
        void TickProcess()
        {
            int i = 0;
            while (true)
            {
                SendCommandSend();
                if (InitFlag)
                {
                    if (ConnectRecvTime.AddSeconds(60) < ConnectSendTime)
                    {
                        //超过1分钟没有收到任何服务器的数据则视为服务器断开
                        InitFlag = false;
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
                Thread.Sleep(1000);
            }
        }
        /// <summary>
        /// 路由器注册
        /// </summary>
        public void RegistRouteDevice()
        {
            JsonObject.StoreRegistRequest regist = new JsonObject.StoreRegistRequest()
            {
                StoreID = StoreID,
                MerchID = MerchID,
                SignKey = ""
            };

            JsonObject json = new JsonObject();
            regist.SignKey = json.GetSignKey(regist, AppSecret);

            JavaScriptSerializer jss = new JavaScriptSerializer();
            string jsonString = jss.Serialize(regist);
            Console.WriteLine("发送 注册信息[" + CurToken + "]：" + jsonString);
            SendData(Encoding.UTF8.GetBytes(jsonString), (byte)TransmiteEnum.通知服务器上线);
            ConnectSendTime = DateTime.Now;
        }
        /// <summary>
        /// 路由器心跳
        /// </summary>
        public void TickConnectServer()
        {
            if (!isRun) return;
            JsonObject.StoreTick tick = new JsonObject.StoreTick()
            {
                Token = CurToken,
                StoreID = StoreID
            };
            JsonObject json = new JsonObject();
            tick.SignKey = json.GetSignKey(tick, AppSecret);

            JavaScriptSerializer jss = new JavaScriptSerializer();
            string jsonString = jss.Serialize(tick);
            //Console.WriteLine("发送 心跳[" + CurToken + "]：" + jsonString);
            SendData(Encoding.UTF8.GetBytes(jsonString), (byte)TransmiteEnum.心跳);
            ConnectSendTime = DateTime.Now;
        }
        public void StoreDataSync(string storeID, string tableName, string idValue, int action)
        {
            string sql = "select * from " + tableName + " where id='" + idValue + "'";
            Assembly asmb = Assembly.LoadFrom("DSS.dll");
            Type t = asmb.GetType("DSS.Table." + tableName);
            object o = System.Activator.CreateInstance(t);
            DataModel model = new DataModel();
            model.CovertToDataModel(sql, ref o);
            JavaScriptSerializer jss = new JavaScriptSerializer();
            string jsonString = jss.Serialize(o);

            if (!InitFlag) return;
            ServerDataItem item = new ServerDataItem();
            item.IDValue = idValue;
            item.JsonData = jsonString;
            item.ReSendTimes = 0;
            item.SN = Guid.NewGuid().ToString().Replace("-", "").ToLower();
            item.SyncType = action;
            item.TableName = tableName;

            SetCommandSend(item);
        }
        void SendProcess()
        {
            while (true)
            {
                try
                {
                    DateTime d = DateTime.Now;

                    //遍历所有用户缓存
                    for (int i = 0; i < 500; i++)
                    {
                        if (ServerRecvDataList[i].IsProcess && ServerRecvDataList[i].SendTime > d.AddSeconds(-3))
                        {
                            //超时处理
                            ServerRecvDataList[i].IsProcess = false;
                        }

                        if (!ServerRecvDataList[i].IsProcess)
                        {
                            if (ServerRecvDataList[i].SN != "")
                            {
                                //初始化要发送的对象
                                JsonObject.DataSyncRequest request = new JsonObject.DataSyncRequest();
                                request.Action = ServerRecvDataList[i].SyncType;
                                request.IdValue = ServerRecvDataList[i].IDValue;
                                request.JsonText = ServerRecvDataList[i].JsonData;
                                request.SignKey = "";
                                request.SN = ServerRecvDataList[i].SN;
                                request.StoreID = StoreID;
                                request.TableName = ServerRecvDataList[i].TableName;
                                request.Token = CurToken;
                                DataModel model = new DataModel();
                                model.Verifiction(request, AppSecret);

                                JavaScriptSerializer jss = new JavaScriptSerializer();
                                string jsonString = jss.Serialize(request);
                                SendData(Encoding.UTF8.GetBytes(jsonString), (byte)TransmiteEnum.门店数据变更同步请求);
                                ServerRecvDataList[i].IsProcess = true;
                                ServerRecvDataList[i].SendTime = DateTime.Now;
                                Thread.Sleep(10);
                            }
                        }
                    }
                    //每1秒钟查询一次
                    Thread.Sleep(1000);
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog(ex);
                }
            }
        }
    }
}
