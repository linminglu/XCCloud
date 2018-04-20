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
	public class Data_Food_SaleService : BaseService<Data_Food_Sale>, IData_Food_SaleService
	{
        public override void SetDal()
        {
        	
        }
        
        public Data_Food_SaleService()
        	: this(false)
        {
            
        }
        
        public Data_Food_SaleService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_Food_SaleDAL>(resolveNew: resolveNew);
        }
	} 
}