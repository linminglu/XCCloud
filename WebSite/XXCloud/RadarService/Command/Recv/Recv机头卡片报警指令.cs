using DSS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RadarService.Command.Recv
{
    public class Recv机头卡片报警指令
    {
        public string 机头地址 = "";
        public string IC卡号码 = "";
        public int 卡片类型;
        public int 报警类别;
        public int 处理结果;
        public string AlertType;
        public UInt16 流水号;
        public byte[] SendData = null;
        public FrameData RecvData;
        public Ask.Ask机头卡片报警指令 应答数据;
        public DateTime SendDataTime;

        public Recv机头卡片报警指令(FrameData f, DateTime RecvDateTime)
        {
            RecvData = f;
            机头地址 = Coding.ConvertData.Hex2String(f.commandData[0]);
            if (f.commandLength < 12) return;
            IC卡号码 = Encoding.ASCII.GetString(f.commandData, 1, 8);
            IC卡号码 = IC卡号码.Replace("\0", "");
            int cardid = 0;
            if (!int.TryParse(IC卡号码, out cardid))
            {
                IC卡号码 = "0";
            }
            卡片类型 = f.commandData[9];
            报警类别 = f.commandData[10];
            处理结果 = f.commandData[11];
            AlertType = "";
            switch (报警类别)
            {
                case 0:
                    AlertType = "用户异常退币报警";
                    break;
                case 1:
                    AlertType = "非法专卡专用解锁";
                    break;
                case 2:
                    AlertType = "专卡专用解锁";
                    break;
                case 3:
                    AlertType = "非法送分券解锁";
                    break;
                case 4:
                    AlertType = "常规解锁";
                    break;
                case 5:
                    AlertType = "退币锁定";
                    break;
                default:
                    AlertType = "未知错误报警";
                    break;
            }
            流水号 = BitConverter.ToUInt16(f.commandData, 12);
            string MCUID = Info.DeviceInfo.GetBufMCUIDInfo(f.routeAddress, 机头地址);
            Info.DeviceInfo.机头信息 机头 = Info.DeviceInfo.GetBufMCUIDDeviceInfo(MCUID);
            DataAccess ac = new DataAccess();
            string 管理卡号 = "0";
            if (机头 != null)
            {
                管理卡号 = 机头.管理卡号;
            }
            if (!int.TryParse(管理卡号, out cardid))
            {
                管理卡号 = "0";
            }
            object obj = null;
            TableMemory.Log_GameAlarm log = null;
            TableMemory.DataModel model = new TableMemory.DataModel();
            int res = HostServer.CheckRepeat(f.routeAddress, 机头地址, IC卡号码, CommandType.机头卡片报警指令, ref obj, 流水号);
            if (res == 0)
            {
                switch (报警类别)
                {
                    case 0:
                    case 1:
                        int d = 0;
                        if (!int.TryParse(管理卡号, out d))
                            管理卡号 = "0";
                        if (!int.TryParse(IC卡号码, out d))
                            IC卡号码 = "0";
                        //string sql = string.Format("INSERT INTO flw_game_alarm (ICCardID,Segment,HeadAddress,AlertType,HappenTime,State,LockGame,LockMember,AlertContent) VALUES ('{3}','{0}','{1}','{2}',GETDATE(),0,0,0,'会员卡号：{4}')",
                        //f.routeAddress, 机头地址, AlertType, 管理卡号, IC卡号码);
                        //ac.Execute(sql);

                        log = new TableMemory.Log_GameAlarm();
                        log.AlertContent = AlertType + "  会员卡号：" + IC卡号码;
                        log.AlertType = (int)报警类别;
                        log.DeviceID = 机头.设备编号;
                        log.GameIndex = 机头.游戏机索引号;
                        log.HappenTime = DateTime.Now;
                        log.HeadAddress = 机头.机头短地址;
                        log.ICCardID = 机头.当前卡片号;
                        log.ID = XCCloudSerialNo.SerialNoHelper.CreateStoreSerialNo(PublicHelper.SystemDefiner.StoreID);
                        log.LockGame = 0;
                        log.LockMember = 0;
                        log.MerchID = PublicHelper.SystemDefiner.MerchID;
                        log.Segment = 机头.路由器段号;
                        log.SiteName = 机头.位置名称;
                        log.State = 0;
                        log.StoreID = PublicHelper.SystemDefiner.StoreID;

                        log.Verifiction = model.Verifiction(log);
                        model.Add(log);
                        break;
                    case 2:
                        {
                            if (卡片类型 == 1)  //管理卡
                            {
                                if (机头.状态.超级解锁卡标识)
                                    AlertType = "超级卡解锁专卡专用解锁";
                                log = new TableMemory.Log_GameAlarm();
                                log.AlertContent = AlertType + "  会员卡号：" + IC卡号码;
                                log.AlertType = (int)报警类别;
                                log.DeviceID = 机头.设备编号;
                                log.GameIndex = 机头.游戏机索引号;
                                log.HappenTime = DateTime.Now;
                                log.HeadAddress = 机头.机头短地址;
                                log.ICCardID = 机头.当前卡片号;
                                log.ID = XCCloudSerialNo.SerialNoHelper.CreateStoreSerialNo(PublicHelper.SystemDefiner.StoreID);
                                log.LockGame = 0;
                                log.LockMember = 0;
                                log.MerchID = PublicHelper.SystemDefiner.MerchID;
                                log.Segment = 机头.路由器段号;
                                log.SiteName = 机头.位置名称;
                                log.StoreID = PublicHelper.SystemDefiner.StoreID;

                                if (!机头.状态.超级解锁卡标识 && 处理结果 == 1)    //普通卡，处理结果成功的直接写2
                                { log.State = 2; }
                                else
                                { log.State = 0; }

                                log.Verifiction = model.Verifiction(log);
                                model.Add(log);
                            }
                        }
                        break;
                    case 3:
                        {
                            机头.状态.是否正在使用限时送分优惠 = (处理结果 == 1);
                            机头.是否为首次投币 = (处理结果 == 1);
                            if (处理结果 == 0)
                            {
                                log = new TableMemory.Log_GameAlarm();
                                log.AlertContent = AlertType + "  会员卡号：" + IC卡号码;
                                log.AlertType = (int)报警类别;
                                log.DeviceID = 机头.设备编号;
                                log.GameIndex = 机头.游戏机索引号;
                                log.HappenTime = DateTime.Now;
                                log.HeadAddress = 机头.机头短地址;
                                log.ICCardID = 机头.当前卡片号;
                                log.ID = XCCloudSerialNo.SerialNoHelper.CreateStoreSerialNo(PublicHelper.SystemDefiner.StoreID);
                                log.LockGame = 0;
                                log.LockMember = 0;
                                log.MerchID = PublicHelper.SystemDefiner.MerchID;
                                log.Segment = 机头.路由器段号;
                                log.SiteName = 机头.位置名称;
                                log.State = 0;
                                log.StoreID = PublicHelper.SystemDefiner.StoreID;

                                log.Verifiction = model.Verifiction(log);
                                model.Add(log);
                                //string qsql = string.Format("INSERT INTO flw_game_alarm (ICCardID,Segment,HeadAddress,AlertType,HappenTime,State,LockGame,LockMember,AlertContent) VALUES ('{3}','{0}','{1}','{2}',GETDATE(),0,0,0,'会员卡号：{4}')",
                                //f.routeAddress, 机头地址, AlertType, 管理卡号, IC卡号码);
                                //ac.Execute(qsql);
                            }
                            else
                            {
                                if (机头.状态.超级解锁卡标识)  //超级管理卡
                                {
                                    log = new TableMemory.Log_GameAlarm();
                                    log.AlertContent = "超级卡解锁限时送分  会员卡号：" + IC卡号码;
                                    log.AlertType = (int)报警类别;
                                    log.DeviceID = 机头.设备编号;
                                    log.GameIndex = 机头.游戏机索引号;
                                    log.HappenTime = DateTime.Now;
                                    log.HeadAddress = 机头.机头短地址;
                                    log.ICCardID = 机头.当前卡片号;
                                    log.ID = XCCloudSerialNo.SerialNoHelper.CreateStoreSerialNo(PublicHelper.SystemDefiner.StoreID);
                                    log.LockGame = 0;
                                    log.LockMember = 0;
                                    log.MerchID = PublicHelper.SystemDefiner.MerchID;
                                    log.Segment = 机头.路由器段号;
                                    log.SiteName = 机头.位置名称;
                                    log.State = 0;
                                    log.StoreID = PublicHelper.SystemDefiner.StoreID;

                                    log.Verifiction = model.Verifiction(log);
                                    model.Add(log);
                                    //string dbsql = string.Format("INSERT INTO flw_game_alarm (ICCardID,Segment,HeadAddress,AlertType,HappenTime,State,LockGame,LockMember,AlertContent) VALUES ('{3}','{0}','{1}','{2}',GETDATE(),0,0,0,'会员卡号：{4}')",
                                    //    f.routeAddress, 机头地址, "超级卡解锁限时送分", 管理卡号, IC卡号码);
                                    //ac.Execute(dbsql);
                                }
                                else
                                {
                                    AlertType = "解锁限时送分";
                                    log = new TableMemory.Log_GameAlarm();
                                    log.AlertContent = AlertType + "  会员卡号：" + IC卡号码;
                                    log.AlertType = (int)报警类别;
                                    log.DeviceID = 机头.设备编号;
                                    log.GameIndex = 机头.游戏机索引号;
                                    log.HappenTime = DateTime.Now;
                                    log.HeadAddress = 机头.机头短地址;
                                    log.ICCardID = 机头.当前卡片号;
                                    log.ID = XCCloudSerialNo.SerialNoHelper.CreateStoreSerialNo(PublicHelper.SystemDefiner.StoreID);
                                    log.LockGame = 0;
                                    log.LockMember = 0;
                                    log.MerchID = PublicHelper.SystemDefiner.MerchID;
                                    log.Segment = 机头.路由器段号;
                                    log.SiteName = 机头.位置名称;
                                    log.State = 2;
                                    log.StoreID = PublicHelper.SystemDefiner.StoreID;

                                    log.Verifiction = model.Verifiction(log);
                                    model.Add(log);
                                    //string dbsql = string.Format("INSERT INTO flw_game_alarm (ICCardID,Segment,HeadAddress,AlertType,HappenTime,State,LockGame,LockMember,AlertContent,EndTime) VALUES ('{3}','{0}','{1}','{2}',GETDATE(),2,0,0,'会员卡号：{4}',GETDATE())",
                                    //    f.routeAddress, 机头地址, AlertType, 管理卡号, IC卡号码);
                                    //ac.Execute(dbsql);
                                }
                            }
                        }
                        break;
                    case 4: //常规解锁，解除报警，修改数据库
                        {
                            if (机头.状态.超级解锁卡标识)  //超级管理卡
                            {
                                log = new TableMemory.Log_GameAlarm();
                                log.AlertContent = "超级卡解锁常规锁定  会员卡号：" + IC卡号码;
                                log.AlertType = (int)报警类别;
                                log.DeviceID = 机头.设备编号;
                                log.GameIndex = 机头.游戏机索引号;
                                log.HappenTime = DateTime.Now;
                                log.HeadAddress = 机头.机头短地址;
                                log.ICCardID = 机头.当前卡片号;
                                log.ID = XCCloudSerialNo.SerialNoHelper.CreateStoreSerialNo(PublicHelper.SystemDefiner.StoreID);
                                log.LockGame = 0;
                                log.LockMember = 0;
                                log.MerchID = PublicHelper.SystemDefiner.MerchID;
                                log.Segment = 机头.路由器段号;
                                log.SiteName = 机头.位置名称;
                                log.State = 0;
                                log.StoreID = PublicHelper.SystemDefiner.StoreID;

                                log.Verifiction = model.Verifiction(log);
                                model.Add(log);

                                //string dbsql = string.Format("INSERT INTO flw_game_alarm (ICCardID,Segment,HeadAddress,AlertType,HappenTime,State,LockGame,LockMember,AlertContent) VALUES ('{3}','{0}','{1}','{2}',GETDATE(),0,0,0,'会员卡号：{4}')",
                                //    f.routeAddress, 机头地址, "超级卡解锁常规锁定", 管理卡号, IC卡号码);
                                //ac.Execute(dbsql);
                            }
                            //string usql = string.Format("update flw_game_alarm set EndTime=GETDATE(),ICCardID='{2}',`state`=1 where `state`=0 and LockGame=1 and Segment='{0}' and HeadAddress='{1}'", f.routeAddress, 机头地址, 管理卡号);
                            //ac.Execute(usql);
                        }
                        break;
                    case 5:
                        {
                            if (机头.状态.超级解锁卡标识)  //超级管理卡
                            {
                                log = new TableMemory.Log_GameAlarm();
                                log.AlertContent = "超级卡解锁退币锁定  会员卡号：" + IC卡号码;
                                log.AlertType = (int)报警类别;
                                log.DeviceID = 机头.设备编号;
                                log.GameIndex = 机头.游戏机索引号;
                                log.HappenTime = DateTime.Now;
                                log.HeadAddress = 机头.机头短地址;
                                log.ICCardID = 机头.当前卡片号;
                                log.ID = XCCloudSerialNo.SerialNoHelper.CreateStoreSerialNo(PublicHelper.SystemDefiner.StoreID);
                                log.LockGame = 0;
                                log.LockMember = 0;
                                log.MerchID = PublicHelper.SystemDefiner.MerchID;
                                log.Segment = 机头.路由器段号;
                                log.SiteName = 机头.位置名称;
                                log.State = 0;
                                log.StoreID = PublicHelper.SystemDefiner.StoreID;

                                log.Verifiction = model.Verifiction(log);
                                model.Add(log);
                                //string dbsql = string.Format("INSERT INTO flw_game_alarm (ICCardID,Segment,HeadAddress,AlertType,HappenTime,State,LockGame,LockMember,AlertContent) VALUES ('{3}','{0}','{1}','{2}',GETDATE(),0,0,0,'会员卡号：{4}')",
                                //    f.routeAddress, 机头地址, "超级卡解锁退币锁定", 管理卡号, IC卡号码);
                                //ac.Execute(dbsql);
                            }
                        }
                        break;
                    default:
                        break;
                }

                if (处理结果 == 1)
                    机头.当前卡片号 = "";

                应答数据 = new Ask.Ask机头卡片报警指令(机头地址, 流水号);
                HostServer.InsertRepeat(f.routeAddress, 机头地址, IC卡号码, CommandType.机头卡片报警指令, CommandType.机头卡片报警指令应答, 应答数据, 流水号, RecvDateTime);
            }
            else if (res == 1)
            {
                应答数据 = (Ask.Ask机头卡片报警指令)obj;
            }
            else
            {
                //重复性检查出错
                return;
            }
            byte[] dataBuf = Coding.ConvertData.GetBytesByObject(应答数据);
            SendData = Coding.ConvertData.GetFrameDataBytes(f, dataBuf, CommandType.机头卡片报警指令应答);

            SendDataTime = DateTime.Now;
        }

        public string GetRecvData(DateTime printDate)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("=============================================\r\n");
            sb.AppendFormat("{0:yyyy-MM-dd HH:mm:ss.fff}  收到数据\r\n", DateTime.Now);
            sb.AppendFormat("{0}\r\n", Coding.ConvertData.BytesToString(RecvData.recvData));
            sb.AppendFormat("指令类别：{0}\r\n", RecvData.commandType);
            sb.AppendFormat("IC卡号码：{0}\r\n", IC卡号码);
            sb.AppendFormat("卡片类型：{0}\r\n", 卡片类型);
            sb.AppendFormat("报警类别：{0}\r\n", AlertType);
            sb.AppendFormat("处理结果：{0}\r\n", 处理结果 == 1 ? "是" : "否");
            sb.AppendFormat("流水号：{0}\r\n", 流水号);
            return sb.ToString();
        }

        public string GetSendData()
        {
            StringBuilder sb = new StringBuilder();
            if (SendData != null)
            {
                sb.Append("=============================================\r\n");
                sb.AppendFormat("{0:yyyy-MM-dd HH:mm:ss.fff}  发送数据\r\n", SendDataTime);
                sb.Append(Coding.ConvertData.BytesToString(SendData) + Environment.NewLine);
                sb.AppendFormat("指令类别：{0}\r\n", CommandType.机头卡片报警指令应答);
                sb.AppendFormat("机头地址：{0}\r\n", 机头地址);
                sb.AppendFormat("流水号：{0}\r\n", 流水号);
            }
            return sb.ToString();
        }
    }
}
