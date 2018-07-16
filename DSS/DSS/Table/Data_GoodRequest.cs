using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
    public class Data_GoodRequest
    {
        public int ID { get; set; }
        public string RequestCode { get; set; }
        public string MerchID { get; set; }
        public string CreateStoreID { get; set; }
        public int CreateUserID { get; set; }
        public DateTime CreateTime { get; set; }
        public int RequstType { get; set; }
        public string RequestReason { get; set; }
        public string RequestOutStoreID { get; set; }
        public int RequestOutDepotID { get; set; }
        public string RequestInStoreID { get; set; }
        public int RequestInDepotID { get; set; }
        public DateTime CheckDate { get; set; }
        public string Verifiction { get; set; }
    }
}
