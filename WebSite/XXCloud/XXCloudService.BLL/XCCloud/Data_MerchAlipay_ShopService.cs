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
	public class Data_MerchAlipay_ShopService : BaseService<Data_MerchAlipay_Shop>, IData_MerchAlipay_ShopService
	{
        public override void SetDal()
        {
        	
        }
        
        public Data_MerchAlipay_ShopService()
        	: this(false)
        {
            
        }
        
        public Data_MerchAlipay_ShopService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_MerchAlipay_ShopDAL>(resolveNew: resolveNew);
        }
	} 
}