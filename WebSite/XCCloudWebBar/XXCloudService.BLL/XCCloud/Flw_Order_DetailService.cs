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
    
    public partial class Flw_Order_DetailService : BaseService<Flw_Order_Detail>, IFlw_Order_DetailService
    {
    	public override void SetDal()
        {
    
        }
    
        public Flw_Order_DetailService()
            : this(false)
        {
    
        }
    
        public Flw_Order_DetailService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IFlw_Order_DetailDAL>(resolveNew: resolveNew);
        }
    
    	public static IFlw_Order_DetailService I
        {
            get
            {
                return BLLContainer.Resolve<IFlw_Order_DetailService>();
            }
        }
    
        public static IFlw_Order_DetailService N
        {
            get
            {
                return BLLContainer.Resolve<IFlw_Order_DetailService>(resolveNew: true);
            }
        }
    }
}