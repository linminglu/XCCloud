﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace XCCloudService.BLL.XCCloud
{
    
    using XCCloudService.DAL.Container;
    using XCCloudService.DAL.IDAL.XCCloud;
    using XCCloudService.BLL.Base;
    using XCCloudService.BLL.Container;
    using XCCloudService.BLL.IBLL.XCCloud;
    using XCCloudService.Model.XCCloud;
    
    public partial class Data_Food_StoreListService : BaseService<Data_Food_StoreList>, IData_Food_StoreListService
    {
    	public override void SetDal()
        {
    
        }
    
        public Data_Food_StoreListService()
            : this(false)
        {
    
        }
    
        public Data_Food_StoreListService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_Food_StoreListDAL>(resolveNew: resolveNew);
        }
    
    	public static IData_Food_StoreListService I
        {
            get
            {
                return BLLContainer.Resolve<IData_Food_StoreListService>();
            }
        }
    
        public static IData_Food_StoreListService N
        {
            get
            {
                return BLLContainer.Resolve<IData_Food_StoreListService>(resolveNew: true);
            }
        }
    }
}
