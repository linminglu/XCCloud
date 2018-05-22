using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudService.Model.CustomModel.XCCloud
{
    public class Data_GoodStorageList
    {
        public int ID { get; set; }
        public string StorageOrderID { get; set; }
        public string StoreName { get; set; }
        public string RealTime { get; set; }
        public string Supplier { get; set; }
        public int? StorageCount { get; set; }
        public decimal? TaxPrice { get; set; }
        public decimal? TotalPrice { get; set; }
        public string LogName { get; set; }
        public string DepotName { get; set; }
        public int? AuthorFlag { get; set; }
        public string AuthorFlagStr { get { return AuthorFlag == 0 ? "未审核" : AuthorFlag == 1 ? "已审核" : AuthorFlag == 2 ? "已拒绝" : string.Empty; } set { } }        
    }
}
