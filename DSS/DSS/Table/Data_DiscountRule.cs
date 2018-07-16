using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
    public class Data_DiscountRule
    {
        public int ID { get; set; }
        public string MerchID { get; set; }
        public string RuleName { get; set; }
        public int RuleLevel { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime Enddate { get; set; }
        public int WeekType { get; set; }
        public string Week { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime NoStartDate { get; set; }
        public DateTime NoEndDate { get; set; }
        public int StoreFreq { get; set; }
        public int StoreCount { get; set; }
        public int ShareCount { get; set; }
        public int MemberFreq { get; set; }
        public int MemberCount { get; set; }
        public int AllowGuest { get; set; }
        public string Note { get; set; }
        public int State { get; set; }
        public string Verifiction { get; set; }
    }
}
