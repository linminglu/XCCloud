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
    public partial class flw_485_savecoinDAL : BaseDAL<flw_485_savecoin>, Iflw_485_savecoinDAL
    {
        public flw_485_savecoinDAL(string containerName)
            : base(containerName)
        { 
            
        }
    }
}
