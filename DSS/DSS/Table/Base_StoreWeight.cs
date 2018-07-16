using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
    public class Base_StoreWeight
    {
        public int ID { get; set; }
        public string MerchID { get; set; }
        public string StoreID { get; set; }
        public int BossID { get; set; }
        public int WeightValue { get; set; }
        public int WeightType { get; set; }
    }
}
