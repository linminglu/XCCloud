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
    public partial class ProjectDAL : BaseDAL<t_project>, IProjectDAL
    {
        public ProjectDAL(string containerName)
            : base(containerName)
        {

        }

    }
    
}
