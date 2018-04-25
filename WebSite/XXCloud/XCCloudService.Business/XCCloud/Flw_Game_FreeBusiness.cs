using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Flw_Game_FreeBusiness
	{        
        public static IFlw_Game_FreeService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IFlw_Game_FreeService>();
            }
            
                       
        }
        
        public static IFlw_Game_FreeService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IFlw_Game_FreeService>(resolveNew: true);
            }
            
                       
        }
	} 
}