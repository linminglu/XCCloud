using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.Common.Enum;
using XCCloudService.Common;

namespace XCCloudService.Model.CustomModel.XCCloud
{
    public class Flw_GameData_WinPrizeList
    {
        public string ID { get; set; }
        public DateTime? WinTime { get; set; }
        public string GameID { get; set; }
        public string GameName { get; set; }
        public string GameSiteName { get; set; }
        public string MemberName { get; set; }
        public int? IntervalTimes { get; set; }
        public decimal? GoodPrice { get; set; }
        public decimal? WinRate { get; set; }        
    }   
}
