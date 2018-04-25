using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Base_UserGroup_GrantBusiness
	{        
        public static IBase_UserGroup_GrantService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IBase_UserGroup_GrantService>();
            }
            
                       
        }
        
        public static IBase_UserGroup_GrantService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IBase_UserGroup_GrantService>(resolveNew: true);
            }
            
                       
        }
	} 
}