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
    
    public partial class Data_GameAPP_MemberRuleService : BaseService<Data_GameAPP_MemberRule>, IData_GameAPP_MemberRuleService
    {
    	public override void SetDal()
        {
    
        }
    
        public Data_GameAPP_MemberRuleService()
            : this(false)
        {
    
        }
    
        public Data_GameAPP_MemberRuleService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_GameAPP_MemberRuleDAL>(resolveNew: resolveNew);
        }
    
    	public static IData_GameAPP_MemberRuleService I
        {
            get
            {
                return BLLContainer.Resolve<IData_GameAPP_MemberRuleService>();
            }
        }
    
        public static IData_GameAPP_MemberRuleService N
        {
            get
            {
                return BLLContainer.Resolve<IData_GameAPP_MemberRuleService>(resolveNew: true);
            }
        }
    }
}