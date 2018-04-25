using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Data_GameInfo_ExtBusiness
	{        
        public static IData_GameInfo_ExtService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_GameInfo_ExtService>();
            }
            
                       
        }
        
        public static IData_GameInfo_ExtService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_GameInfo_ExtService>(resolveNew: true);
            }
            
                       
        }
	} 
}