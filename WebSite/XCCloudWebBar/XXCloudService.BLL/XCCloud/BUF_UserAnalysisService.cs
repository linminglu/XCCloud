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
    
    public partial class BUF_UserAnalysisService : BaseService<BUF_UserAnalysis>, IBUF_UserAnalysisService
    {
    	public override void SetDal()
        {
    
        }
    
        public BUF_UserAnalysisService()
            : this(false)
        {
    
        }
    
        public BUF_UserAnalysisService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IBUF_UserAnalysisDAL>(resolveNew: resolveNew);
        }
    
    	public static IBUF_UserAnalysisService I
        {
            get
            {
                return BLLContainer.Resolve<IBUF_UserAnalysisService>();
            }
        }
    
        public static IBUF_UserAnalysisService N
        {
            get
            {
                return BLLContainer.Resolve<IBUF_UserAnalysisService>(resolveNew: true);
            }
        }
    }
}