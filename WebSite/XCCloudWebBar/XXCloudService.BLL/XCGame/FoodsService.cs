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
    public partial class FoodsService : BaseService<t_foods>, IFoodsService
    {
        private IFoodsDAL StaffDAL;

        public FoodsService(string containerName)
        {
            this.containerName = containerName;
            StaffDAL = DALContainer.Resolve<IFoodsDAL>(this.containerName);
            Dal = StaffDAL;
        }
        public override void SetDal()
        {

        }
    }
    
}
