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
    public class Base_MerchAlipayService : BaseService<Base_MerchAlipay>, IBase_MerchAlipayService
	{
		public override void SetDal()
        {

        }

        public Base_MerchAlipayService()
            : this(false)
        {

        }

        public Base_MerchAlipayService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IBase_MerchAlipayDAL>(resolveNew: resolveNew);
        }
	} 
}