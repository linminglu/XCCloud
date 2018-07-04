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
    public partial class flw_485_coinDAL : BaseDAL<flw_485_coin>, Iflw_485_coinDAL
    {
        public flw_485_coinDAL(string containerName)
            : base(containerName)
        { 
            
        }
    }
}
