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
    
    public partial class Base_UserGrantService : BaseService<Base_UserGrant>, IBase_UserGrantService
    {
    	public override void SetDal()
        {
    
        }
    
        public Base_UserGrantService()
            : this(false)
        {
    
        }
    
        public Base_UserGrantService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IBase_UserGrantDAL>(resolveNew: resolveNew);
        }
    
    	public static IBase_UserGrantService I
        {
            get
            {
                return BLLContainer.Resolve<IBase_UserGrantService>();
            }
        }
    
        public static IBase_UserGrantService N
        {
            get
            {
                return BLLContainer.Resolve<IBase_UserGrantService>(resolveNew: true);
            }
        }
    }
}
