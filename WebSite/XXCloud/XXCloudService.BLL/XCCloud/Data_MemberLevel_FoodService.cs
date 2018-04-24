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
	public class Data_MemberLevel_FoodService : BaseService<Data_MemberLevel_Food>, IData_MemberLevel_FoodService
	{
        public override void SetDal()
        {
            
        }

        public Data_MemberLevel_FoodService()
            : this(false)
        {

        }

        public Data_MemberLevel_FoodService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_MemberLevel_FoodDAL>(resolveNew: resolveNew);
        }
	} 
}