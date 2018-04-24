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
        public string Barcode { get; set; }
        public string GoodTypeStr { get; set; }
        public string GoodName { get; set; }
        public string StoreName { get; set; }
        public Nullable<DateTime> RealTime { get; set; }
        public string Supplier { get; set; }
        public decimal? Price { get; set; }
        public decimal? TotalPrice { get; set; }
        public string LogName { get; set; }
        public string DepotName { get; set; }        
    }
}
