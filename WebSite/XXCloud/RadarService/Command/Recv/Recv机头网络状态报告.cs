using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Remoting.Messaging;
using System.Data;
using RadarService.Notify;

namespace RadarService.Command.Recv
{
    public class Recv机头网络状态报告
    {
        public FrameData RecvData;
        public byte[] SendData;
        public DateTime SendDate;
        public int 登记用户数 = 0;
        public string 设备序列号 = "";

        public void Run(FrameData data)
        {
            List<JsonObject.StatusItem> changeList = new List<JsonObject.StatusItem>();
            try
            {
                设备序列号 = "";
                登记用户数 = data.commandData[0];
                for (int i = 1; i < data.commandLength; i++)
                {
                    bool changed = false;   //状态是否改变
                    bool isAlert = false;
                    bool isLock = false;
                    int alertCount = 0;
                    bool DeviceState = false;
                    string 机头地址 = Coding.ConvertData.Hex2String(data.commandData[i]);
                    string MCUID = XCCouldSerialNo.SerialNoHelper.StringGet("info_" + data.routeAddress + "|" + 机头地址);
                    if (MCUID == null) continue;
                    Info.DeviceInfo.机头信息 机头 = Info.DeviceInfo.GetBufMCUIDDeviceInfo(MCUID);// XCCouldSerialNo.SerialNoHelper.StringGet<Info.DeviceInfo.机头信息>("headinfo_" + MCUID);
                    if (机头 != null)
                    {
                        机头.是否从雷达获取到状态 = true;
                        i++;
                        if (i < data.commandLength)
                        {
                            string status = Coding.ConvertData.Hex2BitString(data.commandData[i]);
                            bool OnlineFlag = (status.Substring(7, 1) == "1");
                            changed = (机头.状态.在线状态 != OnlineFlag);
                            //Console.WriteLine("在线状态变更：" + changed.ToString() + "  状态：" + OnlineFlag + "  状态.在线状态=" + 状态.在线状态 + "  机头地址=" + 机头地址);
                            if (OnlineFlag)
                            {
                                机头.不在线检测计数 = 0;
                                机头.状态.在线状态 = true;
                            }
                            else
                            {
                                机头.不在线检测计数++;
                                if (机头.不在线检测计数 > 2)
                                {
                                    机头.状态.在线状态 = false;
                                }
                            }
                            if (机头.状态.在线状态)
                            {
                                DeviceState = (status.Substring(6, 1) == "1");
                                if (机头.状态.打印机故障 != DeviceState)
                                {
                                    机头.状态.打印机故障 = DeviceState;
                                    UpdateAlertDB(机头, 机头.状态.打印机故障, HeadAlertType.打印机故障, 0);
                                    alertCount++;
                                    isAlert = true;
                                }
                                DeviceState = (status.Substring(5, 1) == "1");
                                if (机头.状态.打印设置错误 != DeviceState)
                                {
                                    机头.状态.打印设置错误 = DeviceState;
                                    UpdateAlertDB(机头, 机头.状态.打印设置错误, HeadAlertType.打印设置故障, 0);
                                    alertCount++;
                                    isAlert = true;
                                }
                                DeviceState = (status.Substring(3, 1) == "1");
                                if (机头.状态.读币器故障 != DeviceState)
                                {
                                    机头.状态.读币器故障 = DeviceState;
                                    UpdateAlertDB(机头, 机头.状态.读币器故障, HeadAlertType.卡头读卡故障, 0);
                                    alertCount++;
                                    isAlert = true;
                                }
                                机头.状态.锁定机头 = (status.Substring(0, 1) == "1");
                                isLock = 机头.状态.锁定机头;
                                机头.状态.锁定机头 = 机头.状态.锁定机头;
                            }
                        }
                        i++;
                        if (i < data.commandLength)
                        {
                            if (机头.状态.在线状态)
                            {
                                string status = Coding.ConvertData.Hex2BitString(data.commandData[i]);
                                DeviceState = (status.Substring(7, 1) == "1");
                                if (机头.状态.高频干扰报警 != DeviceState)
                                {
                                    机头.状态.高频干扰报警 = DeviceState;
                                    if (机头.状态.高频干扰报警)
                                    {
                                        UpdateAlertDB(机头, 机头.状态.高频干扰报警, HeadAlertType.高频干扰报警, 1);
                                        alertCount++;
                                    }
                                    isAlert = true;
                                    isLock = true;
                                }
                                DeviceState = (status.Substring(6, 1) == "1");
                                if (机头.状态.高压干扰报警 != DeviceState)
                                {
                                    机头.状态.高压干扰报警 = DeviceState;
                                    if (机头.状态.高压干扰报警 && alertCount < 4)
                                    {
                                        UpdateAlertDB(机头, 机头.状态.高压干扰报警, HeadAlertType.高压干扰报警, 1);
                                        alertCount++;
                                    }
                                    isAlert = true;
                                    isLock = true;
                                }
                                DeviceState = (status.Substring(5, 1) == "1");
                                if (机头.状态.SSR信号异常 != DeviceState)
                                {
                                    机头.状态.SSR信号异常 = DeviceState;
                                    if (机头.状态.SSR信号异常 && alertCount < 4)
                                    {
                                        UpdateAlertDB(机头, 机头.状态.SSR信号异常, HeadAlertType.SSR信号异常, 1);
                                        alertCount++;
                                    }
                                    isAlert = true;
                                    isLock = true;
                                }
                                DeviceState = (status.Substring(4, 1) == "1");
                                if (机头.状态.CO信号异常 != DeviceState)
                                {
                                    机头.状态.CO信号异常 = DeviceState;
                                    if (机头.状态.CO信号异常 && alertCount < 4)
                                    {
                                        UpdateAlertDB(机头, 机头.状态.CO信号异常, HeadAlertType.CO信号异常, 1);
                                        alertCount++;
                                    }
                                    isAlert = true;
                                    isLock = true;
                                }
                                DeviceState = (status.Substring(3, 1) == "1");
                                if (机头.状态.CO2信号异常 != DeviceState)
                                {
                                    机头.状态.CO2信号异常 = DeviceState;
                                    if (机头.状态.CO2信号异常 && alertCount < 4)
                                    {
                                        UpdateAlertDB(机头, 机头.状态.CO2信号异常, HeadAlertType.CO2信号异常, 1);
                                        alertCount++;
                                    }
                                    isAlert = true;
                                    isLock = true;
                                }
                                if (机头.类型 == Info.DeviceInfo.设备类型.存币机)
                                {
                                    DeviceState = (status.Substring(2, 1) == "1");
                                    if (机头.状态.存币箱满或限时优惠锁定 != DeviceState)
                                    {
                                        机头.状态.存币箱满或限时优惠锁定 = DeviceState;
                                        if (alertCount < 4)
                                        {
                                            UpdateAlertDB(机头, 机头.状态.存币箱满或限时优惠锁定, HeadAlertType.存币箱满报警, 0);
                                            alertCount++;
                                        }
                                        isAlert = true;
                                    }
                                }
                                else
                                {
                                    机头.状态.是否正在使用限时送分优惠 = (status.Substring(2, 1) == "1");
                                }
                                机头.状态.锁定机头 = isLock;
                            }
                        }

                        //if (!changed && alertCount > 0)
                        changed = true;

                        JsonObject.StatusItem item = new JsonObject.StatusItem();
                        item.mcuid = 机头.设备序列号;
                        if (机头.状态.在线状态)
                        {
                            if (isAlert)
                                item.status = "故障";
                            else
                            {
                                if (机头.状态.锁定机头)
                                    item.status = "锁定";
                                else
                                {
                                    if (机头.状态.出币机或存币机正在数币)
                                        item.status = "出币中";
                                    else
                                        item.status = "在线";
                                }
                            }
                        }
                        else
                        {
                            item.status = "离线";
                        }
                        changeList.Add(item);

                        Info.DeviceInfo.SetBufMCUIDDeviceInfo(MCUID, 机头);
                    }
                }
                HostServer.ChangeDeviceStatus(changeList);
            }
            catch
            {
                throw;
            }
        }

        void UpdateAlertDB(Info.DeviceInfo.机头信息 head, bool AlertValue, HeadAlertType AlertType, int lockGame)
        {
            if (AlertValue)
            {
                DataAccess ac = new DataAccess();
                DataTable dt = ac.ExecuteQueryReturnTable("select * from Log_GameAlarm where DeviceID='" + head.设备编号 + "' and AlertType='" + (int)AlertType + "' and State=0");
                if (dt.Rows.Count == 0)
                {
                    TableMemory.Log_GameAlarm log = new TableMemory.Log_GameAlarm();
                    log.AlertContent = AlertType.ToString();
                    log.AlertType = (int)AlertType;
                    log.DeviceID = head.设备编号;
                    log.GameIndex = head.游戏机索引号;
                    log.HappenTime = DateTime.Now;
                    log.HeadAddress = head.机头短地址;
                    log.ICCardID = head.当前卡片号;
                    log.ID = XCCouldSerialNo.SerialNoHelper.CreateStoreSerialNo(PublicHelper.SystemDefiner.StoreID);
                    log.LockGame = lockGame;
                    log.LockMember = 0;
                    log.MerchID = PublicHelper.SystemDefiner.MerchID;
                    log.Segment = head.路由器段号;
                    log.SiteName = head.位置名称;
                    log.State = 0;
                    log.StoreID = PublicHelper.SystemDefiner.StoreID;

                    TableMemory.DataModel model = new TableMemory.DataModel();
                    log.Verifiction = model.Verifiction(log);
                    model.Add(log);
                }
            }
            else
            {
                TableMemory.Log_GameAlarm log = new TableMemory.Log_GameAlarm();
                log.EndTime = DateTime.Now;
                log.State = 1;
                TableMemory.DataModel model = new TableMemory.DataModel();
                model.Update(log, string.Format("where Segment='{0}' and HeadAddress='{1}' and AlertType='{2}' and State=0", head.路由器段号, head.机头短地址, AlertType));
            }
        }
        public Recv机头网络状态报告(FrameData data)
        {
            RecvData = data;
            Run(data);
            SendData = Coding.ConvertData.GetFrameDataBytes(data, null, CommandType.机头网络状态报告应答);
        }
    }
}
