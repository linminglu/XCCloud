using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudWebBar.CacheService;
using XCCloudWebBar.Model.CustomModel.XCCloud;

namespace XCCloudWebBar.Business.XCCloud
{
    public class DeviceWorkInfoBusiness
    {
        public static void AddDevice(string mucId, DeviceWorkInfoModel model)
        {
            DeviceWorkInfoCache.AddObject(mucId, model);
        }

        public static List<DeviceWorkInfoModel> GetDevice(string workStation)
        {
            List<DeviceWorkInfoModel> list = new List<DeviceWorkInfoModel>();
            var query = from item in DeviceWorkInfoCache.DeviceHt.Cast<DeviceWorkInfoModel>()
                        where ((DeviceWorkInfoModel)item).WorkStation.Equals(workStation)
                        select ((DeviceWorkInfoModel)item);
            if (query.Count() == 0)
            {
                return list;
            }
            else
            {
                foreach (var item in query)
                {
                    DeviceWorkInfoModel model = (DeviceWorkInfoModel)item;
                    list.Add(model);
                }
            }

            return list;
        }
    }
}
