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
    public partial class ProjectPlayDAL : BaseDAL<flw_project_play>, IProjectPlayDAL
    {
        public ProjectPlayDAL(string containerName)
            : base(containerName)
        {

        }

    }
    
}
