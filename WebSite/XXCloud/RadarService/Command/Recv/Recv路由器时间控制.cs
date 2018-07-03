using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RadarService.COMObject;

namespace RadarService.Command.Recv
{
    public class Recv路由器时间控制
    {
        public DateTime date;
        public Recv路由器时间控制(FrameData f)
        {
            string dateString = "";
            bool isRight = true;

            if (f.commandLength == 8)
            {
                dateString = string.Format("{0}-{1}-{2} {3}:{4}:{5}", f.commandData[1], f.commandData[2], f.commandData[3], f.commandData[5], f.commandData[6], f.commandData[7]);
                try
                {
                    date = Convert.ToDateTime(dateString);
                    FrmMain.GetInterface.ShowRouteTime(date);
                }
                catch { throw; }
            }
            StringBuilder sb = new StringBuilder();
            sb.Append("=============================================\r\n");
            sb.AppendFormat("{0}  收到数据\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            sb.AppendFormat("{0}\r\n", PubLib.BytesToString(f.recvData));
            sb.AppendFormat("指令类别：{0}\r\n", f.commandType);
            sb.AppendFormat("时间：{0}\r\n", dateString);
            UIClass.接收内容 = sb.ToString();

            if (dateString == "")
                Info.SecrityHeadInfo.错误获取日期次数++;
            if (dateString != "" && Info.SecrityHeadInfo.是否需要授权校验 && Info.SecrityHeadInfo.错误获取日期次数 > 10)
            {
                if (date >= DateTime.Now)
                {
                    if ((date - DateTime.Now).TotalSeconds > 60 * 60 * 24)
                    {
                        //路由器时间与实际时间误差超过1天
                        Info.SecrityHeadInfo.错误获取日期次数++;
                        isRight = false;
                    }
                }
                else
                {
                    if ((DateTime.Now - date).TotalSeconds > 60 * 60 * 24)
                    {
                        //路由器时间与实际时间误差超过1天
                        Info.SecrityHeadInfo.错误获取日期次数++;
                        isRight = false;
                    }
                }
                if (isRight || Info.SecrityHeadInfo.错误获取日期次数 > 10)
                {
                    Info.SecrityHeadInfo.错误获取日期次数 = 0;
                    FrmMain.GetInterface.CheckAuthorDate(date);
                }
            }
            else
            {
                FrmMain.GetInterface.CheckAuthorDate(date);
                Info.SecrityHeadInfo.错误获取日期次数++;
                isRight = false;
            }
        }
    }
}
