using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudService.Model.CustomModel.XCCloud
{
    public class Data_CouponInfoModel
    {
        public int ID { get; set; }
        public string CouponName { get; set; }
        public int? EntryCouponFlag { get; set; }
        public int? CouponType { get; set; }
        public string CouponTypeStr { get; set; }
        public int? PublishCount { get; set; }
        public int? UseCount { get; set; }
        public int? NotAssignedCount { get; set; }
        public int? NotActivatedCount { get; set; }
        public int? ActivatedCount { get; set; }
        private int? _assignedCount;
        public int? AssignedCount
        {
            get
            {
                return PublishCount - NotAssignedCount;
            }
            set
            {
                _assignedCount = value;
            }
        }
        private int? _sendCount;
        public int? SendCount
        {
            get
            {
                return UseCount + ActivatedCount;
            }
            set
            {
                _sendCount = value;
            }
        }     
        public int? AuthorFlag { get; set; }
        public int? AllowOverOther { get; set; }
        public int? OpUserID { get; set; }
        public string OpUserName { get; set; }
        public int? IsLock { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string CreateTime { get; set; }     
        public string Context { get; set; }
    }
}
