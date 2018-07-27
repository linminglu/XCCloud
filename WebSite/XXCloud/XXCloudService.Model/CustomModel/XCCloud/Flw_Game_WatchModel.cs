using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.Common.Enum;
using XCCloudService.Common;

namespace XCCloudService.Model.CustomModel.XCCloud
{
    public class Flw_Game_WatchList
    {
        public string ID { get; set; }
        public string GameName { get; set; }
        public string DeviceName { get; set; }
        public string SiteName { get; set; }
        public string LogName { get; set; }
        public DateTime? CreateTime { get; set; }
        public int? InCoin { get; set; }
        public int? InCoinError { get; set; }
        public int? InCoin2 { get; set; }
        public int? InCoinError2 { get; set; }
        public int? PrizeCount { get; set; }
        public int? PrizeError { get; set; }
        public decimal? GoodPrice { get; set; }
        public int? OutCoin { get; set; }
        public int? OutCoinError { get; set; }
        public int? OutLottery { get; set; }
        public int? OutLotteryError { get; set; }
        public int? Winner { get; set; }
        public int? WinnerError { get; set; }
    }   
}
