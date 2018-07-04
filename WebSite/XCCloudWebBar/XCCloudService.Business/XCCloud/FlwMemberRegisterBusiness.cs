using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudWebBar.CacheService;

namespace XCCloudWebBar.Business.XCCloud
{
    public class FlwMemberRegisterBusiness
    {
        public static void Add(FlwMemberRegisterCacheModel model)
        {
            FlwMemberRegisterCache.Add(model);
        }

        public static FlwMemberRegisterCacheModel GetModel(string orderId)
        {
            return FlwMemberRegisterCache.GetModel(orderId);
        }

        public static bool Exist(string orderId)
        {
            return FlwMemberRegisterCache.Exist(orderId);
        }

        public static void Remove(string storeId)
        {
            FlwMemberRegisterCache.Remove(storeId);
        }

        public static List<FlwMemberRegisterCacheModel> GetOrderListByWorkStation(string storeId, string workStation)
        {
            List<FlwMemberRegisterCacheModel> list = new List<FlwMemberRegisterCacheModel>();
            var query = from item in FlwMemberRegisterCache.MemberRegisterHt
                        where ((FlwMemberRegisterCacheModel)(item.Value)).StoreId.Equals(storeId) && ((FlwMemberRegisterCacheModel)(item.Value)).WorkStation == workStation
                        select item.Key.ToString();
            if (query.Count() == 0)
            {
                return list;
            }
            else
            {
                var models = query.ToList<string>();
                foreach (var m in models)
                {
                    list.Add(FlwMemberRegisterCache.GetModel(m.ToString()));
                }
                return list;
            }
        }
    }
}
