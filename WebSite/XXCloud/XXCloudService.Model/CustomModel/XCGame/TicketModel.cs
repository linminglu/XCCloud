using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using XCCloudService.Common.Enum;
using XCCloudService.Common;

namespace XCCloudService.Model.CustomModel.XCGame
{
    /// <summary>
    /// 门票详情
    /// </summary>
    [DataContract] 
    public class TicketModel
    {
        /// <summary>
        /// 门票名称
        /// </summary>
        [DataMember(Name = "ticketName", Order = 1)]
        public string TicketName { set; get; }

        /// <summary>
        /// 门票状态
        /// </summary>
        [DataMember(Name="state",Order=2)]
        public int? State { set; get; }

        /// <summary>
        /// 门票状态
        /// </summary>
        [DataMember(Name = "stateName", Order = 3)]
        public string StateName { get { return ((TicketState?)State).GetDescription(); } set { } }

        /// <summary>
        /// 门票类别
        /// </summary>
        [DataMember(Name = "ticketType", Order = 4)]
        public int? TicketType { set; get; }

        /// <summary>
        /// 门票类别
        /// </summary>
        [DataMember(Name="ticketTypeName",Order=4)]
        public string TicketTypeName { get { return ((TicketType?)TicketType).GetDescription(); } set { } }

        /// <summary>
        /// 剩余数量
        /// </summary>
        [DataMember(Name = "remainCount", Order = 5)]
        public int? RemainCount { set; get; }

        /// <summary>
        /// 有效日期
        /// </summary>
        [DataMember(Name = "endTime", Order = 6)]
        public DateTime? EndTime { set; get; }

        /// <summary>
        /// 使用说明
        /// </summary>
        [DataMember(Name = "note", Order = 7)]
        public string Note { set; get; }
    }    
}