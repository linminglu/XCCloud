using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Data;
using DSS;
using System.Reflection;

namespace DSS.Server
{
    public class Server
    {
        public class RecvItem
        {
            public EndPoint RecvPoint { get; set; }
            public byte[] RecvData { get; set; }
        }
        public class ClientItemObject
        {
            public ClientDataItem[] data = new ClientDataItem[500];   //最后一条数据
            public EndPoint RemotePoint { get; set; }   //当前客户端连接标识
            public string Token { get; set; }           //门店令牌
            public string AppSecret { get; set; }       //计算签名用的秘钥
            public DateTime UpdateTime { get; set; }    //更新时间
        }
        /// <summary>
        /// 当前数据处理缓存队列，默认缓存500条
        /// </summary>
        public class ClientDataItem
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

        /// <summary>
        /// 所有用户列表
        /// </summary>
        Dictionary<string, ClientItemObject> UserClientList = new Dictionary<string, ClientItemObject>();
        Socket server;
        ManualResetEvent allDone = new ManualResetEvent(false);
        List<byte> recvBUF = new List<byte>();                   //当前接收数据缓存
        public int ServerDataBUFCount { get { return queueRecv.Count; } }
        Queue<FrameData> queueRecv = new Queue<FrameData>();  //待处理数据缓存
        Thread tRun = null;
        Thread sRun = null;
        bool isRun = false;
        const int PacketLength = 1024 * 8;
        public void Init(int Port)
        {
            try
            {
                if (!isRun)
                {
                    isRun = true;

                    tRun = new Thread(new ThreadStart(CommandProcess)) { IsBackground = true, Name = "服务器处理线程" };
                    tRun.Start();

                    sRun = new Thread(new ThreadStart(SendProcess)) { IsBackground = true, Name = "主发处理线程" };
                    sRun.Start();

                    server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    byte[] optionInValue = { Convert.ToByte(false) };
                    byte[] optionOutValue = new byte[4];
                    uint IOC_IN = 0x80000000;
                    uint IOC_VENDOR = 0x18000000;
                    uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
                    server.IOControl((int)SIO_UDP_CONNRESET, optionInValue, optionOutValue);
                    IPEndPoint p = new IPEndPoint(IPAddress.Any, Port);
                    server.Bind((EndPoint)p);

                    IPEndPoint sendP = new IPEndPoint(IPAddress.Any, 0);
                    EndPoint tempRemoteEP = (EndPoint)sendP;

                    StateObject so = new StateObject();
                    so.socket = server;
                    server.BeginReceiveFrom(so.buffer, 0, StateObject.BUF_SIZE, SocketFlags.None, ref tempRemoteEP, new AsyncCallback(ReceiveCallback), so);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("初始化失败");
                Console.WriteLine(ex);
                LogHelper.WriteLog(ex);
            }
        }

        void ReceiveCallback(IAsyncResult ar)
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
                LogHelper.WriteLog(oe);
                throw oe;
            }
            catch (SocketException se)
            {
                Console.WriteLine(se);
                LogHelper.WriteLog(se);
                throw se;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                LogHelper.WriteLog(e);
                // 获得接收失败信息 
                throw e;
            }

            if (readBytes > 0)
            {
                byte[] mybytes = new byte[readBytes];
                Array.Copy(so.buffer, mybytes, readBytes);
                Console.WriteLine(tempRemoteEP.ToString() + "  " + recvBUF.Count);
                FrameData f = new FrameData(mybytes, tempRemoteEP);
                if (f.CheckSuccess)
                    queueRecv.Enqueue(f);
                //收到数据处理
                so.socket.BeginReceiveFrom(so.buffer, 0, StateObject.BUF_SIZE, SocketFlags.None, ref tempRemoteEP, new AsyncCallback(ReceiveCallback), so);
            }
        }
        void InitCommandSend(string storeID)
        {
            for (int i = 0; i < 500; i++)
            {
                UserClientList[storeID].data[i] = new ClientDataItem();
                UserClientList[storeID].data[i].IsProcess = false;
                UserClientList[storeID].data[i].SN = "";
            }
        }
        /// <summary>
        /// 插入需要发送的数据
        /// </summary>
        /// <param name="storeID"></param>
        /// <param name="item"></param>
        void SetCommandSend(string storeID, ClientDataItem item)
        {
            if (UserClientList.ContainsKey(storeID))
            {
                for (int i = 0; i < 500; i++)
                {
                    if (!UserClientList[storeID].data[i].IsProcess && UserClientList[storeID].data[i].SN == "")
                    {
                        //查找到有效空队列
                        UserClientList[storeID].data[i].IDValue = item.IDValue;
                        UserClientList[storeID].data[i].JsonData = item.JsonData;
                        UserClientList[storeID].data[i].ReSendTimes = 0;
                        UserClientList[storeID].data[i].SN = item.SN;
                        UserClientList[storeID].data[i].SyncType = item.SyncType;
                        UserClientList[storeID].data[i].TableName = item.TableName;
                        break;
                    }
                }
            }
        }
        /// <summary>
        /// 清除应答数据
        /// </summary>
        /// <param name="storeID"></param>
        /// <param name="sn"></param>
        void ClearCommandSend(string storeID, string sn)
        {
            if (UserClientList.ContainsKey(storeID))
            {
                for (int i = 0; i < 500; i++)
                {
                    if (UserClientList[storeID].data[i].SN == sn)
                    {
                        //查找到有效空队列
                        UserClientList[storeID].data[i].IsProcess = false;
                        UserClientList[storeID].data[i].SN = "";
                        break;
                    }
                }
            }
        }
        /// <summary>
        /// 获取当前需要发送的队列
        /// </summary>
        /// <param name="storeID"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        bool GetCommandSend(string storeID, out int num)
        {
            num = 0;
            if (UserClientList.ContainsKey(storeID))
            {
                for (int i = 0; i < 500; i++)
                {
                    if (!UserClientList[storeID].data[i].IsProcess && UserClientList[storeID].data[i].SN == "")
                    {
                        //查找到有效空队列
                        num = i;
                        return true;
                    }
                }
            }
            return false;
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
                                case TransmiteEnum.通知服务器上线:
                                    {
                                        //通知服务器上线
                                        JavaScriptSerializer jss = new JavaScriptSerializer();
                                        //JSON数据格式转换
                                        JsonObject.StoreRegistRequest request = jss.Deserialize<JsonObject.StoreRegistRequest>(f.FrameJsontxt);
                                        JsonObject jo = new JsonObject();
                                        //获取当前商户秘钥
                                        string secret = GetAppSecret(request.MerchID);
                                        //计算签名
                                        if (jo.GetSignKey(request, secret) == request.SignKey)
                                        {
                                            //签名验证通过
                                            string token = "";
                                            //更新用户连接信息
                                            RecvClientData(request.StoreID, f.RecvPoint, secret, out token);
                                            //应答
                                            JsonObject.StoreRegistResponse response = new JsonObject.StoreRegistResponse();
                                            response.Token = token;
                                            response.SignKey = jo.GetSignKey(response, secret);
                                            string jsonString = jss.Serialize(response);
                                            SendData(Encoding.UTF8.GetBytes(jsonString), (byte)TransmiteEnum.通知服务器上线应答, f.RecvPoint);
                                            Console.WriteLine("门店注册上线应答：" + jsonString);
                                            //上线时同步
                                            SyncOffData(request.StoreID);
                                        }
                                        else
                                        {
                                            Console.WriteLine("门店注册上线：签名错误");
                                        }
                                    }
                                    break;
                                case TransmiteEnum.心跳:
                                    {
                                        JavaScriptSerializer jss = new JavaScriptSerializer();
                                        //JSON数据格式转换
                                        JsonObject.StoreTick request = jss.Deserialize<JsonObject.StoreTick>(f.FrameJsontxt);
                                        JsonObject jo = new JsonObject();
                                        //获取当前商户秘钥
                                        string secret = GetAppSecretFromDict(request.StoreID);
                                        //计算签名
                                        if (jo.GetSignKey(request, secret) == request.SignKey)
                                        {
                                            //签名验证通过
                                            //更新用户连接信息
                                            if (CheckClientToken(request.StoreID, request.Token))
                                            {
                                                //令牌校验通过
                                                RecvClientData(request.StoreID, true);
                                                //应答心跳数据
                                                SendData(f.RecvData, (byte)TransmiteEnum.心跳, f.RecvPoint);
                                            }
                                            else
                                            {
                                                Console.WriteLine("令牌错误");
                                            }
                                        }
                                        else
                                        {
                                            RecvClientData(request.StoreID, false);
                                            Console.WriteLine("门店心跳：签名错误");
                                        }
                                    }
                                    break;
                                case TransmiteEnum.云端数据变更同步应答:
                                    {
                                        JavaScriptSerializer jss = new JavaScriptSerializer();
                                        //JSON数据格式转换
                                        JsonObject.DataSyncResponse response = jss.Deserialize<JsonObject.DataSyncResponse>(f.FrameJsontxt);
                                        JsonObject jo = new JsonObject();
                                        //获取当前商户秘钥
                                        string secret = GetAppSecretFromDict(response.StoreID);
                                        //计算签名
                                        if (jo.GetSignKey(response, secret) == response.SignKey)
                                        {
                                            //签名验证通过
                                            ClearCommandSend(response.StoreID, response.SN);
                                            ClearDataSyncBUF(response.SN);
                                        }
                                    }
                                    break;
                                case TransmiteEnum.门店数据变更同步请求:
                                    {
                                        JavaScriptSerializer jss = new JavaScriptSerializer();
                                        //JSON数据格式转换
                                        JsonObject.DataSyncRequest request = jss.Deserialize<JsonObject.DataSyncRequest>(f.FrameJsontxt);
                                        JsonObject jo = new JsonObject();
                                        //获取当前商户秘钥
                                        string secret = GetAppSecretFromDict(request.StoreID);
                                        //计算签名
                                        if (jo.GetSignKey(request, secret) == request.SignKey)
                                        {
                                            //签名验证通过
                                            //执行数据同步
                                            if (!StoreSyncOrderList.ContainsKey(request.SN))
                                            {
                                                if (CheckClientToken(request.StoreID, request.Token))
                                                {
                                                    //令牌校验成功
                                                    DSS.DataAccess ac = new DSS.DataAccess();
                                                    if (request.Action == 0)
                                                        ac.SyncAddData(request.TableName, request.JsonText, secret);//新增同步
                                                    else if (request.Action == 1)
                                                        ac.SyncUpdateData(request.TableName, request.IdValue, request.JsonText, secret); //修改同步
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
                                            }
                                            JsonObject.DataSyncResponse response = new JsonObject.DataSyncResponse();
                                            response.SN = request.SN;
                                            response.StoreID = request.StoreID;
                                            response.Token = request.Token;
                                            response.SignKey = jo.GetSignKey(response, secret);
                                            string jsonString = jss.Serialize(response);
                                            SendData(Encoding.UTF8.GetBytes(jsonString), (byte)TransmiteEnum.门店数据变更同步应答, f.RecvPoint);
                                        }
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
        public void SendData(byte[] data, byte cmdType, EndPoint p)
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

            server.BeginSendTo(cmd.ToArray(), 0, cmd.Count(), SocketFlags.None, p, new AsyncCallback(SendCallBack), server);
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
        string GetAppSecretFromDict(string storeID)
        {
            if (UserClientList.ContainsKey(storeID))
                return UserClientList[storeID].AppSecret;
            return "";
        }
        string GetAppSecret(string merchID)
        {
            DataAccess ac = new DataAccess();
            DataTable dt = ac.ExecuteQueryReturnTable("select MerchSecret,ProxyType from Base_MerchantInfo where ID='" + merchID + "'");
            if (dt.Rows.Count > 0)
                return dt.Rows[0]["MerchSecret"].ToString();
            return "";
        }
        string GetAppToken(string storeID)
        {
            if (UserClientList.ContainsKey(storeID))
            {
                return UserClientList[storeID].Token;
            }
            return "";
        }
        void RecvClientData(string storeID, EndPoint p, string secret, out string token)
        {
            if (UserClientList.ContainsKey(storeID))
            {
                UserClientList[storeID].RemotePoint = p;
                UserClientList[storeID].Token = Guid.NewGuid().ToString().Replace("-", "").ToLower();
                UserClientList[storeID].UpdateTime = DateTime.Now;
                UserClientList[storeID].AppSecret = secret;
                token = UserClientList[storeID].Token;
            }
            else
            {
                ClientItemObject o = new ClientItemObject();
                o.RemotePoint = p;
                o.Token = Guid.NewGuid().ToString().Replace("-", "").ToLower();
                o.UpdateTime = DateTime.Now;
                o.AppSecret = secret;
                UserClientList.Add(storeID, o);
                InitCommandSend(storeID);
                token = o.Token;
            }
        }
        void RecvClientData(string storeID, bool IsConnect)
        {
            if (UserClientList.ContainsKey(storeID))
            {
                if (IsConnect)  //连接成功更新时间
                    UserClientList[storeID].UpdateTime = DateTime.Now;
                else            //连接失败清除令牌
                    UserClientList[storeID].Token = "";
            }
        }
        /// <summary>
        /// 校验令牌是否正确
        /// </summary>
        /// <param name="storeID"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        bool CheckClientToken(string storeID, string token)
        {
            if (UserClientList.ContainsKey(storeID))
                return UserClientList[storeID].Token == token;
            return false;
        }
        /// <summary>
        /// 当门店上线时，同步所有的离线数据
        /// </summary>
        /// <param name="storeID"></param>
        void SyncOffData(string storeID)
        {
            DataModel model = new DataModel();
            List<object> dataList = new List<object>();
            if (model.CovertToDataModel("select * from Sync_DataList where StoreID='" + storeID + "'", typeof(DSS.Table.Sync_DataList), out dataList))
            {
                foreach (DSS.Table.Sync_DataList o in dataList)
                {
                    string secret = GetAppSecretFromDict(o.StoreID);
                    CloudDataSync(o.MerchID, secret, o.TableName, o.IDValue, o.SyncType, false);
                    Console.WriteLine("离线同步：table=" + o.TableName + "  id=" + o.IDValue);
                }
            }
        }
        /// <summary>
        /// 同步成功后清除同步标记
        /// </summary>
        /// <param name="storeID"></param>
        /// <param name="sn"></param>
        /// <param name="action"></param>
        void ClearDataSyncBUF(string sn)
        {
            DataModel model = new DataModel();
            object o = new DSS.Table.Sync_DataList();
            if (model.CovertToDataModel("select * from Sync_DataList where SN='" + sn + "' and SyncFlag=0", ref o))
            {
                DSS.Table.Sync_DataList d = (DSS.Table.Sync_DataList)o;
                d.SyncFlag = 1;
                d.SyncTime = DateTime.Now;
                model.Update(d, "where sn='" + sn + "'");
            }
        }
        /// <summary>
        /// 云端数据变更时调用，向门店发送同步
        /// </summary>
        /// <param name="merchID">当前商户编号</param>
        /// <param name="secret">商户秘钥</param>
        /// <param name="tableName">操作表名</param>
        /// <param name="idValue">主键值</param>
        /// <param name="action">0 新增 1 修改 2 删除</param>
        public void CloudDataSync(string merchID, string secret, string tableName, string idValue, int action, bool writeBuf = true)
        {
            DataModel model = new DataModel();
            string sql = "select * from " + tableName + " where id='" + idValue + "'";
            //Assembly asmb = Assembly.LoadFrom("DSS.dll");
            Assembly asmb = Assembly.LoadFrom(AppDomain.CurrentDomain.RelativeSearchPath + "\\DSS.dll");
            Type t = asmb.GetType("DSS.Table." + tableName);
            object o = System.Activator.CreateInstance(t);            

            model.CovertToDataModel(sql, ref o);
            JavaScriptSerializer jss = new JavaScriptSerializer();
            string jsonString = jss.Serialize(o);

            ClientDataItem item = new ClientDataItem();
            item.IDValue = idValue;
            item.JsonData = jsonString;
            item.ReSendTimes = 0;
            item.SN = Guid.NewGuid().ToString().Replace("-", "").ToLower();
            item.SyncType = action;
            item.TableName = tableName;

            DataAccess ac = new DataAccess();
            DataTable dt = ac.ExecuteQueryReturnTable("select distinct StoreID from Base_ChainRule_Store where MerchID='" + merchID + "'");
            foreach (DataRow row in dt.Rows)
            {
                string storeID = row["StoreID"].ToString();
                if (writeBuf)
                {
                    Table.Sync_DataList sync = new Table.Sync_DataList();
                    sync.CreateTime = DateTime.Now;
                    sync.IDValue = idValue;
                    sync.MerchID = merchID;
                    sync.StoreID = storeID;
                    sync.SyncFlag = 0;
                    sync.SyncType = action;
                    sync.TableName = tableName;
                    sync.SN = item.SN;
                    sync.Verifiction = model.Verifiction(sync, secret, true);
                    model.Add(sync, true);
                }

                if (UserClientList.ContainsKey(storeID))
                {
                    if (UserClientList[storeID].UpdateTime > DateTime.Now.AddSeconds(-30))
                    {
                        //心跳没有超时，表示门店同步服务在线允许发送
                        SetCommandSend(storeID, item);
                    }
                }
            }
        }
        /// <summary>
        /// 收到门店同步应答后修改同步缓存
        /// </summary>
        /// <param name="sn">指令流水号</param>
        void ClearSyncTable(string sn)
        {
            //string sql="select * from Sync_DataList"
        }
        void SendProcess()
        {
            while (true)
            {
                try
                {
                    DateTime d = DateTime.Now;

                    foreach (string storeID in UserClientList.Keys)
                    {
                        //遍历所有用户缓存
                        for (int i = 0; i < 500; i++)
                        {
                            if (UserClientList[storeID].data[i].IsProcess && UserClientList[storeID].data[i].SendTime > d.AddSeconds(-3))
                            {
                                //超时处理
                                UserClientList[storeID].data[i].IsProcess = false;
                            }

                            if (!UserClientList[storeID].data[i].IsProcess)
                            {
                                if (UserClientList[storeID].data[i].SN != "")
                                {
                                    //初始化要发送的对象
                                    JsonObject.DataSyncRequest request = new JsonObject.DataSyncRequest();
                                    request.Action = UserClientList[storeID].data[i].SyncType;
                                    request.IdValue = UserClientList[storeID].data[i].IDValue;
                                    request.JsonText = UserClientList[storeID].data[i].JsonData;
                                    request.SignKey = "";
                                    request.SN = UserClientList[storeID].data[i].SN;
                                    request.StoreID = storeID;
                                    request.TableName = UserClientList[storeID].data[i].TableName;
                                    request.Token = UserClientList[storeID].Token;
                                    JsonObject json = new JsonObject();
                                    request.SignKey = json.GetSignKey(request, UserClientList[storeID].AppSecret);

                                    JavaScriptSerializer jss = new JavaScriptSerializer();
                                    string jsonString = jss.Serialize(request);
                                    SendData(Encoding.UTF8.GetBytes(jsonString), (byte)TransmiteEnum.云端数据变更同步请求, UserClientList[storeID].RemotePoint);
                                    UserClientList[storeID].data[i].IsProcess = true;
                                    UserClientList[storeID].data[i].SendTime = DateTime.Now;
                                    Thread.Sleep(10);
                                }
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
