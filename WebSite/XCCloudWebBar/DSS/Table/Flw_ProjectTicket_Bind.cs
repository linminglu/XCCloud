using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
    public class Flw_ProjectTicket_Bind
    {
        public string ID { get; set; }
        public string MerchID { get; set; }
        public string ProjectCode { get; set; }
        public int ProjectID { get; set; }
        public int BuyCount { get; set; }
        public int RemainCount { get; set; }
        public int AllowShareCount { get; set; }
        public int WeightValue { get; set; }
        public string Verifiction { get; set; }
    }
}
