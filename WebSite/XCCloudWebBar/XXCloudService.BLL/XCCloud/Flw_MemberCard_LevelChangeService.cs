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
    
    public partial class Flw_MemberCard_LevelChangeService : BaseService<Flw_MemberCard_LevelChange>, IFlw_MemberCard_LevelChangeService
    {
    	public override void SetDal()
        {
    
        }
    
        public Flw_MemberCard_LevelChangeService()
            : this(false)
        {
    
        }
    
        public Flw_MemberCard_LevelChangeService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IFlw_MemberCard_LevelChangeDAL>(resolveNew: resolveNew);
        }
    
    	public static IFlw_MemberCard_LevelChangeService I
        {
            get
            {
                return BLLContainer.Resolve<IFlw_MemberCard_LevelChangeService>();
            }
        }
    
        public static IFlw_MemberCard_LevelChangeService N
        {
            get
            {
                return BLLContainer.Resolve<IFlw_MemberCard_LevelChangeService>(resolveNew: true);
            }
        }
    }
}
