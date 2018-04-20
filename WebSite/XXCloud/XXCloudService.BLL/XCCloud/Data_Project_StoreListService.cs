using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.DAL.Container;
using XCCloudService.DAL.IDAL.XCCloud;
using XCCloudService.BLL.Base;
using XCCloudService.BLL.IBLL.XCCloud;
using XCCloudService.Model.XCCloud;
namespace XCCloudService.BLL.XCCloud
{
    public class Data_Project_StoreListService : BaseService<Data_Project_StoreList>, IData_Project_StoreListService
	{
        public override void SetDal()
        {

        }

        public Data_Project_StoreListService()
            : this(false)
        {
        }

        public Data_Project_StoreListService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_Project_StoreListDAL>(resolveNew: resolveNew);
        }
	} 
}