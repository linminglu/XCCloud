using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Base_UserInfoBusiness
	{        
        public static IBase_UserInfoService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IBase_UserInfoService>();
            }
            
                       
        }
        
        public static IBase_UserInfoService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IBase_UserInfoService>(resolveNew: true);
            }
            
                       
        }
	} 
}