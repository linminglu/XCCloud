using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Dict_BalanceTypeBusiness
	{        
        public static IDict_BalanceTypeService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IDict_BalanceTypeService>();
            }
            
                       
        }
        
        public static IDict_BalanceTypeService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IDict_BalanceTypeService>(resolveNew: true);
            }
            
                       
        }
	} 
}