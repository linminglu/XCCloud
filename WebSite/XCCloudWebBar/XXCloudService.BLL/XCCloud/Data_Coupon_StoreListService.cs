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
    
    public partial class Data_Coupon_StoreListService : BaseService<Data_Coupon_StoreList>, IData_Coupon_StoreListService
    {
    	public override void SetDal()
        {
    
        }
    
        public Data_Coupon_StoreListService()
            : this(false)
        {
    
        }
    
        public Data_Coupon_StoreListService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_Coupon_StoreListDAL>(resolveNew: resolveNew);
        }
    
    	public static IData_Coupon_StoreListService I
        {
            get
            {
                return BLLContainer.Resolve<IData_Coupon_StoreListService>();
            }
        }
    
        public static IData_Coupon_StoreListService N
        {
            get
            {
                return BLLContainer.Resolve<IData_Coupon_StoreListService>(resolveNew: true);
            }
        }
    }
}