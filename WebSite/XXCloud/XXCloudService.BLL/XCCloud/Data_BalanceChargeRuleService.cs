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
    public class Data_BalanceChargeRuleService : BaseService<Data_BalanceChargeRule>, IData_BalanceChargeRuleService
	{
        public override void SetDal()
        {
        	
        }
        
        public Data_BalanceChargeRuleService()
        	: this(false)
        {
            
        }

        public Data_BalanceChargeRuleService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_BalanceChargeRuleDAL>(resolveNew: resolveNew);
        }
	} 
}