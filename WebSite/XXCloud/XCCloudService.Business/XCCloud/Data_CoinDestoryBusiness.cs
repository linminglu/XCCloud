using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Data_CoinDestoryBusiness
	{        
        public static IData_CoinDestoryService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_CoinDestoryService>();
            }
            
                       
        }
        
        public static IData_CoinDestoryService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_CoinDestoryService>(resolveNew: true);
            }
            
                       
        }
	} 
}