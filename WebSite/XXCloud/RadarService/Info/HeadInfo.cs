using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;

namespace RadarService.Info
{
    public static class HeadInfo
    {
        public class SaveHeadItem
        {
            public string HeadID = "";
            public string CardID = "";
        }

        static Queue<SaveHeadItem> SaveHeadList = new Queue<SaveHeadItem>();

        public enum 设备类型
        {
            游戏机,
            售币机,
            存币机,
            提币机,
            碎票机,
            投币机,
            远程存币机,
        }

        public enum 终端类型
        {
            售币机 = 0x01,
            存币机 = 0x02,
            提币机 = 0x03,
            碎票机 = 0x04,
            投币机 = 0x05,
        }

        public class 存售提币机碎票机参数
        {
            public bool 马达1启用标识;
            public bool 马达2启用标识;
            public int 数码管类型;
            public byte 设备数币计数;
            public byte 卡上增加币数;
            public int 最小数币数;
            public int 存币箱最大存币报警阀值;
            public bool 是否允许打印;
        }

        public class 机头常规属性
        {
            public int 机头索引号 = 0;
            public string 机头编号 = "";
            public string 游戏机编号 = "";
            public string 游戏机名 = "";
            public string 路由器编号 = "";
            public string 机头地址 = "";
            public string 机头长地址 = "";
            public string 当前卡片号 = "";
            public string 管理卡号 = "";
            public string 设备名称 = "";
            public int 当前会员卡级别 = 0;
            public bool 是否为首次投币 = false;
            public int 退币信号超时时间 = 0;
            public int 退币信号超时退币个数 = 0;
            public bool 退币保护启用标志 = false;
        }

        public class 机头干扰开关
        {
            public bool 长地址登记标示 { get; set; }
            public bool bit6 { get; set; }
            public bool 存币箱满或限时优惠锁定 { get; set; }
            public bool CO2信号异常 { get; set; }
            public bool CO信号异常 { get; set; }
            public bool SSR信号异常 { get; set; }
            public bool 高压干扰报警 { get; set; }
            public bool 高频干扰报警 { get; set; }
        }

        public class 投币信息
        {
            public int 投币数 = 0;
            public int 退币数 = 0;
            public int 盈利数 { get { return 投币数 - 退币数; } }
            public int 单次退币上限 = 0;
            public int 每天净退币上限 = 0;
            public int 最小退币数 = 0;
        }
        /// <summary>
        /// 由路由器报告取值
        /// </summary>
        public class 状态值
        {
            public bool 锁定机头 = false;
            public bool 允许打票 = false;
            public bool 机头能上分 = false;
            public bool 读币器故障 = false;
            public bool 非法币或卡报警 = false;
            public bool 打印设置错误 = false;
            public bool 打印机故障 = false;
            public bool 在线状态 = false;
            public bool 是否正在使用限时送分优惠 = false;
            public bool 存币箱是否满 = false;
            public bool 是否忽略超分报警 = false;
            public bool 超级解锁卡标识 = false;
            public bool 出币机或存币机正在数币 = false;
        }
        public class 参数属性
        {
            public int 单次退币限额 = 0;
            public int 投币时扣卡上币数 = 0;
            public int 投币时给游戏机信号数 = 0;
            public int 退币时给游戏机脉冲数比例因子 = 0;
            public int 退币时卡上增加币数比例因子 = 0;
            public int 退币按钮脉宽 = 0;
            public int 异常SSR退币检测次数 = 0;
            public int 异常SSR退币检测速度 = 0;
            public int 首次投币启动间隔 = 0;
            public int 退币速度 = 0;
            public int 退币脉宽 = 0;
            public int 投币速度 = 0;
            public int 投币脉宽 = 0;
            public int 第二路上分线投币时扣卡上币数 = 0;
            public int 第二路上分线投币时给游戏机信号数 = 0;
            public int 第二路上分线上分速度 = 0;
            public int 第二路上分线上分脉宽 = 0;
            public int 第二路上分线首次上分启动间隔 = 0;
            public int 打印灰度 = 0;
        }
        public class 开关信号
        {
            public bool 硬件投币控制 = false;
            public bool 异常SSR退币检测或异常打票控制 = false;
            public bool 保留字 = false;
            public bool 退币超限标志 = false;
            public bool 允许实物退币 = false;
            public bool 允许电子投币 = false;
            public bool 允许十倍投币 = false;
            public bool 允许电子退币或允许打票 = false;
            public bool 第二路上分线上分电平 = false;
            public bool 启用第二路上分信号 = false;
            public bool 转发实物投币 = false;
            public bool SSR退币驱动脉冲电平 = false;
            public bool 数币脉冲电平 = false;
            public bool 投币脉冲电平 = false;
            public bool 增强防止转卡 = false;
            public bool 启用专卡专用 = false;
            public bool 启用异常退币检测 = false;
            public bool 是否启用最小退币功能 = false;
            public bool BO按钮是否维持 = false;
            public bool 退分锁定标志 = false;
            public bool 启用即中即退模式 = false;
            public bool 启用防霸位功能 = false;
            public bool 是否启用卡片专卡专用 = false;
            public bool 启用外部报警检测 = false;
            public bool 启用回路报警检测 = false;
            public bool 启动刷卡即扣 = false;
            public bool 启用刷卡版彩票 = false;
            public bool 只退实物彩票 = false;
            public bool 小票是否打印二维码 = false;
        }
        public class 机头信息
        {
            public 机头常规属性 常规 = new 机头常规属性();
            public 投币信息 投币 = new 投币信息();
            public 状态值 状态 = new 状态值();
            public 参数属性 参数 = new 参数属性();
            public 开关信号 开关 = new 开关信号();
            public 机头干扰开关 报警 = new 机头干扰开关();
            public 设备类型 类型;
            public bool 是否从雷达获取到状态 = false;
            public 存售提币机碎票机参数 存币机 = new 存售提币机碎票机参数();
            public int 临时错误计数;
            public int 不在线检测计数;
            public bool 彩票模式 = false;
            public int 打印小票有效天数 = 0;
            public string 订单编号 = "";
            public bool 位置有效 = false;
            int coinSN = 0;
            public int 远程投币上分流水号
            {
                get
                {
                    coinSN++;
                    if (coinSN == 65535)
                        coinSN = 1;
                    else if (coinSN == 0)
                        coinSN++;
                    return coinSN;
                }
            }
        }

        static List<机头信息> 机头列表 = new List<机头信息>();

        static string[] HeadStatusBuf = new string[255];
        public static char[] SaveDat = new char[255 * 8];
        public static bool SaveFlag = false;

        public static void InitStatusBufFile()
        {
            if (!File.Exists("BUF.dat"))
            {
                StreamWriter sw = new StreamWriter("BUF.dat");
                sw.Write(SaveDat);
                sw.Flush();
                sw.Close();
            }

            StreamReader sr = new StreamReader("BUF.dat");
            sr.Read(SaveDat, 0, SaveDat.Length);

            for (int i = 0; i < 255; i++)
            {
                char[] tmp = new char[8];
                Array.Copy(SaveDat, i * 8, tmp, 0, 8);
                string c = new string(tmp);
                c = c.Replace("\0", "");
                HeadStatusBuf[i] = c;
            }
            sr.Close();

            SaveFlag = true;
        }

        /// <summary>
        /// 将卡号写入缓存文件
        /// </summary>
        /// <param name="hAddress">机头地址</param>
        /// <param name="ICCardID">卡号</param>
        public static void WriteBufHead(string hAddress, string ICCardID)
        {
            lock (SaveDat)
            {
                int i = Convert.ToInt32(hAddress, 16);
                char[] tmp = new char[8];
                if (ICCardID != "")
                {
                    tmp = ICCardID.ToCharArray();
                }
                Array.Copy(tmp, 0, SaveDat, i * 8, 8);
                HeadStatusBuf[i] = ICCardID;
            }
        }

        public static string ReadCard(string hAddress)
        {
            int i = Convert.ToInt32(hAddress, 16);
            return HeadStatusBuf[i];
        }

        /// <summary>
        /// 初始化机头列表
        /// </summary>
        public static void Init()
        {
            try
            {
                int i = 0;

                string sqlAll = "";
                DataTable dtc = DataAccess.ExecuteQueryReturnTable("SELECT COUNT(ID) FROM flw_checkdate;");
                if (dtc.Rows[0][0].ToString() == "0")
                {
                    sqlAll += string.Format("select SUM(Coins) as Coins,Segment,HeadAddress from flw_485_coin where Segment='{0}' and CoinType in (0,1,4) GROUP BY Segment,HeadAddress;", PubLib.路由器段号);
                    sqlAll += string.Format("select SUM(Coins) as Coins,Segment,HeadAddress from flw_485_coin where Segment='{0}' and CoinType in (2,3) GROUP BY Segment,HeadAddress;", PubLib.路由器段号);
                    sqlAll += string.Format("SELECT SUM(Coins) as Coins,Segment,HeadAddress from flw_ticket_exit where Segment='{0}' GROUP BY Segment,HeadAddress;", PubLib.路由器段号);
                }
                else
                {
                    sqlAll += string.Format("select SUM(Coins) as Coins,Segment,HeadAddress from flw_485_coin where Segment='{0}' and CoinType in (0,1,4) and RealTime>(select max(ShiftTime) as endtime from flw_schedule) GROUP BY Segment,HeadAddress;", PubLib.路由器段号);
                    sqlAll += string.Format("select SUM(Coins) as Coins,Segment,HeadAddress from flw_485_coin where Segment='{0}' and CoinType in (2,3) and RealTime>(select max(ShiftTime) as endtime from flw_schedule) GROUP BY Segment,HeadAddress;", PubLib.路由器段号);
                    sqlAll += string.Format("SELECT SUM(Coins) as Coins,Segment,HeadAddress from flw_ticket_exit where Segment='{0}' and RealTime>(select max(ShiftTime) as endtime from flw_schedule) GROUP BY Segment,HeadAddress;", PubLib.路由器段号);
                }

                机头列表 = new List<机头信息>();
                DataTable dt = DataAccess.ExecuteQuery(string.Format("select *,t_head.state as hstate,t_game.state as gstate from t_head,t_game where t_head.gameid=t_game.gameid and Segment='{0}'", PubLib.路由器段号)).Tables[0];
                DataSet ds = DataAccess.ExecuteQuery(sqlAll);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        i++;
                        Console.WriteLine(row["HeadAddress"].ToString().ToUpper());
                        机头信息 机头 = new 机头信息();
                        机头.位置有效 = (row["hstate"].ToString() == "1" && row["gstate"].ToString() == "1");
                        机头.彩票模式 = (row["LotteryMode"].ToString() == "1");
                        机头.常规.机头编号 = row["HeadID"].ToString().ToUpper();
                        机头.常规.机头地址 = row["HeadAddress"].ToString().ToUpper();
                        机头.常规.路由器编号 = row["Segment"].ToString().ToUpper();
                        机头.常规.游戏机编号 = row["GameID"].ToString().ToUpper();
                        机头.常规.机头长地址 = row["MCUID"].ToString().ToUpper();
                        机头.常规.游戏机名 = row["GameName"].ToString().ToUpper();
                        机头.常规.退币信号超时时间 = Convert.ToInt32(row["SSRTimeOut"].ToString());

                        机头.参数.投币时给游戏机信号数 = Convert.ToUInt16(row["PushAddToGame"]);
                        机头.参数.退币时给游戏机脉冲数比例因子 = Convert.ToByte(row["OutReduceFromGame"]);
                        机头.参数.退币时卡上增加币数比例因子 = Convert.ToByte(row["OutAddToCard"]);
                        机头.参数.退币按钮脉宽 = Convert.ToUInt16(row["BOPulse"]);

                        机头.参数.异常SSR退币检测次数 = Convert.ToByte(row["Frequency"]);
                        机头.参数.异常SSR退币检测速度 = Convert.ToByte(row["ExceptOutSpeed"]);
                        机头.参数.首次投币启动间隔 = Convert.ToByte(row["PushStartInterval"]);
                        机头.参数.退币速度 = Convert.ToUInt16(row["OutSpeed"]);
                        机头.参数.退币脉宽 = Convert.ToByte(row["OutPulse"]);
                        机头.参数.投币速度 = Convert.ToUInt16(row["PushSpeed"]);
                        机头.参数.投币脉宽 = Convert.ToByte(row["PushPulse"]);
                        机头.参数.打印灰度 = Convert.ToByte(row["PrinterGray"]);

                        机头.参数.第二路上分线首次上分启动间隔 = Convert.ToByte(row["SecondStartInterval"]);
                        机头.参数.第二路上分线上分速度 = Convert.ToUInt16(row["SecondSpeed"]);
                        机头.参数.第二路上分线上分脉宽 = Convert.ToByte(row["SecondPulse"]);
                        机头.参数.第二路上分线投币时扣卡上币数 = Convert.ToInt32(row["SecondReduceFromCard"]);
                        机头.参数.第二路上分线投币时给游戏机信号数 = Convert.ToInt32(row["SecondAddToGame"]);

                        机头.开关.允许电子退币或允许打票 = Convert.ToBoolean(row["AllowElecOut"]);
                        机头.开关.允许电子投币 = Convert.ToBoolean(row["AllowElecPush"]);
                        机头.开关.允许十倍投币 = Convert.ToBoolean(row["AllowDecuplePush"]);
                        机头.开关.允许实物退币 = Convert.ToBoolean(row["AllowRealOut"]);
                        机头.开关.转发实物投币 = Convert.ToBoolean(row["AllowRealPush"]);
                        机头.开关.硬件投币控制 = Convert.ToBoolean(row["PushControl"]);
                        机头.开关.投币脉冲电平 = Convert.ToBoolean(row["PushLevel"]);
                        机头.开关.第二路上分线上分电平 = Convert.ToBoolean(row["SecondLevel"]);
                        机头.开关.SSR退币驱动脉冲电平 = Convert.ToBoolean(row["OutLevel"]);
                        机头.开关.数币脉冲电平 = Convert.ToBoolean(row["CountLevel"]);
                        机头.开关.启用第二路上分信号 = Convert.ToBoolean(row["UseSecondPush"]);

                        //机头.开关.退币超限标志 = Convert.ToBoolean(row["ExitMaxCheck"]);
                        机头.开关.启用专卡专用 = Convert.ToBoolean(row["GuardConvertCard"]);
                        机头.开关.增强防止转卡 = Convert.ToBoolean(row["StrongGuardConvertCard"]);
                        机头.开关.BO按钮是否维持 = Convert.ToBoolean(row["BOKeep"]);
                        机头.开关.退分锁定标志 = Convert.ToBoolean(row["BOLock"]);
                        机头.开关.启用异常退币检测 = Convert.ToBoolean(row["ExceptOutTest"]);
                        机头.开关.启用即中即退模式 = Convert.ToBoolean(row["Now_Exit"]);
                        机头.开关.启用防霸位功能 = Convert.ToBoolean(row["BanOccupy"]);
                        机头.开关.启用刷卡版彩票 = Convert.ToBoolean(row["ICTicketOperation"]);
                        机头.开关.只退实物彩票 = Convert.ToBoolean(row["OnlyExitLottery"]);
                        机头.开关.启动刷卡即扣 = Convert.ToBoolean(row["ReadCat"]);

                        机头.投币.单次退币上限 = Convert.ToInt32(row["OnceOutLimit"]);
                        机头.投币.每天净退币上限 = Convert.ToInt32(row["OneDayPureOutLimit"]);
                        //int inCoin = 0, outCoin = 0;
                        var r = ds.Tables[0].Select("HeadAddress='" + 机头.常规.机头地址 + "' and Segment='" + 机头.常规.路由器编号 + "'");
                        if (r.Count() > 0)
                        {
                            机头.投币.投币数 = Convert.ToInt32(r[0][0].ToString());
                        }
                        else
                        {
                            机头.投币.投币数 = 0;
                        }
                        r = ds.Tables[1].Select("HeadAddress='" + 机头.常规.机头地址 + "' and Segment='" + 机头.常规.路由器编号 + "'");
                        if (r.Count() > 0)
                        {
                            机头.投币.退币数 = Convert.ToInt32(r[0][0].ToString());
                        }
                        else
                        {
                            机头.投币.退币数 = 0;
                        }
                        r = ds.Tables[2].Select("HeadAddress='" + 机头.常规.机头地址 + "' and Segment='" + 机头.常规.路由器编号 + "'");
                        if (r.Count() > 0)
                        {
                            机头.投币.退币数 += Convert.ToInt32(r[0][0].ToString());
                        }
                        //GetCoins(机头.常规.路由器编号, 机头.常规.机头地址, out inCoin, out outCoin);
                        //机头.投币.投币数 = inCoin;
                        //机头.投币.退币数 = outCoin;
                        if (机头.投币.盈利数 > 机头.投币.每天净退币上限)
                        {
                            机头.开关.退币超限标志 = true;
                        }

                        机头.状态.是否忽略超分报警 = false;
                        机头.状态.锁定机头 = (row["State"].ToString() != "1");

                        机头.状态.机头能上分 = true;
                        机头.状态.允许打票 = true;

                        机头.常规.当前卡片号 = ReadCard(机头.常规.机头地址);
                        机头.常规.是否为首次投币 = (机头.常规.当前卡片号 == "");

                        机头.常规.退币信号超时退币个数 = 机头.常规.退币信号超时时间 / (机头.参数.退币脉宽 + 机头.参数.退币速度);
                        if (机头.常规.退币信号超时时间 % (机头.参数.退币脉宽 + 机头.参数.退币速度) != 0)
                            机头.常规.退币信号超时退币个数++;
                        DataTable dtstate = DataAccess.ExecuteQuery(string.Format("select * from t_head_state where headaddress='{0}' and segment='{1}'", row["HeadAddress"], row["Segment"])).Tables[0];
                        if (dtstate.Rows.Count > 0)
                        {
                            DataRow rState = dtstate.Rows[0];
                            机头.状态.打印机故障 = (Convert.ToBoolean(rState["PrinterHot"]) && Convert.ToBoolean(rState["PrinterHigeVolt"]) && Convert.ToBoolean(rState["PrinterLosePaper"]));
                            机头.状态.读币器故障 = Convert.ToBoolean(rState["ICReadState"]);
                        }
                        DataTable st = DataAccess.ExecuteQueryReturnTable("select * from t_parameters where system='txtTicketDate'");
                        if (st.Rows.Count > 0)
                        {
                            机头.打印小票有效天数 = Convert.ToInt32(st.Rows[0]["ParameterValue"]);
                        }
                        st = DataAccess.ExecuteQueryReturnTable("select * from t_parameters where system='chkPrintBarcode'");
                        if (st.Rows.Count > 0)
                        {
                            机头.开关.小票是否打印二维码 = (st.Rows[0]["IsAllow"].ToString() == "1");
                        }

                        机头.常规.退币保护启用标志 = GameInfo.GetProtectValue(机头.常规.游戏机编号);
                        Console.WriteLine(机头.常规.机头编号 + "  " + 机头.常规.退币保护启用标志);
                        机头列表.Add(机头);
                    }
                }
                dt = DataAccess.ExecuteQueryReturnTable(string.Format("select * from t_device d,t_ss_param p where d.id=p.id and d.Segment='{0}'", PubLib.路由器段号));
                {
                    ; foreach (DataRow row in dt.Rows)
                    {
                        机头信息 机头 = new 机头信息();
                        switch (row["type"].ToString())
                        {
                            case "售币机":
                                机头.类型 = 设备类型.售币机;
                                break;
                            case "存币机":
                                机头.类型 = 设备类型.存币机;
                                break;
                            case "自助提币机":
                            case "提币机":
                                机头.类型 = 设备类型.提币机;
                                break;
                            case "碎票机":
                                机头.类型 = 设备类型.碎票机;
                                break;
                            case "投币机":
                                机头.类型 = 设备类型.投币机;
                                break;
                            case "远程存币机":
                                机头.类型 = 设备类型.远程存币机;
                                break;

                        }
                        机头.常规.设备名称 = row["name"].ToString();
                        机头.常规.机头编号 = row["id"].ToString().ToUpper();
                        机头.常规.机头地址 = row["address"].ToString().ToUpper();
                        机头.常规.路由器编号 = row["Segment"].ToString().ToUpper();
                        机头.常规.机头长地址 = row["MCUID"].ToString().ToUpper();
                        机头.存币机.存币箱最大存币报警阀值 = Convert.ToInt32(row["alert_value"].ToString());
                        机头.存币机.马达1启用标识 = (row["motor1"].ToString() == "1");
                        机头.存币机.马达2启用标识 = (row["motor2"].ToString() == "1");
                        机头.存币机.数码管类型 = Convert.ToInt32(row["nixie_tube_type"].ToString());
                        机头.存币机.设备数币计数 = Convert.ToByte(row["FromDevice"].ToString());
                        机头.存币机.卡上增加币数 = Convert.ToByte(row["ToCard"].ToString());
                        //机头.存币机.最小数币数 = Convert.ToInt32(row["min_coin"].ToString());
                        机头.存币机.是否允许打印 = (row["AllowPrint"].ToString() == "1");
                        机头列表.Add(机头);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex);
            }
        }
        public static void UpdateSSRTimeOutInfo(string gameID, int timeoutCount)
        {
            var heads = 机头列表.Where(p => p.常规.游戏机编号 == gameID);
            if (heads.Count() > 0)
            {
                foreach (机头信息 机头 in heads)
                {
                    机头.常规.退币保护启用标志 = true;
                    机头.常规.退币信号超时时间 = timeoutCount;
                    机头.常规.退币信号超时退币个数 = 机头.常规.退币信号超时时间 / (机头.参数.退币脉宽 + 机头.参数.退币速度);
                    if (机头.常规.退币信号超时时间 % (机头.参数.退币脉宽 + 机头.参数.退币速度) != 0)
                        机头.常规.退币信号超时退币个数++;
                }
            }
        }

        /// <summary>
        /// 更新设备信息
        /// </summary>
        /// <param name="机头"></param>
        public static void AddDevice(机头信息 机头)
        {
            var heads = 机头列表.Where(p => p.常规.机头长地址 == 机头.常规.机头长地址);
            if (heads.Count() > 0)
            {
                foreach (机头信息 head in heads)
                {
                    head.常规.机头编号 = 机头.常规.机头编号;
                    head.常规.机头地址 = 机头.常规.机头地址;
                    head.常规.路由器编号 = 机头.常规.路由器编号;
                    head.常规.机头长地址 = 机头.常规.机头长地址;
                    head.存币机.存币箱最大存币报警阀值 = 机头.存币机.存币箱最大存币报警阀值;
                    head.存币机.马达1启用标识 = 机头.存币机.马达1启用标识;
                    head.存币机.马达2启用标识 = 机头.存币机.马达2启用标识;
                    head.存币机.数码管类型 = 机头.存币机.数码管类型;
                    head.存币机.设备数币计数 = 机头.存币机.设备数币计数;
                    head.存币机.卡上增加币数 = 机头.存币机.卡上增加币数;
                    head.存币机.最小数币数 = 机头.存币机.最小数币数;
                }
            }
            else
            {
                机头列表.Add(机头);
            }
        }
        static void GetCoins(string rAddress, string hAddress, out int inCoin, out int outCoin)
        {
            inCoin = 0;
            outCoin = 0;
            try
            {
                //所有未结账进出币统计，按营业日期来算
                //string sql = string.Format(@"select * from view_head_coin where segment='{0}' and headaddress='{1}'", rAddress, hAddress);
                string sql = string.Format(@"select COUNT(Coins) as Coins from flw_485_coin where CoinType in (0,1,4) and Segment='{0}' and HeadAddress='{1}' and RealTime>(select max(ShiftTime) as endtime from flw_schedule);select COUNT(Coins) as Coins from flw_485_coin where CoinType in (2,3) and Segment='{0}' and HeadAddress='{1}' and RealTime>(select max(ShiftTime) as endtime from flw_schedule);SELECT COUNT(Coins) as Coins from flw_ticket_exit where Segment='{0}' and HeadAddress='{1}' and RealTime>(select max(ShiftTime) as endtime from flw_schedule);", rAddress, hAddress);

                DataSet ds = DataAccess.ExecuteQuery(sql);
                inCoin = Convert.ToInt32(ds.Tables[0].Rows[0][0].ToString());
                outCoin = Convert.ToInt32(ds.Tables[1].Rows[0][0].ToString()) + Convert.ToInt32(ds.Tables[2].Rows[0][0].ToString());
                //DataTable dt = ac.ExecuteQuery(sql).Tables[0];
                //if (dt.Rows.Count > 0)
                //{
                //    DataRow row = dt.Rows[0];
                //    inCoin = Convert.ToInt32(row["in_coin_total"]);
                //    outCoin = Convert.ToInt32(row["out_coin_total"]);
                //}
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex);
            }
        }
        public static string GetHeadAddress(string MCUID)
        {
            try
            {
                var infos = 机头列表.Where(p => p.常规.路由器编号 == PubLib.路由器段号.ToUpper() && p.常规.机头长地址 == MCUID.ToUpper());
                if (infos.Count() > 0)
                {
                    return infos.First().常规.机头地址;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex);
            }
            return "";
        }
        public static 机头信息 GetHeadInfo(string MCUID)
        {
            try
            {
                var infos = 机头列表.Where(p => p.常规.路由器编号 == PubLib.路由器段号.ToUpper() && p.常规.机头长地址 == MCUID.ToUpper());
                if (infos.Count() > 0)
                {
                    return infos.First();
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex);
            }
            return null;
        }
        public static 机头信息 GetHeadInfo(string rAddress, string hAddress)
        {
            try
            {
                var infos = 机头列表.Where(p => p.常规.机头地址.ToUpper() == hAddress.ToUpper());
                if (infos.Count() == 0)
                {
                    return null;
                }
                var info = infos.First();
                return info;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex);
            }
            return null;
        }

        /// <summary>
        /// 更新币信息
        /// </summary>
        /// <param name="rAddress">路由器段号</param>
        /// <param name="hAddress">机头地址</param>
        /// <param name="value">币数</param>
        /// <param name="isIn">是否为投币</param>
        public static void SetCoin(string rAddress, string hAddress, int value, bool isIn)
        {
            try
            {
                var info = 机头列表.Where(p => p.常规.路由器编号 == rAddress.ToUpper() && p.常规.机头地址 == hAddress.ToUpper()).First();
                if (isIn)
                {
                    info.投币.投币数 += value;
                }
                else
                {
                    info.投币.退币数 += value;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex);
            }
        }
        /// <summary>
        /// 根据游戏机统计每个机头投币总额
        /// </summary>
        /// <param name="gameID">游戏机编号</param>
        /// <param name="inCoin">输出投币数</param>
        /// <param name="outCoin">输出退币数</param>
        public static void GetCoinByGame(string gameID, out int inCoin, out int outCoin, out int headCount)
        {
            inCoin = 0;
            outCoin = 0;
            headCount = 0;
            try
            {
                var lst = 机头列表.Where(p => p.常规.游戏机编号 == gameID.ToUpper());
                foreach (var data in lst)
                {
                    inCoin += data.投币.投币数;
                    outCoin += data.投币.退币数;
                }
                headCount = lst.Count();
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex);
            }
        }
        public static List<机头信息> GetAllHead()
        {
            return 机头列表.OrderBy(p => p.常规.机头地址).ToList();
        }
        /// <summary>
        /// 更新机头状态
        /// </summary>
        /// <param name="HeadAddress">机头地址</param>
        /// <param name="Status">状态</param>
        public static void Update(string RouteAddress, string HeadAddress, 状态值 Status, 机头干扰开关 Alert)
        {
            try
            {
                机头信息 机头 = GetHeadInfo(RouteAddress, HeadAddress);
                if (机头 == null) return;
                机头.状态 = Status;
                机头.报警 = Alert;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex);
            }
        }

        public static void Update(string HeadAddress, string RouteAddress, bool isFirstCoin)
        {
            try
            {
                机头信息 机头 = GetHeadInfo(RouteAddress, HeadAddress);
                if (机头 == null) return;
                //机头.状态.是否为首次投币 = isFirstCoin;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex);
            }
        }
        public static void Update(string HeadAddress, string MCUID)
        {
            try
            {
                机头信息 机头 = GetHeadInfo(PubLib.路由器段号, HeadAddress);
                if (机头 == null) return;
                机头.常规.机头长地址 = MCUID;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex);
            }
        }

        public static void UpdateOverCoin(string hAddress)
        {
            foreach (机头信息 h in 机头列表)
            {
                if (h.常规.机头地址 == hAddress && h.常规.路由器编号 == PubLib.路由器段号)
                {
                    h.开关.退币超限标志 = false;
                    h.状态.是否忽略超分报警 = true;
                    return;
                }
            }
        }

        public static void ClearHeadProtect(string gameID)
        {
            foreach (机头信息 h in 机头列表)
            {
                if (h.常规.游戏机编号 == gameID)
                {
                    h.常规.退币保护启用标志 = false;
                }
            }
        }

        public static bool 霸位检查(string hAddress, string ICCardID)
        {
            try
            {
                机头信息 h = GetHeadInfo(PubLib.路由器段号, hAddress);
                if (h != null)
                {
                    //当前机头是否启用专卡专用和防霸位功能
                    //if (h.开关.启用防霸位功能 && h.开关.启用专卡专用)
                    if (h.开关.启用防霸位功能)
                    {
                        var head = 机头列表.Where(p => p.常规.当前卡片号 == ICCardID && p.常规.游戏机编号 == h.常规.游戏机编号);
                        if (head.Count() == 0)
                        {
                            return false;
                        }
                        else if (head.Count() == 1)
                        {
                            return (head.First().常规.机头地址.ToUpper() != hAddress.ToUpper());
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex);
            }
            return false;
        }

        public static List<string> 获取专卡专用占用机头列表(string ICCardID)
        {
            List<string> headlist = new List<string>();
            var heads = 机头列表.Where(p => p.常规.当前卡片号 == ICCardID);
            //var heads = 机头列表.Where(p => p.常规.当前卡片号 == ICCardID && (p.开关.启用专卡专用 || p.开关.是否启用卡片专卡专用));
            if (heads.Count() > 0)
            {
                foreach (机头信息 head in heads)
                {
                    headlist.Add(head.常规.机头地址);
                }
            }
            return headlist;
        }

        public static string 获取所有卡头错误数据()
        {
            StringBuilder sb = new StringBuilder();

            var headlist = 机头列表.Where(p => p.是否从雷达获取到状态);
            foreach (机头信息 head in headlist)
            {
                sb.AppendFormat("机头地址：{0} 错误数：{1} {2}", head.常规.机头地址, head.临时错误计数, Environment.NewLine);
            }

            return sb.ToString();
        }

        public static string 获取不在线机头信息()
        {
            StringBuilder sb = new StringBuilder();
            //var headList = 机头列表.Where(p => p.状态.在线状态 == false && p.不在线检测计数 > 2 && p.常规.路由器编号 == PubLib.路由器段号 && p.常规.机头长地址 != "" && p.是否从雷达获取到状态);
            var headList = 机头列表.Where(p => p.状态.在线状态 == false && p.常规.路由器编号 == PubLib.路由器段号 && p.常规.机头长地址 != "");
            foreach (机头信息 head in headList)
            {
                sb.AppendFormat("机头地址：{0} 长地址：{1} {2}", head.常规.机头地址, head.常规.机头长地址, Environment.NewLine);
            }
            return sb.ToString();
        }

        public static void 复位机头信息()
        {
            DataTable dt = DataAccess.ExecuteQueryReturnTable("select * from t_head");
            foreach (DataRow row in dt.Rows)
            {
                var h = 机头列表.Where(p => p.常规.机头编号.ToUpper() == row["headid"].ToString().ToUpper());
                if (h.Count() > 0)
                {
                    h.First().状态.在线状态 = false;
                    h.First().是否从雷达获取到状态 = false;
                    h.First().状态.锁定机头 = (row["State"].ToString() != "1");
                }
            }
        }

        public static void 清除卡号关联()
        {
            foreach (机头信息 h in 机头列表)
            {
                h.常规.当前卡片号 = "";
                h.常规.管理卡号 = "";
            }
        }
    }
}
