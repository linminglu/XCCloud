using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.Common.Enum;
using XCCloudService.Common;

namespace XCCloudService.Model.CustomModel.XCCloud
{
    public class Data_GoodOutOrderList
    {
        public int ID { get; set; }
        public string OrderID { get; set; }
        public int? OrderType { get; set; }
        public string OrderTypeStr { get { return ((GoodOutOrderType?)OrderType).GetDescription(); } set { } }
        public string CreateTime { get; set; }
        public int? OutCount { get; set; }
        public decimal? OutTotal { get; set; }
        public string LogName { get; set; }
        public string DepotName { get; set; }
        public int? State { get; set; }
        public string StateStr { get { return ((GoodOutInState?)State).GetDescription(); } set { } }
        public string CheckDate { get; set; }
    }
}
