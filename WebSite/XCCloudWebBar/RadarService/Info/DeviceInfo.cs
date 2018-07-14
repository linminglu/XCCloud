using DSS;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadarService.Info
{
    public class DeviceInfo
    {
        public enum 设备类型
        {
            卡头 = 0,
            碎票机 = 1,
            存币机 = 2,
            提币机 = 3,
            售币机 = 4,
            投币机 = 5,
            自助机 = 6,
            闸机 = 7,
            路由器 = 8,
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

        public class 设备扩展参数
        {
            public bool 马达1启用 { get; set; }
            public bool 马达2启用 { get; set; }
            public int 马达1出币比例 { get; set; }
            public int 马达2出币比例 { get; set; }
            public int 最大储币数量 { get; set; }
            public int 设备脉冲数 { get; set; }
            public decimal 余额增加数 { get; set; }
            public int 余额种类 { get; set; }
            public bool 启用数字币 { get; set; }
            public bool 启用双光眼 { get; set; }
            public int SSR电平 { get; set; }
        }
        /// <summary>
        /// 由路由器报告取值
        /// </summary>
        public class 状态值
        {
            public bool 锁定机头 { get; set; }
            public bool 允许打票 { get; set; }
            public bool 机头能上分 { get; set; }
            public bool 读币器故障 { get; set; }
            public bool 非法币或卡报警 { get; set; }
            public bool 打印设置错误 { get; set; }
            public bool 打印机故障 { get; set; }
            public bool 在线状态 { get; set; }
            public bool 是否正在使用限时送分优惠 { get; set; }
            public bool 是否忽略超分报警 { get; set; }
            public bool 超级解锁卡标识 { get; set; }
            public bool 出币机或存币机正在数币 { get; set; }
            public bool 存币箱满或限时优惠锁定 { get; set; }
            public bool CO2信号异常 { get; set; }
            public bool CO信号异常 { get; set; }
            public bool SSR信号异常 { get; set; }
            public bool 高压干扰报警 { get; set; }
            public bool 高频干扰报警 { get; set; }
        }
        public class 退币保护参数
        {
            public bool 退币保护启用标志 = false;
            public int 退币信号超时时间 = 0;
            public int 退币信号超时退币个数 = 0;
            public int 退币保护触发次数 = 0;
        }
        public class 机头信息
        {
            public int 设备编号 = 0;
            public string 路由器段号 = "";
            public string 机头短地址 = "";
            public string 游戏机编号 = "";
            public int 游戏机索引号 = 0;
            public int 游乐项目索引 = 0;
            public string 位置名称 = "";
            public string 设备序列号 = "";
            public string 设备名称 = "";
            public bool 退币超限标志 = false;
            public string 当前卡片号 = "";
            public string 管理卡号 = "";
            public bool 是否为首次投币 = false;
            public int 退币脉宽 = 0;
            public int 退币速度 = 0;
            public bool 小票是否打印二维码 = false;
            public 退币保护参数 退币保护 = new 退币保护参数();
            public 设备扩展参数 扩展参数 = new 设备扩展参数();
            public 投币信息 投币 = new 投币信息();
            public 状态值 状态 = new 状态值();
            public 设备类型 类型;
            public bool 是否从雷达获取到状态 = false;
            public int 临时错误计数;
            public int 不在线检测计数;
            public bool 彩票模式 = false;
            public int 打印小票有效天数 = 0;
            public string 订单编号 = "";
            public bool 位置有效 = false;
            /// <summary>
            /// 0 刷卡扣值 1 扣门票业务
            /// </summary>
            public int 设备业务类别 = 0;
            /// <summary>
            /// 0 入口 1 出口
            /// </summary>
            public int 验票进出方向 = 0;
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
        /// 上位机修改机头参数时，重新加载当前游戏机
        /// </summary>
        /// <param name="MCUID"></param>
        public static void ReloadDeviceInfo(string MCUID)
        {
            DataAccess ac = new DataAccess();
            DataTable st = ac.ExecuteQueryReturnTable("select ParameterValue from Data_Parameters where system='chkPrintBarcode' and StoreID='" + PublicHelper.SystemDefiner.StoreID + "'");
            //获取卡头列表
            DataTable dt = ac.ExecuteQueryReturnTable(string.Format("select b.ID,b.DeviceName,b.type,b.segment,b.Address,b.DeviceStatus,ISNULL(b.GameIndexID,0) as GameIndexID,ISNULL(b.SiteName,'') as SiteName,b.MCUID,ISNULL(g.GameID,'') as GameID,g.OutPulse,g.OutSpeed,g.LotteryMode,g.OnceOutLimit,g.OncePureOutLimit,g.SSRTimeOut,ISNULL(d.ProjectID,0) as ProjectID,ISNULL(d.WorkType,'') as WorkType from Base_DeviceInfo b left join Data_GameInfo g on b.GameIndexID=g.ID left join Data_Project_BindDevice d on b.ID=d.DeviceID where b.MCUID='{0}'", MCUID));
            if (dt.Rows.Count > 0)
            {
                try
                {
                    DataRow row = dt.Rows[0];
                    机头信息 机头 = new 机头信息();
                    机头.位置有效 = (row["DeviceStatus"].ToString() == "1");
                    机头.类型 = (设备类型)row["type"];
                    机头.设备编号 = Convert.ToInt32(row["ID"]);
                    机头.路由器段号 = row["Segment"].ToString();
                    机头.机头短地址 = row["Address"].ToString();
                    机头.游戏机索引号 = Convert.ToInt32(row["GameIndexID"]);
                    机头.游戏机编号 = row["GameID"].ToString();
                    机头.游乐项目索引 = Convert.ToInt32(row["ProjectID"]);
                    if (row["WorkType"].ToString() != "")
                    {
                        机头.设备业务类别 = 1;
                        机头.验票进出方向 = Convert.ToInt32(row["WorkType"]);
                    }
                    机头.设备序列号 = row["MCUID"].ToString();
                    机头.设备名称 = row["DeviceName"].ToString();
                    机头.位置名称 = row["SiteName"].ToString();
                    机头.当前卡片号 = "";
                    机头.是否为首次投币 = true;

                    机头.扩展参数 = GetExtParamets(机头.设备编号);
                    SetBufMCUIDInfo(机头.路由器段号, 机头.机头短地址.ToLower(), MCUID);

                    if (机头.类型 == 设备类型.卡头)
                    {
                        机头.彩票模式 = (row["LotteryMode"].ToString() == "1");
                        机头.退币保护.退币信号超时时间 = Convert.ToInt32(row["SSRTimeOut"]);
                        机头.退币脉宽 = Convert.ToInt32(row["OutPulse"]);
                        机头.退币速度 = Convert.ToInt32(row["OutSpeed"]);
                        机头.投币.单次退币上限 = Convert.ToInt32(row["OnceOutLimit"]);
                        机头.投币.每天净退币上限 = Convert.ToInt32(row["OncePureOutLimit"]);
                        if (机头.投币.盈利数 > 机头.投币.每天净退币上限)
                        {
                            机头.退币超限标志 = true;
                        }
                        机头.退币保护.退币信号超时退币个数 = 机头.退币保护.退币信号超时时间 / (机头.退币脉宽 + 机头.退币速度);
                        if (机头.退币保护.退币信号超时时间 % (机头.退币脉宽 + 机头.退币速度) != 0)
                            机头.退币保护.退币信号超时退币个数++;
                    }

                    机头.状态.是否忽略超分报警 = false;
                    机头.状态.锁定机头 = false;

                    机头.状态.机头能上分 = true;
                    机头.状态.允许打票 = true;

                    if (st.Rows.Count > 0)
                    {
                        机头.小票是否打印二维码 = (st.Rows[0]["ParameterValue"].ToString() == "1");
                    }
                    SetBufMCUIDDeviceInfo(MCUID, 机头);
                }
                catch (Exception ex)
                {
                    LogHelper.LogHelper.WriteLog(ex);
                }
            }
        }
        /// <summary>
        /// 初始化机头列表，抄表部分重新
        /// </summary>
        public static void Init()
        {
            int tims = 0;
            try
            {
                DataAccess ac = new DataAccess();
                DataTable st = ac.ExecuteQueryReturnTable("select ParameterValue from Data_Parameters where system='chkPrintBarcode' and StoreID='" + PublicHelper.SystemDefiner.StoreID + "'");
                //获取卡头列表
                DataTable dt = ac.ExecuteQueryReturnTable(string.Format("select b.ID,b.DeviceName,b.type,b.segment,b.Address,b.DeviceStatus,ISNULL(b.GameIndexID,0) as GameIndexID,ISNULL(b.SiteName,'') as SiteName,b.MCUID,ISNULL(g.GameID,'') as GameID,g.OutPulse,g.OutSpeed,g.LotteryMode,g.OnceOutLimit,g.OncePureOutLimit,g.SSRTimeOut,ISNULL(d.ProjectID,0) as ProjectID,ISNULL(d.WorkType,'') as WorkType from Base_DeviceInfo b left join Data_GameInfo g on b.GameIndexID=g.ID left join Data_Project_BindDevice d on b.ID=d.DeviceID where segment<>'' and b.MerchID='{0}' and b.StoreID='{1}'", PublicHelper.SystemDefiner.MerchID, PublicHelper.SystemDefiner.StoreID));
                foreach (DataRow row in dt.Rows)
                {
                    tims++;
                    string MCUID = row["MCUID"].ToString();

                    机头信息 h = null;// XCCloudSerialNo.SerialNoHelper.StringGet<机头信息>("headinfo_"+MCUID);
                    if (h == null)
                    {
                        机头信息 机头 = new 机头信息();
                        机头.位置有效 = (row["DeviceStatus"].ToString() == "1");
                        机头.类型 = (设备类型)row["type"];
                        机头.设备编号 = Convert.ToInt32(row["ID"]);
                        机头.路由器段号 = row["Segment"].ToString();
                        机头.机头短地址 = row["Address"].ToString();
                        机头.游戏机索引号 = Convert.ToInt32(row["GameIndexID"]);
                        机头.游戏机编号 = row["GameID"].ToString();
                        机头.游乐项目索引 = Convert.ToInt32(row["ProjectID"]);
                        if (row["WorkType"].ToString() != "")
                        {
                            机头.设备业务类别 = 1;
                            机头.验票进出方向 = Convert.ToInt32(row["WorkType"]);
                        }
                        机头.设备序列号 = row["MCUID"].ToString();
                        机头.设备名称 = row["DeviceName"].ToString();
                        机头.位置名称 = row["SiteName"].ToString();
                        机头.当前卡片号 = "";
                        机头.是否为首次投币 = true;

                        机头.扩展参数 = GetExtParamets(机头.设备编号);
                        //string m = XCCloudSerialNo.SerialNoHelper.StringGet("info_"+机头.路由器段号 + "|" + 机头.机头短地址);

                        //if (m == null)
                        SetBufMCUIDInfo(机头.路由器段号, 机头.机头短地址.ToLower(), MCUID);

                        if (机头.类型 == 设备类型.卡头)
                        {
                            机头.彩票模式 = (row["LotteryMode"].ToString() == "1");
                            机头.退币保护.退币信号超时时间 = Convert.ToInt32(row["SSRTimeOut"]);
                            机头.退币脉宽 = Convert.ToInt32(row["OutPulse"]);
                            机头.退币速度 = Convert.ToInt32(row["OutSpeed"]);
                            机头.投币.单次退币上限 = Convert.ToInt32(row["OnceOutLimit"]);
                            机头.投币.每天净退币上限 = Convert.ToInt32(row["OncePureOutLimit"]);
                            if (机头.投币.盈利数 > 机头.投币.每天净退币上限)
                            {
                                机头.退币超限标志 = true;
                            }
                            机头.退币保护.退币信号超时退币个数 = 机头.退币保护.退币信号超时时间 / (机头.退币脉宽 + 机头.退币速度);
                            if (机头.退币保护.退币信号超时时间 % (机头.退币脉宽 + 机头.退币速度) != 0)
                                机头.退币保护.退币信号超时退币个数++;
                        }

                        机头.状态.是否忽略超分报警 = false;
                        机头.状态.锁定机头 = false;

                        机头.状态.机头能上分 = true;
                        机头.状态.允许打票 = true;

                        if (st.Rows.Count > 0)
                        {
                            机头.小票是否打印二维码 = (st.Rows[0]["ParameterValue"].ToString() == "1");
                        }
                        SetBufMCUIDDeviceInfo(MCUID, 机头);
                    }
                    else
                    {
                        string m = GetBufMCUIDInfo(h.路由器段号,h.机头短地址.ToLower());
                        if (m == null)
                            SetBufMCUIDInfo(h.路由器段号, h.机头短地址.ToLower(), MCUID);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogHelper.WriteLog(ex);
            }
        }

        public static 设备扩展参数 GetExtParamets(int deviceID)
        {
            DataAccess ac = new DataAccess();
            DataTable dt = ac.ExecuteQueryReturnTable("select * from Base_DeviceInfo_Ext where DeviceID='" + deviceID + "'");
            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                设备扩展参数 p = new 设备扩展参数();
                p.马达1启用 = (row["Motor1EN"].ToString() == "1");
                p.马达2启用 = (row["Motor2EN"].ToString() == "1");
                p.马达1出币比例 = Convert.ToInt32(row["Motor1Coin"]);
                p.马达2出币比例 = Convert.ToInt32(row["Motor2Coin"]);
                p.最大储币数量 = Convert.ToInt32(row["MaxSaveCount"]);
                p.设备脉冲数 = Convert.ToInt32(row["FromDevice"]);
                p.余额增加数 = Convert.ToDecimal(row["ToCard"]);
                p.余额种类 = Convert.ToInt32(row["BalanceIndex"]);
                p.启用数字币 = (row["DigitCoinEN"].ToString() == "1");
                p.启用双光眼 = (row["DubleCheck"].ToString() == "1");
                p.SSR电平 = Convert.ToInt32(row["SSRLevel"]);
                return p;
            }
            return null;
        }
        public static bool TBProtect(机头信息 head)
        {
            head.退币保护.退币保护触发次数++;
            if (head.退币保护.退币保护触发次数 >= 3)
            {
                head.退币保护.退币保护触发次数 = 0;  //重新清零计数                        
                return true;
            }
            return false;
        }

        public static void SetBufMCUIDDeviceInfo(string mcuid, 机头信息 head)
        {
            XCCloudSerialNo.SerialNoHelper.StringSet<机头信息>("headinfo_" + mcuid, head);
        }
        public static 机头信息 GetBufMCUIDDeviceInfo(string mcuid)
        {
            return  XCCloudSerialNo.SerialNoHelper.StringGet<机头信息>("headinfo_" + mcuid);
        }
        public static 机头信息 GetBufMCUIDDeviceInfo(string segment, string headAddress)
        {
            string mcuid = GetBufMCUIDInfo(segment, headAddress);
            return GetBufMCUIDDeviceInfo(mcuid);
        }
        public static void SetBufMCUIDInfo(string segment,string headAddress,string mcuid)
        {
            XCCloudSerialNo.SerialNoHelper.StringSet("mcuidinfo_" + segment + "|" + headAddress, mcuid);
        }
        public static string GetBufMCUIDInfo(string segment, string headAddress)
        {
            return XCCloudSerialNo.SerialNoHelper.StringGet("mcuidinfo_" + segment + "|" + headAddress);
        }
    }
}
