using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Base_UserGrantBusiness
	{        
        public static IBase_UserGrantService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IBase_UserGrantService>();
            }
            
                       
        }
        
        public static IBase_UserGrantService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IBase_UserGrantService>(resolveNew: true);
            }
            
                       
        }
	} 
}