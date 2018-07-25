using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
    public class Flw_GameData_WinPrize
    {
        public string ID { get; set; }
        public string MerchID { get; set; }
        public string StoreID { get; set; }
        public int? GameIndex { get; set; }
        public string GameSiteName { get; set; }
        public int? DeviceID { get; set; }
        public int? IntervalTimes { get; set; }
        public decimal? IntervalPrice { get; set; }
        public int? WatchCount { get; set; }
        public int? PrizeType { get; set; }
        public int? BalanceIndex { get; set; }
        public int? GoodID { get; set; }
        public decimal? PrizeCount { get; set; }
        public string MemberName { get; set; }
        public string ICCardID { get; set; }
        public string MemberID { get; set; }
        public decimal? RemainBalance { get; set; }
        public string CreateStoreID { get; set; }
        public DateTime? WinTime { get; set; }
        public decimal? GoodPrice { get; set; }
        public int? GiveFreeType { get; set; }
        public int? GiveFreeRuleID { get; set; }
        public decimal? GiveFreeCount { get; set; }
        public string Note { get; set; }
        public DateTime? CheckDate { get; set; }
        public int? SyncFlag { get; set; }
        public string Verifiction { get; set; }
    }
}
