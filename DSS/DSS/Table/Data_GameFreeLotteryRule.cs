using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
    public class Data_GameFreeLotteryRule
    {
        public int ID { get; set; }
        public string MerchID { get; set; }
        public string StoreID { get; set; }
        public int GameIndex { get; set; }
        public int MemberLevelID { get; set; }
        public int BaseLottery { get; set; }
        public int FreeCount { get; set; }
        public string Verifiction { get; set; }
    }
}
