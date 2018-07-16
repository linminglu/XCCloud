using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
    public class Data_FreeGiveRule
    {
        public int ID { get; set; }
        public string MerchID { get; set; }
        public string RuleName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int FreeBalanceIndex { get; set; }
        public int FreeCount { get; set; }
        public int AllowGuest { get; set; }
        public int IDReadType { get; set; }
        public int PeriodType { get; set; }
        public int SpanCount { get; set; }
        public int SpanType { get; set; }
        public int GetTimes { get; set; }
        public int RuleLevel { get; set; }
        public int State { get; set; }
        public string Verifiction { get; set; }
    }
}
