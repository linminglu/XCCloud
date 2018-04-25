using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Flw_Game_WatchBusiness
	{        
        public static IFlw_Game_WatchService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IFlw_Game_WatchService>();
            }
            
                       
        }
        
        public static IFlw_Game_WatchService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IFlw_Game_WatchService>(resolveNew: true);
            }
            
                       
        }
	} 
}