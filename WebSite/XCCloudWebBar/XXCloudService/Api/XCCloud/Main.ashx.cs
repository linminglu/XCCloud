﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XCCloudWebBar.Base;
using XCCloudWebBar.BLL.Container;
using XCCloudWebBar.BLL.IBLL.XCCloud;
using XCCloudWebBar.BLL.XCCloud;
using XCCloudWebBar.Common;
using XCCloudWebBar.Model.CustomModel.XCCloud;
using XCCloudWebBar.Model.XCCloud;
using System.Transactions;
using System.Data.SqlClient;
using XCCloudWebBar.BLL.CommonBLL;
using XCCloudWebBar.CacheService;
using XCCloudWebBar.Business;
using XXCloudService.Api.XCCloud.Common;

namespace XXCloudService.Api.XCCloud
{
    /// <summary>
    /// Main 的摘要说明
    /// </summary>
    public class Main : ApiBase
    {

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetMenus(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string logId = userTokenKeyModel.LogId;
                int logType = (int)userTokenKeyModel.LogType;
                string merchId = (userTokenKeyModel.DataModel != null) ? (userTokenKeyModel.DataModel as TokenDataModel).MerchID : string.Empty;
                string storeId = (userTokenKeyModel.DataModel != null) ? (userTokenKeyModel.DataModel as TokenDataModel).StoreID : string.Empty;

                //返回商户信息和功能菜单信息
                string sql = " exec  SP_GetMenus @LogType,@LogID,@MerchID,@StoreID";
                SqlParameter[] parameters = new SqlParameter[4];
                parameters[0] = new SqlParameter("@LogType", logType);
                parameters[1] = new SqlParameter("@LogID", Convert.ToInt32(logId));
                parameters[2] = new SqlParameter("@MerchID", merchId);
                parameters[3] = new SqlParameter("@StoreID", storeId);
                System.Data.DataSet ds = XCCloudBLL.ExecuteQuerySentence(sql, parameters);
                if (ds.Tables.Count != 1)
                {
                    errMsg = "获取数据异常";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var list = Utils.GetModelList<MenuInfoModel>(ds.Tables[0]);
                
                //实例化一个根节点
                MenuInfoModel rootRoot = new MenuInfoModel();
                rootRoot.ParentID = 0;
                TreeHelper.LoopToAppendChildren(list, rootRoot);

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, rootRoot.Children);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }
    }
}