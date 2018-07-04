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
    public partial class MemberlevelDAL : BaseDAL<t_memberlevel>, IMemberlevelDAL
    {
        public MemberlevelDAL(string containerName)
            : base(containerName)
        { 
            
        }
   
    }
}
