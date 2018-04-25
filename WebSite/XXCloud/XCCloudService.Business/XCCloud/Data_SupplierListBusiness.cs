using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Data_SupplierListBusiness
	{        
        public static IData_SupplierListService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_SupplierListService>();
            }
            
                       
        }
        
        public static IData_SupplierListService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_SupplierListService>(resolveNew: true);
            }
            
                       
        }
	} 
}