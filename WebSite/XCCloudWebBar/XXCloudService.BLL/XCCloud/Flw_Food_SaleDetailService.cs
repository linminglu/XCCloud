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
    
    public partial class Flw_Food_SaleDetailService : BaseService<Flw_Food_SaleDetail>, IFlw_Food_SaleDetailService
    {
    	public override void SetDal()
        {
    
        }
    
        public Flw_Food_SaleDetailService()
            : this(false)
        {
    
        }
    
        public Flw_Food_SaleDetailService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IFlw_Food_SaleDetailDAL>(resolveNew: resolveNew);
        }
    
    	public static IFlw_Food_SaleDetailService I
        {
            get
            {
                return BLLContainer.Resolve<IFlw_Food_SaleDetailService>();
            }
        }
    
        public static IFlw_Food_SaleDetailService N
        {
            get
            {
                return BLLContainer.Resolve<IFlw_Food_SaleDetailService>(resolveNew: true);
            }
        }
    }
}
