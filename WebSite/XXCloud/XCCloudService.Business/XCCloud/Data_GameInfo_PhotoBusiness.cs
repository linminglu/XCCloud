using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Data_GameInfo_PhotoBusiness
	{        
        public static IData_GameInfo_PhotoService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_GameInfo_PhotoService>();
            }
            
                       
        }
        
        public static IData_GameInfo_PhotoService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_GameInfo_PhotoService>(resolveNew: true);
            }
            
                       
        }
	} 
}