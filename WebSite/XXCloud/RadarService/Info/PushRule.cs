using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;

namespace RadarService.Info
{
    public class PushRule
    {
        public class 游戏机投币规则
        {
            public bool 允许投币 { get; set; }
            public bool 允许退币 { get; set; }
            public int 扣值1类别 { get; set; }
            public int 扣值1数量 { get; set; }
            public int 扣值2类别 { get; set; }
            public int 扣值2数量 { get; set; }
        }

        public 游戏机投币规则 获取游戏机规则(int GameIndex, int MemberLevelID)
        {
            游戏机投币规则 rule = new 游戏机投币规则();
            DataAccess ac = new DataAccess();
            string sql = string.Format(@"
            declare @storeid varchar(15)
            declare @gameid int
            declare @memberlevelid int

            set @storeid='{0}'
            set @gameid='{1}'
            set @memberlevelid='{2}'
            set datefirst 1--设置一周第一天为周一
            select Allow_In,Allow_Out,PushBalanceIndex1,PushCoin1,PushBalanceIndex2,PushCoin2 from 
	            (select ID,Allow_In,Allow_Out,PushBalanceIndex1,PushCoin1,PushBalanceIndex2,PushCoin2,Level from Data_PushRule where 
	            StoreID=@storeid 
	            and GETDATE() between CONVERT(varchar(20), StartDate, 23) +' '+ CONVERT(varchar(20), StartTime, 8) and CONVERT(varchar(20), EndDate, 23) +' '+ CONVERT(varchar(20), EndTime, 8)	--判断日期是否可用
	            and (
	            (WeekType=0 and CHARINDEX(CAST( datepart(weekday, CONVERT(datetime, GETDATE())) as varchar(1)),week)>0)	--自定义方法是否包含当前周，周一1 周日7
	            or (WeekType=1 and (select COUNT(DayType) n from XC_HolidayList where CONVERT(varchar(20), WorkDay, 23)=CONVERT(varchar(20), GETDATE(), 23) and DayType=0)>0) --工作日方法判断当前是否为工作日
	            or (WeekType=2 and (select COUNT(DayType) n from XC_HolidayList where CONVERT(varchar(20), WorkDay, 23)=CONVERT(varchar(20), GETDATE(), 23) and DayType=1)>0) --周末方式判断当前是否为周末
	            or (WeekType=3 and (select COUNT(DayType) n from XC_HolidayList where CONVERT(varchar(20), WorkDay, 23)=CONVERT(varchar(20), GETDATE(), 23) and DayType=2)>0) --周末方式判断当前是否为周末
	            )
	            ) r,
	            (select PushRuleID from Data_PushRule_GameList where StoreID=@storeid and GameID=@gameid) g,
	            (select PushRuleID from Data_PushRule_MemberLevelList where StoreID=@storeid and MemberLevelID=@memberlevelid) m 
            where r.ID=g.PushRuleID and r.ID=m.PushRuleID order by r.Level desc
            ", PublicHelper.SystemDefiner.StoreID, GameIndex, MemberLevelID);
            DataTable dt = ac.ExecuteQueryReturnTable(sql);
            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                rule.扣值1类别 = Convert.ToInt32(row["PushBalanceIndex1"]);
                rule.扣值1数量 = Convert.ToInt32(row["PushCoin1"]);
                rule.扣值2类别 = Convert.ToInt32(row["PushBalanceIndex2"]);
                rule.扣值2数量 = Convert.ToInt32(row["PushCoin2"]);
                rule.允许投币 = (row["Allow_In"].ToString() == "1");
                rule.允许退币 = (row["Allow_Out"].ToString() == "1");
            }
            else
            {
                //没有投币规则，则查找游戏机本身投币值
                sql = "select AllowElecPush,AllowElecOut,PushBalanceIndex1,PushCoin1,PushBalanceIndex2,PushCoin2 from Data_GameInfo where ID='" + GameIndex + "'";
                dt = ac.ExecuteQueryReturnTable(sql);
                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    rule.扣值1类别 = Convert.ToInt32(row["PushBalanceIndex1"]);
                    rule.扣值1数量 = Convert.ToInt32(row["PushCoin1"]);
                    rule.扣值2类别 = Convert.ToInt32(row["PushBalanceIndex2"]);
                    rule.扣值2数量 = Convert.ToInt32(row["PushCoin2"]);
                    rule.允许投币 = (row["AllowElecPush"].ToString() == "1");
                    rule.允许退币 = (row["AllowElecOut"].ToString() == "1");
                }
            }
            return rule;
        }
    }
}
