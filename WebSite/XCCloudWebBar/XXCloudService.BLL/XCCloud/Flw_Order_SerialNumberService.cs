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
    
    public partial class Flw_Order_SerialNumberService : BaseService<Flw_Order_SerialNumber>, IFlw_Order_SerialNumberService
    {
    	public override void SetDal()
        {
    
        }
    
        public Flw_Order_SerialNumberService()
            : this(false)
        {
    
        }
    
        public Flw_Order_SerialNumberService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IFlw_Order_SerialNumberDAL>(resolveNew: resolveNew);
        }
    
    	public static IFlw_Order_SerialNumberService I
        {
            get
            {
                return BLLContainer.Resolve<IFlw_Order_SerialNumberService>();
            }
        }
    
        public static IFlw_Order_SerialNumberService N
        {
            get
            {
                return BLLContainer.Resolve<IFlw_Order_SerialNumberService>(resolveNew: true);
            }
        }
    }
}
