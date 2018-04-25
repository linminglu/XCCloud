using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Data_CoinStorageBusiness
	{        
        public static IData_CoinStorageService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_CoinStorageService>();
            }
            
                       
        }
        
        public static IData_CoinStorageService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_CoinStorageService>(resolveNew: true);
            }
            
                       
        }
	} 
}