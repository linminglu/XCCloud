using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Data_GameInfoBusiness
	{        
        public static IData_GameInfoService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_GameInfoService>();
            }
            
                       
        }
        
        public static IData_GameInfoService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_GameInfoService>(resolveNew: true);
            }
            
                       
        }
	} 
}