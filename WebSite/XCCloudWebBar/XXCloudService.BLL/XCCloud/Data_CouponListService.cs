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
    
    public partial class Data_CouponListService : BaseService<Data_CouponList>, IData_CouponListService
    {
    	public override void SetDal()
        {
    
        }
    
        public Data_CouponListService()
            : this(false)
        {
    
        }
    
        public Data_CouponListService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_CouponListDAL>(resolveNew: resolveNew);
        }
    
    	public static IData_CouponListService I
        {
            get
            {
                return BLLContainer.Resolve<IData_CouponListService>();
            }
        }
    
        public static IData_CouponListService N
        {
            get
            {
                return BLLContainer.Resolve<IData_CouponListService>(resolveNew: true);
            }
        }
    }
}