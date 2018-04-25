using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Base_MemberInfoBusiness
	{        
        public static IBase_MemberInfoService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IBase_MemberInfoService>();
            }
            
                       
        }
        
        public static IBase_MemberInfoService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IBase_MemberInfoService>(resolveNew: true);
            }
            
                       
        }
	} 
}