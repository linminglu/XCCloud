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
    
    public partial class Flw_MemberLevelFreeService : BaseService<Flw_MemberLevelFree>, IFlw_MemberLevelFreeService
    {
    	public override void SetDal()
        {
    
        }
    
        public Flw_MemberLevelFreeService()
            : this(false)
        {
    
        }
    
        public Flw_MemberLevelFreeService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IFlw_MemberLevelFreeDAL>(resolveNew: resolveNew);
        }
    
    	public static IFlw_MemberLevelFreeService I
        {
            get
            {
                return BLLContainer.Resolve<IFlw_MemberLevelFreeService>();
            }
        }
    
        public static IFlw_MemberLevelFreeService N
        {
            get
            {
                return BLLContainer.Resolve<IFlw_MemberLevelFreeService>(resolveNew: true);
            }
        }
    }
}