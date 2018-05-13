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
        /// 调拨出库门店
        /// </summary>
        public string OutStoreName { get; set; }
        /// <summary>
        /// 调拨出库仓库
        /// </summary>
        public string OutDepotName { get; set; }
        /// <summary>
        /// 出库时间
        /// </summary>
        public string OutDepotTime { get; set; }
        /// <summary>
        /// 调拨入库门店
        /// </summary>
        public string InStoreName { get; set; }
        /// <summary>
        /// 调拨入库仓库
        /// </summary>
        public string InDepotName { get; set; }
        /// <summary>
        /// 入库时间
        /// </summary>
        public string InDepotTime { get; set; }
        /// <summary>
        /// 调拨状态
        /// </summary>
        public int? State { get; set; }
        /// <summary>
        /// 调拨状态[字符串]
        /// </summary>
        public string StateStr { get { return State != null ? ((WorkflowState)State).GetDescription() : string.Empty; } set { } }
        /// <summary>
        /// 调拨说明
        /// </summary>
        public string Note { get; set; }
        /// <summary>
        /// 允许的操作
        /// </summary>
        public IEnumerable<int> PermittedTriggers { get; set; }
    }

}
