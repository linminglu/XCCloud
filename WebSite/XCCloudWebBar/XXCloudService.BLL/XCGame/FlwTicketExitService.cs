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
    public partial class FlwTicketExitService : BaseService<flw_ticket_exit>, IFlwTicketExitService
    {
        private IFlwTicketExitDAL StaffDAL;

        public FlwTicketExitService(string containerName)
        {
            this.containerName = containerName;
            StaffDAL = DALContainer.Resolve<IFlwTicketExitDAL>(this.containerName);
            Dal = StaffDAL;
        }
        public override void SetDal()
        {

        }
    }
}
