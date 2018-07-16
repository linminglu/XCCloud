using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
    public class Data_GivebackRule
    {
        public int ID { get; set; }
        public string MerchID { get; set; }
        public int MemberLevelID { get; set; }
        public int BackMin { get; set; }
        public int BackMax { get; set; }
        public int BackScale { get; set; }
        public int ExitCardMin { get; set; }
        public int AllowBackPrincipal { get; set; }
        public int Backtype { get; set; }
        public int TotalDays { get; set; }
        public int AllowContainToday { get; set; }
        public string Verifiction { get; set; }
    }
}
