using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
    public class Data_Parameters
    {
        public int ID { get; set; }
        public string StoreID { get; set; }
        public string System { get; set; }
        public string ParameterName { get; set; }
        public int IsAllow { get; set; }
        public string ParameterValue { get; set; }
        public string Note { get; set; }
    }
}
