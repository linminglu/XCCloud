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
    
    public partial class Flw_485_SaveCoinService : BaseService<Flw_485_SaveCoin>, IFlw_485_SaveCoinService
    {
    	public override void SetDal()
        {
    
        }
    
        public Flw_485_SaveCoinService()
            : this(false)
        {
    
        }
    
        public Flw_485_SaveCoinService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IFlw_485_SaveCoinDAL>(resolveNew: resolveNew);
        }
    }
}