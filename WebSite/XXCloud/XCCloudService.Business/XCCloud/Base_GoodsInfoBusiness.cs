using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Base_GoodsInfoBusiness
	{        
        public static IBase_GoodsInfoService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IBase_GoodsInfoService>();
            }
            
                       
        }
        
        public static IBase_GoodsInfoService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IBase_GoodsInfoService>(resolveNew: true);
            }
            
                       
        }
	} 
}