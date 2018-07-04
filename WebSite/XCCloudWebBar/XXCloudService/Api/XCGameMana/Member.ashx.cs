using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using XCCloudWebBar.Base;
using XCCloudWebBar.BLL.CommonBLL;
using XCCloudWebBar.BLL.Container;
using XCCloudWebBar.BLL.IBLL.XCGameManager;
using XCCloudWebBar.Business.Common;
using XCCloudWebBar.Business.WeiXin;
using XCCloudWebBar.Business.XCGame;
using XCCloudWebBar.Business.XCGameMana;
using XCCloudWebBar.CacheService;
using XCCloudWebBar.Common;
using XCCloudWebBar.Model.CustomModel.XCGame;
using XCCloudWebBar.Model.CustomModel.XCGameManager;
using XCCloudWebBar.Model.XCGameManager;

namespace XXCloudService.Api.XCGameMana
{
    /// <summary>
    /// Membercard 的摘要说明
    /// </summary>
    public class Member : ApiBase
    {
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MobileToken)]
        public object getMemberCard(Dictionary<string, object> dicParas)
        {
            try
            {
                string mobile = string.Empty;
                MobileTokenModel mobileTokenModel = (MobileTokenModel)(dicParas[Constant.MobileTokenModel]);
                string sql = "exec SelectMenber @Mobile,@Return output";
                SqlParameter[] parameters = new SqlParameter[2];
                parameters[0] = new SqlParameter("@Mobile", mobileTokenModel.Mobile);
                parameters[1] = new SqlParameter("@Return", 0);
                parameters[1].Direction = System.Data.ParameterDirection.Output;
                System.Data.DataSet ds = XCGameManabll.ExecuteQuerySentence(sql, parameters);
                DataTable dt = ds.Tables[0];
                if (dt.Rows.Count == 0)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "用户未开卡");
                }
                var list = Utils.GetModelList<TmpmemberModel>(ds.Tables[0]).ToList();

                return ResponseModelFactory<List<TmpmemberModel>>.CreateModel(isSignKeyReturn, list);
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCGameMemberToken)]
        public object getMemberSaveMoney(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string xcGameDBName = string.Empty;
                string state = "0";//设备状态
                string stateName = string.Empty;
                string password = string.Empty;
                XCGameMemberTokenModel memberTokenKeyModel = (XCGameMemberTokenModel)(dicParas[Constant.XCGameMemberTokenModel]);
                if (memberTokenKeyModel == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "会员令牌不存在");
                }
                string MobileToken = dicParas.ContainsKey("mobileToken") ? dicParas["mobileToken"].ToString() : string.Empty;
                if (MobileToken == "")
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "手机令牌不能为空");
                }

                string terminalNo = dicParas.ContainsKey("deviceToken") ? dicParas["deviceToken"].ToString() : string.Empty;
                IDeviceService deviceService = BLLContainer.Resolve<IDeviceService>();
                var deviceModel = deviceService.GetModels(p => p.TerminalNo.Equals(terminalNo, StringComparison.OrdinalIgnoreCase)).FirstOrDefault<t_device>();
                if (deviceModel == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "终端号不存在");
                }
                string DeviceStoreID = deviceModel.StoreId;
                string StoreID = memberTokenKeyModel.StoreId;
                StoreBusiness store = new StoreBusiness();
                if (!store.IsEffectiveStore(memberTokenKeyModel.StoreId, out xcGameDBName, out password, out errMsg))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, errMsg);
                }
                if (DeviceStoreID != StoreID)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "当前设备无效");
                }
                if (DeviceStateBusiness.ExistDeviceState(deviceModel.StoreId, deviceModel.DeviceId))
                {
                    state = DeviceStateBusiness.GetDeviceState(deviceModel.StoreId, deviceModel.DeviceId);
                }
                if (state != "1")
                {
                    stateName = DeviceStateBusiness.GetStateName(state);
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, stateName);
                }

                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.T, "");
            }
            catch(Exception e)
            {
                throw e;
            }
        }
    }
}