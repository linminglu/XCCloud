using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudService.Model.CustomModel.XCCloud
{
    [DataContract]
    public class MemberCouponDetailModel
    {
        [DataMember(Name = "couponId", Order = 1)]
        public int CouponId { get; set; }

        [DataMember(Name = "couponName", Order = 2)]
        public string CouponName { get; set; }

        [DataMember(Name = "couponType", Order = 3)]
        public int CouponType { get; set; }

        [DataMember(Name = "couponCode", Order = 4)]
        public string CouponCode { get; set; }

        [DataMember(Name = "startDate", Order = 5)]
        public string StartDate { get; set; }

        [DataMember(Name = "endDate", Order = 6)]
        public string EndDate { get; set; }

        [DataMember(Name = "startTime", Order = 7)]
        public string StartTime { get; set; }

        [DataMember(Name = "endTime", Order = 8)]
        public string EndTime { get; set; }

        [DataMember(Name = "noStartDate", Order = 9)]
        public string NoStartDate { get; set; }

        [DataMember(Name = "noEndDate", Order = 10)]
        public string NoEndDate { get; set; }

        [DataMember(Name = "useDay", Order = 11)]
        public string UseDay { get; set; }

        [DataMember(Name = "useDesc", Order = 12)]
        public string UseDesc { get; set; }

        [DataMember(Name = "jackpotDesc", Order = 13)]
        public string JackpotDesc { get; set; }

        [DataMember(Name = "freeCouponDesc", Order = 14)]
        public string FreeCouponDesc { get; set; }

        [DataMember(Name = "stateName", Order = 15)]
        public string StateName { get; set; }

        [DataMember(Name = "lockStatuName", Order = 16)]
        public string LockStatuName { get; set; }
    }

    [DataContract]
    public class MemberCouponTypeModel
    {
        [DataMember(Name = "couponType", Order = 1)]
        public int CouponType { get; set; }

        [DataMember(Name = "couponTypeName", Order = 2)]
        public string CouponTypeName { get; set; }

        [DataMember(Name = "MemberCouponDetail", Order = 3)]
        public List<MemberCouponDetailModel> MemberCouponDetail { set; get; }
    }

    
}
