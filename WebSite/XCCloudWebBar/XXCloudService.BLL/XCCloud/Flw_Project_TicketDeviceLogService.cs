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
    
    public partial class Flw_Project_TicketDeviceLogService : BaseService<Flw_Project_TicketDeviceLog>, IFlw_Project_TicketDeviceLogService
    {
    	public override void SetDal()
        {
    
        }
    
        public Flw_Project_TicketDeviceLogService()
            : this(false)
        {
    
        }
    
        public Flw_Project_TicketDeviceLogService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IFlw_Project_TicketDeviceLogDAL>(resolveNew: resolveNew);
        }
    
    	public static IFlw_Project_TicketDeviceLogService I
        {
            get
            {
                return BLLContainer.Resolve<IFlw_Project_TicketDeviceLogService>();
            }
        }
    
        public static IFlw_Project_TicketDeviceLogService N
        {
            get
            {
                return BLLContainer.Resolve<IFlw_Project_TicketDeviceLogService>(resolveNew: true);
            }
        }
    }
}