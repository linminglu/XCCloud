using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Base_SettleOrgBusiness
	{        
        public static IBase_SettleOrgService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IBase_SettleOrgService>();
            }
            
                       
        }
        
        public static IBase_SettleOrgService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IBase_SettleOrgService>(resolveNew: true);
            }
            
                       
        }
	} 
}