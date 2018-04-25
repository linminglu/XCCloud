using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Data_ProjectTimeInfoBusiness
	{        
        public static IData_ProjectTimeInfoService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_ProjectTimeInfoService>();
            }
            
                       
        }
        
        public static IData_ProjectTimeInfoService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_ProjectTimeInfoService>(resolveNew: true);
            }
            
                       
        }
	} 
}