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
    
    public partial class XC_SyncBufService : BaseService<XC_SyncBuf>, IXC_SyncBufService
    {
    	public override void SetDal()
        {
    
        }
    
        public XC_SyncBufService()
            : this(false)
        {
    
        }
    
        public XC_SyncBufService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IXC_SyncBufDAL>(resolveNew: resolveNew);
        }
    
    	public static IXC_SyncBufService I
        {
            get
            {
                return BLLContainer.Resolve<IXC_SyncBufService>();
            }
        }
    
        public static IXC_SyncBufService N
        {
            get
            {
                return BLLContainer.Resolve<IXC_SyncBufService>(resolveNew: true);
            }
        }
    }
}