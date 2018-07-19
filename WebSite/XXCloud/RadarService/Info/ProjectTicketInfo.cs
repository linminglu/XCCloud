using DSS;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadarService.Info
{
    /// <summary>
    /// 门票对象
    /// </summary>
    public class ProjectTicketInfo
    {
        public class 门票信息
        {
            public string BindID { get; set; }
            public string ProjectCode { get; set; }
            public int RemainCount { get; set; }
            public int TicketType { get; set; }
            public string TicketName { get; set; }
            public int AllowExitTimes { get; set; }
            public TableMemory.Flw_Project_TicketInfo 票 { get; set; }
        }

        public class 门票使用信息
        {
            public string ProjectCode { get; set; }
            public string FlwID { get; set; }
            public DateTime InTime { get; set; }
            public DateTime? OutTime { get; set; }
            public int OutMinuteTotal { get; set; }
        }

        //public static bool GetEffectProjectTicket(int DeviceID, string CardIndex ,out 门票信息 TicketInfo)
        //{

        //}

        public static 门票信息 GetProjectTicket(int DeviceID, string CardIndex)
        {
            门票信息 t = new 门票信息();
            DataAccess ac = new DataAccess();
            string sql = string.Format(@"declare @deviceid int
                                            declare @cardindex varchar(32)
                                            declare @merchid varchar(15)

                                            set @deviceid='{0}'
                                            set @cardindex='{1}'
                                            set @merchid='{2}'
 
                                            select b.ID as bID,t.ProjectID,t.Barcode,b.AllowShareCount,b.RemainCount,e.TicketType,e.TicketName,e.AllowExitTimes,e.NoStartDate,e.NoEndDate,e.AllowRestrict,e.RestrictShareCount,e.RestrictPeriodType,e.RestrictPreiodValue,e.RestrctCount from
                                            (
                                            select b.ProjectID,b.TicketType,a.WorkType,b.Barcode,b.SaleTime from
                                            (select p.ID,b.WorkType from 
                                                (select ID from Data_ProjectInfo where MerchID=@merchid) p,
                                                (select ProjectID,DeviceID,WorkType from Data_Project_BindDevice where MerchID=@merchid) b 
                                            where p.ID=b.ProjectID and b.DeviceID=@deviceid) a,
                                            (select t.Barcode,b.ProjectID,t.SaleTime,t.TicketType from 
                                                (select Barcode,SaleTime,TicketType from Flw_Project_TicketInfo where MerchID=@merchid and CardID=@cardindex and EndTime>GETDATE()) t,
                                                (select ProjectCode,ProjectID from Flw_ProjectTicket_Bind where MerchID=@merchid) b 
                                            where t.Barcode=b.ProjectCode) b
                                            where a.ID=b.ProjectID 
                                            ) t,Flw_ProjectTicket_Entry e,Flw_ProjectTicket_Bind b
                                            where e.ProjectCode=t.Barcode and b.ProjectCode=t.Barcode
                                            and 
                                            ((e.EffactType = 0 and DATEADD(DAY,case  
                                                            when e.EffactPeriodType=0 then e.EffactPeriodValue 
                                                            when e.EffactPeriodType=1 then e.EffactPeriodValue * 7 
                                                            when e.EffactPeriodType=2 then e.EffactPeriodValue * 30
                                                            when e.EffactPeriodType=3 then e.EffactPeriodValue * 90
                                                            else e.EffactPeriodValue * 365 end,t.SaleTime)<=GETDATE()	--计算生效时间
                                            and DATEADD(DAY,case  
                                                            when e.VaildPeriodType=0 then e.VaildPeriodValue 
                                                            when e.VaildPeriodType=1 then e.VaildPeriodValue * 7 
                                                            when e.VaildPeriodType=2 then e.VaildPeriodValue * 30
                                                            when e.VaildPeriodType=3 then e.VaildPeriodValue * 90
                                                            else e.VaildPeriodValue * 365 end,t.SaleTime)>GETDATE())	--计算失效效时间
                                            or (e.EffactType = 1 and GETDATE() between e.VaildStartDate and e.VaildStartDate))	--按时间范围算
                                            and (
                                                (e.WeekType=0 and CHARINDEX(CAST( datepart(weekday, CONVERT(datetime, GETDATE())) as varchar(1)),week)>0)	--自定义方法是否包含当前周，周一1 周日7
                                                or (e.WeekType=1 and (select COUNT(DayType) n from XC_HolidayList where CONVERT(varchar(20), WorkDay, 23)=CONVERT(varchar(20), GETDATE(), 23) and DayType=0)>0) --工作日方法判断当前是否为工作日
                                                or (e.WeekType=2 and (select COUNT(DayType) n from XC_HolidayList where CONVERT(varchar(20), WorkDay, 23)=CONVERT(varchar(20), GETDATE(), 23) and DayType=1)>0) --周末方式判断当前是否为周末
                                                or (e.WeekType=3 and (select COUNT(DayType) n from XC_HolidayList where CONVERT(varchar(20), WorkDay, 23)=CONVERT(varchar(20), GETDATE(), 23) and DayType=2)>0) --周末方式判断当前是否为周末
                                                )
                                            and (GETDATE() between CONVERT(datetime,CONVERT(varchar(10),getdate(),23)+' '+ CONVERT(varchar(8),StartTime,14)) and CONVERT(datetime,CONVERT(varchar(10),getdate(),23)+' '+ CONVERT(varchar(8),e.EndTime,14)))--判定是否在可用时段内",
                                            DeviceID, CardIndex, PublicHelper.SystemDefiner.MerchID);
            DataTable dt = ac.ExecuteQueryReturnTable(sql);
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    门票信息 ticket = new 门票信息();
                    ticket.BindID = row["bID"].ToString();
                    ticket.ProjectCode = row["Barcode"].ToString();
                    ticket.RemainCount = Convert.ToInt32(row["RemainCount"]);
                    ticket.TicketType = Convert.ToInt32(row["TicketType"]);
                    ticket.TicketName = row["TicketName"].ToString();
                    ticket.AllowExitTimes = Convert.ToInt32(row["AllowExitTimes"]);
                    if (!判断是否在不可用日期(row)) continue;
                    if (!期限票判断使用频率规则(row)) continue;
                    if (t.ProjectCode == "")
                    {
                        if (t.TicketType > ticket.TicketType)
                            t = ticket;
                        else
                            continue;
                    }
                    else
                        t = ticket;
                }
                if (t == null) return null;
                t.票 = new TableMemory.Flw_Project_TicketInfo();
                TableMemory.DataModel model = new TableMemory.DataModel();
                object o = t.票;
                model.CovertToDataModel("select * from Flw_Project_TicketInfo where Barcode='" + t.ProjectCode + "'", ref o);
                t.票 = o as TableMemory.Flw_Project_TicketInfo;
                return t;
            }
            return null;
        }
        static bool 判断是否在不可用日期(DataRow row)
        {
            if (row["NoStartDate"] == DBNull.Value || row["NoEndDate"] == DBNull.Value) return true;//没有不可用日期
            DateTime d, d1, d2;
            d = DateTime.Now;
            d1 = Convert.ToDateTime(row["NoStartDate"]);
            d2 = Convert.ToDateTime(row["NoEndDate"]);
            if (d >= d1 && d <= d2) return false;
            return true;
        }
        static bool 期限票判断使用频率规则(DataRow row)
        {
            bool allow = row["AllowRestrict"].ToString() == "1";
            if (!allow) return true;
            int projectID = Convert.ToInt32(row["ProjectID"]);
            string barcode = row["Barcode"].ToString();
            bool share = row["RestrictShareCount"].ToString() == "1";
            int ptype = Convert.ToInt32(row["RestrictPeriodType"]);
            int pvalue = Convert.ToInt32(row["RestrictPreiodValue"]);
            int pcount = Convert.ToInt32(row["RestrctCount"]);
            string sql = "";
            if (share && ptype == 0)
                //使用次数共享，按小时间隔
                sql = "select COUNT(u.ID) as num from Flw_Project_TicketUse u,Data_Project_BindDevice b where u.DeviceID=b.DeviceID and u.ProjectTicketCode='" + barcode + "' and DATEADD(mi," + pvalue + ",u.InTime)>=GETDATE()";
            else if (share && ptype == 1)
                //使用次数共享，按小时间隔
                sql = "select COUNT(u.ID) as num from Flw_Project_TicketUse u,Data_Project_BindDevice b where u.DeviceID=b.DeviceID and u.ProjectTicketCode='" + barcode + "' and DATEADD(d," + pvalue + ",u.InTime)>=GETDATE()";
            else if (!share && ptype == 0)
                //使用次数共享，按小时间隔
                sql = "select COUNT(u.ID) as num from Flw_Project_TicketUse u,Data_Project_BindDevice b where u.DeviceID=b.DeviceID and u.ProjectTicketCode='" + barcode + "' and u.ProjectTicketCode='" + projectID + "' and DATEADD(mi," + pvalue + ",u.InTime)>=GETDATE()";
            else
                //使用次数共享，按小时间隔
                sql = "select COUNT(u.ID) as num from Flw_Project_TicketUse u,Data_Project_BindDevice b where u.DeviceID=b.DeviceID and u.ProjectTicketCode='" + barcode + "' and u.ProjectTicketCode='" + projectID + "' and DATEADD(d," + pvalue + ",u.InTime)>=GETDATE()";
            DataAccess ac = new DataAccess();
            DataTable dt = ac.ExecuteQueryReturnTable(sql);
            return Convert.ToInt32(dt.Rows[0][0]) < pcount;
        }
        /// <summary>
        /// 判断是否存在二次进场的票
        /// 出场后在有效累计时间内可免费进场
        /// </summary>
        /// <param name="head"></param>
        /// <param name="member"></param>
        /// <param name="ProjectCode"></param>
        /// <returns></returns>
        public static bool 判断是否需要二次入场(int outTotal, string ticketCode, int projectID, out string FlwID)
        {
            FlwID = "";
            门票使用信息 info = XCCloudSerialNo.SerialNoHelper.StringGet<门票使用信息>("ticket_" + ticketCode + "_" + projectID);
            if (info != null)
            {
                FlwID = info.FlwID; //入场时的业务流水号
                if (info.OutTime != null)
                {
                    //门票已出场
                    int count = (int)(DateTime.Now - (DateTime)info.OutTime).TotalMinutes;
                    if (PublicHelper.SystemDefiner.AllowProjectAddup)
                    {
                        //门票离场时间需要累加
                        if (count + info.OutMinuteTotal <= outTotal)
                        {
                            //离场累计时间正常可以二次进场
                            info.OutMinuteTotal += count;
                            //更新离场时间缓存
                            XCCloudSerialNo.SerialNoHelper.StringSet<门票使用信息>("ticket_" + ticketCode + "_" + projectID, info);
                            return true;
                        }
                    }
                    else
                    {
                        //门票离场时间不需要累加
                        if (count <= outTotal)
                        {
                            //离场时间正常可以二次进场
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// 判断是否需要验证入闸顺序
        /// </summary>
        /// <param name="head"></param>
        /// <returns></returns>
        public static bool 门票验票入闸顺序校验(Info.DeviceInfo.机头信息 head)
        {
            DataAccess ac = new DataAccess();
            string sql = "select p.AdjOrder from Data_ProjectInfo p,Data_Project_BindDevice d where p.ID=d.ProjectID and d.DeviceID='" + head.设备编号 + "'";
            DataTable dt = ac.ExecuteQueryReturnTable(sql);
            if (dt.Rows.Count > 0)
                return (dt.Rows[0]["AdjOrder"].ToString() == "1");
            return false;
        }

        public static bool 获取门票使用状态(string ProjectCode, Info.DeviceInfo.机头信息 head)
        {
            DataAccess ac = new DataAccess();
            if (head.设备业务类别 == 1 && head.验票进出方向 == 0)
            {
                DataTable dt = ac.ExecuteQueryReturnTable("select * from Flw_Project_TicketUse where ProjectTicketCode='" + ProjectCode + "' and OutTime is not null");
                return (dt.Rows.Count > 0);
            }
            else if (head.设备业务类别 == 1 && head.验票进出方向 == 1)
            {
                DataTable dt = ac.ExecuteQueryReturnTable("select * from Flw_Project_TicketUse where ProjectTicketCode='" + ProjectCode + "' and OutTime is null");
                return (dt.Rows.Count > 0);
            }
            return false;
        }
    }
}
