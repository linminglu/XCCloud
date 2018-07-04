using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudWebBar.BLL.Base;
using XCCloudWebBar.BLL.IBLL.XCGameManager;
using XCCloudWebBar.DAL.Container;
using XCCloudWebBar.DAL.IDAL.XCGameManager;
using XCCloudWebBar.Model.XCGameManager;


namespace XCCloudWebBar.BLL.XCGameManager
{
    public partial class StoreService : BaseService<t_store>, IStoreService
    {
        private IStoreDAL storeDAL = DALContainer.Resolve<IStoreDAL>();
        public override void SetDal()
        {
            Dal = storeDAL;
        }
    }
}
