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
    public partial class Project_buy_codelistService : BaseService<flw_project_buy_codelist>, IProject_buy_codelistService
    {
        private IProject_buy_codelistDAL StaffDAL;

        public Project_buy_codelistService(string containerName)
        {
            this.containerName = containerName;
            StaffDAL = DALContainer.Resolve<IProject_buy_codelistDAL>(this.containerName);
            Dal = StaffDAL;
        }
        public override void SetDal()
        {

        }
    }
    
}
