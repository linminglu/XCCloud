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
    public partial class MemberlevelService : BaseService<t_memberlevel>, IMemberlevelService
    {
        private IMemberlevelDAL StaffDAL;

        public MemberlevelService(string containerName)
        {
            this.containerName = containerName;
            StaffDAL = DALContainer.Resolve<IMemberlevelDAL>(this.containerName);
            Dal = StaffDAL;
        }
        public override void SetDal()
        {

        }
    }
   
}
