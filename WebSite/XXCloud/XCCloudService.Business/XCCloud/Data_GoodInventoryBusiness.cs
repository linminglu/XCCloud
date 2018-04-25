using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Data_GoodInventoryBusiness
	{        
        public static IData_GoodInventoryService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_GoodInventoryService>();
            }
            
                       
        }
        
        public static IData_GoodInventoryService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_GoodInventoryService>(resolveNew: true);
            }
            
                       
        }
	} 
}