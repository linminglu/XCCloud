using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudWebBar.Common.Enum;
using XCCloudWebBar.Common;

namespace XCCloudWebBar.Model.CustomModel.XCCloud
{
    public class Data_ProjectInfoList
    {
        public int ID { get; set; }
        public string ProjectName { get; set; }
        public int? ProjectType { get; set; }
        public string ProjectTypeStr { get; set; }
        public int? AreaType { get; set; }
        public string AreaName { get; set; }
        public int? ChargeType { get; set; }
        public string ChargeTypeStr { get { return ChargeType == 0 ? "按次" : ChargeType == 1 ? "计时" : string.Empty; } }
        public int? State { get; set; }
        public string Note { get; set; }
    }
}
