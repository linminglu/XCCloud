using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.XCCloud;
using Dapper;
using System.Data.SqlClient;
using XCCloudService.Model.CustomModel.XCCloud;
using XCCloudService.Model.WeiXin;

namespace XCCloudService.Business.XCCloud
{
    public class XCCloudStoreBusiness
    {
        public static bool IsEffectiveStore(string storeId)
        {
            XCCloudService.BLL.IBLL.XCCloud.IBase_StoreInfoService base_StoreInfoService = BLLContainer.Resolve<XCCloudService.BLL.IBLL.XCCloud.IBase_StoreInfoService>();
            return base_StoreInfoService.Any(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase));
        }

        public static bool IsSingleStore(string merchId)
        {
            return Base_StoreInfoService.I.GetCount(p => p.MerchID.Equals(merchId)) == 1;
        }

        public static bool IsSingleStore(string merchId, out string storeId)
        {
            storeId = string.Empty;
            if (IsSingleStore(merchId))
            {
                storeId = Base_StoreInfoService.I.GetModels(p => p.MerchID.Equals(merchId)).FirstOrDefault().StoreID;
                return true;
            }

            return false;
        }

        public static List<GamePushRule> GetGamePushRule(string storeId, int gameId, int memberLevelId)
        {
            string strSql = @"declare @storeid varchar(15)
                            declare @gameid int
                            declare @memberlevelid int

                            set @storeid='{0}'
                            set @gameid={1}
                            set @memberlevelid={2}
                            set datefirst 1--设置一周第一天为周一
                            select ID,Allow_In,Allow_Out,PushBalanceIndex1,PushCoin1,PushBalanceIndex2,PushCoin2 from 
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
                            where r.ID=g.PushRuleID and r.ID=m.PushRuleID order by r.Level desc";
            strSql = string.Format(strSql, storeId, gameId, memberLevelId);

            List<GamePushRule> currList = null;
            using (SqlConnection connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["XCCloudDB"].ToString()))
            {
                currList = connection.Query<GamePushRule>(strSql).ToList();
                
            }
            if(currList == null)
            {
                currList = new List<GamePushRule>();
            }
            return currList;
        }

        public static List<CardBalance> GetCardStoreBalanceList(string merchId, string storeId, string cardId)
        {
            //当前门店正价余额集合
            var balances = from a in Data_Card_BalanceService.I.GetModels(t => t.MerchID == merchId && t.CardIndex == cardId)
                           join b in Data_Card_Balance_StoreListService.I.GetModels(t => t.StoreID == storeId) on a.ID equals b.CardBalanceID
                           join c in Dict_BalanceTypeService.I.GetModels(t => t.MerchID == merchId) on a.BalanceIndex equals c.ID
                           select new
                           {
                               BalanceIndex = a.BalanceIndex,
                               BalanceName = c.TypeName,
                               Balance = a.Balance
                           };
            //当前门店赠送余额集合
            var balanceFrees = from a in Data_Card_Balance_FreeService.I.GetModels(t => t.MerchID == merchId && t.CardIndex == cardId)
                               join b in Data_Card_Balance_StoreListService.I.GetModels(t => t.StoreID == storeId) on a.ID equals b.CardBalanceID
                               join c in Dict_BalanceTypeService.I.GetModels(t => t.MerchID == merchId) on a.BalanceIndex equals c.ID
                               select new
                               {
                                   BalanceIndex = a.BalanceIndex,
                                   BalanceName = c.TypeName,
                                   Balance = a.Balance
                               };
            var balanceList = balances.Concat(balanceFrees).ToList();
            var balanceGroup = balanceList.GroupBy(t => new { t.BalanceIndex, t.BalanceName })
                .Select(t => new { BalanceIndex = t.Key.BalanceIndex.Value, BalanceName = t.Key.BalanceName, Quantity = t.Sum(item => item.Balance.Value) }).ToList();

            List<CardBalance> list = new List<CardBalance>();
            foreach (var item in balanceGroup)
            {
                CardBalance cb = new CardBalance();
                cb.BalanceIndex = item.BalanceIndex;
                cb.BalanceName = item.BalanceName;
                cb.Quantity = item.Quantity;
                list.Add(cb);
            }
            return list;
        }
    }
}
