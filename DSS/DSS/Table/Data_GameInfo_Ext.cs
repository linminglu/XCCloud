using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
    public class Data_GameInfo_Ext
    {
        public int ID { get; set; }
        public int GameID { get; set; }
        public string GameCode { get; set; }
        public decimal Area { get; set; }
        public DateTime ChangeTime { get; set; }
        public int Evaluation { get; set; }
        public int Price { get; set; }
        public string MerchID { get; set; }
        public string StoreID { get; set; }
        public int ValidFlag { get; set; }
        public decimal LowLimit { get; set; }
        public decimal HighLimit { get; set; }
    }
}
