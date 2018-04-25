using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Base_UserGroupBusiness
	{        
        public static IBase_UserGroupService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IBase_UserGroupService>();
            }
            
                       
        }
        
        public static IBase_UserGroupService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IBase_UserGroupService>(resolveNew: true);
            }
            
                       
        }
	} 
}