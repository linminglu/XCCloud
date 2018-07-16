using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
    public class Base_MerchantInfo
    {
        public string ID { get; set; }
        public int MerchType { get; set; }
        public string MerchAccount { get; set; }
        public int MerchStatus { get; set; }
        public string MerchPassword { get; set; }
        public string MerchSecret { get; set; }
        public int ProxyType { get; set; }
        public int MerchTag { get; set; }
        public string WxOpenID { get; set; }
        public string WxUnionID { get; set; }
        public string Mobil { get; set; }
        public string MerchName { get; set; }
        public int AllowCreateCount { get; set; }
        public int AllowCreateSub { get; set; }
        public int CreateType { get; set; }
        public string CreateUserID { get; set; }
        public string Comment { get; set; }
        public string Verifiction { get; set; }
    }
}
