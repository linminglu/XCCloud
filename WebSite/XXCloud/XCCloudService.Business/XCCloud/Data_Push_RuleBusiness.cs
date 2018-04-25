using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Data_Push_RuleBusiness
	{        
        public static IData_Push_RuleService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_Push_RuleService>();
            }
            
                       
        }
        
        public static IData_Push_RuleService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_Push_RuleService>(resolveNew: true);
            }
            
                       
        }
	} 
}