using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.Common.Enum;
using XCCloudService.Common;

namespace XCCloudService.Model.CustomModel.XCCloud
{
    public class Flw_MemberGivebackList
    {
        public string ID { get; set; }
        public string ICCardID { get; set; }
        public DateTime? RealTime { get; set; }
        public int? MayCoins { get; set; }
        public int? Coins { get; set; }
        public decimal? WinMoney { get; set; }
        public DateTime? LastTime { get; set; }               
        public string LogName { get; set; }
        public string WorkStation { get; set; }
        public string AuthorName { get; set; }
        public string Note { get; set; }
    }   
}
