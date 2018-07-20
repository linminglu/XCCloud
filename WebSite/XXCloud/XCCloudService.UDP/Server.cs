﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Text;
using System.IO;
using XCCloudService.SocketService.UDP.Factory;
using XCCloudService.Common;
using System.Collections;
using XCCloudService.Model.Socket.UDP;
using XCCloudService.Business.Common;
using XCCloudService.Model.CustomModel.Common;
using XCCloudService.SocketService.TCP.Business;
using XCCloudService.SocketService.TCP.Model;
using XCCloudService.Common.Enum;
using XCCloudService.Model.CustomModel.XCGame;
using XCCloudService.Business.XCGameMana;
using XCCloudService.Business.XCGame;
using XCCloudService.SocketService.UDP.Security;
using XCCloudService.SocketService.UDP.Common;
using XCCloudService.Pay;


namespace XCCloudService.SocketService.UDP
{
    public class Server
    {
        static Socket client;
        static ManualResetEvent allDone = new ManualResetEvent(false);
        static List<byte> recvBUF = new List<byte>();                   //当前接收数据缓存
        public static int ServerDataBUFCount { get { return queueRecv.Count; } }
        static Queue<UDPClientItemBusiness.ClientItem> queueRecv = new Queue<UDPClientItemBusiness.ClientItem>();  //待处理数据缓存
        static bool isPacket = false;
        static Thread tRun = null;
        static Thread tFailRun = null;
        static Thread tAnswerRun = null;
        static bool isRun = false;
        static string SavePath, StorePassword;
        static List<TransmiteObject.文件传输结构> fileRecvBUF = new List<TransmiteObject.文件传输结构>();
        static List<TransmiteObject.文件下载结构> fileSendBUF = new List<TransmiteObject.文件下载结构>();
        const int PacketLength = 1024 * 8;
        class AskItem
        {
            public EndPoint RemotePoint { get; set; }
            public string Secret { get; set; }
        }
        /// <summary>
        /// 指令流水号对应远程节点
        /// 用于异步应答远程指令
        /// </summary>
        static Dictionary<string, AskItem> AskSNList = new Dictionary<string, AskItem>();

        public static Socket SocketServer
        {
            get { return client; }
        }

        static void SystemInit(int Port)
        {
            try
            {
                if (!isRun)
                {
                    isRun = true;

                    tRun = new Thread(new ThreadStart(CommandProcess)) { IsBackground = true, Name = "服务器处理线程" };
                    tRun.Start();

                    tFailRun = new Thread(new ThreadStart(FailResponseProcess)) { IsBackground = true, Name = "响应失败服务器处理线程" };
                    tFailRun.Start();

                    tAnswerRun = new Thread(new ThreadStart(FailResponseProcess)) { IsBackground = true, Name = "响应失败服务器处理线程" };
                    tAnswerRun.Start();

                    client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    byte[] optionInValue = { Convert.ToByte(false) };
                    byte[] optionOutValue = new byte[4];
                    uint IOC_IN = 0x80000000;
                    uint IOC_VENDOR = 0x18000000;
                    uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
                    client.IOControl((int)SIO_UDP_CONNRESET, optionInValue, optionOutValue);
                    IPEndPoint p = new IPEndPoint(IPAddress.Any, Port);
                    client.Bind((EndPoint)p);

                    IPEndPoint sendP = new IPEndPoint(IPAddress.Any, 0);
                    EndPoint tempRemoteEP = (EndPoint)sendP;

                    StateObject so = new StateObject();
                    so.socket = client;
                    client.BeginReceiveFrom(so.buffer, 0, StateObject.BUF_SIZE, SocketFlags.None, ref tempRemoteEP, new AsyncCallback(ReceiveCallback), so);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("初始化失败");
                Console.WriteLine(ex);
                LogHelper.SaveLog(TxtLogType.UPDService, TxtLogContentType.Exception, TxtLogFileType.Time, "SystemInit:" + ex.Message);
            }
        }
        public static void Init(int Port)
        {
            SystemInit(Port);
        }

        public static void Init(int Port, string savepath, string password)
        {
            SavePath = savepath;
            StorePassword = password;
            SystemInit(Port);
        }

        public static void End()
        {
            client.Close();
        }

        public static void ReceiveCallback(IAsyncResult ar)
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

                //throw oe;
                return;
            }
            catch (SocketException se)
            {
                Console.WriteLine(se);
                LogHelper.SaveLog(TxtLogType.UPDService, TxtLogContentType.Exception, TxtLogFileType.Time, "ReceiveCallback:" + se.Message);
                throw se;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                LogHelper.SaveLog(TxtLogType.UPDService, TxtLogContentType.Exception, TxtLogFileType.Time, "ReceiveCallback:" + e.Message);
                // 获得接收失败信息 
                throw e;
            }

            if (readBytes > 0)
            {

                //接受处理代码
                byte[] mybytes = new byte[readBytes];
                Array.Copy(so.buffer, mybytes, readBytes);

                if (DataFactory.IsProtocolData(mybytes))
                {
                    UDPClientItemBusiness.ClientItem item = new UDPClientItemBusiness.ClientItem() { data = mybytes, remotePoint = tempRemoteEP };
                    lock (queueRecv)
                    {
                        queueRecv.Enqueue(item);
                    }
                }
                //收到数据处理
                so.socket.BeginReceiveFrom(so.buffer, 0, StateObject.BUF_SIZE, SocketFlags.None, ref tempRemoteEP, new AsyncCallback(ReceiveCallback), so);
            }
            else
            {
                LogHelper.SaveLog(TxtLogType.UPDService, TxtLogContentType.Exception, TxtLogFileType.Time, "readBytes = 0:" + tempRemoteEP.Serialize());
            }
        }

        public static void Send(string IP, int Port, byte[] data)
        {
            try
            {
                IPEndPoint clients = new IPEndPoint(IPAddress.Parse(IP), Port);
                EndPoint epSender = (EndPoint)clients;
                client.BeginSendTo(data, 0, data.Length, SocketFlags.None, epSender, new AsyncCallback(SendCallBack), client);

            }
            catch (Exception e)
            {
                LogHelper.SaveLog(TxtLogType.UPDService, TxtLogContentType.Exception, TxtLogFileType.Day, "Send(string IP, int Port, byte[] data);" + e.Message);
                //throw;
            }
        }
        public static void Send(EndPoint p, byte[] data)
        {
            try
            {
                client.BeginSendTo(data, 0, data.Length, SocketFlags.None, p, new AsyncCallback(SendCallBack), client);

            }
            catch (Exception e)
            {
                LogHelper.SaveLog(TxtLogType.UPDService, TxtLogContentType.Exception, TxtLogFileType.Day, "Send(string IP, int Port, byte[] data);" + e.Message);
                //throw;
            }
        }

        public static void SendCallBack(IAsyncResult ar)
        {
            try
            {
                Socket Client = (Socket)ar.AsyncState;
                int n = Client.EndSend(ar);
            }
            catch (Exception e)
            {
                LogHelper.SaveLog(TxtLogType.UPDService, TxtLogContentType.Exception, TxtLogFileType.Time, "SendCallBack:" + e.Message);
            }
        }

        static void CommandProcess()
        {
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                if (queueRecv.Count > 0)
                {
                    try
                    {
                        sb = new StringBuilder();

                        UDPClientItemBusiness.ClientItem item = null;
                        lock (queueRecv)
                        {
                            item = queueRecv.Dequeue();
                            if (item == null) sb.Append("queueRecv出列为空" + Environment.NewLine);
                        }
                        if (item != null)
                        {
                            int requestTransmiteEnumValue = 0;
                            string data = string.Empty;
                            string clientId = string.Empty;
                            int packId = 0;
                            int packNum = 0;
                            DataFactory.GetProtocolData(item.data, out requestTransmiteEnumValue, out data, out packId, out packNum);
                            //Console.WriteLine("接收数据 [" + ((TransmiteEnum)requestTransmiteEnumValue).ToString() + "]：" + data);
                            switch ((TransmiteEnum)requestTransmiteEnumValue)
                            {
                                case TransmiteEnum.雷达注册授权:
                                    CommandHandler.RadarRegister(data, item);
                                    break;
                                case TransmiteEnum.设备状态变更通知:
                                    CommandHandler.DeviceStateChange(data, item);
                                    break;
                                case TransmiteEnum.雷达心跳:
                                    CommandHandler.RadarHeat(data, item);
                                    break;
                                case TransmiteEnum.远程设备控制指令响应:
                                    CommandHandler.DeviceControl(data, item);
                                    break;
                                case TransmiteEnum.雷达通知指令:
                                    CommandHandler.RadarNotify(data, item);
                                    break;
                                case TransmiteEnum.远程门店账目查询指令响应:
                                    CommandHandler.StoreQuery(data, item);
                                    break;
                                case TransmiteEnum.游戏机游乐项目属性修改响应:
                                    CommandHandler.ProjectChangeInsNotify(data, item);
                                    break;
                                //case TransmiteEnum.数据变更同步请求:
                                //    CommandHandler.DataChangeInsNotify(data, item);
                                //break;
                                case TransmiteEnum.卡头解绑同步响应:
                                    CommandHandler.CardHeadResetInsNotify(data, item);
                                    break;
                                case TransmiteEnum.门店条码支付请求:
                                    {
                                        string sn = "", secret = "";
                                        CommandHandler.ScanPayRequest(data, item, out sn, out secret);
                                        if (!AskSNList.ContainsKey(sn))
                                        {
                                            AskItem ai = new AskItem();
                                            ai.Secret = secret;
                                            ai.RemotePoint = item.remotePoint;
                                            AskSNList.Add(sn, ai);
                                        }
                                        else
                                        {
                                            AskSNList[sn].RemotePoint = item.remotePoint;
                                            AskSNList[sn].Secret = secret;
                                        }
                                        ScanPayRequestModel scanpay = JsonHelper.DataContractJsonDeserializer<ScanPayRequestModel>(data);
                                        PayOrderHelper payHelper = new PayOrderHelper();
                                        string errMsg = string.Empty;
                                        bool ret = payHelper.BarcodePay(scanpay.OrderID, scanpay.AuthCode, out errMsg);
                                        if(!ret)
                                        {
                                            AskScanPayResult("0", errMsg, "");
                                        }
                                    }
                                    break;
                                //case TransmiteEnum.远程门店账目应答通知指令:
                                //    CommandHandler.StoreQueryNotify(data, item, packId, packNum);
                                //    break;
                                //case TransmiteEnum.远程门店会员卡数据请求响应:
                                //    CommandHandler.MemberQueryNotify(data, item);
                                //    break;
                                //case TransmiteEnum.远程门店门票数据请求响应:
                                //    CommandHandler.TicketQueryNotify(data, item);
                                //    break;
                                //case TransmiteEnum.远程门店门票操作请求响应:
                                //    CommandHandler.TicketOperateNotify(data, item);
                                //    break;
                                //case TransmiteEnum.远程门店彩票数据请求响应:
                                //    CommandHandler.LotteryQueryNotify(data, item);
                                //    break;
                                //case TransmiteEnum.远程门店彩票操作请求响应:
                                //    CommandHandler.LotteryOperateNotify(data, item);
                                //    break;
                                //case TransmiteEnum.远程门店出票条码数据请求响应:
                                //    CommandHandler.OutTicketQueryNotify(data, item);
                                //    break;
                                //case TransmiteEnum.远程门店出票条码操作请求响应:
                                //    CommandHandler.OutTicketOperateNotify(data, item);
                                //    break;
                                //case TransmiteEnum.黄牛卡信息查询请求响应:
                                //    CommandHandler.CattleMemberCardQueryNotify(data, item);
                                //    break;
                                //case TransmiteEnum.远程门店会员转账操作请求响应:
                                //    CommandHandler.MemberTransOperateNotify(data, item);
                                //    break;
                                //case TransmiteEnum.远程门店运行参数数据请求响应:
                                //    CommandHandler.ParamQueryNotify(data, item);
                                //    break;
                                //case TransmiteEnum.远程门店员工手机号校验请求响应:
                                //    CommandHandler.UserPhoneQueryNotify(data, item);
                                //    break;
                                default:
                                    break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        string exMsg = string.Format("{0}{1}{2}{3}", "CommandProcess:", Utils.GetException(ex), "sb参数值:", sb.ToString());
                        LogHelper.SaveLog(TxtLogType.UPDService, TxtLogContentType.Exception, TxtLogFileType.Time, exMsg);
                    }
                }
                else
                {
                    Thread.Sleep(50);
                }
            }
        }


        static void FailResponseProcess()
        {
            if (UDPSocketAnswerBusiness.UDPSocketAnswer.Count > 0)
            {

                var query = from item in UDPSocketAnswerBusiness.UDPSocketAnswer.Cast<DictionaryEntry>()
                            where ((UDPSocketAnswerModel)item.Value).CreateTime < DateTime.Now.AddSeconds(-10)
                            select item.Key.ToString();
                if (query.Count() > 0)
                {
                    foreach (var key in query.ToList<string>())
                    {
                        UDPSocketAnswerModel model = UDPSocketAnswerBusiness.GetAnswerModel(key);
                        XCCloudService.SocketService.UDP.Server.Send(model.IP, model.Port, model.Data);
                    }
                }
                Thread.Sleep(1000);
            }
        }



        static void AnswerStoreQueryProcess()
        {
            if (UDPSocketStoreQueryAnswerBusiness.Answer.Count > 0)
            {

                var query = from item in UDPSocketStoreQueryAnswerBusiness.Answer.Cast<DictionaryEntry>()
                            where ((UDPSocketStoreQueryAnswerModel)item.Value).CreateTime < DateTime.Now.AddSeconds(-100)
                            select item.Key.ToString();
                if (query.Count() > 0)
                {
                    foreach (var key in query.ToList<string>())
                    {
                        UDPSocketStoreQueryAnswerBusiness.Remove(key);
                    }
                }
                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// 获取客户端
        /// </summary>
        /// <param name="radarToken"></param>
        /// <returns></returns>
        public static UDPClientItemBusiness.ClientItem GetClientItem(string radarToken)
        {
            UDPClientItemBusiness.ClientItem clientItem = ClientList.ClientListObj.Where<UDPClientItemBusiness.ClientItem>(p => p.gID.Equals(radarToken)).FirstOrDefault<UDPClientItemBusiness.ClientItem>();
            if (clientItem == null)
            {
                return null;
            }
            else
            {
                return clientItem;
            }
        }

        //static void SendFile(string ip)
        //{
        //    string savePath = SavePath;
        //    string path = string.Empty;
        //    var curfiledown = fileSendBUF.Where(p => p.IP == ip);
        //    if (curfiledown.Count() > 0)
        //    {
        //        TransmiteObject.文件下载结构 file = curfiledown.First();
        //        string fileFullPath = string.Format("{0}\\{1}\\{2}", path, savePath, file.fileName);//合并路径生成文件存放路径
        //        if (File.Exists(fileFullPath))
        //        {
        //            using (FileStream fs = new FileStream(fileFullPath, FileMode.Open, FileAccess.Read))
        //            {
        //                if (file.PacketIndex == 0)
        //                {
        //                    file.PacketCount = (UInt16)(fs.Length / PacketLength);
        //                    file.PacketRemain = (int)(fs.Length % PacketLength);
        //                    if (file.PacketRemain != 0) file.PacketCount++;
        //                }
        //                fs.Position = PacketLength * file.PacketIndex;
        //                byte[] read;
        //                if (file.PacketIndex < file.PacketCount - 1)
        //                {
        //                    read = new byte[PacketLength];
        //                }
        //                else
        //                {
        //                    read = new byte[file.PacketRemain];
        //                }
        //                fs.Read(read, 0, read.Length);
        //                fs.Close();
        //                List<byte> data = new List<byte>();
        //                data.AddRange(BitConverter.GetBytes(file.PacketIndex));
        //                data.AddRange(BitConverter.GetBytes(file.PacketCount));
        //                data.AddRange(read);
        //                byte[] sendData = ServiceObjectConvert.协议编码((byte)TransmiteEnum.下载文件, data.ToArray());
        //                string[] addr = ip.Split(':');
        //                Send(addr[0], Convert.ToInt32(addr[1]), sendData);
        //                file.PacketIndex++;
        //            }
        //        }
        //        else
        //        {
        //            List<byte> data = new List<byte>();
        //            data.AddRange(BitConverter.GetBytes(0));
        //            data.AddRange(BitConverter.GetBytes(0));
        //            byte[] sendData = ServiceObjectConvert.协议编码((byte)TransmiteEnum.下载文件, data.ToArray());
        //            string[] addr = ip.Split(':');
        //            Send(addr[0], Convert.ToInt32(addr[1]), sendData);
        //        }
        //    }
        //}

        static void CheckFile(string ip, int pIndex, int pCount)
        {
            int i = 0;
            bool IsFinish = false;  //是否完成文件接收           
            if (pIndex == pCount)
            {
                IsFinish = true;
                var filedatas = fileRecvBUF.Where(p => p.IP == ip).OrderBy(p => p.PacketIndex);
                foreach (TransmiteObject.文件传输结构 file in filedatas)
                {
                    if (file.PacketIndex != i)
                    {
                        IsFinish = false;
                        break;
                    }
                    i++;
                }
            }
            else
            {
                IsFinish = false;
            }
            if (IsFinish)
            {
                var filedatas = fileRecvBUF.Where(p => p.IP == ip).OrderBy(p => p.PacketIndex);
                string filename = Encoding.GetEncoding("gb2312").GetString(filedatas.First().PacketBytes.ToArray());

                string savePath = SavePath + "\\uploadFiles";

                if (!Directory.Exists(savePath))//存放的默认文件夹是否存在
                {
                    Directory.CreateDirectory(savePath);//不存在则创建
                }
                string fileFullPath = string.Format("{0}\\{1}", savePath, filename);//合并路径生成文件存放路径
                if (File.Exists(fileFullPath)) File.Delete(fileFullPath);
                //创建文件流，读取流中的数据生成文件
                using (FileStream fs = new FileStream(fileFullPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    try
                    {
                        foreach (TransmiteObject.文件传输结构 file in filedatas)
                        {
                            if (file.PacketIndex != 0)
                            {
                                fs.Write(file.PacketBytes.ToArray(), 0, file.PacketBytes.Count);
                            }
                            fileRecvBUF.Remove(file);
                        }
                        fs.Close();
                    }
                    catch (Exception ex)
                    {
                        LogHelper.SaveLog(TxtLogType.UPDService, TxtLogContentType.Exception, TxtLogFileType.Time, "CheckFile:" + ex.Message);
                        return;
                    }
                }
            }
        }
        /// <summary>
        /// 应答门店条码支付结果
        /// </summary>
        /// <param name="result">支付结果 0 支付失败 1 支付成功</param>
        /// <param name="msg">失败时返回错误消息</param>
        /// <param name="sn">应答支付流水号</param>
        public static bool AskScanPayResult(string result, string msg, string sn)
        {
            if (AskSNList.ContainsKey(sn))
            {
                EndPoint p = AskSNList[sn].RemotePoint;
                ScanPayResponseModel response = new ScanPayResponseModel(result, msg, sn);
                response.SignKey = SignKeyHelper.GetSignKey(response, AskSNList[sn].Secret);
                //对象序列化为字节数组
                byte[] dataByteArr = JsonHelper.DataContractJsonSerializerToByteArray(response);
                //生成发送数据包
                byte[] requestPackages = DataFactory.CreateResponseProtocolData(TransmiteEnum.远程门店出票条码数据请求, dataByteArr);
                Send(p, requestPackages);
                AskSNList.Remove(sn);
                return true;
            }
            return false;
        }
    }
}
