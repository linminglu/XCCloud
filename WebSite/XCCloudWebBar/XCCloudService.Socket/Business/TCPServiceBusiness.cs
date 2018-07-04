using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using XCCloudWebBar.Business.Common;
using XCCloudWebBar.Common;
using XCCloudWebBar.Common.Enum;
using XCCloudWebBar.Model.CustomModel.Common;
using XCCloudWebBar.SocketService.TCP.Client;
using XCCloudWebBar.SocketService.TCP.Common;

namespace XCCloudWebBar.SocketService.TCP.Business
{
    public class TCPServiceBusiness
    {
        public static XCCloudWebBar.SocketService.TCP.Server Server { set; get; }

        public static void Send(string mobile, object data)
        {
            if (Server != null)
            {
                Socket socket = null;
                string ip = string.Empty;
                LogHelper.SaveLog(TxtLogType.TCPService, TxtLogContentType.Debug, TxtLogFileType.Day, "出币TCPSend:" + mobile + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                if (Server.GetSocket(mobile, ref socket, out ip))
                {
                    string dataJson = Utils.SerializeObject(data).ToString();
                    byte[] msgBuffer = Server.PackageServerData(dataJson);
                    socket.BeginSendTo(msgBuffer, 0, msgBuffer.Length, SocketFlags.None, socket.RemoteEndPoint, new AsyncCallback(Server.SendCallBack), socket);
                    LogHelper.SaveLog(TxtLogType.TCPService, TxtLogContentType.Debug, TxtLogFileType.Day, "出币BeginSendTo:" + mobile + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                }
            }
        }
    }
}
