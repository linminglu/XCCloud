using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudWebBar.BLL.Container;
using XCCloudWebBar.BLL.XCCloud;
using Dapper;
using System.Data.SqlClient;
using XCCloudWebBar.Model.CustomModel.XCCloud;
using XCCloudWebBar.Model.WeiXin;
using XCCloudWebBar.Model.XCCloud;
using System.Data;

namespace XCCloudWebBar.Business.XCCloud
{
    public class XCCloudStoreBusiness
    {
        public static bool IsEffectiveStore(string storeId)
        {
            XCCloudWebBar.BLL.IBLL.XCCloud.IBase_StoreInfoService base_StoreInfoService = BLLContainer.Resolve<XCCloudWebBar.BLL.IBLL.XCCloud.IBase_StoreInfoService>();
            return base_StoreInfoService.Any(p => p.ID.Equals(storeId, StringComparison.OrdinalIgnoreCase));
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
                storeId = Base_StoreInfoService.I.GetModels(p => p.MerchID.Equals(merchId)).FirstOrDefault().ID;
                return true;
            }

            return false;
        }

        public static List<GamePushRule> GetGamePushRule(string merchId, string storeId, int gameId, int memberLevelId)
        {
            string strSql = @"DECLARE @merchId VARCHAR(15)
                            DECLARE @storeid VARCHAR(15)
                            DECLARE @gameid INT
                            DECLARE @memberlevelid INT

                            SET @merchId='{0}'
                            SET @storeid='{1}'
                            SET @gameid={2}
                            SET @memberlevelid={3}
                            SET DATEFIRST 1--设置一周第一天为周一
                            SELECT ID,Allow_In,Allow_Out,PushBalanceIndex1,PushBalanceName1,PushCoin1,PushBalanceIndex2,PushBalanceName2,PushCoin2 FROM   
	                            (SELECT ID,Allow_In,Allow_Out,PushBalanceIndex1,PushCoin1,PushBalanceIndex2,PushCoin2,Level FROM Data_PushRule WHERE 
	                            StoreID=@storeid 
                                AND GETDATE() BETWEEN StartDate AND EndDate --判断日期
	                            AND CONVERT(varchar(100), GETDATE(), 108) BETWEEN StartTime AND EndTime	--判断时间段是否可用
	                            AND (
		                            (WeekType=0 AND CHARINDEX(CAST(DATEPART(WEEKDAY, CONVERT(DATETIME, GETDATE())) AS VARCHAR(1)),week)>0)	--自定义方法是否包含当前周，周一1 周日7
		                            OR (WeekType=1 AND (SELECT COUNT(DayType) n from XC_HolidayList where CONVERT(varchar(20), WorkDay, 23)=CONVERT(varchar(20), GETDATE(), 23) AND DayType=0)>0) --工作日方法判断当前是否为工作日
		                            OR (WeekType=2 AND (SELECT COUNT(DayType) n from XC_HolidayList where CONVERT(varchar(20), WorkDay, 23)=CONVERT(varchar(20), GETDATE(), 23) AND DayType=1)>0) --周末方式判断当前是否为周末
		                            OR (WeekType=3 AND (SELECT COUNT(DayType) n from XC_HolidayList where CONVERT(varchar(20), WorkDay, 23)=CONVERT(varchar(20), GETDATE(), 23) AND DayType=2)>0) --周末方式判断当前是否为周末
		                            )
	                            ) r,
	                            (SELECT PushRuleID FROM Data_PushRule_GameList WHERE StoreID=@storeid AND GameID=@gameid) g,
	                            (SELECT PushRuleID FROM Data_PushRule_MemberLevelList WHERE StoreID=@storeid AND MemberLevelID=@memberlevelid) m, 
	                            (SELECT id AS BalanceIndex, TypeName AS PushBalanceName1 FROM Dict_BalanceType WHERE [State] = 1 AND MerchID=@merchId) l,
	                            (SELECT id AS BalanceIndex, TypeName AS PushBalanceName2 FROM Dict_BalanceType WHERE [State] = 1 AND MerchID=@merchId) n
                            where r.ID=g.PushRuleID AND r.ID=m.PushRuleID AND r.PushBalanceIndex1 = l.BalanceIndex AND r.PushBalanceIndex2 = n.BalanceIndex 
                            ORDER BY r.Level DESC";
            strSql = string.Format(strSql, merchId, storeId, gameId, memberLevelId);

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

        public static Data_MemberLevel GetMemberLevel(int memberLevelId)
        {
            Data_MemberLevel level = Data_MemberLevelService.I.GetModels(t => t.ID == memberLevelId).FirstOrDefault();
            return level;
        }

        public static List<FoodInfoViewModel> GetFoodInfoList(string merchId, string storeId, int memberLevelId)
        {
            var param = new DynamicParameters();
            param.Add("@merchId", merchId, DbType.String);
            param.Add("@storeId", storeId, DbType.String);
            param.Add("@memberlevelId", memberLevelId, DbType.Int32);

            List<FoodInfoViewModel> foodList = null;
            using (SqlConnection connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["XCCloudDB"].ToString()))
            {
                foodList = connection.Query<FoodInfoViewModel>("sp_GetFoodInfoList", param, null, true, null, CommandType.StoredProcedure).ToList();

            }
            if (foodList == null)
            {
                foodList = new List<FoodInfoViewModel>();
            }
            return foodList;
        }
    }
}
