using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Base_ChainRuleBusiness
	{        
        public static IBase_ChainRuleService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IBase_ChainRuleService>();
            }
            
                       
        }
        
        public static IBase_ChainRuleService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IBase_ChainRuleService>(resolveNew: true);
            }
            
                       
        }
	} 
}