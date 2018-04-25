using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Data_BalanceType_StoreListBusiness
	{        
        public static IData_BalanceType_StoreListService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_BalanceType_StoreListService>();
            }
            
                       
        }
        
        public static IData_BalanceType_StoreListService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_BalanceType_StoreListService>(resolveNew: true);
            }
            
                       
        }
	} 
}