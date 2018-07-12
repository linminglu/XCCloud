
using PalletService.Business.SysConfig;
using PalletService.Common;
using PalletService.SocketService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PalletService.Init
{
    public class ApplicationInit
    {
        public static void Init()
        {
            try
            {
                TCPInit();
                string appStartPath = System.Windows.Forms.Application.StartupPath;
                SysConfigBusiness.Init(appStartPath);
                LogHelper.SaveLog(TxtLogType.SystemInit, "ApplicationInit.Init()");
            }
            catch(Exception e)
            {
                LogHelper.SaveLog(TxtLogType.SystemInit, e.Message);
            }
        }

        private static void TCPInit()
        {
            try
            {
                Server socketServer = new Server();
                socketServer.Start(SysConfigBusiness.TCPPort);
                LogHelper.SaveLog(TxtLogType.SystemInit, "TCP Init Sucess");
            }
            catch (Exception ex)
            {
                LogHelper.SaveLog(TxtLogType.SystemInit, "TCP Init Fail..." + Utils.GetException(ex));
            }
        }
    }
}
