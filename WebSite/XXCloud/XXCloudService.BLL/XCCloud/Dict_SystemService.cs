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
    
    public partial class Dict_SystemService : BaseService<Dict_System>, IDict_SystemService
    {
    	public override void SetDal()
        {
    
        }
    
        public Dict_SystemService()
            : this(false)
        {
    
        }
    
        public Dict_SystemService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IDict_SystemDAL>(resolveNew: resolveNew);
        }
    
    	public static IDict_SystemService I
        {
            get
            {
                return BLLContainer.Resolve<IDict_SystemService>();
            }
        }
    
        public static IDict_SystemService N
        {
            get
            {
                return BLLContainer.Resolve<IDict_SystemService>(resolveNew: true);
            }
        }
    }
}
