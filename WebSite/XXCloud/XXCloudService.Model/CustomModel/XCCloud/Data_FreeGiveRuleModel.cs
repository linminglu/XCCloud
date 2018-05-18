using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.Common.Enum;
using XCCloudService.Common;

namespace XCCloudService.Model.CustomModel.XCCloud
{
    public class Data_FreeGiveRuleList
    {
        /// <summary>
        /// 规则ID
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// 规则名称
        /// </summary>
        public string RuleName { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public string StartTime { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public string EndTime { get; set; }
        /// <summary>
        /// 赠送余额类别
        /// </summary>
        public int? FreeBalanceIndex { get; set; }
        /// <summary>
        /// 赠送余额类别
        /// </summary>
        public string FreeBalanceStr { get; set; }
        /// <summary>
        /// 赠送数量
        /// </summary>
        public int? FreeCount { get; set; }
        /// <summary>
        /// 允许散客
        /// </summary>
        public int? AllowGuest { get; set; }
        /// <summary>
        /// 允许散客
        /// </summary>
        public string AllowGuestStr { get { return AllowGuest == 1 ? "是" : AllowGuest == 0 ? "否" : string.Empty; } set { } }
        /// <summary>
        /// 散客身份确定方式
        /// </summary>
        public int? IDReadType { get; set; }
        /// <summary>
        /// 散客身份确定方式
        /// </summary>
        public string IDReadTypeStr { get { return IDReadType == 0 ? "手工录入" : IDReadType == 1 ? "读卡器读取" : string.Empty; } set { } }
        /// <summary>
        /// 赠送周期类别
        /// </summary>
        public int? PeriodType { get; set; }
        /// <summary>
        /// 赠送周期类别
        /// </summary>
        public string PeriodTypeStr { get { return PeriodType == 0 ? "固定周期" : PeriodType == 1 ? "自然周期" : string.Empty; } set { } }
        /// <summary>
        /// 间隔数量
        /// </summary>
        public int? SpanCount { get; set; }
        /// <summary>
        /// 间隔类别
        /// </summary>
        public int? SpanType { get; set; }
        /// <summary>
        /// 间隔类别ID
        /// </summary>
        public string SpanTypeStr { get { return ((SpanType?)SpanType).GetDescription(); } set { } }        
        /// <summary>
        /// 状态
        /// </summary>
        public int? State { get; set; }
        /// <summary>
        /// 优先级别
        /// </summary>
        public int? RuleLevel { get; set; }
    }
}
