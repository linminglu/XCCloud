using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
    public class Data_Jackpot_Level
    {
        public int ID { get; set; }
        public string MerchID { get; set; }
        public int ActiveID { get; set; }
        public string LevelName { get; set; }
        public int GoodID { get; set; }
        public int GoodCount { get; set; }
        public decimal Probability { get; set; }
        public string Verifiction { get; set; }
    }
}
