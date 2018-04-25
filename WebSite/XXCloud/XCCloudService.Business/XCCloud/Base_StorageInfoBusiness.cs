using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Base_StorageInfoBusiness
	{        
        public static IBase_StorageInfoService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IBase_StorageInfoService>();
            }
            
                       
        }
        
        public static IBase_StorageInfoService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IBase_StorageInfoService>(resolveNew: true);
            }
            
                       
        }
	} 
}