using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using System.Net;
using System.Net.Sockets;

namespace RadarService.Info
{
    public class XCUdp
    {
        class StateObject
        {
            public const int BUF_SIZE = 1024 * 128;
            public Socket socket;
            public byte[] buffer = new byte[BUF_SIZE];
        }

        Socket client;
        ManualResetEvent allDone = new ManualResetEvent(false);
        string ServerIP;
        int ServerPort;
        List<byte> recvBUF = new List<byte>();
        Thread tRun = null;

        public delegate void 收到应答(string HeadID, int SN);
        public event 收到应答 OnRecvAnswer;
        public void RecvAnswer(string HeadID, int SN)
        {
            if (OnRecvAnswer != null)
            {
                OnRecvAnswer(HeadID, SN);
            }
        }

        public XCUdp(string IP, int Port)
        {
            ServerIP = IP;
            ServerPort = Port;

            ServerIP = IP;
            ServerPort = Port;
            client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            byte[] optionInValue = { Convert.ToByte(false) };
            byte[] optionOutValue = new byte[4];
            uint IOC_IN = 0x80000000;
            uint IOC_VENDOR = 0x18000000;
            uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
            client.IOControl((int)SIO_UDP_CONNRESET, optionInValue, optionOutValue);
            IPEndPoint serverPoint = new IPEndPoint(IPAddress.Parse(IP), Port);
            client.Connect((EndPoint)serverPoint);

            IPEndPoint sendP = new IPEndPoint(IPAddress.Any, 0);
            EndPoint tempRemoteEP = (EndPoint)sendP;

            StateObject so = new StateObject();
            so.socket = client;
            client.BeginReceiveFrom(so.buffer, 0, StateObject.BUF_SIZE, SocketFlags.None, ref tempRemoteEP, new AsyncCallback(RecvCallBack), so);
        }

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
                try
                {
                    string iccard = Encoding.UTF8.GetString(mybytes, 0, 4);
                    int sn = BitConverter.ToUInt16(mybytes, 6);
                    RecvAnswer(iccard, sn);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                //收到数据处理
                so.socket.BeginReceiveFrom(so.buffer, 0, StateObject.BUF_SIZE, SocketFlags.None, ref tempRemoteEP, new AsyncCallback(RecvCallBack), so);
            }
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

        public void Send(byte[] data)
        {
            IPEndPoint clients = new IPEndPoint(IPAddress.Parse(ServerIP), ServerPort);
            EndPoint epSender = (EndPoint)clients;
            client.BeginSendTo(data, 0, data.Length, SocketFlags.None, epSender, new AsyncCallback(SendCallBack), client);
        }
    }

    /// <summary>
    /// 投币时通知外部服务器
    /// </summary>
    public class BoradPush
    {
        /// <summary>
        /// 是否允许通知
        /// </summary>
        static bool AllowBorad = false;
        static string ServerIP = "";
        static int ServerPort = 0;

        static XCUdp udp;
        static Thread tRun;

        public class BoradItem
        {
            public string ICCard = "";
            public string HeadID = "";
            public int Coins = 0;
            public int SerialNum = 0;
            public DateTime SendTime;
            public int SendCount = 0;
            public bool isFirst = true;
            List<byte> BufData = new List<byte>();
            public List<byte> SendDate
            {
                get
                {
                    if (isFirst)
                    {
                        SendTime = DateTime.Now;
                        isFirst = false;
                        List<byte> l = new List<byte>();
                        l.AddRange(Encoding.GetEncoding("gb2312").GetBytes(ICCard));
                        l.AddRange(Encoding.GetEncoding("gb2312").GetBytes(HeadID));
                        l.Add((byte)Coins);
                        l.AddRange(BitConverter.GetBytes((UInt16)SerialNum));
                        l.Add((byte)(((UInt16)SendTime.Year) / 256));
                        l.Add((byte)(((UInt16)SendTime.Year) % 256));
                        l.Add((byte)SendTime.Month);
                        l.Add((byte)SendTime.Day);
                        l.Add((byte)SendTime.DayOfWeek);
                        l.Add((byte)SendTime.Hour);
                        l.Add((byte)SendTime.Minute);
                        l.Add((byte)SendTime.Second);
                        l.Add(0x0d);
                        l.Add(0x0a);
                        return l;
                    }
                    else
                    {
                        SendCount++;
                        return BufData;
                    }
                }
            }
        }

        static List<BoradItem> list = new List<BoradItem>();

        public static void Init()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("mysql.xml");
            try
            {
                XmlNode node = doc.SelectSingleNode("Connection/ROUTE");
                AllowBorad = node.Attributes["Board"].InnerText == "1";
                ServerIP = node.Attributes["BoradIP"].InnerText;
                ServerPort = Convert.ToInt32(node.Attributes["BoradPort"].InnerText);

                if (udp == null)
                {
                    udp = new XCUdp(ServerIP, ServerPort);
                    udp.OnRecvAnswer += new XCUdp.收到应答(XCUdp_OnRecvAnswer);

                    tRun = new Thread(new ThreadStart(Run));
                    tRun.IsBackground = true;
                    tRun.Name = "对外广播重发线程";
                    tRun.Start();
                }
            }
            catch
            {
                //AllowBorad = false;
                AllowBorad = true;
                ServerIP = "192.168.1.3";
                ServerPort = 12345;
                if (udp == null)
                {
                    udp = new XCUdp(ServerIP, ServerPort);
                    udp.OnRecvAnswer += new XCUdp.收到应答(XCUdp_OnRecvAnswer);

                    tRun = new Thread(new ThreadStart(Run));
                    tRun.IsBackground = true;
                    tRun.Name = "对外广播重发线程";
                    tRun.Start();
                }
            }
        }

        static void XCUdp_OnRecvAnswer(string HeadID, int SN)
        {
            Answer(HeadID, SN);
        }

        static void Answer(string headID, int sn)
        {
            lock (list)
            {
                foreach (BoradItem item in list)
                {
                    if (item.HeadID.ToLower() == headID.ToLower() && item.SerialNum == sn)
                    {
                        list.Remove(item);
                        break;
                    }
                }
            }
        }

        public static void SendBoard(BoradItem item)
        {
            if (!AllowBorad) return;

            var l = list.Where(p => p.HeadID == item.HeadID && p.SerialNum == item.SerialNum);
            if (l.Count() > 0)
                return;

            byte[] data = item.SendDate.ToArray();
            udp.Send(data);
            list.Add(item);
        }

        static void Run()
        {
            while (true)
            {
                lock (list)
                {
                    DateTime d = DateTime.Now;
                    bool isDel = false;
                continueline:
                    foreach (BoradItem item in list)
                    {
                        if (item.SendTime.AddSeconds(2) < d)
                        {
                            if (item.SendCount < 10)
                            {
                                byte[] data = item.SendDate.ToArray();
                                udp.Send(data);
                            }
                            else
                            {
                                list.Remove(item);
                                isDel = true;
                                break;
                            }
                        }
                    }
                    if (isDel)
                    {
                        Thread.Sleep(10);
                        goto continueline;
                    }
                }
                Thread.Sleep(1000);
            }
        }
    }
}
