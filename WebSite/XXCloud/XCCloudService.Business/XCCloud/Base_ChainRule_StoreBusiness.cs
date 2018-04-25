using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Base_ChainRule_StoreBusiness
	{        
        public static IBase_ChainRule_StoreService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IBase_ChainRule_StoreService>();
            }
            
                       
        }
        
        public static IBase_ChainRule_StoreService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IBase_ChainRule_StoreService>(resolveNew: true);
            }
            
                       
        }
	} 
}