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
    
    public partial class Flw_Project_TicketUseService : BaseService<Flw_Project_TicketUse>, IFlw_Project_TicketUseService
    {
    	public override void SetDal()
        {
    
        }
    
        public Flw_Project_TicketUseService()
            : this(false)
        {
    
        }
    
        public Flw_Project_TicketUseService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IFlw_Project_TicketUseDAL>(resolveNew: resolveNew);
        }
    
    	public static IFlw_Project_TicketUseService I
        {
            get
            {
                return BLLContainer.Resolve<IFlw_Project_TicketUseService>();
            }
        }
    
        public static IFlw_Project_TicketUseService N
        {
            get
            {
                return BLLContainer.Resolve<IFlw_Project_TicketUseService>(resolveNew: true);
            }
        }
    }
}
