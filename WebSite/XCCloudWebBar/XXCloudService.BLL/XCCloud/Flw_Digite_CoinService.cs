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
    
    public partial class Flw_Digite_CoinService : BaseService<Flw_Digite_Coin>, IFlw_Digite_CoinService
    {
    	public override void SetDal()
        {
    
        }
    
        public Flw_Digite_CoinService()
            : this(false)
        {
    
        }
    
        public Flw_Digite_CoinService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IFlw_Digite_CoinDAL>(resolveNew: resolveNew);
        }
    
    	public static IFlw_Digite_CoinService I
        {
            get
            {
                return BLLContainer.Resolve<IFlw_Digite_CoinService>();
            }
        }
    
        public static IFlw_Digite_CoinService N
        {
            get
            {
                return BLLContainer.Resolve<IFlw_Digite_CoinService>(resolveNew: true);
            }
        }
    }
}
