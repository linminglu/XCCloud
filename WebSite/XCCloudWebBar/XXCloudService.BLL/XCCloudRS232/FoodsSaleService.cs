using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudWebBar.BLL.Base;
using XCCloudWebBar.BLL.IBLL.XCCloudRS232;
using XCCloudWebBar.DAL.Container;
using XCCloudWebBar.DAL.IDAL.XCCloudRS232;
using XCCloudWebBar.Model.XCCloudRS232;

namespace XCCloudWebBar.BLL.XCCloudRS232
{
    public partial class FoodSaleService : BaseService<flw_food_sale>, IFoodSaleService
    {
        private IFoodSaleDAL deviceDAL = DALContainer.Resolve<IFoodSaleDAL>();
        public override void SetDal()
        {
            Dal = deviceDAL;
        }
    }
 
}
