using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
    public class Flw_GameAPP_MemberRule
    {
        public string ID { get; set; }
        public int RuleID { get; set; }
        public string MerchID { get; set; }
        public string StoreID { get; set; }
        public int GameIndexID { get; set; }
        public int DeviceID { get; set; }
        public string SiteName { get; set; }
        public int MemberLevelID { get; set; }
        public int PushBalanceIndex1 { get; set; }
        public decimal PushCoin1 { get; set; }
        public int PushBalanceIndex2 { get; set; }
        public decimal PushCoin2 { get; set; }
        public int PlayCount { get; set; }
        public string Verifiction { get; set; }
    }
}
