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
    
    public partial class Flw_MemberCard_BalanceChargeService : BaseService<Flw_MemberCard_BalanceCharge>, IFlw_MemberCard_BalanceChargeService
    {
    	public override void SetDal()
        {
    
        }
    
        public Flw_MemberCard_BalanceChargeService()
            : this(false)
        {
    
        }
    
        public Flw_MemberCard_BalanceChargeService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IFlw_MemberCard_BalanceChargeDAL>(resolveNew: resolveNew);
        }
    
    	public static IFlw_MemberCard_BalanceChargeService I
        {
            get
            {
                return BLLContainer.Resolve<IFlw_MemberCard_BalanceChargeService>();
            }
        }
    
        public static IFlw_MemberCard_BalanceChargeService N
        {
            get
            {
                return BLLContainer.Resolve<IFlw_MemberCard_BalanceChargeService>(resolveNew: true);
            }
        }
    }
}