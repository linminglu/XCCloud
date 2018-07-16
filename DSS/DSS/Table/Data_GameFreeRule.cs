using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
    public class Data_GameFreeRule
    {
        public int ID { get; set; }
        public string MerchID { get; set; }
        public int RuleType { get; set; }
        public int GameIndexID { get; set; }
        public int MemberLevelID { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int NeedCoin { get; set; }
        public int FreeCoin { get; set; }
        public int ExitCoin { get; set; }
        public int State { get; set; }
        public DateTime CreateTime { get; set; }
        public string Verifiction { get; set; }
    }
}
