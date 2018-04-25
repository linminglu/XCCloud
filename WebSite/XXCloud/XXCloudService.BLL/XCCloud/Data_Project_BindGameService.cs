using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.DAL.Container;
using XCCloudService.DAL.IDAL.XCCloud;
using XCCloudService.BLL.Base;
using XCCloudService.BLL.IBLL.XCCloud;
using XCCloudService.Model.XCCloud;
namespace XCCloudService.BLL.XCCloud
{
	public class Data_Project_BindGameService : BaseService<Data_Project_BindGame>, IData_Project_BindGameService
	{
        public override void SetDal()
        {
        	
        }
        
        public Data_Project_BindGameService()
        	: this(false)
        {
            
        }
        
        public Data_Project_BindGameService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_Project_BindGameDAL>(resolveNew: resolveNew);
        }
	} 
}