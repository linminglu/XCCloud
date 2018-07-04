using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudWebBar.DAL.Base;
using XCCloudWebBar.DAL.IDAL.XCGame;
using XCCloudWebBar.Model.XCGame;

namespace XCCloudWebBar.DAL.XCGame
{
    public partial class Project_buyDAL : BaseDAL<flw_project_buy>, IProject_buyDAL
    {
        public Project_buyDAL(string containerName)
            : base(containerName)
        {

        }

    }
   
}
