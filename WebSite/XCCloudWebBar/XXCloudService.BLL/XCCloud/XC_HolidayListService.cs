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
    
    public partial class XC_HolidayListService : BaseService<XC_HolidayList>, IXC_HolidayListService
    {
    	public override void SetDal()
        {
    
        }
    
        public XC_HolidayListService()
            : this(false)
        {
    
        }
    
        public XC_HolidayListService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IXC_HolidayListDAL>(resolveNew: resolveNew);
        }
    
    	public static IXC_HolidayListService I
        {
            get
            {
                return BLLContainer.Resolve<IXC_HolidayListService>();
            }
        }
    
        public static IXC_HolidayListService N
        {
            get
            {
                return BLLContainer.Resolve<IXC_HolidayListService>(resolveNew: true);
            }
        }
    }
}