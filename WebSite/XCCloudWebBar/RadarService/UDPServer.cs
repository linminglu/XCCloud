using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RadarService
{
    public class UDPServer
    {
        public class StateObject
        {
            public const int BUF_SIZE = 1024 * 128;
            public Socket socket;
            public byte[] buffer = new byte[BUF_SIZE];
        }

        static int 接收字节计数 = 0;
        static int 发送字节计数 = 0;

        const int MaxPacketSize = 200;
        /// <summary>
        /// 帧头 68
        /// </summary>
        public const byte FrameHead = 0x68;
        /// <summary>
        /// 帧尾 16
        /// </summary>
        public const byte FrameButtom = 0x16;
        /// <summary>
        /// 引导区字节 FE
        /// </summary>
        public const byte FrameBlankWord = 0xfe;

        Socket client;
        Thread tRun;
        ManualResetEvent allDone = new ManualResetEvent(false);

        public delegate void 互联网数据接收(byte[] data, EndPoint p);
        public event 互联网数据接收 OnInternetDataRecv;
        void InternetDataRecv(byte[] data, EndPoint p)
        {
            if (OnInternetDataRecv != null)
            {
                OnInternetDataRecv(data, p);
            }
        }

        public delegate void 数据交换速率(int RecvSpeed, int SendSpeed);
        public event 数据交换速率 OnTransferSpeed;
        void TransferSpeed(int RecvSpeed, int SendSpeed)
        {
            if (OnTransferSpeed != null)
            {
                OnTransferSpeed(RecvSpeed, SendSpeed);
            }
        }

        Thread tSpeed = null;

        public bool isRun = false;
        /// <summary>
        /// 启动绑定端口
        /// </summary>
        /// <param name="Port"></param>
        /// <returns></returns>
        public bool Init(int Port)
        {
            try
            {
                if (!isRun)
                {
                    isRun = true;

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
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("初始化失败");
                Console.WriteLine(ex);
                LogHelper.LogHelper.WriteLog(ex);
            }
            return false;
        }
        /// <summary>
        /// 异步接收回调
        /// </summary>
        /// <param name="ar"></param>
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
                LogHelper.LogHelper.WriteLog(oe);
                throw oe;
            }
            catch (SocketException se)
            {
                Console.WriteLine(se);
                LogHelper.LogHelper.WriteLog(se);
                throw se;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                LogHelper.LogHelper.WriteLog(e);
                // 获得接收失败信息 
                throw e;
            }

            if (readBytes > 0)
            {
                接收字节计数 += readBytes;
                byte[] mybytes = new byte[readBytes];
                Array.Copy(so.buffer, mybytes, readBytes);
                EndPoint CurPoint = tempRemoteEP;
                InternetDataRecv(mybytes, CurPoint);
                //收到数据处理
                so.socket.BeginReceiveFrom(so.buffer, 0, StateObject.BUF_SIZE, SocketFlags.None, ref tempRemoteEP, new AsyncCallback(ReceiveCallback), so);
            }
        }

        public void Close()
        {
            isRun = false;
            client.Close();
        }

        public void SendData(byte[] data, EndPoint p)
        {
            try
            {
                Console.WriteLine("发送：" + Coding.ConvertData.BytesToString(data));
                client.SendTo(data, p);
                发送字节计数 += data.Length;
            }
            catch (Exception ex)
            {
                LogHelper.LogHelper.WriteLog(ex);
            }
        }

        void RunSpeed()
        {
            while (true)
            {
                TransferSpeed(接收字节计数, 发送字节计数);
                接收字节计数 = 0;
                发送字节计数 = 0;
                Thread.Sleep(1000);
            }
        }
    }
}
