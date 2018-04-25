using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Data_DigitCoinBusiness
	{        
        public static IData_DigitCoinService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_DigitCoinService>();
            }
            
                       
        }
        
        public static IData_DigitCoinService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_DigitCoinService>(resolveNew: true);
            }
            
                       
        }
	} 
}