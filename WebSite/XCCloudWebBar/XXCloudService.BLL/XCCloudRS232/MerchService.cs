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
    public partial class MerchService : BaseService<Base_MerchInfo>, IMerchService
    {
        private IMerchDAL merchDAL = DALContainer.Resolve<IMerchDAL>();
        public override void SetDal()
        {
            Dal = merchDAL;
        }
    }
}
