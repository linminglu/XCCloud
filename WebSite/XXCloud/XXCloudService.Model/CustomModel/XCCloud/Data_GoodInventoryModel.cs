using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.Common.Enum;
using XCCloudService.Common;

namespace XCCloudService.Model.CustomModel.XCCloud
{
    public class Data_GoodInventoryList
    {
        public int ID { get; set; }
        /// <summary>
        /// 商品条码
        /// </summary>
        public string Barcode { get; set; }
        /// <summary>
        /// 商品ID
        /// </summary>
        public int GoodID { get; set; }
        /// <summary>
        /// 商品类别
        /// </summary>
        public int? GoodType { get; set; }
        /// <summary>
        /// 商品类别[字符串]
        /// </summary>
        public string GoodTypeStr { get; set; }
        /// <summary>
        /// 商品名称
        /// </summary>
        public string GoodName { get; set; }
        /// <summary>
        /// 盘点类别
        /// </summary>
        public int? InventoryType { get; set; }
        /// <summary>
        /// 添加来源
        /// </summary>
        public string Source { get; set; }
        /// <summary>
        /// 库存下限
        /// </summary>
        public int? MinValue { get; set; }
        /// <summary>
        /// 库存上限
        /// </summary>
        public int? MaxValue { get; set; }
        /// <summary>
        /// 期初时间
        /// </summary>
        public string InitialTime { get; set; }
        /// <summary>
        /// 期初库存
        /// </summary>
        public int? InitialValue { get; set; }
        /// <summary>
        /// 期初平均成本
        /// </summary>
        public decimal? InitialAvgValue { get; set; }
        /// <summary>
        /// 期初库存金额
        /// </summary>
        public decimal? InitialTotal { get { return Math.Round((InitialValue * InitialAvgValue) ?? 0, 2, MidpointRounding.AwayFromZero); } set { } }
        /// <summary>
        /// 当前库存
        /// </summary>
        public int? RemainCount { get; set; }
        /// <summary>
        /// 当前库存金额
        /// </summary>
        public decimal? RemainTotal { get { return Math.Round((RemainCount * InitialAvgValue) ?? 0, 2, MidpointRounding.AwayFromZero); } set { } }
        /// <summary>
        /// 盘点人
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 盘点时间
        /// </summary>
        public string InventoryTime { get; set; }
        /// <summary>
        /// 盘点数量
        /// </summary>
        public int? InventoryCount { get; set; }
        /// <summary>
        /// 误差数量
        /// </summary>
        public int? ErrorCount { get { return Math.Abs((RemainCount - InventoryCount) ?? 0); } set { } }        
        /// <summary>
        /// 误差金额
        /// </summary>
        public decimal? ErrorTotal { get { return Math.Round((ErrorCount * InitialAvgValue) ?? 0, 2, MidpointRounding.AwayFromZero); } set { } }
        /// <summary>
        /// 误差率
        /// </summary>
        public decimal? ErrorRate { get { return RemainTotal > 0 ? (Math.Round(((ErrorCount * InitialAvgValue * 100) / RemainTotal) ?? 0, 2, MidpointRounding.AwayFromZero)) : 0; } set { } }
    }
}
