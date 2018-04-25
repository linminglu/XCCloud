using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Base_StoreWeight_GameBusiness
	{        
        public static IBase_StoreWeight_GameService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IBase_StoreWeight_GameService>();
            }
            
                       
        }
        
        public static IBase_StoreWeight_GameService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IBase_StoreWeight_GameService>(resolveNew: true);
            }
            
                       
        }
	} 
}