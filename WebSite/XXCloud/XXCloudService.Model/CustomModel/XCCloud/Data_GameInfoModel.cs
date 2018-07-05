using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.Common.Enum;
using XCCloudService.Common;

namespace XCCloudService.Model.CustomModel.XCCloud
{
    public class Data_GameInfoList
    {
        public int ID { get; set; }
        public string GameID { get; set; }
        public string GameName { get; set; }
        public int? GameType { get; set; }
        public string GameTypeStr { get; set; }
        public string AreaName { get; set; }
        public decimal? Area { get; set; }
        public DateTime? ChangeTime { get; set; }
        public int? Price { get; set; }
        public int? PushCoin1 { get; set; }
        public string AllowElecPushStr { get; set; }
        public string LotteryModeStr { get; set; }
        public string ReadCatStr { get; set; }
        public string StateStr { get; set; }        
    }
}
