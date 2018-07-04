using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudWebBar.BLL.Base;
using XCCloudWebBar.BLL.IBLL.XCGame;
using XCCloudWebBar.DAL.Container;
using XCCloudWebBar.DAL.IDAL;
using XCCloudWebBar.DAL.XCGame.IDAL;
using XCCloudWebBar.Model;
using XCCloudWebBar.Model.XCGame;

namespace XCCloudWebBar.BLL.XCGame
{
    public partial class GameService : BaseService<t_game>, IGameService
    {
        private IGameDAL StaffDAL;

        public GameService(string containerName)
        {
            this.containerName = containerName;
            StaffDAL = DALContainer.Resolve<IGameDAL>(this.containerName);
            Dal = StaffDAL;
        }
        public override void SetDal()
        {

        }
    }
}
