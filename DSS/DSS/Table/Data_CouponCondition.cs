using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
    public class Data_CouponCondition
    {
        public int ID { get; set; }
        public int CouponID { get; set; }
        public int ConditionType { get; set; }
        public int ConditionID { get; set; }
        public int ConnectType { get; set; }
        public string ConditionValue { get; set; }
    }
}
