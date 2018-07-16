using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
    public class Data_GoodStorage_Detail
    {
        public int ID { get; set; }
        public string MerchID { get; set; }
        public string StorageOrderID { get; set; }
        public int DepotID { get; set; }
        public int GoodID { get; set; }
        public int StorageCount { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal Price { get; set; }
        public decimal Tax { get; set; }
        public decimal TaxPrice { get; set; }
        public string Verifiction { get; set; }
    }
}
