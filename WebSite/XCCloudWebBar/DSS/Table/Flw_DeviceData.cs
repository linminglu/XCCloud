using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
    public class Flw_DeviceData
    {
        public string ID { get; set; }
        public string MerchID { get; set; }
        public string StoreID { get; set; }
        public int DeviceID { get; set; }
        public int GameIndexID { get; set; }
        public string SiteName { get; set; }
        public int SN { get; set; }
        public int ACKControlValue { get; set; }
        public int ACKPulesCount { get; set; }
        public int ACKFreeCoin { get; set; }
        public int BusinessType { get; set; }
        public int State { get; set; }
        public DateTime RealTime { get; set; }
        public string MemberID { get; set; }
        public string CreateStoreID { get; set; }
        public string MemberName { get; set; }
        public string ICCardID { get; set; }
        public int BalanceIndex { get; set; }
        public decimal Coin { get; set; }
        public decimal RemainBalance { get; set; }
        public string OrderID { get; set; }
        public string Note { get; set; }
        public DateTime CheckDate { get; set; }
        public string Verifiction { get; set; }
    }
}
