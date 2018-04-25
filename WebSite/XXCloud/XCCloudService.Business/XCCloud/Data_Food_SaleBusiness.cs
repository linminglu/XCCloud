using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Data_Food_SaleBusiness
	{        
        public static IData_Food_SaleService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_Food_SaleService>();
            }
            
                       
        }
        
        public static IData_Food_SaleService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_Food_SaleService>(resolveNew: true);
            }
            
                       
        }
	} 
}