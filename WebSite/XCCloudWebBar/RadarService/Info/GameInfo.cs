using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Data;
using DSS.Table;
using DSS;
namespace RadarService.Info
{
    public static class GameInfo
    {
        public class 显示参数
        {
            public string StoreName { get; set; }
            public string GameName { get; set; }
            public string SiteName { get; set; }
            public string PushName1 { get; set; }
            public string PushName2 { get; set; }
            public string OutName { get; set; }
        }
        public class 投币信息
        {
            public int 投币数 = 0;
            public int 退币数 = 0;
            public int 盈利数 { get { return 投币数 - 退币数; } }
        }
        public class 通用参数配置
        {
            public string 游戏机名 { get; set; }
            public string 游戏机类别 { get; set; }
            public int 游戏机类别索引 { get; set; }
            public string 游戏机编号 { get; set; }
            public bool 设备有效状态 { get; set; }
            public bool 退彩票模式 { get; set; }
            public bool 只退实物彩票 { get; set; }
            public bool 礼品掉落检测 { get; set; }
            public List<会员电子彩票赠送配置> 会员送彩票规则 { get; set; }
        }
        public class 会员电子彩票赠送配置
        {
            public int 会员卡级别编号 { get; set; }
            public int 彩票基数 { get; set; }
            public int 送彩票数 { get; set; }
        }
        public class 投币参数配置
        {
            public bool 允许电子投币 { get; set; }
            public bool 允许实物投币 { get; set; }
            public bool 刷卡即扣 { get; set; }
            public int 刷卡即扣延时 { get; set; }
            public int 按钮1余额类别 { get; set; }
            public int 按钮1投币基数 { get; set; }
            public int 按钮2余额类别 { get; set; }
            public int 按钮2投币基数 { get; set; }
            public int 按钮1投币脉冲数 { get; set; }
            public int 按钮2投币脉冲数 { get; set; }
            public int 投币速度 { get; set; }
            public int 投币脉宽 { get; set; }
            public int 投币电平 { get; set; }
            public int 投币首次间隔时间 { get; set; }
            public bool 启用上分通道 { get; set; }
            public int 按钮1上分脉冲数 { get; set; }
            public int 按钮2上分脉冲数 { get; set; }
            public int 上分速度 { get; set; }
            public int 上分脉宽 { get; set; }
            public int 上分电平 { get; set; }
            public int 上分首次间隔时间 { get; set; }
        }
        public class 退币参数配置
        {
            public bool 允许电子出币 { get; set; }
            public bool 允许实物出币 { get; set; }
            public bool 退币锁定 { get; set; }
            public bool 即中即退 { get; set; }
            public bool 退币按钮保持 { get; set; }
            public int 退币按钮脉宽 { get; set; }
            public int 退币方式 { get; set; }
            public int 退币SSR电平 { get; set; }
            public int 退币MR电平 { get; set; }
            public int 退币速度 { get; set; }
            public int 退币脉宽 { get; set; }
            public int 退币时游戏机出币基数 { get; set; }
            public int 退币时余额增加基数 { get; set; }
            public int 退币余额类别 { get; set; }
        }
        public class 博彩参数配置
        {
            public bool 专卡专用 { get; set; }
            public bool 增强专卡专用 { get; set; }
            public bool 遥控器偷分检测 { get; set; }
            public bool 偷分报警模块 { get; set; }
            public bool 异常出币检测 { get; set; }
            public int 异常出币检测时间 { get; set; }
            public int 异常出币检测次数 { get; set; }
            public int 单次退币限额 { get; set; }
            public int 每日净退币限额 { get; set; }
            public bool 二合一模式 { get; set; }
            public bool 霸位检测 { get; set; }
            public bool 是否参与返还 { get; set; }
        }
        public class 游戏机信息
        {
            public int 游戏机索引号 { get; set; }
            public string 游戏机编号 { get; set; }
            public string 商户编号 { get; set; }
            public string 门店编号 { get; set; }
            public string 区域名称 { get; set; }
            public int 退币保护盾延时 { get; set; }
            /// <summary>
            /// 0 ，1 普通刷卡，2 按次门票，3 计时项目
            /// </summary>
            public int 游戏机消费类别 { get; set; }
            public 投币信息 投币 = new 投币信息();
            public bool 游戏机有效 = false;
            public List<验票设备> 设备列表 = new List<验票设备>();
            public 通用参数配置 通用参数 = new 通用参数配置();
            public 投币参数配置 投币参数 = new 投币参数配置();
            public 退币参数配置 退币参数 = new 退币参数配置();
            public 博彩参数配置 博彩参数 = new 博彩参数配置();
        }
        public class 验票设备
        {
            public int 设备编号 { get; set; }
            public int 工作方式 { get; set; }
            public bool 校验顺序 { get; set; }
            public bool 人脸识别 { get; set; }
            public bool 刷卡识别 { get; set; }
            public bool 二维码识别 { get; set; }
            public bool 现金使用 { get; set; }
        }
        public static void ReloadGame(int gameIndex)
        {
            try
            {
                DataAccess ac = new DataAccess();
                DataTable dt = ac.ExecuteQuery("select ISNULL(a.AreaName,'') as AreaName,g.* from (select t.GameTypeName,t.DictValue,g.* from Data_GameInfo g,(select ID,GameTypeName,DictValue from ViewGameType) t where g.ID='" + gameIndex + "') g left join Data_GroupArea a on g.AreaID=a.ID").Tables[0];
                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    string sid = row["ID"].ToString();
                    Console.WriteLine(sid);
                    游戏机信息 游戏机 = new 游戏机信息();
                    游戏机.游戏机有效 = (row["State"].ToString() == "1");
                    游戏机.游戏机编号 = row["GameID"].ToString().ToUpper();
                    游戏机.通用参数.游戏机名 = row["GameName"].ToString();
                    游戏机.通用参数.游戏机类别 = row["GameTypeName"].ToString();
                    游戏机.通用参数.退彩票模式 = (row["LotteryMode"].ToString() == "1");
                    游戏机.通用参数.只退实物彩票 = (row["OnlyExitLottery"].ToString() == "1");
                    游戏机.通用参数.礼品掉落检测 = (row["chkCheckGift"].ToString() == "1");
                    游戏机.通用参数.设备有效状态 = (row["State"].ToString() == "1");
                    游戏机.通用参数.游戏机类别索引 = Convert.ToInt32(row["GameType"]);

                    DataModel model = new DataModel();
                    List<object> outList = new List<object>();
                    model.CovertToDataModel("select * from Data_GameFreeLotteryRule where GameIndex='" + 游戏机.游戏机索引号 + "'", typeof(Data_GameFreeLotteryRule), out outList);
                    游戏机.通用参数.会员送彩票规则 = new List<会员电子彩票赠送配置>();
                    foreach (object o in outList)
                        游戏机.通用参数.会员送彩票规则.Add(o as 会员电子彩票赠送配置);

                    游戏机.博彩参数.单次退币限额 = Convert.ToInt32(row["OnceOutLimit"]);
                    游戏机.博彩参数.每日净退币限额 = Convert.ToInt32(row["OncePureOutLimit"]);
                    游戏机.博彩参数.霸位检测 = (row["BanOccupy"].ToString() == "1");
                    游戏机.门店编号 = PublicHelper.SystemDefiner.StoreID;
                    游戏机.区域名称 = row["AreaName"].ToString();
                    游戏机.商户编号 = PublicHelper.SystemDefiner.MerchID;
                    游戏机.博彩参数.是否参与返还 = (row["NotGiveBack"].ToString() == "1");
                    游戏机.退币保护盾延时 = Convert.ToInt32(row["SSRTimeOut"]);
                    游戏机.退币参数.退币锁定 = (row["BOLock"].ToString() == "1");

                    游戏机.游戏机索引号 = Convert.ToInt32(row["ID"]);
                    游戏机.游戏机消费类别 = Convert.ToInt32(row["DictValue"]);
                    if (游戏机.游戏机消费类别 == 2)
                    {
                        int v = GetProjectType(游戏机.游戏机索引号);
                        if (v != 0)
                            游戏机.游戏机消费类别 = v;
                    }

                    游戏机.投币参数.允许电子投币 = (row["AllowElecPush"].ToString() == "1");
                    游戏机.投币参数.允许实物投币 = (row["AllowRealPush"].ToString() == "1");
                    游戏机.投币参数.刷卡即扣 = (row["ReadCat"].ToString() == "1");
                    游戏机.投币参数.刷卡即扣延时 = Convert.ToInt32(row["ReadDelay"]);
                    游戏机.投币参数.按钮1余额类别 = Convert.ToInt32(row["PushBalanceIndex1"]);
                    游戏机.投币参数.按钮1投币基数 = Convert.ToInt32(row["PushCoin1"]);
                    游戏机.投币参数.按钮1投币脉冲数 = Convert.ToInt32(row["PushAddToGame1"]);
                    游戏机.投币参数.按钮1上分脉冲数 = Convert.ToInt32(row["SecondAddToGame1"]);
                    游戏机.投币参数.按钮2余额类别 = Convert.ToInt32(row["PushBalanceIndex2"]);
                    游戏机.投币参数.按钮2投币基数 = Convert.ToInt32(row["PushCoin2"]);
                    游戏机.投币参数.按钮2投币脉冲数 = Convert.ToInt32(row["PushAddToGame2"]);
                    游戏机.投币参数.按钮2上分脉冲数 = Convert.ToInt32(row["SecondAddToGame2"]);
                    游戏机.投币参数.启用上分通道 = (row["UseSecondPush"].ToString() == "1");
                    游戏机.投币参数.上分电平 = Convert.ToInt32(row["SecondLevel"]);
                    游戏机.投币参数.上分脉宽 = Convert.ToInt32(row["SecondPulse"]);
                    游戏机.投币参数.上分速度 = Convert.ToInt32(row["SecondSpeed"]);
                    游戏机.投币参数.上分首次间隔时间 = Convert.ToInt32(row["SecondStartInterval"]);
                    游戏机.投币参数.投币电平 = Convert.ToInt32(row["PushLevel"]);
                    游戏机.投币参数.投币脉宽 = Convert.ToInt32(row["PushPulse"]);
                    游戏机.投币参数.投币速度 = Convert.ToInt32(row["PushSpeed"]);
                    游戏机.投币参数.投币首次间隔时间 = Convert.ToInt32(row["PushStartInterval"]);
                    游戏机.退币参数.允许电子出币 = (row["AllowElecOut"].ToString() == "1");
                    游戏机.退币参数.即中即退 = (row["NowExit"].ToString() == "1");
                    游戏机.退币参数.退币方式 = Convert.ToInt32(row["OutMode"]);
                    游戏机.退币参数.退币按钮脉宽 = Convert.ToInt32(row["BOPulse"]);
                    游戏机.退币参数.退币SSR电平 = Convert.ToInt32(row["OutLevel"]);
                    游戏机.退币参数.退币脉宽 = Convert.ToInt32(row["OutPulse"]);
                    游戏机.退币参数.退币速度 = Convert.ToInt32(row["OutSpeed"]);
                    游戏机.退币参数.退币MR电平 = Convert.ToInt32(row["CountLevel"]);
                    游戏机.退币参数.退币余额类别 = Convert.ToInt32(row["OutBalanceIndex"]);
                    游戏机.退币参数.退币时游戏机出币基数 = Convert.ToInt32(row["OutReduceFromGame"]);
                    游戏机.退币参数.退币时余额增加基数 = Convert.ToInt32(row["OutAddToCard"]);
                    游戏机.博彩参数.专卡专用 = (row["GuardConvertCard"].ToString() == "1");
                    游戏机.博彩参数.增强专卡专用 = (row["StrongGuardConvertCard"].ToString() == "1");
                    游戏机.博彩参数.二合一模式 = (row["ICTicketOperation"].ToString() == "1");
                    游戏机.博彩参数.遥控器偷分检测 = (row["PushAddToGame2"].ToString() == "1");
                    游戏机.博彩参数.偷分报警模块 = (row["OutsideAlertCheck"].ToString() == "1");
                    游戏机.博彩参数.异常出币检测 = (row["ExceptOutTest"].ToString() == "1");
                    游戏机.博彩参数.异常出币检测次数 = Convert.ToInt32(row["Frequency"]);
                    游戏机.博彩参数.异常出币检测时间 = Convert.ToInt32(row["ExceptOutSpeed"]);

                    if (row["DictValue"].ToString() == "2")
                    {
                        //游乐项目
                        dt = ac.ExecuteQueryReturnTable("select d.* from Data_ProjectInfo p,Data_Project_BindDevice d where p.ID=d.ProjectID and p.GameIndex='" + 游戏机.游戏机索引号 + "'");
                        if (dt.Rows.Count > 0)
                        {
                            DataRow r = dt.Rows[0];
                            验票设备 t = new 验票设备();
                            t.二维码识别 = (r["ReadQRCode"].ToString() == "1");
                            t.工作方式 = Convert.ToInt32(r["WorkType"]);
                            t.人脸识别 = (r["ReadFace"].ToString() == "1");
                            t.设备编号 = Convert.ToInt32(r["DeviceID"]);
                            t.刷卡识别 = (r["ReadCard"].ToString() == "1");
                            t.现金使用 = (r["AllowCash"].ToString() == "1");
                            游戏机.设备列表.Add(t);
                        }
                    }
                    SetGameInfo(游戏机);
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogHelper.WriteLog(ex);
            }
        }
        /// <summary>
        /// 初始化游戏机信息
        /// </summary>
        public static void Init()
        {
            try
            {
                DataAccess ac = new DataAccess();
                DataTable dt = ac.ExecuteQuery("select ISNULL(a.AreaName,'') as AreaName,g.* from (select t.GameTypeName,t.DictValue,g.* from Data_GameInfo g,(select ID,GameTypeName,DictValue from ViewGameType) t where StoreID='" + PublicHelper.SystemDefiner.StoreID + "' and t.ID=g.GameType and g.State=1) g left join Data_GroupArea a on g.AreaID=a.ID").Tables[0];
                foreach (DataRow row in dt.Rows)
                {
                    string sid = row["ID"].ToString();
                    Console.WriteLine(sid);
                    游戏机信息 游戏机 = null;// XCCloudSerialNo.SerialNoHelper.StringGet<游戏机信息>(sid);
                    if (游戏机 == null)
                    {
                        游戏机 = new 游戏机信息();
                        游戏机.游戏机有效 = (row["State"].ToString() == "1");
                        游戏机.游戏机编号 = row["GameID"].ToString().ToUpper();
                        游戏机.通用参数.游戏机名 = row["GameName"].ToString();
                        游戏机.通用参数.游戏机类别 = row["GameTypeName"].ToString();
                        游戏机.通用参数.退彩票模式 = (row["LotteryMode"].ToString() == "1");
                        游戏机.通用参数.只退实物彩票 = (row["OnlyExitLottery"].ToString() == "1");
                        游戏机.通用参数.礼品掉落检测 = (row["chkCheckGift"].ToString() == "1");
                        游戏机.通用参数.设备有效状态 = (row["State"].ToString() == "1");
                        游戏机.通用参数.游戏机类别索引 = Convert.ToInt32(row["GameType"]);

                        DataModel model = new DataModel();
                        List<object> outList = new List<object>();
                        model.CovertToDataModel("select * from Data_GameFreeLotteryRule where GameIndex='" + 游戏机.游戏机索引号 + "'", typeof(Data_GameFreeLotteryRule), out outList);
                        游戏机.通用参数.会员送彩票规则 = new List<会员电子彩票赠送配置>();
                        foreach (object o in outList)
                            游戏机.通用参数.会员送彩票规则.Add(o as 会员电子彩票赠送配置);

                        游戏机.博彩参数.单次退币限额 = Convert.ToInt32(row["OnceOutLimit"]);
                        游戏机.博彩参数.每日净退币限额 = Convert.ToInt32(row["OncePureOutLimit"]);
                        游戏机.博彩参数.霸位检测 = (row["BanOccupy"].ToString() == "1");
                        游戏机.门店编号 = PublicHelper.SystemDefiner.StoreID;
                        游戏机.区域名称 = row["AreaName"].ToString();
                        游戏机.商户编号 = PublicHelper.SystemDefiner.MerchID;
                        游戏机.博彩参数.是否参与返还 = (row["NotGiveBack"].ToString() == "1");
                        游戏机.退币保护盾延时 = Convert.ToInt32(row["SSRTimeOut"]);
                        游戏机.退币参数.退币锁定 = (row["BOLock"].ToString() == "1");

                        游戏机.游戏机索引号 = Convert.ToInt32(row["ID"]);
                        游戏机.游戏机消费类别 = Convert.ToInt32(row["DictValue"]);
                        if (游戏机.游戏机消费类别 == 2)
                        {
                            int v = GetProjectType(游戏机.游戏机索引号);
                            if (v != 0)
                                游戏机.游戏机消费类别 = v;
                        }

                        游戏机.投币参数.允许电子投币 = (row["AllowElecPush"].ToString() == "1");
                        游戏机.投币参数.允许实物投币 = (row["AllowRealPush"].ToString() == "1");
                        游戏机.投币参数.刷卡即扣 = (row["ReadCat"].ToString() == "1");
                        游戏机.投币参数.刷卡即扣延时 = Convert.ToInt32(row["ReadDelay"]);
                        游戏机.投币参数.按钮1余额类别 = Convert.ToInt32(row["PushBalanceIndex1"]);
                        游戏机.投币参数.按钮1投币基数 = Convert.ToInt32(row["PushCoin1"]);
                        游戏机.投币参数.按钮1投币脉冲数 = Convert.ToInt32(row["PushAddToGame1"]);
                        游戏机.投币参数.按钮1上分脉冲数 = Convert.ToInt32(row["SecondAddToGame1"]);
                        游戏机.投币参数.按钮2余额类别 = Convert.ToInt32(row["PushBalanceIndex2"]);
                        游戏机.投币参数.按钮2投币基数 = Convert.ToInt32(row["PushCoin2"]);
                        游戏机.投币参数.按钮2投币脉冲数 = Convert.ToInt32(row["PushAddToGame2"]);
                        游戏机.投币参数.按钮2上分脉冲数 = Convert.ToInt32(row["SecondAddToGame2"]);
                        游戏机.投币参数.启用上分通道 = (row["UseSecondPush"].ToString() == "1");
                        游戏机.投币参数.上分电平 = Convert.ToInt32(row["SecondLevel"]);
                        游戏机.投币参数.上分脉宽 = Convert.ToInt32(row["SecondPulse"]);
                        游戏机.投币参数.上分速度 = Convert.ToInt32(row["SecondSpeed"]);
                        游戏机.投币参数.上分首次间隔时间 = Convert.ToInt32(row["SecondStartInterval"]);
                        游戏机.投币参数.投币电平 = Convert.ToInt32(row["PushLevel"]);
                        游戏机.投币参数.投币脉宽 = Convert.ToInt32(row["PushPulse"]);
                        游戏机.投币参数.投币速度 = Convert.ToInt32(row["PushSpeed"]);
                        游戏机.投币参数.投币首次间隔时间 = Convert.ToInt32(row["PushStartInterval"]);
                        游戏机.退币参数.允许电子出币 = (row["AllowElecOut"].ToString() == "1");
                        游戏机.退币参数.即中即退 = (row["NowExit"].ToString() == "1");
                        游戏机.退币参数.退币方式 = Convert.ToInt32(row["OutMode"]);
                        游戏机.退币参数.退币按钮脉宽 = Convert.ToInt32(row["BOPulse"]);
                        游戏机.退币参数.退币SSR电平 = Convert.ToInt32(row["OutLevel"]);
                        游戏机.退币参数.退币脉宽 = Convert.ToInt32(row["OutPulse"]);
                        游戏机.退币参数.退币速度 = Convert.ToInt32(row["OutSpeed"]);
                        游戏机.退币参数.退币MR电平 = Convert.ToInt32(row["CountLevel"]);
                        游戏机.退币参数.退币余额类别 = Convert.ToInt32(row["OutBalanceIndex"]);
                        游戏机.退币参数.退币时游戏机出币基数 = Convert.ToInt32(row["OutReduceFromGame"]);
                        游戏机.退币参数.退币时余额增加基数 = Convert.ToInt32(row["OutAddToCard"]);
                        游戏机.博彩参数.专卡专用 = (row["GuardConvertCard"].ToString() == "1");
                        游戏机.博彩参数.增强专卡专用 = (row["StrongGuardConvertCard"].ToString() == "1");
                        游戏机.博彩参数.二合一模式 = (row["ICTicketOperation"].ToString() == "1");
                        游戏机.博彩参数.遥控器偷分检测 = (row["PushAddToGame2"].ToString() == "1");
                        游戏机.博彩参数.偷分报警模块 = (row["OutsideAlertCheck"].ToString() == "1");
                        游戏机.博彩参数.异常出币检测 = (row["ExceptOutTest"].ToString() == "1");
                        游戏机.博彩参数.异常出币检测次数 = Convert.ToInt32(row["Frequency"]);
                        游戏机.博彩参数.异常出币检测时间 = Convert.ToInt32(row["ExceptOutSpeed"]);

                        if (row["DictValue"].ToString() == "2")
                        {
                            //游乐项目
                            dt = ac.ExecuteQueryReturnTable("select d.* from Data_ProjectInfo p,Data_Project_BindDevice d where p.ID=d.ProjectID and p.GameIndex='" + 游戏机.游戏机索引号 + "'");
                            if (dt.Rows.Count > 0)
                            {
                                DataRow r = dt.Rows[0];
                                验票设备 t = new 验票设备();
                                t.二维码识别 = (r["ReadQRCode"].ToString() == "1");
                                t.工作方式 = Convert.ToInt32(r["WorkType"]);
                                t.人脸识别 = (r["ReadFace"].ToString() == "1");
                                t.设备编号 = Convert.ToInt32(r["DeviceID"]);
                                t.刷卡识别 = (r["ReadCard"].ToString() == "1");
                                t.现金使用 = (r["AllowCash"].ToString() == "1");
                                游戏机.设备列表.Add(t);
                            }
                        }
                        SetGameInfo(游戏机);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogHelper.WriteLog(ex);
            }
        }
        public static List<string> GetDeviceList(int gameIndex)
        {
            DataAccess ac = new DataAccess();
            DataTable dt = ac.ExecuteQueryReturnTable("select MCUID from Base_DeviceInfo where GameIndexID='" + gameIndex + "'");
            List<string> s = new List<string>();
            foreach (DataRow row in dt.Rows)
            {
                s.Add(row["MCUID"].ToString());
            }
            return s;
        }
        static int GetProjectType(int GameIndex)
        {
            DataAccess ac = new DataAccess();
            DataTable dt = ac.ExecuteQueryReturnTable("select ChargeType from Data_ProjectInfo where GameIndex='" + GameIndex + "'");
            if (dt.Rows.Count > 0)
            {
                if (dt.Rows[0]["ChargeType"].ToString() == "0")
                    return 2;   //按次扣
                else
                    return 3;   //按时扣
            }
            return 0;
        }

        public static void SetGameInfo(游戏机信息 game)
        {
            XCCloudSerialNo.SerialNoHelper.StringSet<GameInfo.游戏机信息>("gameinfo_" + game.游戏机索引号.ToString(), game);
        }

        public static 游戏机信息 GetGameInfo(int gameIndex)
        {
            return XCCloudSerialNo.SerialNoHelper.StringGet<Info.GameInfo.游戏机信息>("gameinfo_" + gameIndex);
        }
    }
}
