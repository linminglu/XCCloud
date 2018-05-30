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

}
