using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudService.Model.CustomModel.XCCloud
{
    public class Data_GoodsStockList
    {
        public int ID { get; set; }
        /// <summary>
        /// 商品条码
        /// </summary>
        public string Barcode { get; set; }
        /// <summary>
        /// 商品ID
        /// </summary>
        public int? GoodID { get; set; }
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
        /// 门店ID
        /// </summary>
        public string StoreID { get; set; }
        /// <summary>
        /// 门店名称
        /// </summary>
        public string StoreName { get; set; }               
        /// <summary>
        /// 备注
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// 商品来源
        /// </summary>
        public string Source { get { return string.IsNullOrEmpty(StoreID) ? "总店" : StoreName; } set { } }
        /// <summary>
        /// 库存下限
        /// </summary>
        public int? MinValue { get; set; }
        /// <summary>
        /// 库存上限
        /// </summary>
        public int? MaxValue { get; set; }
        /// <summary>
        /// 可调拨数
        /// </summary>
        public int? AvailableCount { get; set; }
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
        public decimal? InitialTotal { get { return InitialAvgValue * InitialValue; } set { } }
        /// <summary>
        /// 当前库存
        /// </summary>
        public int? RemainCount { get; set; }
        /// <summary>
        /// 当前库存金额
        /// </summary>
        public decimal? RemainTotal { get { return InitialAvgValue * RemainCount; } set { } }
    }

}
