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
    
    public partial class Log_OperationService : BaseService<Log_Operation>, ILog_OperationService
    {
    	public override void SetDal()
        {
    
        }
    
        public Log_OperationService()
            : this(false)
        {
    
        }
    
        public Log_OperationService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<ILog_OperationDAL>(resolveNew: resolveNew);
        }
    
    	public static ILog_OperationService I
        {
            get
            {
                return BLLContainer.Resolve<ILog_OperationService>();
            }
        }
    
        public static ILog_OperationService N
        {
            get
            {
                return BLLContainer.Resolve<ILog_OperationService>(resolveNew: true);
            }
        }
    }
}
