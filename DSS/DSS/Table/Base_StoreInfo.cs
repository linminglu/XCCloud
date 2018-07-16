using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.Table
{
    public class Base_StoreInfo
    {
        public string ID { get; set; }
        public string ParentID { get; set; }
        public string MerchID { get; set; }
        public string StoreName { get; set; }
        public int StoreTag { get; set; }
        public string Password { get; set; }
        public DateTime AuthorExpireDate { get; set; }
        public string AreaCode { get; set; }
        public string Address { get; set; }
        public decimal Lng { get; set; }
        public decimal Lat { get; set; }
        public string Contacts { get; set; }
        public string IDCard { get; set; }
        public DateTime IDExpireDate { get; set; }
        public string Mobile { get; set; }
        public string ShopSignPhoto { get; set; }
        public string LicencePhoto { get; set; }
        public string LicenceID { get; set; }
        public DateTime LicenceExpireDate { get; set; }
        public string BankType { get; set; }
        public string BankCode { get; set; }
        public string BankAccount { get; set; }
        public int SelttleType { get; set; }
        public int SettleID { get; set; }
        public int StoreState { get; set; }
        public string Verifiction { get; set; }
    }
}
