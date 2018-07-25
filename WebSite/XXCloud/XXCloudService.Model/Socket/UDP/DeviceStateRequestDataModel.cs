using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudService.Model.Socket.UDP
{    
    [DataContract]
    public class DeviceStateRequestDataModel
    {
        public DeviceStateRequestDataModel(string token, string mcuid, string status, string storeId, string signKey)
        {
            this.Token = token;
            this.MCUId = mcuid;
            this.Status = status; 
            this.StoreId = storeId;
            this.SignKey = signKey;
        }

        /// <summary>
        /// 雷达令牌
        /// </summary>
        [DataMember(Name = "token", Order = 1)]
        public string Token { set; get; }

        /// <summary>
        /// 设备ID
        /// </summary>
        [DataMember(Name = "mcuid", Order = 2)]
        public string MCUId { set; get; }

        /// <summary>
        /// 设备状态
        /// </summary>
        [DataMember(Name = "status", Order = 3)]
        public string Status { set; get; }

        public string StoreId { set; get; }

        /// <summary>
        /// 签名
        /// </summary>
        [DataMember(Name = "signkey", Order = 4)]
        public string SignKey { set; get; }
    }

    public class DeviceStateDataModel
    {
        public string token { get; set; }
        public string signkey { get; set; }
        public List<DeviceListStateModel> deviceList { get; set; }
    }

    public class DeviceListStateModel
    {
        public string mcuid { get; set; }
        public string status { get; set; }
    }

    public class DeviceStateCacheModel
    {
        public string Token { get; set; }
        public string MCUID { get; set; }
        public string State { get; set; }
        public string SignKey { get; set; }
    }
}
