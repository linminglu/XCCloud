//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace XCCloudService.BLL.XCCloud
{
    using System;
    using System.Collections.Generic;
    
    using XCCloudService.DAL.Container;
    using XCCloudService.DAL.IDAL.XCCloud;
    using XCCloudService.BLL.Base;
    using XCCloudService.BLL.IBLL.XCCloud;
    using XCCloudService.Model.XCCloud;
    
    public partial class Store_GameTotalService : BaseService<Store_GameTotal>, IStore_GameTotalService
    {
    	public override void SetDal()
        {
    
        }
    
        public Store_GameTotalService()
            : this(false)
        {
    
        }
    
        public Store_GameTotalService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IStore_GameTotalDAL>(resolveNew: resolveNew);
        }
    }
}
