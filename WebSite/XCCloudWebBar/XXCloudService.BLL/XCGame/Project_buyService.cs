using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudWebBar.BLL.Base;
using XCCloudWebBar.BLL.IBLL.XCGame;
using XCCloudWebBar.DAL.Container;
using XCCloudWebBar.DAL.IDAL.XCGame;
using XCCloudWebBar.Model.XCGame;

namespace XCCloudWebBar.BLL.XCGame
{
    public partial class Project_buyService : BaseService<flw_project_buy>, IProject_buyService
    {
        private IProject_buyDAL StaffDAL;

        public Project_buyService(string containerName)
        {
            this.containerName = containerName;
            StaffDAL = DALContainer.Resolve<IProject_buyDAL>(this.containerName);
            Dal = StaffDAL;
        }
        public override void SetDal()
        {

        }
    }
   
}
