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
    
    public partial class Data_Discount_RecordMemberService : BaseService<Data_Discount_RecordMember>, IData_Discount_RecordMemberService
    {
    	public override void SetDal()
        {
    
        }
    
        public Data_Discount_RecordMemberService()
            : this(false)
        {
    
        }
    
        public Data_Discount_RecordMemberService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_Discount_RecordMemberDAL>(resolveNew: resolveNew);
        }
    
    	public static IData_Discount_RecordMemberService I
        {
            get
            {
                return BLLContainer.Resolve<IData_Discount_RecordMemberService>();
            }
        }
    
        public static IData_Discount_RecordMemberService N
        {
            get
            {
                return BLLContainer.Resolve<IData_Discount_RecordMemberService>(resolveNew: true);
            }
        }
    }
}
