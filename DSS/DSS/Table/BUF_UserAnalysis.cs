using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
    public class BUF_UserAnalysis
    {
        public int ID { get; set; }
        public string MerchID { get; set; }
        public string StoreID { get; set; }
        public string MemberID { get; set; }
        public DateTime CheckDate { get; set; }
        public decimal CurrentUsePrice { get; set; }
    }
}
