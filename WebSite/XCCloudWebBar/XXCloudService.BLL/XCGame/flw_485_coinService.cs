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
    public partial class flw_485_coinService : BaseService<flw_485_coin>, Iflw_485_coinService
    {
        private Iflw_485_coinDAL StaffDAL;

        public flw_485_coinService(string containerName)
        {
            this.containerName = containerName;
            StaffDAL = DALContainer.Resolve<Iflw_485_coinDAL>(this.containerName);
            Dal = StaffDAL;
        }
        public override void SetDal()
        {

        }
    }
    
}
