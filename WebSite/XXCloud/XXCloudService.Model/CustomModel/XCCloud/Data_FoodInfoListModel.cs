using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudService.Model.CustomModel.XCCloud
{
    public class Data_FoodInfoListModel
    {
        public int FoodID { get; set; }
        public string FoodName { get; set; }
        public decimal? ClientPrice { get; set; }
        public decimal? MemberPrice { get; set; }
        public string FoodTypeStr { get; set; }
        public string AllowInternet { get; set; }
        public string AllowPrint { get; set; }
        public string ForeAuthorize { get; set; }
        public string AllowQuickFood { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }
}
