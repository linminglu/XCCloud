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
    
    public partial class Flw_DiscountRuleService : BaseService<Flw_DiscountRule>, IFlw_DiscountRuleService
    {
    	public override void SetDal()
        {
    
        }
    
        public Flw_DiscountRuleService()
            : this(false)
        {
    
        }
    
        public Flw_DiscountRuleService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IFlw_DiscountRuleDAL>(resolveNew: resolveNew);
        }
    
    	public static IFlw_DiscountRuleService I
        {
            get
            {
                return BLLContainer.Resolve<IFlw_DiscountRuleService>();
            }
        }
    
        public static IFlw_DiscountRuleService N
        {
            get
            {
                return BLLContainer.Resolve<IFlw_DiscountRuleService>(resolveNew: true);
            }
        }
    }
}