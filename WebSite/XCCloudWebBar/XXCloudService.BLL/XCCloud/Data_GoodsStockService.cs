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
    
    public partial class Data_GoodsStockService : BaseService<Data_GoodsStock>, IData_GoodsStockService
    {
    	public override void SetDal()
        {
    
        }
    
        public Data_GoodsStockService()
            : this(false)
        {
    
        }
    
        public Data_GoodsStockService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_GoodsStockDAL>(resolveNew: resolveNew);
        }
    
    	public static IData_GoodsStockService I
        {
            get
            {
                return BLLContainer.Resolve<IData_GoodsStockService>();
            }
        }
    
        public static IData_GoodsStockService N
        {
            get
            {
                return BLLContainer.Resolve<IData_GoodsStockService>(resolveNew: true);
            }
        }
    }
}