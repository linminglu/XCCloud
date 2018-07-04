using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudWebBar.BLL.Base;
using XCCloudWebBar.BLL.IBLL.XCCloudRS232;
using XCCloudWebBar.DAL.Container;
using XCCloudWebBar.DAL.IDAL.XCCloudRS232;
using XCCloudWebBar.Model.XCCloudRS232;

namespace XCCloudWebBar.BLL.XCCloudRS232
{
    public partial class DeviceService : BaseService<Base_DeviceInfo>, IDeviceService
    {
        private IDeviceDAL deviceDAL = DALContainer.Resolve<IDeviceDAL>();
        public override void SetDal()
        {
            Dal = deviceDAL;
        }
    }
}
