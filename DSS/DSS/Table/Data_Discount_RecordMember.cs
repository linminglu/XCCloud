using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
    public class Data_Discount_RecordMember
    {
        public int ID { get; set; }
        public string MerchID { get; set; }
        public string StoreID { get; set; }
        public int MemberID { get; set; }
        public int DiscountRuleID { get; set; }
        public DateTime RecordDate { get; set; }
        public int FreqType { get; set; }
        public int UseCount { get; set; }
        public string Verifiction { get; set; }
    }
}
