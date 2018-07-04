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
    public partial class FoodsaleService : BaseService<flw_food_sale>, IFoodsaleService
    {
        private IFoodsaleDAL StaffDAL;

        public FoodsaleService(string containerName)
        {
            this.containerName = containerName;
            StaffDAL = DALContainer.Resolve<IFoodsaleDAL>(this.containerName);
            Dal = StaffDAL;
        }
        public override void SetDal()
        {

        }
    }
    
}
