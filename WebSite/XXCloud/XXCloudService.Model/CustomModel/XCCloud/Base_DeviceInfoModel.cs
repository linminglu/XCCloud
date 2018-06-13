using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.Common;

namespace XCCloudService.Model.CustomModel.XCCloud
{
    public class Base_DeviceInfoList
    {
        public int ID { get; set; }

        public string MCUID { get; set; }

        public string DeviceName { get; set; }
        
        public int? DeviceType { get; set; }
        public string DeviceTypeStr { get { return ((XCCloudService.Common.Enum.DeviceType?)DeviceType).GetDescription(); } set { } }
       
        public int? GameType { get; set; }
        
        public string GameTypeStr { get; set; }
        
        public string GameName { get; set; }
       
        public string AreaName { get; set; }
       
        public string SiteName { get; set; }
        public string segment { get; set; }
        public string Address { get; set; }
        public string DeviceRunning { get; set; }
        public int? DeviceStatus { get; set; }
        public string DeviceStatusStr { get; set; }
    }

    public class GameCoinInfoModel
    {
        public GameCoinInfoModel()
        {
            this.GameType = -1;
            this.GameCoinList = new List<GameCoinInfo>();
            this.GamePushRules = new List<GamePushRule>();
        }
        public string DeviceName { get; set; }

        public string DeviceCategoryName { get; set; }

        public int DeviceType { get; set; }

        public int GameType { get; set; }

        public List<GameCoinInfo> GameCoinList { get; set; }

        public List<GamePushRule> GamePushRules { get; set; }
    }

    public class GameFreeRule
    {
        public int RuleId { get; set; }

        public int NeedCoin { get; set; }

        public int FreeCoin { get; set; }

        public int ExitCoin { get; set; }
    }

    public class GameCoinInfo
    {
        public int PlayCount { get; set; }

        public string Amount { get; set; }
    }

    public class GamePushRule
    {
        public int Id { get; set; }

        public int Allow_In { get; set; }

        public int Allow_Out { get; set; }

        public int PushBalanceIndex1 { get; set; }

        public int PushCoin1 { get; set; }

        public int PushBalanceIndex2 { get; set; }

        public int PushCoin2 { get; set; }
    }
}
