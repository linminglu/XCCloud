﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace XCCloudService.BLL.XCCloud
{
    
    using XCCloudService.DAL.Container;
    using XCCloudService.DAL.IDAL.XCCloud;
    using XCCloudService.BLL.Base;
    using XCCloudService.BLL.Container;
    using XCCloudService.BLL.IBLL.XCCloud;
    using XCCloudService.Model.XCCloud;
    
    public partial class Data_DiscountRuleService : BaseService<Data_DiscountRule>, IData_DiscountRuleService
    {
    	public override void SetDal()
        {
    
        }
    
        public Data_DiscountRuleService()
            : this(false)
        {
    
        }
    
        public Data_DiscountRuleService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_DiscountRuleDAL>(resolveNew: resolveNew);
        }
    
    	public static IData_DiscountRuleService I
        {
            get
            {
                return BLLContainer.Resolve<IData_DiscountRuleService>();
            }
        }
    
        public static IData_DiscountRuleService N
        {
            get
            {
                return BLLContainer.Resolve<IData_DiscountRuleService>(resolveNew: true);
            }
        }
    }
}