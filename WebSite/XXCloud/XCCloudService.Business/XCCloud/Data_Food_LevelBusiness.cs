using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Data_Food_LevelBusiness
	{        
        public static IData_Food_LevelService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_Food_LevelService>();
            }
            
                       
        }
        
        public static IData_Food_LevelService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_Food_LevelService>(resolveNew: true);
            }
            
                       
        }
	} 
}