using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Data_JackpotInfoBusiness
	{        
        public static IData_JackpotInfoService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_JackpotInfoService>();
            }
            
                       
        }
        
        public static IData_JackpotInfoService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_JackpotInfoService>(resolveNew: true);
            }
            
                       
        }
	} 
}