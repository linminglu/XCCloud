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
    
    public partial class Data_GoodInventoryService : BaseService<Data_GoodInventory>, IData_GoodInventoryService
    {
    	public override void SetDal()
        {
    
        }
    
        public Data_GoodInventoryService()
            : this(false)
        {
    
        }
    
        public Data_GoodInventoryService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_GoodInventoryDAL>(resolveNew: resolveNew);
        }
    
    	public static IData_GoodInventoryService I
        {
            get
            {
                return BLLContainer.Resolve<IData_GoodInventoryService>();
            }
        }
    
        public static IData_GoodInventoryService N
        {
            get
            {
                return BLLContainer.Resolve<IData_GoodInventoryService>(resolveNew: true);
            }
        }
    }
}
