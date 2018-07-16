using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
    public class Base_UserGroup_Grant
    {
        public int ID { get; set; }
        public int GroupID { get; set; }
        public int FunctionID { get; set; }
        public int IsAllow { get; set; }
    }
}
