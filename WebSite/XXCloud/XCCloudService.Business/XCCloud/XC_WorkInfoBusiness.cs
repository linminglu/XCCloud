using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class XC_WorkInfoBusiness
	{        
        public static IXC_WorkInfoService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IXC_WorkInfoService>();
            }
            
                       
        }
        
        public static IXC_WorkInfoService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IXC_WorkInfoService>(resolveNew: true);
            }                       
        }
	} 
}