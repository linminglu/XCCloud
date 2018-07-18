using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using XCCloudService.Base;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
using XCCloudService.Common;
using XCCloudService.Common.Enum;
using XCCloudService.Common.Extensions;
using XCCloudService.DBService.BLL;
using XCCloudService.Model.CustomModel.XCCloud;
using XCCloudService.Model.XCCloud;
using XXCloudService.Api.XCCloud.Common;

namespace XXCloudService.Api.XCCloud
{
    [Authorize(Roles = "MerchUser, StoreUser")]
    /// <summary>
    /// GroupVerify 的摘要说明
    /// </summary>
    public class GroupVerify : ApiBase
    {

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryGroupVerify(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

                IData_WorkstationService data_WorkstationService = BLLContainer.Resolve<IData_WorkstationService>(resolveNew: true);
                IBase_UserInfoService base_UserInfoService = BLLContainer.Resolve<IBase_UserInfoService>(resolveNew:true);
                IFlw_Schedule_UserInfoService flw_Schedule_UserInfoService = BLLContainer.Resolve<IFlw_Schedule_UserInfoService>(resolveNew: true);
                IBase_StoreInfoService base_StoreInfoService = BLLContainer.Resolve<IBase_StoreInfoService>(resolveNew:true);
                IDict_SystemService dict_SystemService = BLLContainer.Resolve<IDict_SystemService>(resolveNew:true);
                IFlw_GroupVerityService flw_GroupVerityService = BLLContainer.Resolve<IFlw_GroupVerityService>(resolveNew:true);
                IQueryable<Flw_GroupVerity> query = flw_GroupVerityService.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase));
                if (userTokenKeyModel.LogType == (int)RoleType.StoreUser)
                {
                    query = flw_GroupVerityService.GetModels(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase));
                }
                int GroupTypeId = dict_SystemService.GetModels(p => p.DictKey.Equals("团购渠道") && p.PID == 0).FirstOrDefault().ID;
                var result = from a in query
                             join b in dict_SystemService.GetModels() on (a.GroupType + "") equals b.DictValue into b1
                             from b in b1.DefaultIfEmpty()
                             join c in base_StoreInfoService.GetModels() on a.StoreID equals c.ID
                             join d in
                                 (
                                      from d in flw_Schedule_UserInfoService.GetModels()
                                      join e in base_UserInfoService.GetModels() on d.UserID equals e.ID
                                      select new { d.ScheduleID, e.LogName }
                                 ) on a.ScheduleID equals d.ScheduleID
                             join f in data_WorkstationService.GetModels() on a.WorkStationID equals f.ID
                             orderby a.ID
                             select new
                             {
                                 TicketCode = a.TicketCode,
                                 GroupTypeStr = b != null ? b.DictKey : string.Empty,
                                 Coin = a.Coin,
                                 VerityTime = a.VerityTime,
                                 StoreName = c.StoreName,
                                 WorkStation = f.WorkStation,
                                 LogName = d.LogName
                             };

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, result);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

    }
}