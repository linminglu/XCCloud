using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.Common.Enum;
using XCCloudService.Common;

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
        public string AuthorFlagStr { get { return ((GoodOutInState?)AuthorFlag).GetDescription(); } set { } }        
    }
}
