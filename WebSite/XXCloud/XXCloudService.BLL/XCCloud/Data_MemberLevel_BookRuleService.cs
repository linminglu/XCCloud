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
    
    public partial class Data_MemberLevel_BookRuleService : BaseService<Data_MemberLevel_BookRule>, IData_MemberLevel_BookRuleService
    {
    	public override void SetDal()
        {
    
        }
    
        public Data_MemberLevel_BookRuleService()
            : this(false)
        {
    
        }
    
        public Data_MemberLevel_BookRuleService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_MemberLevel_BookRuleDAL>(resolveNew: resolveNew);
        }
    
    	public static IData_MemberLevel_BookRuleService I
        {
            get
            {
                return BLLContainer.Resolve<IData_MemberLevel_BookRuleService>();
            }
        }
    
        public static IData_MemberLevel_BookRuleService N
        {
            get
            {
                return BLLContainer.Resolve<IData_MemberLevel_BookRuleService>(resolveNew: true);
            }
        }
    }
}
