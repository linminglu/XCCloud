using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudService.Model.Socket.UDP
{    
    [DataContract]
    public class DeviceControlRequestDataModel
    {
        public DeviceControlRequestDataModel(string storeId, string mobile, string icCardId, string segment, string mcuid, string action, int count, string sn, string orderId, string storePassword, int foodId, string ruleId, string ruleType)
        {
            this.StoreId = storeId;
            this.Mobile = mobile;
            this.Segment = segment;
            this.MCUId = mcuid;
            this.Action = action;
            this.Coins = count;
            this.SN = sn;
            this.OrderId = orderId;
            this.StorePassword = storePassword;
            this.FoodId = foodId;
            this.IcCardId = icCardId;
            this.RuleId = ruleId;
            this.RuleType = ruleType;
        }

        /// <summary>
        /// 门店编号
        /// </summary>
        [DataMember(Name = "storeId", Order = 1)]
        public string StoreId { set; get; }

        /// <summary>
        /// 手机号码
        /// </summary>
        [DataMember(Name = "mobile", Order = 1)]
        public string Mobile { set; get; }

        /// <summary>
        /// 会员卡号
        /// </summary>
        [DataMember(Name = "icCardId", Order = 1)]
        public string IcCardId { set; get; }

        /// <summary>
        /// 段地址
        /// </summary>
        [DataMember(Name = "segment", Order = 2)]
        public string Segment { set; get; }

        /// <summary>
        /// 门店内雷达段号
        /// </summary>
        [DataMember(Name = "mcuid", Order = 3)]
        public string MCUId { set; get; }

        /// <summary>
        /// 控制类别1 出币
        /// </summary>
        [DataMember(Name = "action", Order = 4)]
        public string Action { set; get; }

        /// <summary>
        /// 控制计数
        /// </summary>
        [DataMember(Name = "count", Order = 5)]
        public int Coins { set; get; }


        /// <summary>
        /// 业务流水号
        /// </summary>
        [DataMember(Name = "sn", Order = 6)]
        public string SN { set; get; }


        /// <summary>
        /// 订单号
        /// </summary>
        [DataMember(Name = "orderid", Order = 7)]
        public string OrderId { set; get; }

        public string StorePassword { set; get; }

        public int FoodId { set; get; }

        [DataMember(Name = "ruleId", Order = 8)]
        public string RuleId { set; get; }

        [DataMember(Name = "ruletype", Order = 9)]
        public string RuleType { set; get; }


    }

    [Serializable]
    [DataContract]
    public class RemoteDeviceControlRequestDataModel
    {
        public RemoteDeviceControlRequestDataModel(string token, string mcuid, string iccardId, string action, string ruleType, string ruleId, string count, string orderId, string sn)
        {
            this.Token = token;
            this.MCUID = mcuid;
            this.ICCardId = iccardId;
            this.Action = action;
            this.RuleType = ruleType;
            this.RuleId = ruleId;
            this.Count = count;
            this.OrderId = orderId;
            this.SN = sn;
        }

        [DataMember(Name = "token", Order = 0)]
        public string Token { get; set; }

        [DataMember(Name = "mcuid", Order = 1)]
        public string MCUID { get; set; }

        [DataMember(Name = "iccardid", Order = 2)]
        public string ICCardId { get; set; }

        [DataMember(Name = "action", Order = 3)]
        public string Action { get; set; }

        [DataMember(Name = "ruletype", Order = 4)]
        public string RuleType { get; set; }

        [DataMember(Name = "ruleid", Order = 5)]
        public string RuleId { get; set; }

        [DataMember(Name = "count", Order = 6)]
        public string Count { get; set; }

        [DataMember(Name = "orderid", Order = 7)]
        public string OrderId { get; set; }

        [DataMember(Name = "sn", Order = 8)]
        public string SN { get; set; }

        [DataMember(Name = "signkey", Order = 9)]
        public string SignKey { get; set; }
    }
}
