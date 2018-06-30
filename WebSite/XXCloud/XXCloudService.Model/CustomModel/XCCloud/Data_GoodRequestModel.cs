using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.Common;
using XCCloudService.Common.Enum;

namespace XCCloudService.Model.CustomModel.XCCloud
{
    public class Data_GoodRequestList
    {
        /// <summary>
        /// 调拨单ID
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// 调拨单号
        /// </summary>
        public string RequestCode { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public string CreateTime { get; set; }
        /// <summary>
        /// 创建门店
        /// </summary>
        public string CreateStore { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateUser { get; set; }
        /// <summary>
        /// 调拨方式
        /// </summary>
        public int? RequstType { get; set; }
        /// <summary>
        /// 调拨方式[字符串]
        /// </summary>
        public string RequstTypeStr { get { return RequstType != null ? ((RequestType)RequstType).GetDescription() : string.Empty; } set { } }
        /// <summary>
        /// 调拨出库门店ID
        /// </summary>
        public string OutStoreID { get; set; }
        /// <summary>
        /// 调拨出库门店
        /// </summary>
        public string OutStoreName { get; set; }
        /// <summary>
        /// 调拨出库仓库ID
        /// </summary>
        public int? OutDepotID { get; set; }
        /// <summary>
        /// 调拨出库仓库
        /// </summary>
        public string OutDepotName { get; set; }
        /// <summary>
        /// 出库时间
        /// </summary>
        public string OutDepotTime { get; set; }
        /// <summary>
        /// 调拨入库门店ID
        /// </summary>
        public string InStoreID { get; set; }
        /// <summary>
        /// 调拨入库门店
        /// </summary>
        public string InStoreName { get; set; }
        /// <summary>
        /// 调拨入库仓库ID
        /// </summary>
        public int? InDepotID { get; set; }
        /// <summary>
        /// 调拨入库仓库
        /// </summary>
        public string InDepotName { get; set; }
        /// <summary>
        /// 入库时间
        /// </summary>
        public string InDepotTime { get; set; }
        /// <summary>
        /// 调拨原因
        /// </summary>
        public string RequestReason { get; set; }
        /// <summary>
        /// 调拨状态
        /// </summary>
        public int? State { get; set; }
        /// <summary>
        /// 调拨状态[字符串]
        /// </summary>
        public string StateStr { get { return State != null ? ((State)State).GetDescription() : string.Empty; } set { } }
        /// <summary>
        /// 调拨说明
        /// </summary>
        public string Note { get; set; }
        /// <summary>
        /// 允许的操作
        /// </summary>
        public IEnumerable<int> PermittedTriggers { get; set; }
    }

    public class Data_GoodRequest_ListList
    {
        public int ID { get; set; }
        public int? GoodID { get; set; }
        public string Barcode { get; set; }
        public string GoodName { get; set; }
        public string GoodTypeStr { get; set; }
        public int? RequestCount { get; set; }
        public int? SendCount { get; set; }
        public int? StorageCount { get; set; }
        public decimal? CostPrice { get; set; }
        public decimal? Tax { get; set; }
        public int? RemainCount { get; set; }
        public int? MinValue { get; set; }
        public int? AvailableCount { get { var availableCount = (RemainCount ?? 0) - (MinValue ?? 0); return availableCount > 0 ? availableCount : 0; } set { } }
        public int? LogistType { get; set; }
        public string LogistTypeStr { get; set; }
        public string LogistOrderID { get; set; }
        public string SendTime { get; set; }
        public int? OutDepotID { get; set; }
        public string OutDepotName { get; set; }
    }

    public class Data_GoodSendDealList
    {
        public int RequestID { get; set; }
        public int? GoodID { get; set; }
        public string Barcode { get; set; }
        public string GoodName { get; set; }
        public string GoodTypeStr { get; set; }
        public int? RequestCount { get; set; }
        public int? FinishCount { get; set; }        
    }

    public class Data_GoodRequestDealList
    {
        public int ID { get; set; }
        public int RequestID { get; set; }
        public int? GoodID { get; set; }
        public string Barcode { get; set; }
        public string GoodName { get; set; }
        public string GoodTypeStr { get; set; }
        public int? RequestCount { get; set; }
        public int? SendCount { get; set; }
        public decimal? CostPrice { get; set; }
        public decimal? Tax { get; set; }
    }

    public class Data_GoodRequestExitList
    {
        public int ID { get; set; }
        public int RequestID { get; set; }
        public int? GoodID { get; set; }
        public string Barcode { get; set; }
        public string GoodName { get; set; }
        public string GoodTypeStr { get; set; }
        public int? RequestCount { get; set; }
        public int? SendCount { get; set; }
        public int? StorageCount { get; set; }
        public int? ExitedCount { get; set; }
        public decimal? CostPrice { get; set; }
        public decimal? Tax { get; set; }
    }
}
