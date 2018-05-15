using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudService.Model.CustomModel.XCCloud
{
    public class Base_GoodsInfoList
    {
        public int ID { get; set; }
        /// <summary>
        /// 商品条码
        /// </summary>
        public string Barcode { get; set; }
        /// <summary>
        /// 商品类别
        /// </summary>
        public int GoodType { get; set; }
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
        /// 总店ID
        /// </summary>
        public string MerchID { get; set; }
        /// <summary>
        /// 总店名称
        /// </summary>
        public string MerchName { get; set; }
        /// <summary>
        /// 入库状态
        /// </summary>
        public int? AllowStorage { get; set; }
        /// <summary>
        /// 入库状态[字符串]
        /// </summary>
        public string AllowStorageStr { get { return AllowStorage == 1 ? "允许" : "不允许"; } set { } }
        /// <summary>
        /// 备注
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// 商品来源
        /// </summary>
        public string Source { get { return string.IsNullOrEmpty(StoreID) ? "总店" : StoreName; } set { } }

        /// <summary>
        /// 销售价格
        /// </summary>
        public decimal? Price { get; set; }

        /// <summary>
        /// 销售积分
        /// </summary>
        public int? AllowCreatePoint { get; set; }
    }

}
