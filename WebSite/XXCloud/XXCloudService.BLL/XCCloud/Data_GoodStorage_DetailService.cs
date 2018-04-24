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
	public class Data_GoodStorage_DetailService : BaseService<Data_GoodStorage_Detail>, IData_GoodStorage_DetailService
	{
        public override void SetDal()
        {
        	
        }
        
        public Data_GoodStorage_DetailService()
        	: this(false)
        {
            
        }
        
        public Data_GoodStorage_DetailService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_GoodStorage_DetailDAL>(resolveNew: resolveNew);
        }
	} 
}