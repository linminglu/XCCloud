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
    
    public partial class Base_WechatFunctionService : BaseService<Base_WechatFunction>, IBase_WechatFunctionService
    {
    	public override void SetDal()
        {
    
        }
    
        public Base_WechatFunctionService()
            : this(false)
        {
    
        }
    
        public Base_WechatFunctionService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IBase_WechatFunctionDAL>(resolveNew: resolveNew);
        }
    
    	public static IBase_WechatFunctionService I
        {
            get
            {
                return BLLContainer.Resolve<IBase_WechatFunctionService>();
            }
        }
    
        public static IBase_WechatFunctionService N
        {
            get
            {
                return BLLContainer.Resolve<IBase_WechatFunctionService>(resolveNew: true);
            }
        }
    }
}
