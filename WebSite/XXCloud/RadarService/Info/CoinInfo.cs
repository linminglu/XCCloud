using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Reflection;
using RadarService.Command.Ask;
using RadarService.HKAPI;
using System.Net;
using DSS;

namespace RadarService.Info
{
    public static class CoinInfo
    {
        public class 会员卡信息
        {
            public string 会员卡索引 { get; set; }
            public string 会员编号 { get; set; }
            public string 会员卡号 { get; set; }
            public string 会员姓名 { get; set; }
            public string 开卡门店 { get; set; }
            public int 第一投币比例 { get; set; }
            public int 第一余额 { get; set; }
            public int 第二投币比例 { get; set; }
            public int 第二余额 { get; set; }
            public int 会员级别 { get; set; }
            public bool 允许投币 { get; set; }
            public bool 允许退币 { get; set; }
            public bool 专卡专用 { get; set; }
            public bool 锁会员 { get; set; }
            public int 会员状态 { get; set; }
        }


        public static void Init(string ICCardID = "")
        {
            //            try
            //            {
            //                string allSQL = @"select COUNT(Coins) as Coins,ICCardID from flw_485_coin where CoinType in (0,1,4) and RealTime>(select max(ShiftTime) as endtime from flw_schedule) GROUP BY ICCardID;
            //select COUNT(Coins) as Coins,ICCardID from flw_485_coin where CoinType in (2,3) and RealTime>(select max(ShiftTime) as endtime from flw_schedule) GROUP BY ICCardID;
            //SELECT COUNT(Coins) as Coins,ICCardID from flw_ticket_exit where RealTime>(select max(ShiftTime) as endtime from flw_schedule) GROUP BY ICCardID;";
            //                DataSet ds = null;

            //                string sql = "";
            //                if (ICCardID == "")
            //                {
            //                    会员卡列表 = new List<会员卡信息>();
            //                    sql = "SELECT m.ICCardID, m.Balance, m.Lottery, m.MemberLevelID, m.HasUpDownCoin, m.RepeatCode,'' as CommentText, m.`Lock`, l.AllowExitCoinToCard, l.LockHead from t_member m, t_memberlevel l where m.MemberLevelID=l.MemberLevelID";

            //                    ds = DataAccess.ExecuteQuery(allSQL);
            //                }
            //                else
            //                {
            //                    sql = string.Format("SELECT m.ICCardID, m.Balance, m.Lottery, m.MemberLevelID, m.HasUpDownCoin, m.RepeatCode,'' as CommentText, m.`Lock`, l.AllowExitCoinToCard, l.LockHead from t_member m, t_memberlevel l where m.MemberLevelID=l.MemberLevelID and m.ICCardID='{0}'", ICCardID);
            //                }

            //                DataTable dt = DataAccess.ExecuteQuery(sql).Tables[0];
            //                if (dt.Rows.Count > 0)
            //                {
            //                    foreach (DataRow row in dt.Rows)
            //                    {
            //                        会员卡信息 会员卡 = new 会员卡信息();
            //                        Type t = 会员卡.GetType();
            //                        int i = 0;
            //                        foreach (PropertyInfo pi in t.GetProperties())
            //                        {
            //                            if (row[dt.Columns[i]].GetType().Name != "DBNull")
            //                            {
            //                                if (pi.PropertyType.FullName.ToLower().IndexOf("boolean") >= 0)
            //                                {
            //                                    pi.SetValue(会员卡, Convert.ToBoolean(row[dt.Columns[i]]), null);
            //                                }
            //                                else
            //                                {
            //                                    if (dt.Columns[i].ColumnName == "ICCardID")
            //                                    {
            //                                        pi.SetValue(会员卡, row[dt.Columns[i]].ToString(), null);
            //                                    }
            //                                    else
            //                                    {
            //                                        pi.SetValue(会员卡, row[dt.Columns[i]], null);
            //                                    }
            //                                }
            //                            }
            //                            i++;
            //                        }

            //                        if (ICCardID == "")
            //                        {
            //                            var r = ds.Tables[0].Select(string.Format("ICCardID='{0}'", 会员卡.会员卡号));
            //                            if (r.Count() > 0)
            //                            {
            //                                会员卡.投币数 = Convert.ToInt32(r[0][0].ToString());
            //                            }
            //                            else
            //                            {
            //                                会员卡.投币数 = 0;
            //                            }
            //                            r = ds.Tables[1].Select(string.Format("ICCardID='{0}'", 会员卡.会员卡号));
            //                            if (r.Count() > 0)
            //                            {
            //                                会员卡.退币数 = Convert.ToInt32(r[0][0].ToString());
            //                            }
            //                            else
            //                            {
            //                                会员卡.退币数 = 0;
            //                            }
            //                            r = ds.Tables[2].Select(string.Format("ICCardID='{0}'", 会员卡.会员卡号));
            //                            if (r.Count() > 0)
            //                            {
            //                                会员卡.退币数 += Convert.ToInt32(r[0][0].ToString());
            //                            }
            //                        }
            //                        var list = 会员卡列表.Where(p => p.会员卡号 == 会员卡.会员卡号);
            //                        if (ICCardID == "")
            //                        {
            //                            if (list.Count() == 0)
            //                            {
            //                                会员卡列表.Add(会员卡);
            //                            }
            //                        }
            //                        else
            //                        {
            //                            if (list.Count() != 0)
            //                            {
            //                                会员卡信息 卡 = list.First();
            //                                卡.币余额 = 会员卡.币余额;
            //                                卡.动态密码 = 会员卡.动态密码;
            //                            }
            //                            else
            //                            {
            //                                会员卡列表.Add(会员卡);
            //                            }
            //                        }
            //                    }
            //                }
            //                else
            //                {
            //                    if (ICCardID != "")
            //                    {
            //                        var list = 会员卡列表.Where(p => p.会员卡号 == ICCardID);
            //                        if (list.Count() > 0)
            //                        {
            //                            foreach (会员卡信息 卡 in list)
            //                            {
            //                                会员卡列表.Remove(卡);
            //                            }
            //                        }
            //                    }
            //                }

            //                sql = "select * from t_parameters where system='txtMemberCoinMax'";
            //                dt = DataAccess.ExecuteQueryReturnTable(sql);
            //                if (dt.Rows.Count > 0)
            //                {
            //                    PubLib.会员余额上限 = Convert.ToInt32(dt.Rows[0]["ParameterValue"]);
            //                }
            //            }
            //            catch (Exception ex)
            //            {
            //                LogHelper.WriteLog(ex);
            //            }
        }

        ///// <summary>
        ///// 电子币投币检测
        ///// </summary>
        ///// <param name="ICCard">电子币号</param>
        ///// <param name="RepeadCode">动态密码</param>
        ///// <param name="rAddress">路由器段号</param>
        ///// <param name="hAddres">机头地址</param>
        //public static DataMember.电子币投币应答结构 电子币投币(UInt32 UID, string rAddress, string hAddres)
        //{
        //    int pCoin = 0;
        //    int gInCoin, gOutCoin, headCount;
        //    bool isAllowOut = false;
        //    DataMember.电子币投币应答结构 投币应答 = new DataMember.电子币投币应答结构();
        //    投币应答.机头能打票 = false;
        //    投币应答.机头能上分 = false;
        //    投币应答.锁机头 = false;
        //    投币应答.发脉冲数 = 0;
        //    投币应答.数字币编号 = UID.ToString();
        //    投币应答.扣币数 = 0;
        //    var head = HeadInfo.GetHeadInfo(rAddress, hAddres);
        //    if (head == null)
        //    {
        //        return null;
        //    }

        //    int balance = 0;
        //    DataTable dt = DataAccess.ExecuteQueryReturnTable(string.Format("select ICCardID, MemberLevelID,`Lock`,Balance from t_member where UID='{0}'", UID));
        //    if (dt.Rows.Count > 0)
        //    {
        //        DataRow row = dt.Rows[0];
        //        int tmpICCardID = 0;
        //        int.TryParse(row["ICCardID"].ToString(), out tmpICCardID);
        //        投币应答.数字币编号 = tmpICCardID.ToString();

        //        if (Convert.ToInt32(row["Lock"].ToString()) != 1)
        //        {
        //            pCoin = (head.开关.启用第二路上分信号) ? head.参数.第二路上分线投币时扣卡上币数 : head.参数.投币时扣卡上币数;
        //            if (pCoin == 0)
        //            {
        //                DataTable sd = DataAccess.ExecuteQueryReturnTable("select * from t_game where gameid='" + head.常规.游戏机编号 + "'");
        //                if (sd.Rows.Count > 0)
        //                {
        //                    head.参数.第二路上分线投币时扣卡上币数 = Convert.ToInt32(sd.Rows[0]["SecondReduceFromCard"]);
        //                    head.参数.投币时扣卡上币数 = Convert.ToInt32(sd.Rows[0]["PushReduceFromCard"]);
        //                    pCoin = (head.开关.启用第二路上分信号) ? head.参数.第二路上分线投币时扣卡上币数 : head.参数.投币时扣卡上币数;
        //                }
        //            }
        //            投币应答.币数 = Convert.ToInt32(row["Balance"].ToString());
        //            投币应答.机头能上分 = true;

        //            int 倍数 = 0;
        //            if (pCoin != 0)
        //                倍数 = 投币应答.币数 / pCoin;
        //            else
        //                LogHelper.WriteLog(string.Format("投币规则出错，投币数为0  UID={0}  HeadAddress={1} Segment={2} 卡号={3}", UID, hAddres, rAddress, 投币应答.数字币编号));

        //            if (倍数 > 0)
        //            {
        //                投币应答.扣币数 = 倍数 * pCoin;
        //                投币应答.发脉冲数 = (head.开关.启用第二路上分信号) ? 倍数 * head.参数.第二路上分线投币时给游戏机信号数 : 倍数 * head.参数.投币时给游戏机信号数;
        //                balance = 投币应答.币数 - 投币应答.扣币数;
        //                head.投币.投币数 += 投币应答.扣币数;
        //                UIClass.总投币数 += 投币应答.扣币数;

        //                DBUpdate.AddList(string.Format("update t_member set Balance='{0}' where ICCardID='{1}'", balance, 投币应答.数字币编号));
        //            }
        //        }

        //        //校验是否允许出票
        //        //if (isAllowOut)
        //        //{
        //        投币应答.机头能打票 = true;
        //        //}
        //        投币应答.锁机头 = head.状态.锁定机头;

        //        HeadInfo.GetCoinByGame(head.常规.游戏机编号, out gInCoin, out gOutCoin, out headCount);
        //        List<ServiceDll.TransmiteObject.机头状态结构> listStatus = new List<ServiceDll.TransmiteObject.机头状态结构>();
        //        ServiceDll.TransmiteObject.机头状态结构 h = new ServiceDll.TransmiteObject.机头状态结构();
        //        h.hAddress = hAddres;
        //        h.InCoins = head.投币.投币数;
        //        h.OutCoins = head.投币.退币数;
        //        h.WinCoins = head.投币.盈利数;
        //        h.IsInCoin = true;
        //        h.IsOnline = true;
        //        h.IsOutCoin = false;
        //        h.rAddress = rAddress;
        //        h.GameInCoins = gInCoin;
        //        h.GameOutCoins = gOutCoin;
        //        h.GameWinCoins = gInCoin - gOutCoin;
        //        listStatus.Add(h);
        //        ServiceDllCall.AddData(listStatus);
        //        //ServiceDll.ClientCall.机头状态汇报(listStatus.ToArray());
        //    }

        //    //添加投币流水账
        //    TableMemory.flw_485_coin.Table t = new TableMemory.flw_485_coin.Table();

        //    t.HeadAddress = hAddres;
        //    t.Balance = balance;
        //    t.Coins = 投币应答.扣币数;
        //    t.CoinType = "4";//数字币投币
        //    t.ICCardID = 投币应答.数字币编号;
        //    t.RealTime = DateTime.Now;
        //    t.Segment = rAddress;
        //    TableMemory.flw_485_coin.Add(t);

        //    return 投币应答;
        //}

        //public static DataMember.电子币出币应答结构 电子币出币(int coinCount, string rAddress, string hAddres, DateTime OPtime)
        //{
        //    DataMember.电子币出币应答结构 出币应答 = new DataMember.电子币出币应答结构();
        //    int pCoin = 0, bValue = 0;
        //    bool isAllowOut = false, isAllowIn = false, isAllowZKZY = false; ;

        //    出币应答.机头能打票 = false;
        //    出币应答.机头能上分 = false;
        //    出币应答.锁机头 = false;
        //    出币应答.条码 = 0;
        //    出币应答.动态密码 = 0;

        //    var head = HeadInfo.GetHeadInfo(PubLib.路由器段号, hAddres);
        //    if (head == null)
        //    {
        //        return null;
        //    }
        //    //判断机头是否锁定
        //    //判断机头是否允许上下分
        //    //判断机头是否锁定
        //    //判断机头在线状态

        //    if (head.常规.退币保护启用标志 && head.常规.退币信号超时退币个数 - 1 <= coinCount && head.常规.退币信号超时退币个数 + 1 >= coinCount)
        //    {
        //        //触发了退币保护盾功能
        //        if (GameInfo.TBProtect(head.常规.游戏机编号))
        //        {
        //            //已经达到报警次数,锁定机头
        //            Command.Ask.Ask机头锁定解锁指令 a = new Command.Ask.Ask机头锁定解锁指令(head.常规.机头地址, true);
        //        }
        //    }
        //    else
        //    {
        //        GameInfo.ClearTimeoutCount(head.常规.游戏机编号, head.常规.退币保护启用标志);
        //    }

        //    if (head.状态.是否正在使用限时送分优惠)
        //    {
        //        if (head.投币.最小退币数 == 0)
        //        {
        //            DataTable mind = DataAccess.ExecuteQueryReturnTable(string.Format("select r.ExitCoin from flw_game_free f,t_game_free_rule r where f.RuleID=r.ID and f.HeadID='{0}' ORDER BY RealTime DESC", head.常规.机头编号));
        //            if (mind.Rows.Count > 0)
        //            {
        //                head.投币.最小退币数 = Convert.ToInt32(mind.Rows[0][0].ToString());
        //            }
        //            //msg += "检查最小退币数\r\n";
        //        }
        //        if (head.投币.最小退币数 > coinCount)
        //        {
        //            //不满足退币条件
        //            PushRule.GetCurRule(head.常规.游戏机编号, head.常规.当前会员卡级别, out pCoin, out isAllowOut, out isAllowIn, out isAllowZKZY);
        //            int 倍数 = coinCount / pCoin;
        //            出币应答.发脉冲数 = (head.开关.启用第二路上分信号) ? 倍数 * head.参数.第二路上分线投币时给游戏机信号数 : 倍数 * head.参数.投币时给游戏机信号数;
        //            出币应答.是否将退币上回游戏机 = true;
        //            出币应答.是否正在使用限时送分优惠券 = true;
        //            //msg += "限时送分中不满足退币条件\r\n";
        //            return 出币应答;
        //        }
        //        else
        //        {
        //            ////满足退币条件
        //            //head.常规.当前卡片号 = "";
        //            //head.常规.是否为首次投币 = true;
        //            //head.投币.退币数 += coinCount;
        //            //head.状态.是否正在使用限时送分优惠 = false;
        //            //member.退币数 += Coins;
        //            //UIClass.总退币数 += Coins;
        //            //bValue += Coins;
        //            //投币应答.是否正在使用限时送分优惠券 = false;
        //            //投币应答.币余额 = bValue;
        //            //res = true;
        //            //msg += "限时送分中满足退币条件\r\n";
        //        }
        //    }
        //    head.常规.是否为首次投币 = true;
        //    出币应答.发脉冲数 = 0;
        //    出币应答.是否将退币上回游戏机 = false;
        //    出币应答.是否正在使用限时送分优惠券 = false;
        //    出币应答.机头能上分 = true;

        //    //校验是否允许出票
        //    head.投币.退币数 += coinCount;
        //    UIClass.总退币数 += coinCount;
        //    {
        //        int gInCoin, gOutCoin, headCount;
        //        出币应答.机头能打票 = true;
        //        Random r = new Random();
        //        出币应答.动态密码 = (byte)r.Next(0, 255);

        //        TableMemory.flw_ticket_exit.Table t = new TableMemory.flw_ticket_exit.Table();
        //        t.Segment = rAddress;
        //        t.HeadAddress = hAddres;
        //        t.Coins = coinCount;
        //        t.PWD = PubLib.Hex2String(出币应答.动态密码);
        //        t.RealTime = OPtime;
        //        t.ICCardID = head.常规.当前卡片号;
        //        出币应答.条码 = TableMemory.flw_ticket_exit.CreateBarCode(t);
        //        head.开关.退币超限标志 = false;
        //        出币应答.超额锁 = false;

        //        List<ServiceDll.TransmiteObject.机头状态结构> listStatus = new List<ServiceDll.TransmiteObject.机头状态结构>();
        //        ServiceDll.TransmiteObject.机头状态结构 h = new ServiceDll.TransmiteObject.机头状态结构();
        //        h.hAddress = hAddres;
        //        h.IsOutCoin = true;

        //        if (((head.投币.盈利数 < 0 - head.投币.每天净退币上限) || coinCount > head.投币.单次退币上限) && !head.状态.是否忽略超分报警)
        //        {
        //            出币应答.机头能打票 = true;
        //            TableMemory.flw_ticket_exit.LockBarCode(出币应答.条码);
        //            出币应答.超额锁 = true;
        //            head.开关.退币超限标志 = true;
        //            h.IsOutCoin = false;
        //            h.IsOver = true;

        //            string sql = string.Format("INSERT INTO flw_game_alarm (ICCardID,Segment,HeadAddress,AlertType,HappenTime,State,LockGame,LockMember,AlertContent) VALUES (0,'{0}','{1}','机头净退币超分',GETDATE(),0,1,0,'机头净退币超分')", PubLib.路由器段号, head.常规.机头地址);
        //            DBUpdate.AddList(sql);
        //        }

        //        HeadInfo.GetCoinByGame(head.常规.游戏机编号, out gInCoin, out gOutCoin, out headCount);

        //        h.InCoins = head.投币.投币数;
        //        h.OutCoins = head.投币.退币数;
        //        h.WinCoins = head.投币.盈利数;
        //        h.IsInCoin = false;
        //        h.IsOnline = true;
        //        h.GameInCoins = gInCoin;
        //        h.GameOutCoins = gOutCoin;
        //        h.GameWinCoins = gInCoin - gOutCoin;
        //        h.rAddress = rAddress;
        //        listStatus.Add(h);
        //        ServiceDllCall.AddData(listStatus);
        //        //ServiceDll.ClientCall.机头状态汇报(listStatus.ToArray());

        //        head.常规.当前卡片号 = "";
        //    }

        //    出币应答.锁机头 = head.状态.锁定机头;

        //    return 出币应答;
        //}

        public static bool 电子投币(Info.DeviceInfo.机头信息 head, int Coins, string ICCard, int RepeadCode, ref DataMember.IC卡模式进出币应答结构 投币应答, ref string msg, ref bool SkipWriteFlw, out int FreeCoin)
        {
            bool res = false;
            FreeCoin = 0;
            msg = "";
            SkipWriteFlw = true;
            Info.GameInfo.游戏机信息 game = XCCloudSerialNo.SerialNoHelper.StringGet<Info.GameInfo.游戏机信息>("gameinfo_" + head.游戏机索引号.ToString());
            if (game == null)
            {
                msg = "错误机台信息";
                return false;
            }
            会员卡信息 member = CoinInfo.GetCoinInfo(ICCard, RepeadCode, head, game);
            if (member == null)
            {
                msg = "会员数据无效";
                return false;
            }
            if (member.第一余额 >= Coins)
            {
                TableMemory.Flw_DeviceData data = new TableMemory.Flw_DeviceData();
                data.ID = XCCloudSerialNo.SerialNoHelper.CreateStoreSerialNo(PublicHelper.SystemDefiner.StoreID);
                data.MerchID = PublicHelper.SystemDefiner.MerchID;
                data.StoreID = PublicHelper.SystemDefiner.StoreID;
                data.DeviceID = head.设备编号;
                data.BusinessType = (int)CoinType.电子投币;
                data.RealTime = DateTime.Now;
                data.MemberID = member.会员编号;
                data.MemberName = member.会员姓名;
                data.ICCardID = member.会员卡号;
                data.Coin = Coins;
                data.OrderID = data.ID;
                data.Note = "投币 " + game.通用参数.游戏机名 + " | " + head.位置名称;
                data.CheckDate = Convert.ToDateTime(XCCloudSerialNo.SerialNoHelper.StringGet("营业日期"));

                投币应答.超出当日机头最大净退币上线 = false;
                投币应答.机头能上分 = member.允许投币;
                投币应答.机头能打票 = member.允许退币;
                投币应答.是否启用卡片专卡专用 = member.专卡专用;
                投币应答.是否将退币上回游戏机 = false;
                投币应答.锁机头 = false;

                Info.PushRule.游戏机投币规则 rule = new PushRule.游戏机投币规则();
                PushRule p = new PushRule();
                rule = p.获取游戏机规则(head.游戏机索引号, member.会员级别);

                int Multiple = 0;
                if (game.投币参数.启用上分通道)
                {
                    data.BalanceIndex = game.投币参数.按钮1余额类别;
                    Multiple = Coins / rule.扣值1数量;
                    投币应答.发脉冲数 = game.投币参数.按钮1上分脉冲数 * Multiple;
                    投币应答.币余额 = member.第一余额 - (game.投币参数.按钮1投币基数 * Multiple);
                }
                else
                {
                    data.BalanceIndex = game.投币参数.按钮1余额类别;
                    Multiple = Coins / rule.扣值1数量;
                    投币应答.发脉冲数 = game.投币参数.按钮1投币脉冲数 * Multiple;
                    投币应答.币余额 = member.第一余额 - (game.投币参数.按钮1投币基数 * Multiple);
                }
                data.RemainBalance = 投币应答.币余额;
                TableMemory.DataModel model = new TableMemory.DataModel();
                data.Verifiction = model.Verifiction(data);
                model.Add(data);

                会员余额扣除(member.会员卡索引, data.BalanceIndex, Coins);
                res = true;
            }
            else
            {
                res = false;
                msg = "余额不足";
            }
            return res;
        }
        static bool 执行退币数据操作(DeviceInfo.机头信息 head, UInt16 SN, int Coins, 会员卡信息 member, CoinType cType)
        {
            DataAccess ac = new DataAccess();
            string sql = "select * from Flw_DeviceData where DeviceID='" + head.设备编号 + "' and SN='" + SN + "' and DATEADD(mi,20,RealTime)>GETDATE()";
            DataTable dt = ac.ExecuteQueryReturnTable(sql);
            if (dt.Rows.Count > 0)
            {
                //重复数据，不处理
                return false;
            }


            return false;
        }
        static bool 电子退币SQL(DeviceInfo.机头信息 head, UInt16 SN, int Coins, string ICCard, int RepeadCode, CoinType cType, EndPoint p, ref DataMember.IC卡模式进出币应答结构 投币应答, ref string msg)
        {
            bool res = false;
            int pCoin = 0, bValue = 0;
            bool isAllowOut = false, isAllowIn = false, isAllowZKZY = false;
            bool 是否超分报警 = false;
            Info.GameInfo.游戏机信息 game = XCCloudSerialNo.SerialNoHelper.StringGet<Info.GameInfo.游戏机信息>("gameinfo_" + head.游戏机索引号.ToString());
            if (game == null)
            {
                msg = "错误机台信息";
                return false;
            }
            会员卡信息 member = GetCoinInfo(ICCard, RepeadCode, head, game);
            isAllowIn = member.允许投币;
            isAllowOut = member.允许退币;
            isAllowZKZY = member.专卡专用;
            if (member == null) return res;
            XCCloudSerialNo.SerialNoHelper.StringSet("headexitcoin_" + member.会员编号, DateTime.Now);

            if (head.彩票模式 && cType == CoinType.电子退币)
                cType = CoinType.IC退彩票;

            //启用退币保护，并且退币数误差在设定值正负1个范围内则触发事件
            if (head.退币保护.退币保护启用标志 && head.退币保护.退币信号超时退币个数 - 1 <= Coins && head.退币保护.退币信号超时退币个数 + 1 >= Coins)
            {
                msg += "触发退币保护功能\r\n";
                if (DeviceInfo.TBProtect(head))
                {
                    member.允许投币 = false;
                    //锁定卡头
                    Command.Ask.Ask机头锁定解锁指令 a = new Command.Ask.Ask机头锁定解锁指令(head, true, p);
                    //锁定会员卡
                    object o = new TableMemory.Data_Member_Card();
                    TableMemory.DataModel model = new TableMemory.DataModel();
                    model.CovertToDataModel("select * from Data_Member_Card where ID='" + member.会员卡索引 + "'", ref o);
                    TableMemory.Data_Member_Card c = o as TableMemory.Data_Member_Card;
                    c.IsLock = 1;
                    c.Verifiction = model.Verifiction(c);
                    model.Update(c, "where ID='" + member.会员卡索引 + "'");
                    //创建报警信息
                    TableMemory.Log_GameAlarm alerm = new TableMemory.Log_GameAlarm();
                    alerm.AlertContent = "触发退币保护盾机制";
                    alerm.AlertType = 9;
                    alerm.DeviceID = head.设备编号;
                    alerm.EndTime = null;
                    alerm.GameIndex = head.游戏机索引号;
                    alerm.HappenTime = DateTime.Now;
                    alerm.HeadAddress = head.机头短地址;
                    alerm.ICCardID = ICCard;
                    alerm.ID = XCCloudSerialNo.SerialNoHelper.CreateStoreSerialNo(PublicHelper.SystemDefiner.StoreID);
                    alerm.LockGame = 1;
                    alerm.LockMember = 1;
                    alerm.MerchID = PublicHelper.SystemDefiner.MerchID;
                    alerm.Segment = head.路由器段号;
                    alerm.SiteName = head.位置名称;
                    alerm.State = 0;
                    alerm.StoreID = PublicHelper.SystemDefiner.StoreID;
                    alerm.Verifiction = model.Verifiction(alerm);
                    model.Add(alerm);
                    return false;
                }
            }
            else
            {
                //正常退币
                head.退币保护.退币保护触发次数 = 0;
                TableMemory.Flw_DeviceData dd = new TableMemory.Flw_DeviceData();


            }

            head.当前卡片号 = "";
            head.是否为首次投币 = true;

            head.投币.退币数 += Coins;
            bValue += Coins;
            //if (!head.彩票模式)
            //{
            //    member.退币数 += Coins;
            //    UIClass.总退币数 += Coins;
            //}
            投币应答.锁机头 = false;
            投币应答.机头能上分 = isAllowIn;
            投币应答.机头能打票 = isAllowOut;
            msg += "正常退币数据\r\n";
            if ((0 - head.投币.盈利数 > head.投币.每天净退币上限 || Coins > head.投币.单次退币上限) && !head.状态.是否忽略超分报警 && !head.彩票模式)
            {
                投币应答.锁机头 = true;
                投币应答.超出当日机头最大净退币上线 = true;
                msg += "触发超额报警\r\n";
                是否超分报警 = true;
            }
            else
            {
                投币应答.锁机头 = false;
                投币应答.超出当日机头最大净退币上线 = false;
            }
            投币应答.币余额 = bValue;
            //if (head.订单编号 != "")
            //{
            //    FrmMain.GetInterface.ControlFinish(XCSocketService.ActionEnum.退币, head.订单编号, "成功", Coins.ToString());
            //    head.订单编号 = "";
            //}
            return true;
        }

        //    sqlString = string.Format("exec TBProc '{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}'",
        //            PubLib.路由器段号,
        //            head.常规.机头地址,
        //            SN,
        //            Coins,
        //            ICCard,
        //            (int)cType,
        //            ((bValue > PubLib.会员余额上限 || member.退币数 - member.投币数 > PubLib.会员净退币数上限) && head.彩票模式) ? 1 : 0,
        //            0,
        //            是否超分报警 ? 1 : 0,
        //            res ? 1 : 0,
        //            head.彩票模式 ? 1 : 0);

        //    DataTable dt = sql.ExecuteQueryReturnTable(sqlString);
        //    DataRow row = dt.Rows[0];
        //    //有效电子投币
        //    if (head.彩票模式)
        //        bValue = Convert.ToInt32(row["lottery"].ToString());
        //    else
        //        bValue = Convert.ToInt32(row["balance"].ToString());
        //    投币应答.币余额 = bValue;
        //    投币应答.是否启用卡片专卡专用 = (row["LockHead"].ToString() == "1");
        //    投币应答.是否正在使用限时送分优惠券 = head.状态.是否正在使用限时送分优惠;
        //    msg += "找到会员信息\r\n";

        //    if (head.彩票模式)
        //    {
        //        member.锁会员 = false;
        //        member.票余额 = bValue;
        //    }
        //    else
        //    {
        //        member.币余额 = bValue;
        //        if (bValue > PubLib.会员余额上限 || member.退币数 - member.投币数 > PubLib.会员净退币数上限)
        //        {
        //            member.锁会员 = true;
        //            msg += "写入新余额并锁定会员\r\n";
        //        }
        //        else
        //        {
        //            member.锁会员 = false;
        //            msg += "写入新余额\r\n";
        //        }
        //    }

        //    return res;
        //}

        //        static bool 电子退币(HeadInfo.机头信息 head, int Coins, string ICCard, int RepeadCode, ref DataMember.IC卡模式进出币应答结构 投币应答, ref string msg)
        //        {
        //            bool res = false;
        //            int pCoin = 0, bValue = 0;
        //            bool isAllowOut = false, isAllowIn = false, isAllowZKZY = false; ;
        //            会员卡信息 member = GetCoinInfo(ICCard);
        //            if (member == null) return res;
        //            member.退币时间 = DateTime.Now;

        //            //启用退币保护，并且退币数误差在设定值正负1个范围内则触发事件
        //            if (head.常规.退币保护启用标志 && head.常规.退币信号超时退币个数 - 1 <= Coins && head.常规.退币信号超时退币个数 + 1 >= Coins)
        //            {
        //                msg += "触发退币保护功能\r\n";
        //                if (GameInfo.TBProtect(head.常规.游戏机编号))
        //                {
        //                    string sql = string.Format("UPDATE t_member SET HasUpDownCoin=0 WHERE ICCardID='{2}';INSERT INTO flw_game_alarm (ICCardID,Segment,HeadAddress,AlertType,HappenTime,State,LockGame,LockMember,AlertContent) VALUES ('{2}','{0}','{1}','退分保护机头锁定',GETDATE(),0,0,0,'会员卡号：{2}');",
        //                        PubLib.路由器段号, head.常规.机头地址, ICCard);
        //                    DataAccess.Execute(sql);
        //                    member.机头上下分 = false;
        //                    Command.Ask.Ask机头锁定解锁指令 a = new Command.Ask.Ask机头锁定解锁指令(head.常规.机头地址, true);
        //                    return false;
        //                }
        //            }
        //            else
        //            {
        //                GameInfo.ClearTimeoutCount(head.常规.游戏机编号, head.常规.退币保护启用标志);
        //            }

        //            DataTable dt = DataAccess.ExecuteQueryReturnTable(string.Format("SELECT m.ICCardID, m.MemberLevelID, m.`Lock`, m.Balance, m.Lottery, m.RepeatCode, l.AllowExitCoinToCard, l.LockHead from t_member m, t_memberlevel l where m.MemberLevelID=l.MemberLevelID and m.ICCardID='{0}'", ICCard, RepeadCode));
        //            if (dt.Rows.Count > 0)
        //            {
        //                //有效电子投币
        //                DataRow row = dt.Rows[0];
        //                //bValue = Convert.ToInt32(row["Balance"].ToString());
        //                if (head.彩票模式)
        //                    bValue = member.票余额;
        //                else
        //                    bValue = member.币余额;
        //                投币应答.币余额 = bValue;
        //                投币应答.是否启用卡片专卡专用 = (row["LockHead"].ToString() == "1");
        //                投币应答.是否正在使用限时送分优惠券 = head.状态.是否正在使用限时送分优惠;
        //                msg += "找到会员信息\r\n";
        //                if (!head.状态.是否正在使用限时送分优惠)  //正常退币
        //                {
        //                    head.常规.当前卡片号 = "";
        //                    head.常规.是否为首次投币 = true;
        //                    PushRule.GetCurRule(head.常规.游戏机编号, Convert.ToInt32(row["MemberLevelID"].ToString()), out pCoin, out isAllowOut, out isAllowIn, out isAllowZKZY);
        //                    head.投币.退币数 += Coins;
        //                    bValue += Coins;
        //                    if (!head.彩票模式)
        //                    {
        //                        member.退币数 += Coins;
        //                        UIClass.总退币数 += Coins;
        //                    }
        //                    投币应答.锁机头 = false;
        //                    投币应答.机头能上分 = isAllowIn;
        //                    投币应答.机头能打票 = isAllowOut;
        //                    msg += "正常退币数据\r\n";
        //                    if ((0 - head.投币.盈利数 > head.投币.每天净退币上限 || Coins > head.投币.单次退币上限) && !head.状态.是否忽略超分报警 && !head.彩票模式)
        //                    {
        //                        投币应答.锁机头 = true;
        //                        投币应答.超出当日机头最大净退币上线 = true;
        //                        msg += "触发超额报警\r\n";
        //                        string sql = string.Format("INSERT INTO flw_game_alarm (ICCardID,Segment,HeadAddress,AlertType,HappenTime,State,LockGame,LockMember,AlertContent) VALUES (0,'{0}','{1}','机头净退币超分',GETDATE(),0,1,0,'机头净退币超分')", PubLib.路由器段号, head.常规.机头地址);
        //                        DBUpdate.AddList(sql);
        //                    }
        //                    else
        //                    {
        //                        投币应答.锁机头 = false;
        //                        投币应答.超出当日机头最大净退币上线 = false;
        //                    }
        //                    投币应答.币余额 = bValue;
        //                    res = true;
        //                }
        //                else   //当前机头正在使用限时送分，则要判断最小退币数
        //                {
        //                    if (head.投币.最小退币数 == 0)
        //                    {
        //                        DataTable mind = DataAccess.ExecuteQueryReturnTable(string.Format("select r.ExitCoin from flw_game_free f,t_game_free_rule r where f.RuleID=r.ID and f.HeadID='{0}' ORDER BY RealTime DESC", head.常规.机头编号));
        //                        if (mind.Rows.Count > 0)
        //                        {
        //                            head.投币.最小退币数 = Convert.ToInt32(mind.Rows[0][0].ToString());
        //                        }
        //                        msg += "检查最小退币数\r\n";
        //                    }
        //                    if (head.投币.最小退币数 > Coins)
        //                    {
        //                        //不满足退币条件
        //                        PushRule.GetCurRule(head.常规.游戏机编号, Convert.ToInt32(row["MemberLevelID"].ToString()), out pCoin, out isAllowOut, out isAllowIn, out isAllowZKZY);
        //                        投币应答.机头能上分 = isAllowIn;
        //                        投币应答.机头能打票 = isAllowOut;
        //                        int 倍数 = Coins / pCoin;
        //                        投币应答.发脉冲数 = (head.开关.启用第二路上分信号) ? 倍数 * head.参数.第二路上分线投币时给游戏机信号数 : 倍数 * head.参数.投币时给游戏机信号数;
        //                        投币应答.是否将退币上回游戏机 = true;
        //                        msg += "限时送分中不满足退币条件\r\n";
        //                    }
        //                    else
        //                    {
        //                        //满足退币条件
        //                        head.常规.当前卡片号 = "";
        //                        head.常规.是否为首次投币 = true;
        //                        head.投币.退币数 += Coins;
        //                        head.状态.是否正在使用限时送分优惠 = false;
        //                        member.退币数 += Coins;
        //                        UIClass.总退币数 += Coins;
        //                        bValue += Coins;
        //                        投币应答.是否正在使用限时送分优惠券 = false;
        //                        投币应答.币余额 = bValue;
        //                        res = true;
        //                        msg += "限时送分中满足退币条件\r\n";
        //                    }
        //                }
        //                if (head.彩票模式)
        //                {
        //                    member.锁会员 = false;
        //                    member.票余额 = bValue;
        //                    DBUpdate.AddList(string.Format("update t_member set Lottery={0} where ICCardID='{1}';", bValue, ICCard));
        //                }
        //                else
        //                {
        //                    member.币余额 = bValue;
        //                    if (bValue > PubLib.会员余额上限 || member.退币数 - member.投币数 > PubLib.会员净退币数上限)
        //                    {
        //                        member.锁会员 = true;
        //                        DBUpdate.AddList(string.Format("update t_member set Balance={0},`Lock`=1 where ICCardID='{1}';", bValue, ICCard));
        //                        msg += "写入新余额并锁定会员\r\n";
        //                    }
        //                    else
        //                    {
        //                        member.锁会员 = false;
        //                        DBUpdate.AddList(string.Format("update t_member set Balance={0} where ICCardID='{1}';", bValue, ICCard));
        //                        msg += "写入新余额\r\n";
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                msg += "未找到会员信息\r\n";
        //            }
        //            return res;
        //        }

        //        static bool 电子存提币(HeadInfo.机头信息 head, int Coins, CoinType cType, string ICCard, ref DataMember.IC卡模式进出币应答结构 投币应答)
        //        {
        //            DataTable dt = DataAccess.ExecuteQueryReturnTable(string.Format("SELECT m.ICCardID, m.MemberLevelID, m.`Lock`, m.Balance, m.RepeatCode, l.AllowExitCoinToCard, l.LockHead from t_member m, t_memberlevel l where m.MemberLevelID=l.MemberLevelID and m.ICCardID='{0}'", ICCard));
        //            if (dt.Rows.Count > 0)
        //            {
        //                DataRow row = dt.Rows[0];
        //                int bValue = Convert.ToInt32(row["Balance"].ToString());

        //                if (cType == CoinType.电子存币)
        //                {
        //                    int 扣币数 = (int)(Coins / head.存币机.设备数币计数) * head.存币机.卡上增加币数;
        //                    bValue += 扣币数;
        //                    head.投币.投币数 += 扣币数;
        //                    UIClass.总投币数 += 扣币数;
        //                    if (扣币数 > 0)
        //                        DBUpdate.AddList(string.Format("INSERT INTO flw_485_savecoin (Segment,HeadAddress,ICCardID,Coins,Balance,RealTime) values ('{0}','{1}','{2}','{3}','{4}',GETDATE());",
        //                            head.常规.路由器编号, head.常规.机头地址, ICCard, 扣币数, bValue));
        //                }
        //                else if (cType == CoinType.会员卡提币)
        //                {
        //                    if (bValue < Coins) return false;

        //                    投币应答.发脉冲数 = (int)(Coins / head.存币机.卡上增加币数) * head.存币机.设备数币计数;
        //                    bValue -= Coins;
        //                    head.投币.退币数 += Coins;
        //                    UIClass.总退币数 += Coins;

        //                    DBUpdate.AddList(string.Format("INSERT INTO flw_coin_sale (ICCardID,WorkStation,Coins,Balance,WorkType,MacAddress,DiskID,RealTime,ScheduleID,UserID,IsBirthday,AuthorID) values ('{0}','提币机','{1}','{2}','6','{3}','{3}',GETDATE(),'0','0','0','0');", ICCard, Coins, bValue, head.常规.机头长地址));
        //                }

        //                投币应答.币余额 = bValue;
        //                DBUpdate.AddList(string.Format("update t_member set Balance={0} where ICCardID='{1}';", bValue, ICCard));
        //                return true;
        //            }
        //            return false;
        //        }

        //        static bool 电子碎票(HeadInfo.机头信息 head, int Coins, CoinType cType, string ICCard, ref DataMember.IC卡模式进出币应答结构 投币应答)
        //        {
        //            DataTable dt = DataAccess.ExecuteQueryReturnTable(string.Format("SELECT m.ICCardID, m.MemberLevelID, m.`Lock`, m.Lottery, l.AllowExitCoinToCard, l.LockHead from t_member m, t_memberlevel l where m.MemberLevelID=l.MemberLevelID and m.ICCardID='{0}'", ICCard));
        //            if (dt.Rows.Count > 0)
        //            {
        //                DataRow row = dt.Rows[0];
        //                int bValue = Convert.ToInt32(row["Lottery"].ToString());

        //                //int 扣币数 = (int)(Coins / head.存币机.设备数币计数) * head.存币机.卡上增加币数;
        //                //bValue += 扣币数;

        //                bValue += Coins;

        //                投币应答.币余额 = bValue;

        //                DBUpdate.AddList(string.Format("INSERT INTO flw_lottery (WorkType,GameID,HeadID,LotteryCount,ICCardID,RealTime,State) values ('2','{0}','{1}','{2}','{3}',GETDATE(),1);", head.常规.游戏机编号, head.常规.机头编号, Coins, ICCard));
        //                DBUpdate.AddList(string.Format("update t_member set Lottery={0} where ICCardID='{1}';", bValue, ICCard));
        //                DBUpdate.AddList(string.Format(@"declare @iccardid varchar(10)
        //declare @balance int
        //declare @lottery int
        //declare @charge int
        //declare @remain int
        //declare @workstation varchar(50)
        //declare @scheduleid int
        //declare @userid int
        //declare @coinscale int
        //declare @lotteryscale int
        //declare @n int
        //
        //set @iccardid='{0}'
        //select Lottery,Balance from t_member where ICCardID=@iccardid
        //
        //if(not exists(select * from t_parameters where system='coinChargeScale' and IsAllow='1'))
        //begin
        //	return
        //end
        //if(not exists(select * from t_parameters where system='lotteryChargeScale' and IsAllow='1'))
        //begin
        //	return
        //end
        //select @coinscale=ParameterValue from t_parameters where system='coinChargeScale' and IsAllow='1'
        //select @lotteryscale=ParameterValue from t_parameters where system='lotteryChargeScale' and IsAllow='1'
        //
        //select @workstation=WorkStation,@scheduleid=ID,@userid=UserID from flw_schedule where State=0 and MacAddress<>'' order by OpenTime desc
        //select @lottery=Lottery,@balance=Balance from t_member where ICCardID=@iccardid
        //set @n=@lottery / @lotteryscale
        //set @charge=@n * @coinscale
        //set @remain=@lottery - @n * @lotteryscale
        //update t_member set Balance=Balance+@charge,Lottery=@remain where ICCardID=@iccardid
        //insert into flw_coin_sale (ICCardID,WorkStation,Coins,Balance,WorkType,ScheduleID,RealTime,UserID,IsBirthday,MacAddress,DiskID,AuthorID)
        //	values (@iccardid,@workstation,@charge,@balance+@charge,'4',@scheduleid,GETDATE(),@userid,'0',@workstation,@workstation,'0')", ICCard));
        //                return true;
        //            }
        //            return false;
        //        }

        //        static bool 其他进出币(HeadInfo.机头信息 head, int Coins, CoinType cType)
        //        {
        //            if (cType == CoinType.实物投币 || cType == CoinType.电子存币)
        //            {
        //                head.投币.投币数 += Coins;
        //                UIClass.总投币数 += Coins;
        //                return true;
        //            }
        //            else if (cType == CoinType.实物退币)
        //            {
        //                head.投币.退币数 += Coins;
        //                UIClass.总退币数 += Coins;
        //                return true;
        //            }
        //            else if (cType == CoinType.实物彩票)
        //                return true;
        //            else if (cType == CoinType.远程存币通知)
        //            {
        //                head.状态.出币机或存币机正在数币 = false;
        //                FrmMain.GetInterface.ControlFinish(XCSocketService.ActionEnum.存币, head.订单编号, "成功", Coins.ToString());
        //                return true;
        //            }
        //            return false;
        //        }

        public static DataMember.IC卡模式进出币应答结构 刷卡投币(string ICCard, int Coins, UInt16 SN, CoinType cType, int RepeadCode, Info.DeviceInfo.机头信息 head, EndPoint p, ref string msg, bool isPush, string PushAddr, out int FreeCoin)
        {
            FreeCoin = 0;
            int gInCoin, gOutCoin, headCount;
            bool skip = false;
            msg = "";
            DataMember.IC卡模式进出币应答结构 投币应答 = new DataMember.IC卡模式进出币应答结构();
            投币应答.机头能打票 = false;
            投币应答.机头能上分 = false;
            投币应答.锁机头 = false;
            try
            {

                string type = "";
                if (PublicHelper.SystemDefiner.ProxyType == 1)
                {
                    HKApi api = new HKApi();

                    if (cType == CoinType.电子退币 || cType == CoinType.实物退币)
                        type = "1";
                    else if (cType == CoinType.IC退彩票)
                        type = "52";
                    else if (cType == CoinType.礼品掉落)
                        type = "51";
                    if (type != "")
                        api.DevicePrize(head.订单编号, type, Coins.ToString());
                }

                //LogHelper.WriteLog("测试调用：   客户类别=" + Info.SecrityHeadInfo.客户类别.ToString() + "  cType=" + cType.ToString() + "   type=" + type);

                switch (cType)
                {
                    case CoinType.电子投币:
                        if (!电子投币(head, Coins, ICCard, RepeadCode, ref 投币应答, ref msg, ref skip, out FreeCoin))
                        {
                            return 投币应答;
                        }
                        //else
                        //{
                        //    Info.BoradPush.BoradItem item = new BoradPush.BoradItem()
                        //    {
                        //        ICCard = ICCard,
                        //        HeadID = head.常规.机头编号,
                        //        Coins = Coins,
                        //        SerialNum = SN
                        //    };
                        //    Info.BoradPush.SendBoard(item);
                        //}
                        break;
                    case CoinType.电子退币:
                        if (!电子退币SQL(head, SN, Coins, ICCard, RepeadCode, cType, p, ref 投币应答, ref msg))
                        {
                            return 投币应答;
                        }
                        skip = true;
                        break;
                    case CoinType.电子存币:
                    case CoinType.会员卡提币:
                        //if (!电子存提币(head, Coins, cType, ICCard, ref 投币应答))
                        //{
                        //    return 投币应答;
                        //}
                        break;
                    case CoinType.电子碎票:
                        //if (!电子碎票(head, Coins, cType, ICCard, ref 投币应答))
                        //{
                        //    return 投币应答;
                        //}
                        break;
                    case CoinType.远程出币通知:
                        {
                            //投币应答.机头能打票 = true;
                            //投币应答.机头能上分 = true;
                            //skip = true;
                            //FrmMain.GetInterface.ControlFinish(XCSocketService.ActionEnum.出币, head.订单编号, "成功", Coins.ToString());
                            //head.状态.出币机或存币机正在数币 = false;
                        }
                        break;
                    default:
                        //if (!其他进出币(head, Coins, cType))
                        //{
                        //    return 投币应答;
                        //}
                        //投币应答.机头能打票 = true;
                        //投币应答.机头能上分 = true;
                        break;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return 投币应答;
        }

        public static bool 液晶卡头查询(string ICCardID, Info.DeviceInfo.机头信息 head, int RepeadCode, out int Coin1, out int Balance1, out int Coin2, out int Balance2, out bool isAllowIn, out bool isAllowOut, out bool isAllowZKZY, out byte cardType, out string ticketName, out int remainCount, out DateTime endDate)
        {
            Coin1 = 0;
            Balance1 = 0;
            Coin2 = 0;
            Balance2 = 0;
            isAllowIn = false;
            isAllowOut = false;
            isAllowZKZY = false;
            cardType = 0x00;
            ticketName = "";
            remainCount = 0;
            endDate = DateTime.Now;

            Info.GameInfo.游戏机信息 game = XCCloudSerialNo.SerialNoHelper.StringGet<Info.GameInfo.游戏机信息>("gameinfo_" + head.游戏机索引号.ToString());
            会员卡信息 card = GetCoinInfo(ICCardID, RepeadCode, head, game);
            if (card != null)
            {
                if (card.会员状态 == 1 && !card.锁会员)
                    cardType = 0x01;
                Info.ProjectTicketInfo.门票信息 ticket = ProjectTicketInfo.GetProjectTicket(head.设备编号, card.会员卡索引);

                if (ticket != null)
                {
                    isAllowIn = card.允许投币;
                    isAllowOut = false;
                    isAllowZKZY = false;
                    Coin1 = card.第一投币比例;
                    Balance1 = card.第一余额;
                    Coin2 = card.第二投币比例;
                    Balance2 = card.第二余额;
                    ticketName = ticket.TicketName;
                    remainCount = ticket.RemainCount;
                    if (ticket.票.FirstUseTime == null)
                        endDate = ticket.票.EndTime;
                    else
                        endDate = ((DateTime)ticket.票.FirstUseTime).AddDays(ticket.票.WriteOffDays);
                    XCCloudSerialNo.SerialNoHelper.StringSet<Info.ProjectTicketInfo.门票信息>("ticketin_" + head.设备序列号, ticket);
                }
                else
                {
                    isAllowIn = card.允许投币;
                    isAllowOut = card.允许退币;
                    isAllowZKZY = card.专卡专用;
                    Coin1 = card.第一投币比例;
                    Balance1 = card.第一余额;
                    Coin2 = card.第二投币比例;
                    Balance2 = card.第二余额;
                    XCCloudSerialNo.SerialNoHelper.StringSet<Info.ProjectTicketInfo.门票信息>("ticketin_" + head.设备序列号, null);
                }
                return true;
            }
            XCCloudSerialNo.SerialNoHelper.StringSet<Info.ProjectTicketInfo.门票信息>("ticketin_" + head.设备序列号, null);
            return false;
        }

        public static DataMember.液晶卡头进出币应答结构 液晶卡头进出币(Info.DeviceInfo.机头信息 head, string ICCard, int Coins, byte UseType, UInt16 SN, CoinType cType, int RepeadCode, ref string msg, bool isPush, string PushAddr, out int FreeCoin)
        {
            FreeCoin = 0;
            DataMember.液晶卡头进出币应答结构 应答结构 = new DataMember.液晶卡头进出币应答结构();
            bool SkipWriteFlw = false;
            switch (cType)
            {
                case CoinType.电子投币:
                    if (!液晶卡头投币(head, Coins, UseType, cType, ICCard, RepeadCode, ref 应答结构, ref msg, ref SkipWriteFlw, out FreeCoin))
                    {
                        return 应答结构;
                    }
                    break;
            }

            return 应答结构;
        }
        static void 会员余额扣除(string cardIndex, int balanceIndex, decimal balance)
        {
            TableMemory.DataModel model = new TableMemory.DataModel();
            object o = new TableMemory.Data_Card_Balance();
            model.CovertToDataModel("select * from Data_Card_Balance where CardIndex='" + cardIndex + "' and BalanceIndex='" + balanceIndex + "'", ref o);
            TableMemory.Data_Card_Balance b = o as TableMemory.Data_Card_Balance;
            o = new TableMemory.Data_Card_Balance_Free();
            model.CovertToDataModel("select * from Data_Card_Balance_Free where CardIndex='" + cardIndex + "' and BalanceIndex='" + balanceIndex + "'", ref o);
            TableMemory.Data_Card_Balance_Free f = o as TableMemory.Data_Card_Balance_Free;
            o = new TableMemory.Dict_BalanceType();
            model.CovertToDataModel("select * from Dict_BalanceType where ID='" + balanceIndex + "'", ref o);
            TableMemory.Dict_BalanceType bt = o as TableMemory.Dict_BalanceType;
            decimal n1 = balance * b.Balance / (b.Balance + f.Balance);
            decimal n2 = balance * f.Balance / (b.Balance + f.Balance);
            if (bt.AddingType == 0)
            {
                //小数不保留
                n1 = Convert.ToDecimal((int)n1);
                n2 = Convert.ToDecimal(balance - (int)n1);
            }
            else if (bt.AddingType == 1)
            {
                //保留全部，只要有小数就进位
                if (((int)(n1 * 100)) % 100 > 0)
                    n1 = Convert.ToDecimal(((int)n1) + 1);
                n2 = Convert.ToDecimal(balance - (int)n1);
            }
            else
            {
                n1 = Decimal.Round(n1, bt.DecimalNumber);
                n2 = Decimal.Round(n2, bt.DecimalNumber);
            }
            b.Balance = b.Balance - n1;
            b.Verifiction = model.Verifiction(b);
            model.Update(b, "where ID='" + b.ID + "'");
            f.Balance = f.Balance - n2;
            f.Verifiction = model.Verifiction(f);
            model.Update(f, "where ID='" + f.ID + "'");
        }
        public static bool 液晶卡头投币(Info.DeviceInfo.机头信息 head, int Coins, byte UseType, CoinType cType, string ICCard, int RepeadCode, ref DataMember.液晶卡头进出币应答结构 投币应答, ref string msg, ref bool SkipWriteFlw, out int FreeCoin)
        {
            FreeCoin = 0;
            msg = "";
            SkipWriteFlw = true;

            Info.GameInfo.游戏机信息 game = XCCloudSerialNo.SerialNoHelper.StringGet<Info.GameInfo.游戏机信息>("gameinfo_" + head.游戏机索引号.ToString());
            if (game == null)
            {
                msg = "错误机台信息";
                return false;
            }
            会员卡信息 member = GetCoinInfo(ICCard, RepeadCode, head, game);
            if (member == null)
            {
                msg = "未找到有效会员";
                return false;
            }
            if (门票消耗(head, member, game, Coins, UseType, ref 投币应答)) return true;
            //扣除余额
            if ((member.第一余额 >= Coins && UseType == 1) || (member.第二余额 >= Coins && UseType == 2))
            {
                TableMemory.Flw_DeviceData data = new TableMemory.Flw_DeviceData();
                data.ID = XCCloudSerialNo.SerialNoHelper.CreateStoreSerialNo(PublicHelper.SystemDefiner.StoreID);
                data.MerchID = PublicHelper.SystemDefiner.MerchID;
                data.StoreID = PublicHelper.SystemDefiner.StoreID;
                data.DeviceID = head.设备编号;
                data.BusinessType = (int)cType;
                data.RealTime = DateTime.Now;
                data.MemberID = member.会员编号;
                data.MemberName = member.会员姓名;
                data.ICCardID = member.会员卡号;
                data.Coin = Coins;
                data.OrderID = data.ID;
                data.Note = "投币 " + game.通用参数.游戏机名 + " | " + head.位置名称;
                data.CheckDate = Convert.ToDateTime(XCCloudSerialNo.SerialNoHelper.StringGet("营业日期"));

                投币应答.超出当日机头最大净退币上线 = false;
                投币应答.机头能上分 = member.允许投币;
                投币应答.是否将退币上回游戏机 = member.允许退币;
                投币应答.是否启用卡片专卡专用 = member.专卡专用;
                投币应答.送币数 = 0;
                投币应答.锁机头 = false;

                Info.PushRule.游戏机投币规则 rule = new PushRule.游戏机投币规则();
                PushRule p = new PushRule();
                rule = p.获取游戏机规则(head.游戏机索引号, member.会员级别);

                int Multiple = 0;
                if (game.投币参数.启用上分通道)
                {
                    if (UseType == 1)
                    {
                        data.BalanceIndex = game.投币参数.按钮1余额类别;
                        Multiple = Coins / rule.扣值1数量;
                        投币应答.发脉冲数 = game.投币参数.按钮1上分脉冲数 * Multiple;
                        投币应答.币余额 = member.第一余额 - (rule.扣值1数量 * Multiple);
                    }
                    else
                    {
                        data.BalanceIndex = game.投币参数.按钮2余额类别;
                        Multiple = Coins / rule.扣值2数量;
                        投币应答.发脉冲数 = game.投币参数.按钮2上分脉冲数 * Multiple;
                        投币应答.币余额 = member.第二余额 - (rule.扣值2数量 * Multiple);
                    }
                }
                else
                {
                    if (UseType == 1)
                    {
                        data.BalanceIndex = game.投币参数.按钮1余额类别;
                        Multiple = Coins / rule.扣值1数量;
                        投币应答.发脉冲数 = game.投币参数.按钮1投币脉冲数 * Multiple;
                        投币应答.币余额 = member.第一余额 - (rule.扣值1数量 * Multiple);
                    }
                    else
                    {
                        data.BalanceIndex = game.投币参数.按钮2余额类别;
                        Multiple = Coins / rule.扣值2数量;
                        投币应答.发脉冲数 = game.投币参数.按钮2投币脉冲数 * Multiple;
                        投币应答.币余额 = member.第二余额 - (rule.扣值2数量 * Multiple);
                    }
                }
                data.RemainBalance = 投币应答.币余额;
                TableMemory.DataModel model = new TableMemory.DataModel();
                data.Verifiction = model.Verifiction(data);
                model.Add(data);

                会员余额扣除(member.会员卡索引, data.BalanceIndex, Coins);

                return true;
            }
            else
            {
                //余额不足
                msg = "余额不足";
                return false;
            }
        }
        /// <summary>
        /// 门票消耗规则，期限票——团体票——次票
        /// 同级别下按过期时间优先使用
        /// </summary>
        /// <param name="head"></param>
        /// <param name="member"></param>
        /// <param name="game"></param>
        /// <param name="Coins"></param>
        /// <param name="UseType"></param>
        /// <param name="投币应答"></param>
        /// <returns></returns>
        public static bool 门票消耗(Info.DeviceInfo.机头信息 head, 会员卡信息 member, GameInfo.游戏机信息 game, int Coins, byte UseType, ref DataMember.液晶卡头进出币应答结构 投币应答)
        {
            if (game.游戏机消费类别 == 2)
            {
                if (head.验票进出方向 == 0)
                {
                    //是否需要入闸判断
                    if (ProjectTicketInfo.门票验票入闸顺序校验(head))
                    {
                        Info.ProjectTicketInfo.门票信息 ticket = XCCloudSerialNo.SerialNoHelper.StringGet<Info.ProjectTicketInfo.门票信息>("ticketin_" + head.设备序列号);
                        if (ticket == null) return false;   //门票缓存未找到，刷卡时校验的缓存
                        //判断是否存在需要二次入闸的门票
                        string code = "";
                        string id = "";
                        if (ProjectTicketInfo.判断是否需要二次入场(ticket.AllowExitTimes, ticket.ProjectCode, head.游乐项目索引, out id))
                        {
                            //存在需要二次进场的票
                            return 游乐项目二次进场(code, id, ref 投币应答);
                        }
                        else
                        {
                            //不存需要二次进场的票，则判断是否有门票正在使用
                            return 游乐项目正常入闸(head, member, game, ticket.BindID, ref 投币应答);
                        }
                    }
                }
            }
            return false;
        }
        static bool 游乐项目二次进场(string projectCode, string flwID, ref DataMember.液晶卡头进出币应答结构 投币应答)
        {
            TableMemory.DataModel model = new TableMemory.DataModel();
            object o = new TableMemory.Flw_Project_TicketUse();
            if (model.CovertToDataModel("select * from Flw_Project_TicketUse where ID='" + flwID + "'", ref o))
            {
                TableMemory.Flw_Project_TicketUse ticket = o as TableMemory.Flw_Project_TicketUse;
                ticket.OutMinuteTotal += (int)(DateTime.Now - (DateTime)ticket.OutTime).TotalMinutes;
                ticket.OutTime = null;
                ticket.Verifiction = model.Verifiction(ticket);
                model.Update(ticket, "where ID='" + flwID + "'");

                投币应答.超出当日机头最大净退币上线 = false;
                投币应答.机头能打票 = false;
                投币应答.机头能上分 = true;
                投币应答.是否将退币上回游戏机 = false;
                投币应答.是否启用卡片专卡专用 = false;
                投币应答.是否正在使用限时送分优惠券 = false;
                投币应答.送币数 = 0;
                投币应答.锁机头 = false;

                o = new TableMemory.Flw_Project_TicketInfo();
                if (model.CovertToDataModel("select * from Flw_Project_TicketInfo where Barcode='" + projectCode + "'", ref o))
                {
                    TableMemory.Flw_Project_TicketInfo t = o as TableMemory.Flw_Project_TicketInfo;
                    switch (t.TicketType)
                    {
                        case 0: //次票
                            投币应答.币余额 = 100;
                            投币应答.发脉冲数 = 1;
                            break;
                        case 1: //团体票
                            投币应答.币余额 = 0;
                            投币应答.发脉冲数 = 100;
                            break;
                    }
                }
                return true;
            }
            return false;
        }
        static bool 游乐项目正常入闸(Info.DeviceInfo.机头信息 head, Info.CoinInfo.会员卡信息 member, GameInfo.游戏机信息 game, string BindID, ref DataMember.液晶卡头进出币应答结构 投币应答)
        {
            Info.ProjectTicketInfo.门票信息 ticket = ProjectTicketInfo.GetProjectTicket(head.设备编号, member.会员卡索引);
            if (ticket != null)
            {
                //合适的扣除门票
                if (ticket.RemainCount > 0)
                {
                    //存在门票则扣除门票
                    switch (ticket.TicketType)
                    {
                        case 0: //次票
                            ticket.RemainCount = ticket.RemainCount - 1;
                            投币应答.币余额 = ticket.RemainCount;
                            投币应答.发脉冲数 = 1;
                            break;
                        case 1: //团体票
                            投币应答.币余额 = 0;
                            投币应答.发脉冲数 = ticket.RemainCount;
                            ticket.RemainCount = 0;
                            break;
                    }
                    投币应答.超出当日机头最大净退币上线 = false;
                    投币应答.机头能打票 = false;
                    投币应答.机头能上分 = true;
                    投币应答.是否将退币上回游戏机 = false;
                    投币应答.是否启用卡片专卡专用 = false;
                    投币应答.是否正在使用限时送分优惠券 = false;
                    投币应答.送币数 = 0;
                    投币应答.锁机头 = false;
                    TableMemory.DataModel model = new TableMemory.DataModel();

                    //新增门票核销记录
                    TableMemory.Flw_Project_TicketUse t = new TableMemory.Flw_Project_TicketUse();
                    t.DeviceID = head.设备编号;
                    t.DeviceName = head.设备名称;
                    t.ID = XCCloudSerialNo.SerialNoHelper.CreateStoreSerialNo(PublicHelper.SystemDefiner.StoreID);
                    t.MemberID = member.会员编号;
                    t.MerchID = PublicHelper.SystemDefiner.MerchID;
                    t.ProjectTicketCode = ticket.ProjectCode;
                    t.StoreID = PublicHelper.SystemDefiner.StoreID;
                    t.InTime = DateTime.Now;
                    t.OutMinuteTotal = 0;
                    t.OutTime = null;
                    t.Verifiction = model.Verifiction(t);
                    model.Add(t);
                    //新增闸机进出记录
                    TableMemory.Flw_Project_TicketDeviceLog l = new TableMemory.Flw_Project_TicketDeviceLog();
                    l.DeviceID = head.设备编号;
                    l.ID = XCCloudSerialNo.SerialNoHelper.CreateStoreSerialNo(PublicHelper.SystemDefiner.StoreID);
                    l.LogTime = DateTime.Now;
                    l.LogType = 0;
                    l.ProjectTicketCode = ticket.ProjectCode;
                    l.TicketUseID = t.ID;
                    model.Add(l);
                    //更新门票信息
                    //门票.票.RemainCount = 门票.RemainCount;
                    if (ticket.票.FirstUseTime == null)
                        ticket.票.FirstUseTime = DateTime.Now;
                    ticket.票.Verifiction = model.Verifiction(ticket.票);
                    model.Update(ticket.票, "where ID='" + ticket.票.ID + "'");
                    //获取指定门票绑定信息
                    object o = new TableMemory.Flw_ProjectTicket_Bind();
                    if (model.CovertToDataModel("select * from Flw_ProjectTicket_Bind where ID='" + BindID + "'", ref o))
                    {
                        TableMemory.Flw_ProjectTicket_Bind b = o as TableMemory.Flw_ProjectTicket_Bind;
                        b.RemainCount = ticket.RemainCount;
                        b.Verifiction = model.Verifiction(b);
                        model.Update(b, "where ID='" + BindID + "'");
                    }
                    return true;
                }
            }
            return false;
        }
        public static bool IC卡查询(string ICCardID, Info.DeviceInfo.机头信息 head, int RepeadCode, out UInt32 Balance, out int Coins, out bool isAllowIn, out bool isAllowOut, out bool isAllowZKZY, out byte cardType, out IC卡权限结构 权限)
        {
            Balance = 0;
            Coins = 0;
            cardType = 0;
            isAllowIn = false;
            isAllowOut = false;
            isAllowZKZY = false;
            权限 = new IC卡权限结构();
            string msg = "";
            Info.GameInfo.游戏机信息 game = XCCloudSerialNo.SerialNoHelper.StringGet<Info.GameInfo.游戏机信息>("gameinfo_" + head.游戏机索引号.ToString());
            if (game == null)
            {
                msg = "错误机台信息";
                return false;
            }
            会员卡信息 card = GetCoinInfo(ICCardID, RepeadCode, head, game);
            if (card != null)
            {
                if (card.会员状态 == 1 && !card.锁会员)
                    cardType = 0x01;
                isAllowIn = card.允许投币;
                isAllowOut = card.允许退币;
                isAllowZKZY = card.专卡专用;
                Coins = card.第一投币比例;
                Balance = (UInt32)card.第一余额;
                XCCloudSerialNo.SerialNoHelper.StringSet<Info.ProjectTicketInfo.门票信息>("ticketin_" + head.设备序列号, null);
                return true;
            }
            return false;
        }
        //public static JsonObject.RemoteProjectInfo GetProjectTicketInfo(string Barcode)
        //{
        //    int c = 0;
        //    DateTime d;
        //    try
        //    {
        //        JsonObject.RemoteProjectInfo p = new JsonObject.RemoteProjectInfo();
        //        DataTable dt = DataAccess.ExecuteQueryReturnTable("select b.ID,p.ProjectName,b.EndTime,b.RemainCount,b.ProjectType,b.State from flw_project_buy_codelist b,t_project p where b.ProjectID=p.id where b.Barcode='" + Barcode + "'");
        //        if (dt.Rows.Count > 0)
        //        {
        //            DataRow row = dt.Rows[0];
        //            p.endtime = row["endtime"].ToString();
        //            p.id = row["id"].ToString();
        //            p.projectname = row["projectname"].ToString();
        //            if (row["projecttype"].ToString() == "0")
        //                p.projecttype = "次票";
        //            else
        //                p.projecttype = "有效期";
        //            int.TryParse(row["remaincount"].ToString(), out c);
        //            p.remaincount = c.ToString();
        //            if (c > 0)
        //            {
        //                d = Convert.ToDateTime(p.endtime);
        //                if (d < DateTime.Now)
        //                    p.state = "已过期";
        //                else
        //                {
        //                    switch (row["state"].ToString())
        //                    {
        //                        case "0":
        //                            p.state = "未使用";
        //                            break;
        //                        case "1":
        //                            p.state = "已使用";
        //                            break;
        //                        case "2":
        //                            p.state = "被锁定";
        //                            break;
        //                    }
        //                }
        //            }
        //            else
        //                p.state = "已使用";

        //            return p;
        //        }
        //        return null;
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.WriteLog(ex);
        //        return null;
        //    }
        //}

        //public static JsonObject.RemoteLotteryInfo GetLotteryTicketInfo(string Barcode)
        //{
        //    try
        //    {
        //        JsonObject.RemoteLotteryInfo p = new JsonObject.RemoteLotteryInfo();
        //        DataTable dt = DataAccess.ExecuteQueryReturnTable("select ParameterValue from t_parameters where system='txtTicketDate' and IsAllow='1'");
        //        int days = 0;
        //        if (dt.Rows.Count > 0)
        //            int.TryParse(dt.Rows[0][0].ToString(), out days);
        //        dt = DataAccess.ExecuteQueryReturnTable("select l.id,l.LotteryCount,ISNULL(g.GameName,'') as GameName,case when l.HeadID='' then l.WorkStation else l.HeadID end as HeadID ,l.State,l.PrintTime,l.WorkStation,l.WorkType,l.Barcode from flw_lottery l  left join t_game g on l.GameID=g.GameID where l.Barcode='" + Barcode + "'");
        //        if (dt.Rows.Count > 0)
        //        {
        //            DataRow row = dt.Rows[0];
        //            p.gamename = row["gamename"].ToString();
        //            p.headinfo = row["HeadID"].ToString();
        //            p.id = row["id"].ToString();
        //            p.lottery = row["LotteryCount"].ToString();
        //            p.printdate = row["PrintTime"].ToString();
        //            if (Convert.ToDateTime(p.printdate) < DateTime.Now.AddDays(0 - days))
        //                p.state = "已过期";
        //            else
        //            {
        //                switch (row["state"].ToString())
        //                {
        //                    case "0":
        //                        p.state = "未使用";
        //                        break;
        //                    case "1":
        //                        p.state = "已使用";
        //                        break;
        //                    case "2":
        //                        p.state = "被锁定";
        //                        break;
        //                }
        //            }
        //            return p;
        //        }
        //        return null;
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.WriteLog(ex);
        //        return null;
        //    }
        //}

        //public static JsonObject.RemotePrintTicketInfo GetPrintTicketInfo(ref string Barcode)
        //{
        //    try
        //    {
        //        if (Barcode.IndexOf('/') >= 0)
        //        {
        //            //需要解析二维码信息
        //            string[] s = Barcode.Split('/');
        //            Barcode = s[s.Length - 1];
        //        }
        //        if (Barcode.Length > 14)
        //            Barcode = Barcode.Substring(Barcode.Length - 14, 14);

        //        JsonObject.RemotePrintTicketInfo p = new JsonObject.RemotePrintTicketInfo();
        //        DataTable dt = DataAccess.ExecuteQueryReturnTable("select ParameterValue from t_parameters where system='txtTicketDate' and IsAllow='1'");
        //        int days = 0;
        //        if (dt.Rows.Count > 0)
        //            int.TryParse(dt.Rows[0][0].ToString(), out days);
        //        dt = DataAccess.ExecuteQueryReturnTable("select t.id,t.Coins,g.GameName,h.HeadID,t.State,t.RealTime as PrintTime from flw_ticket_exit t,t_head h,t_game g where t.Segment=h.Segment and t.HeadAddress=h.HeadAddress and h.GameID=g.GameID and t.Barcode='" + Barcode + "'");
        //        if (dt.Rows.Count > 0)
        //        {
        //            DataRow row = dt.Rows[0];
        //            p.gamename = row["gamename"].ToString();
        //            p.headinfo = row["HeadID"].ToString();
        //            p.id = row["id"].ToString();
        //            p.coins = row["Coins"].ToString();
        //            p.printdate = row["PrintTime"].ToString();
        //            if (Convert.ToDateTime(p.printdate) < DateTime.Now.AddDays(0 - days))
        //                p.state = "已过期";
        //            else
        //            {
        //                switch (row["state"].ToString())
        //                {
        //                    case "0":
        //                        p.state = "未兑";
        //                        break;
        //                    case "1":
        //                        p.state = "已使用";
        //                        break;
        //                    case "2":
        //                        p.state = "被锁定";
        //                        break;
        //                }
        //            }
        //            return p;
        //        }
        //        return null;
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.WriteLog(ex);
        //        return null;
        //    }
        //}

        //public static JsonObject.RemoteICCardInfo GetMemberInfo(string ICCardID)
        //{
        //    try
        //    {
        //        JsonObject.RemoteICCardInfo m = new JsonObject.RemoteICCardInfo();
        //        DataTable dt = DataAccess.ExecuteQueryReturnTable("select m.ICCardID,m.MemberName,m.Gender,m.Birthday,m.CertificalID,m.Mobile,m.Balance,m.Point,m.Deposit,m.MemberState,m.Lottery,m.Note,l.MemberLevelName,m.EndDate from t_member m,t_memberlevel l where m.MemberLevelID=l.MemberLevelID and ICCardID='" + ICCardID + "'");
        //        if (dt.Rows.Count > 0)
        //        {
        //            DataRow row = dt.Rows[0];
        //            m.balance = row["Balance"].ToString();
        //            m.birthday = row["birthday"].ToString();
        //            m.certificalID = row["certificalID"].ToString();
        //            m.deposit = row["deposit"].ToString();
        //            m.endDate = row["endDate"].ToString();
        //            m.gender = row["gender"].ToString();
        //            m.icCardID = row["icCardID"].ToString();
        //            m.lottery = row["lottery"].ToString();
        //            m.memberLevelName = row["memberLevelName"].ToString();
        //            m.memberName = row["memberName"].ToString();
        //            m.memberState = row["memberState"].ToString();
        //            m.mobile = row["mobile"].ToString();
        //            m.note = row["note"].ToString();
        //            m.point = row["point"].ToString();
        //            m.storeId = "";
        //            m.storeName = "";

        //            return m;
        //        }
        //        return null;
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.WriteLog(ex);
        //        return null;
        //    }
        //}
        public static 会员卡信息 GetCoinInfo(string ICCardID, int RepeadCode, Info.DeviceInfo.机头信息 head, Info.GameInfo.游戏机信息 game)
        {
            try
            {
                会员卡信息 card = new 会员卡信息();

                DataAccess ac = new DataAccess();
                string sql = string.Format("select c.StoreID,c.IsLock,c.CardStatus,c.EndDate,c.MemberID,c.ID as CardIndex,cr.*,m.* from Data_Member_Card c,Data_MemberLevel m,Data_Card_Right cr,Data_Card_Right_StoreList crs where c.ID=cr.CardID and c.MemberLevelID=m.MemberLevelID and cr.ID=crs.CardRightID and c.ICCardID='{0}' and c.MerchID='{1}' and crs.StoreID='{3}'", ICCardID, PublicHelper.SystemDefiner.MerchID, RepeadCode, PublicHelper.SystemDefiner.StoreID);
                //string sql = string.Format("select c.MemberID,c.ID as CardIndex,cr.*,m.* from Data_Member_Card c,Data_MemberLevel m,Data_Card_Right cr,Data_Card_Right_StoreList crs where c.ID=cr.CardID and c.MemberLevelID=m.MemberLevelID and cr.ID=crs.CardRightID and c.ICCardID='{0}' and c.MerchID='{1}' and c.RepeatCode='{2}' and crs.StoreID='{3}'", ICCardID, HostServer.MerchID, RepeadCode, HostServer.StoreID);
                DataTable dt = ac.ExecuteQueryReturnTable(sql);
                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    card.开卡门店 = row["StoreID"].ToString();
                    card.锁会员 = (row["IsLock"].ToString() == "1");
                    card.会员状态 = Convert.ToInt32(row["CardStatus"]);
                    card.会员卡号 = ICCardID;
                    card.会员编号 = row["MemberID"].ToString();
                    card.会员卡索引 = row["CardIndex"].ToString();
                    if (Convert.ToDateTime(row["EndDate"]) < DateTime.Now)
                        card.会员状态 = 2;
                    card.允许投币 = (row["AllowPush"].ToString() == "1");
                    card.允许退币 = (row["AllowExitCoinToCard"].ToString() == "1" && row["AllowOut"].ToString() == "1");
                    string MemberID = dt.Rows[0]["MemberID"].ToString();
                    //找到有效卡
                    if (game != null)
                    {
                        card.专卡专用 = (row["LockHead"].ToString() == "1" || game.博彩参数.专卡专用);
                        //找到对应游戏机
                        if (game.投币参数.按钮1投币基数 > 0)
                        {
                            card.第一投币比例 = game.投币参数.按钮1投币基数;
                            card.第一余额 = Convert.ToInt32(GetBalanceCount(card.会员卡索引, game.投币参数.按钮1余额类别));
                        }
                        if (game.投币参数.按钮2投币基数 > 0)
                        {
                            card.第二投币比例 = game.投币参数.按钮2投币基数;
                            card.第二余额 = Convert.ToInt32(GetBalanceCount(card.会员卡索引, game.投币参数.按钮2余额类别));
                        }
                    }
                    else
                    {
                        if (head.类型 == DeviceInfo.设备类型.存币机)
                        {
                            card.第一余额 = Convert.ToInt32(GetBalanceCount(card.会员卡索引, head.扩展参数.余额种类));
                        }
                    }
                    sql = "select UserName from Base_MemberInfo where ID='" + card.会员编号 + "'";
                    dt = ac.ExecuteQueryReturnTable(sql);
                    if (dt.Rows.Count > 0)
                    {
                        card.会员姓名 = dt.Rows[0][0].ToString();
                    }
                }
                return card;
            }
            catch (Exception ex)
            {
                LogHelper.LogHelper.WriteLog(ex);
                return null;
            }
        }
        public static string GetBalanceName(int balanceIndex)
        {
            DataAccess ac = new DataAccess();
            DataTable dt = ac.ExecuteQueryReturnTable("select TypeName from Dict_BalanceType where ID='" + balanceIndex + "'");
            if (dt.Rows.Count > 0)
            {
                return dt.Rows[0]["TypeName"].ToString();
            }
            return "";
        }

        public static decimal GetBalanceCount(string cardindex, int balanceIndex)
        {
            DataAccess ac = new DataAccess();
            DataTable dt = ac.ExecuteQueryReturnTable(string.Format(@"select b.BalanceIndex,b.Balance+bf.Balance as Balance from
                                                                        Data_Card_Balance b,Data_Card_Balance_Free bf
                                                                        where b.CardIndex=bf.CardIndex and b.BalanceIndex=bf.BalanceIndex and b.CardIndex='{0}' and b.BalanceIndex='{1}' and b.MerchID='{2}'"
                                , cardindex, balanceIndex, PublicHelper.SystemDefiner.MerchID));
            if (dt.Rows.Count > 0)
            {
                return Convert.ToDecimal(dt.Rows[0]["Balance"]);
            }
            return 0;
        }
    }
}
