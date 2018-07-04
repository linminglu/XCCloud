using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XCCloudWebBar.Base;
using XCCloudWebBar.BLL.CommonBLL;
using XCCloudWebBar.BLL.Container;
using XCCloudWebBar.BLL.IBLL.XCGameManager;
using XCCloudWebBar.Business.XCGameMana;
using XCCloudWebBar.Common;
using XCCloudWebBar.Common.Enum;
using XCCloudWebBar.Model.CustomModel.XCGameManager;
using XCCloudWebBar.Model.XCGameManager;

namespace XXCloudService.Api.XCGameMana
{
    /// <summary>
    /// Device 的摘要说明
    /// </summary>
    public class Device : ApiBase
    {
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MethodToken)]
        public object register(Dictionary<string, object> dicParas)
        {
            try
            {
                string storeId = dicParas.ContainsKey("storeId") ? dicParas["storeId"].ToString() : string.Empty;
                string mcuId = dicParas.ContainsKey("mcuId") ? dicParas["mcuId"].ToString() : string.Empty;
                XCCloudWebBar.Model.CustomModel.XCGameManager.StoreCacheModel storeCacheModel = null;
                string errMsg = string.Empty;
                string deviceToken = string.Empty;
                //验证门店信息
                XCCloudWebBar.Business.XCGameMana.StoreBusiness xcGameManaStoreBusiness = new XCCloudWebBar.Business.XCGameMana.StoreBusiness();
                if (!xcGameManaStoreBusiness.IsEffectiveStore(storeId, ref storeCacheModel, out errMsg))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "门店不存在");
                }
                //获取子库中是否存在mcuId
                XCCloudWebBar.BLL.IBLL.XCGame.IDeviceService xcGameDeviceService = BLLContainer.Resolve<XCCloudWebBar.BLL.IBLL.XCGame.IDeviceService>(storeCacheModel.StoreDBName);
                var xcGameDeviceModel = xcGameDeviceService.GetModels(p => p.MCUID.Equals(mcuId)).FirstOrDefault<XCCloudWebBar.Model.XCGame.t_device>();
                if (xcGameDeviceModel == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "设备不存在");
                }
                //获取总库中是否存在
                XCCloudWebBar.BLL.IBLL.XCGameManager.IDeviceService xcGameManaDeviceService = BLLContainer.Resolve<XCCloudWebBar.BLL.IBLL.XCGameManager.IDeviceService>();
                var xcGameManaDeviceModel = xcGameManaDeviceService.GetModels(p => p.StoreType == (int)(XCGameManaDeviceStoreType.Store) && p.DeviceId.Equals(mcuId)).FirstOrDefault<XCCloudWebBar.Model.XCGameManager.t_device>();
                if (xcGameManaDeviceModel != null)
                {
                    if (!xcGameManaDeviceModel.DeviceName.Equals(xcGameDeviceModel.name) || !xcGameManaDeviceModel.DeviceType.Equals(xcGameDeviceModel.type))
                    {
                        xcGameManaDeviceModel.DeviceType = xcGameDeviceModel.type;
                        xcGameManaDeviceModel.DeviceName = xcGameDeviceModel.name;
                        xcGameManaDeviceService.Update(xcGameManaDeviceModel);
                    }
                    deviceToken = xcGameManaDeviceModel.TerminalNo;
                }
                else
                {
                    deviceToken = DeviceManaBusiness.GetDeviceToken();
                    bool isExist = false;
                    while (isExist == false)
                    {
                        if (xcGameManaDeviceService.GetCount(p => p.TerminalNo.Equals(deviceToken)) == 0)
                        {
                            isExist = true;
                        }
                        else 
                        {
                            deviceToken = DeviceManaBusiness.GetDeviceToken();
                        }
                        System.Threading.Thread.Sleep(100);
                    }
                    xcGameManaDeviceModel = new XCCloudWebBar.Model.XCGameManager.t_device();
                    xcGameManaDeviceModel.DeviceId = xcGameDeviceModel.MCUID;
                    xcGameManaDeviceModel.TerminalNo = deviceToken;
                    xcGameManaDeviceModel.DeviceName = xcGameDeviceModel.name;
                    xcGameManaDeviceModel.DeviceType = xcGameDeviceModel.type;
                    xcGameManaDeviceModel.StoreId = storeId;
                    xcGameManaDeviceModel.Note = xcGameDeviceModel.note;
                    xcGameManaDeviceModel.StoreType = (int)(XCGameManaDeviceStoreType.Store);
                    xcGameManaDeviceService.Add(xcGameManaDeviceModel);
                }

                var obj = new { deviceToken = deviceToken };
                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn,obj);
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// 添加设备信息
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MethodToken)]
        public object addDevice(Dictionary<string, object> dicParas)
        {
            string storeId = dicParas.ContainsKey("storeId") ? dicParas["storeId"].ToString() : string.Empty;
            string mcuId = dicParas.ContainsKey("mcuId") ? dicParas["mcuId"].ToString() : string.Empty;
            string deviceToken = string.Empty;
            if (string.IsNullOrEmpty(storeId))
            {
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "门店号无效");
            }

            if (string.IsNullOrEmpty(mcuId))
            {
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "设备ID无效");
            }

            int storeids = int.Parse(storeId);
            IStoreService storeService = BLLContainer.Resolve<IStoreService>();
            var menlist = storeService.GetModels(x => x.id == storeids.ToString()).FirstOrDefault<t_store>();
            if (menlist == null)
            {
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.T, "门店号无效");
            }
            deviceToken = DeviceManaBusiness.GetDeviceToken();
            int StoreType = 1;
            string dbname = menlist.store_dbname;
            if (dbname == "XCCloudRS232")
            {
                StoreType = 0;
            }
            IDeviceService device = BLLContainer.Resolve<IDeviceService>();
            var devicelist = device.GetModels(x => x.DeviceId == mcuId&&x.StoreId==storeId).FirstOrDefault<t_device>();
            if (devicelist != null)
            {
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "该店号已经存在该设备信息!");
            }
            string sql = @"select a.MCUID,b.GameName,b.GameType from t_head a inner join t_game b on a.GameID = b.GameID where MCUID = '"+ mcuId + "'";
            System.Data.DataSet ds = XCGameBLL.ExecuteQuerySentence(sql, dbname, null);
            if (ds.Tables[0].Rows.Count > 0)
            {
                DeviceModel deviceModel = Utils.GetModelList<DeviceModel>(ds.Tables[0])[0];              
                t_device device1 = new t_device();
                device1.TerminalNo = deviceToken;
                device1.StoreId = storeId;
                device1.StoreType = StoreType;
                device1.DeviceName = deviceModel.GameName;
                device1.DeviceType = deviceModel.GameType;
                device1.DeviceId = deviceModel.MCUID;
                device.Add(device1);
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.T, "");
            }
            return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "未查询到相关数据");
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MethodToken)]
        public object getDeviceState(Dictionary<string, object> dicParas)
        {
            string storeId = string.Empty;
            string state = string.Empty;
            string stateName = string.Empty;
            string mcuId = string.Empty;
            string segment = string.Empty;
            string xcGameDBName = string.Empty;
            int deviceIdentityId;
            string storePassword = string.Empty;
            string storeName = string.Empty;
            string errMsg = string.Empty;
            string deviceId = string.Empty;
            string deviceToken = dicParas.ContainsKey("deviceToken") ? dicParas["deviceToken"].ToString() : string.Empty;

            XCGameManaDeviceStoreType deviceStoreType;
            if (!ExtendBusiness.checkXCGameManaDeviceInfo(deviceToken, out deviceStoreType, out storeId, out deviceId))
            {
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "终端号不存在");
            }

            if (deviceStoreType == XCGameManaDeviceStoreType.Store)
            {
                //验证门店信息和设备状态是否为启用状态
                if (!ExtendBusiness.checkStoreDeviceInfo(deviceStoreType, storeId, deviceId, out segment, out mcuId, out xcGameDBName, out deviceIdentityId, out storePassword, out storeName, out errMsg))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, errMsg);
                }
                //验证雷达设备缓存状态
                if (!ExtendBusiness.checkRadarDeviceState(deviceStoreType, storeId, deviceId, out state,out stateName,out errMsg))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, errMsg);
                }

                var obj = new {
                    deviceToken = deviceToken,
                    deviceId = deviceId,
                    mcuId = mcuId,
                    segment = segment,
                    storeName = storeName
                };
                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, obj);
            }
            else
            {
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "终端号无效");
            }
        }
    }
}