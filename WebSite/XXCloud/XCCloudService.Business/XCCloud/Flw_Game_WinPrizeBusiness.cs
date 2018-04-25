using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Flw_Game_WinPrizeBusiness
	{        
        public static IFlw_Game_WinPrizeService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IFlw_Game_WinPrizeService>();
            }
            
                       
        }
        
        public static IFlw_Game_WinPrizeService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IFlw_Game_WinPrizeService>(resolveNew: true);
            }
            
                       
        }
	} 
}