using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudWebBar.Common;

namespace XCCloudWebBar.Model.CustomModel.XCCloud
{
    public class Data_Push_RuleList
    {
        public int ID { get; set; }
        public string GameName { get; set; }
        public string MemberLevelName { get; set; }
        private string Week { get; set; }
        public int? PushCoin1 { get; set; }
        public int? PushCoin2 { get; set; }
        public int? Allow_Out { get; set; }
        public int? Allow_In { get; set; }
        public int? Level { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string Period
        {
            get
            {
                return Utils.TimeSpanToStr(StartTime) + "~" + Utils.TimeSpanToStr(EndTime);
            }
            set { }
        }       
    }
}
