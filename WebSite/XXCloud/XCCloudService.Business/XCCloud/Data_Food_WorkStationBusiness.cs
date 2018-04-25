using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Data_Food_WorkStationBusiness
	{        
        public static IData_Food_WorkStationService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_Food_WorkStationService>();
            }
            
                       
        }
        
        public static IData_Food_WorkStationService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_Food_WorkStationService>(resolveNew: true);
            }
            
                       
        }
	} 
}