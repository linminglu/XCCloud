using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
    public class Base_UserInfo
    {
        public int ID { get; set; }
        public int AgentID { get; set; }
        public string MerchID { get; set; }
        public string StoreID { get; set; }
        public int UserType { get; set; }
        public int IsAdmin { get; set; }
        public string LogName { get; set; }
        public string LogPassword { get; set; }
        public string OpenID { get; set; }
        public string RealName { get; set; }
        public string Mobile { get; set; }
        public string ICCardID { get; set; }
        public DateTime CreateTime { get; set; }
        public int Status { get; set; }
        public int Auditor { get; set; }
        public DateTime AuditorTime { get; set; }
        public int UserGroupID { get; set; }
        public int AuthorTempID { get; set; }
        public string UnionID { get; set; }
        public int SwitchMerch { get; set; }
        public int SwitchStore { get; set; }
        public int SwitchWorkstation { get; set; }
        public string Verifiction { get; set; }
    }
}
