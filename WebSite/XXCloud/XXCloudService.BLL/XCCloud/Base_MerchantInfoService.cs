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
    
    public partial class Base_MerchantInfoService : BaseService<Base_MerchantInfo>, IBase_MerchantInfoService
    {
    	public override void SetDal()
        {
    
        }
    
        public Base_MerchantInfoService()
            : this(false)
        {
    
        }
    
        public Base_MerchantInfoService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IBase_MerchantInfoDAL>(resolveNew: resolveNew);
        }
    
    	public static IBase_MerchantInfoService I
        {
            get
            {
                return BLLContainer.Resolve<IBase_MerchantInfoService>();
            }
        }
    
        public static IBase_MerchantInfoService N
        {
            get
            {
                return BLLContainer.Resolve<IBase_MerchantInfoService>(resolveNew: true);
            }
        }
    }
}
