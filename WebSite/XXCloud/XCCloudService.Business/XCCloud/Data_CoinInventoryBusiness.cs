using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Data_CoinInventoryBusiness
	{        
        public static IData_CoinInventoryService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_CoinInventoryService>();
            }
            
                       
        }
        
        public static IData_CoinInventoryService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_CoinInventoryService>(resolveNew: true);
            }
            
                       
        }
	} 
}