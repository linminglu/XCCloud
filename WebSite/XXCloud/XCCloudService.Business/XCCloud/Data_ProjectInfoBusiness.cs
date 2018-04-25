using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Data_ProjectInfoBusiness
	{        
        public static IData_ProjectInfoService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_ProjectInfoService>();
            }
            
                       
        }
        
        public static IData_ProjectInfoService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_ProjectInfoService>(resolveNew: true);
            }
            
                       
        }
	} 
}