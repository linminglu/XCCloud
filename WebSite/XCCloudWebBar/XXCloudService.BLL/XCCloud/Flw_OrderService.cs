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
    
    public partial class Flw_OrderService : BaseService<Flw_Order>, IFlw_OrderService
    {
    	public override void SetDal()
        {
    
        }
    
        public Flw_OrderService()
            : this(false)
        {
    
        }
    
        public Flw_OrderService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IFlw_OrderDAL>(resolveNew: resolveNew);
        }
    
    	public static IFlw_OrderService I
        {
            get
            {
                return BLLContainer.Resolve<IFlw_OrderService>();
            }
        }
    
        public static IFlw_OrderService N
        {
            get
            {
                return BLLContainer.Resolve<IFlw_OrderService>(resolveNew: true);
            }
        }
    }
}