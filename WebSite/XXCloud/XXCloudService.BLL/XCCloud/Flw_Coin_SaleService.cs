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
    
    public partial class Flw_Coin_SaleService : BaseService<Flw_Coin_Sale>, IFlw_Coin_SaleService
    {
    	public override void SetDal()
        {
    
        }
    
        public Flw_Coin_SaleService()
            : this(false)
        {
    
        }
    
        public Flw_Coin_SaleService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IFlw_Coin_SaleDAL>(resolveNew: resolveNew);
        }
    }
}
