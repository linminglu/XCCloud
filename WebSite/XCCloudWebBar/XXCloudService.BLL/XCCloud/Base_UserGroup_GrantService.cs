﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace XCCloudWebBar.BLL.XCCloud
{
    
    using XCCloudWebBar.DAL.Container;
    using XCCloudWebBar.DAL.IDAL.XCCloud;
    using XCCloudWebBar.BLL.Base;
    using XCCloudWebBar.BLL.Container;
    using XCCloudWebBar.BLL.IBLL.XCCloud;
    using XCCloudWebBar.Model.XCCloud;
    
    public partial class Base_UserGroup_GrantService : BaseService<Base_UserGroup_Grant>, IBase_UserGroup_GrantService
    {
    	public override void SetDal()
        {
    
        }
    
        public Base_UserGroup_GrantService()
            : this(false)
        {
    
        }
    
        public Base_UserGroup_GrantService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IBase_UserGroup_GrantDAL>(resolveNew: resolveNew);
        }
    
    	public static IBase_UserGroup_GrantService I
        {
            get
            {
                return BLLContainer.Resolve<IBase_UserGroup_GrantService>();
            }
        }
    
        public static IBase_UserGroup_GrantService N
        {
            get
            {
                return BLLContainer.Resolve<IBase_UserGroup_GrantService>(resolveNew: true);
            }
        }
    }
}