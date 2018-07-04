using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudWebBar.Model.CustomModel.XCCloud
{
    [DataContract]
    public class DeviceWorkInfoModel
    {
        public DeviceWorkInfoModel(string mcuId,int status,DateTime createTime,string workStation)
        {
            this.MCUId = mcuId;
            this.WorkStation = workStation;
            this.Status = status;
            this.CreateTime = createTime;
            this.StatusName = GetWorkInfoStatusName(status);
        }

        private string GetWorkInfoStatusName(int status)
        {
            string statusName = string.Empty;
            switch (status)
            {
                case 0: statusName = "正常待机"; break;
                case 1: statusName = "正在出币"; break;
                case 2: statusName = "卡币或缺币"; break;
                case 3: statusName = "脱机"; break;
                default:return "无效状态";
            }
            return statusName;
        }

        [DataMember(Name = "mcuId", Order = 1)]
        public string MCUId { set; get; }

        public string WorkStation { set; get; }

        public int Status { set; get; }

        [DataMember(Name = "statusName", Order = 2)]
        public string StatusName { set; get; }

        public DateTime CreateTime { set; get; }
    }
}
