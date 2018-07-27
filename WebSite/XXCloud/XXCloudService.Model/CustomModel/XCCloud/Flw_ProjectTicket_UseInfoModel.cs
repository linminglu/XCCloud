using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.Common.Enum;
using XCCloudService.Common;

namespace XCCloudService.Model.CustomModel.XCCloud
{
    public class Flw_ProjectTicket_UseInfoList
    {
        public string ID { get; set; }
        public string Barcode { get; set; }
        public DateTime? CheckDate { get; set; }
        public string ProjectName { get; set; }
        public string AreaName { get; set; }
        public string TicketName { get; set; }
        public int? TicketType { get; set; }
        public string TicketTypeStr { get { return TicketType == 0 ? "主票" : TicketType == 1 ? "陪同票" : string.Empty; } set { } }
        public string MemeberName { get; set; }
        public int? BuyCount { get; set; }
        public int? RemainCount { get; set; }
        public string InOrOutStateStr { get; set; }
        public DateTime? InTime { get; set; }
        public string InSiteName { get; set; }
        public string InDeviceName { get; set; }
        public int? Intype { get; set; }
        public string InInfo { get { return InSiteName + "+" + InDeviceName + "+" + ((DeviceType?)Intype).GetDescription(); } set { } }
        public DateTime? OutTime { get; set; }
        public string OutSiteName { get; set; }
        public string OutDeviceName { get; set; }
        public int? Outtype { get; set; }
        public string OutInfo { get { return OutSiteName + "+" + OutDeviceName + "+" + ((DeviceType?)Outtype).GetDescription(); } set { } }
        public string IsReentry { get; set; }
        public int? PlayTime { get; set; }
        public int? LogType { get; set; }
        public string LogTypeStr { get { return ((TicketDeviceLogType?)LogType).GetDescription(); } set { } }
        public string BalanceIndexStr { get; set; }
        public decimal? Total { get; set; }
    }   
}
