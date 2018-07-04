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
    public partial class Checkdate_ScheduleService : BaseService<flw_checkdate_schedule>, ICheckdate_ScheduleService
    {
        private ICheckdate_ScheduleDAL StaffDAL;

        public Checkdate_ScheduleService(string containerName)
        {
            this.containerName = containerName;
            StaffDAL = DALContainer.Resolve<ICheckdate_ScheduleDAL>(this.containerName);
            Dal = StaffDAL;
        }
        public override void SetDal()
        {

        }
    }
}
