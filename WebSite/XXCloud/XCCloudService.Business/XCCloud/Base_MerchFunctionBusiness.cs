using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Base_MerchFunctionBusiness
	{        
        public static IBase_MerchFunctionService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IBase_MerchFunctionService>();
            }
            
                       
        }
        
        public static IBase_MerchFunctionService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IBase_MerchFunctionService>(resolveNew: true);
            }
            
                       
        }
	} 
}