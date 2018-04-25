using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Data_BalanceChargeRuleBusiness
	{        
        public static IData_BalanceChargeRuleService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_BalanceChargeRuleService>();
            }
            
                       
        }
        
        public static IData_BalanceChargeRuleService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_BalanceChargeRuleService>(resolveNew: true);
            }
            
                       
        }
	} 
}