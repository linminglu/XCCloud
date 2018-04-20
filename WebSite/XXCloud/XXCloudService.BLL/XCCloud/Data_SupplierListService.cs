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
	public class Data_SupplierListService : BaseService<Data_SupplierList>, IData_SupplierListService
	{
        public override void SetDal()
        {
        	
        }
        
        public Data_SupplierListService()
        	: this(false)
        {
            
        }
        
        public Data_SupplierListService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_SupplierListDAL>(resolveNew: resolveNew);
        }
	} 
}