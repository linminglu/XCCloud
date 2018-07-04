using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudWebBar.Model.CustomModel.XCCloud;

namespace XCCloudWebBar.CacheService
{
    public class FlwMemberRegisterCache
    {
        private static Dictionary<string, object> _memberRegisterHt = new Dictionary<string, object>();

        public static Dictionary<string, object> MemberRegisterHt
        {
            get { return _memberRegisterHt; }
        }

        public static void Add(FlwMemberRegisterCacheModel model)
        {
            _memberRegisterHt.Add(model.FlwOrderId, model);
        }

        public static FlwMemberRegisterCacheModel GetModel(string orderId)
        {
            return (FlwMemberRegisterCacheModel)(_memberRegisterHt[orderId]);
        }

        public static bool Exist(string orderId)
        {
            return _memberRegisterHt.ContainsKey(orderId);
        }

        public static void Remove(string storeId)
        {
            _memberRegisterHt.Remove(storeId);
        }
    }

    public class FlwMemberRegisterCacheModel
    {
        public FlwMemberRegisterCacheModel(string merchId, string storeId, string flwOrderId, string workStation, int workStationId, RegisterMember regMember)
        {
            this.MerchId = merchId;
            this.StoreId = storeId;
            this.FlwOrderId = flwOrderId;
            this.WorkStation = workStation;
            this.CreateTime = System.DateTime.Now;
            this.WorkStationId = workStationId;
            this.RegisterMember = regMember;
        }

        public string MerchId { set; get; }

        public string StoreId { set; get; }

        public string FlwOrderId { set; get; }

        public DateTime CreateTime { set; get; }

        public int WorkStationId { set; get; }

        public string WorkStation { set; get; }

        public RegisterMember RegisterMember { set; get; }
    }
}
