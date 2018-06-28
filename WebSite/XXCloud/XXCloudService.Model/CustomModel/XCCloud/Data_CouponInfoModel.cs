using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.Common.Enum;

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
        public int? AssignedCount
        {
            get
            {
                return EntryCouponFlag == (int)CouponFlag.Entry ? (PublishCount - NotAssignedCount) : 0; //电子券直接派发，调拨数默认为0
            }
            set { }
        }
        public int? SendCount
        {
            get
            {
                return UseCount + ActivatedCount;
            }
            set { }
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

    [DataContract]
    public class Data_CouponParamModel
    {
        [DataMember(Name = "couponId", Order = 1)]
        public int CouponId { set; get; }

        [DataMember(Name = "couponCode", Order = 2)]
        public string CouponCode { set; get; }

        [DataMember(Name = "couponFee", Order = 3)]
        public decimal CouponFee { set; get; }

        [DataMember(Name = "couponType", Order = 4)]
        public int CouponType { set; get; }

        [DataMember(Name = "couponTypeName", Order = 5)]
        public string CouponTypeName { set; get; }

        [DataMember(Name = "couponNote", Order = 6)]
        public string CouponNote { set; get; }
    }
}
