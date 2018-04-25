using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Data_GameFreeRuleBusiness
	{        
        public static IData_GameFreeRuleService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_GameFreeRuleService>();
            }
            
                       
        }
        
        public static IData_GameFreeRuleService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_GameFreeRuleService>(resolveNew: true);
            }
            
                       
        }
	} 
}