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
    
    public partial class Flw_MemberInfo_ChangeService : BaseService<Flw_MemberInfo_Change>, IFlw_MemberInfo_ChangeService
    {
    	public override void SetDal()
        {
    
        }
    
        public Flw_MemberInfo_ChangeService()
            : this(false)
        {
    
        }
    
        public Flw_MemberInfo_ChangeService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IFlw_MemberInfo_ChangeDAL>(resolveNew: resolveNew);
        }
    
    	public static IFlw_MemberInfo_ChangeService I
        {
            get
            {
                return BLLContainer.Resolve<IFlw_MemberInfo_ChangeService>();
            }
        }
    
        public static IFlw_MemberInfo_ChangeService N
        {
            get
            {
                return BLLContainer.Resolve<IFlw_MemberInfo_ChangeService>(resolveNew: true);
            }
        }
    }
}
