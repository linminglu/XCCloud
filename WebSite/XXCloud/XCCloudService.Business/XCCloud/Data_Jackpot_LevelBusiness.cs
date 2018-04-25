using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Data_Jackpot_LevelBusiness
	{        
        public static IData_Jackpot_LevelService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_Jackpot_LevelService>();
            }
            
                       
        }
        
        public static IData_Jackpot_LevelService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_Jackpot_LevelService>(resolveNew: true);
            }
            
                       
        }
	} 
}