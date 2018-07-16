using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
    public class Data_Game_StockInfo
    {
        public int ID { get; set; }
        public int GameIndex { get; set; }
        public int GoodID { get; set; }
        public int DeportID { get; set; }
        public int MinValue { get; set; }
        public int MaxValue { get; set; }
        public DateTime InitialTime { get; set; }
        public int InitialCount { get; set; }
        public int RemainCount { get; set; }
        public string Note { get; set; }
        public string Verifiction { get; set; }
    }
}
