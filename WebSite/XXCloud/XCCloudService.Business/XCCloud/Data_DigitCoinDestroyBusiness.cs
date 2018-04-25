using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Data_DigitCoinDestroyBusiness
	{        
        public static IData_DigitCoinDestroyService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_DigitCoinDestroyService>();
            }
            
                       
        }
        
        public static IData_DigitCoinDestroyService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_DigitCoinDestroyService>(resolveNew: true);
            }
            
                       
        }
	} 
}