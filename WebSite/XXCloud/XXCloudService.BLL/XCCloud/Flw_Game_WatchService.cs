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
    
    public partial class Flw_Game_WatchService : BaseService<Flw_Game_Watch>, IFlw_Game_WatchService
    {
    	public override void SetDal()
        {
    
        }
    
        public Flw_Game_WatchService()
            : this(false)
        {
    
        }
    
        public Flw_Game_WatchService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IFlw_Game_WatchDAL>(resolveNew: resolveNew);
        }
    
    	public static IFlw_Game_WatchService I
        {
            get
            {
                return BLLContainer.Resolve<IFlw_Game_WatchService>();
            }
        }
    
        public static IFlw_Game_WatchService N
        {
            get
            {
                return BLLContainer.Resolve<IFlw_Game_WatchService>(resolveNew: true);
            }
        }
    }
}
