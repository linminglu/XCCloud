using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.Base;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
using XCCloudService.Model.XCCloud;

namespace XCCloudService.Business.XCCloud
{
    public class WorkStationBusiness
    {
        public static bool RegisteWorkStation(string dogId, string workStation,out string merchId,out string storeId)
        {
            IBase_StoreDogListService storeDogService = BLLContainer.Resolve<IBase_StoreDogListService>();
            var storeDogModel = storeDogService.GetModels(p => p.DogID.Equals(dogId)).ToList<Base_StoreDogList>()[0];
            if (storeDogModel == null)
            {
                merchId = storeDogModel.MerchID;
                storeId = storeDogModel.StoreID;
                return false;
            }

            IData_WorkstationService workstationService = BLLContainer.Resolve<IData_WorkstationService>();
            var wsCount = workstationService.GetModels(p => p.WorkStation.Equals(workStation)).Count();
            if (wsCount > 0)
            {
                merchId = storeDogModel.MerchID;
                storeId = storeDogModel.StoreID;
                return true;
            }
            else
            {
                Data_Workstation wsModel = new Data_Workstation();
                wsModel.MerchID = storeDogModel.MerchID;
                wsModel.StoreID = storeDogModel.StoreID;
                wsModel.WorkStation = workStation;
                wsModel.DepotID = 0;
                wsModel.MacAddress = workStation;
                wsModel.DiskID = workStation;
                wsModel.State = 0;
                workstationService.AddModel(wsModel);

                merchId = storeDogModel.MerchID;
                storeId = storeDogModel.StoreID;

                return true;
            }
        }


        public static bool GetWorkStation(string merchId, string storeId, string workStation, out int workStationId, out string errMsg)
        {
            errMsg = string.Empty;
            workStationId = 0;
            IData_WorkstationService workstationService = BLLContainer.Resolve<IData_WorkstationService>();
            var model = workstationService.GetModels(p => p.MerchID.Equals(merchId) && p.StoreID.Equals(storeId) && p.WorkStation.Equals(workStation) && p.State == 1).ToList<Data_Workstation>()[0];
            if (model == null)
            {
                workStationId = model.ID;
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
