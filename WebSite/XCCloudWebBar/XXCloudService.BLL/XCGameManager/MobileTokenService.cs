using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudWebBar.BLL.Base;
using XCCloudWebBar.BLL.IBLL.XCGameManager;
using XCCloudWebBar.DAL.Container;
using XCCloudWebBar.DAL.IDAL.XCGameManager;
using XCCloudWebBar.Model.XCGameManager;

namespace XCCloudWebBar.BLL.XCGameManager
{
    public partial class MobileTokenService : BaseService<t_MobileToken>, IMobileTokenService
    {
        private IMobileTokenDAL apirequestlogDAL = DALContainer.Resolve<IMobileTokenDAL>();
        public override void SetDal()
        {
            Dal = apirequestlogDAL;
        }
    }
   
}
