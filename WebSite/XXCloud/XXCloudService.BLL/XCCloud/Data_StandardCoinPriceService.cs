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
    
    public partial class Data_StandardCoinPriceService : BaseService<Data_StandardCoinPrice>, IData_StandardCoinPriceService
    {
    	public override void SetDal()
        {
    
        }
    
        public Data_StandardCoinPriceService()
            : this(false)
        {
    
        }
    
        public Data_StandardCoinPriceService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_StandardCoinPriceDAL>(resolveNew: resolveNew);
        }
    
    	public static IData_StandardCoinPriceService I
        {
            get
            {
                return BLLContainer.Resolve<IData_StandardCoinPriceService>();
            }
        }
    
        public static IData_StandardCoinPriceService N
        {
            get
            {
                return BLLContainer.Resolve<IData_StandardCoinPriceService>(resolveNew: true);
            }
        }
    }
}
