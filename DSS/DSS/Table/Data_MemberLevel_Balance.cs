using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
    public class Data_MemberLevel_Balance
    {
        public int ID { get; set; }
        public string MerchID { get; set; }
        public int MemberLevelID { get; set; }
        public int BalanceIndex { get; set; }
        public int NeedAuthor { get; set; }
        public int ChargeOFF { get; set; }
        public int MaxSaveCount { get; set; }
        public int MaxUplife { get; set; }
        public string Verifiction { get; set; }
    }
}
