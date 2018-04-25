using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
namespace XCCloudService.Business.XCCloud
{
	public class Data_MemberLevelFreeBusiness
	{        
        public static IData_MemberLevelFreeService Instance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_MemberLevelFreeService>();
            }
            
                       
        }
        
        public static IData_MemberLevelFreeService NewInstance
        {
        	get 
            {
                return BLLContainer.Resolve<IData_MemberLevelFreeService>(resolveNew: true);
            }
            
                       
        }
	} 
}