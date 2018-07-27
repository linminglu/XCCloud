using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using XCCloudService.Base;
using XCCloudService.BLL.CommonBLL;
using XCCloudService.BLL.XCCloud;
using XCCloudService.Business.Common;
using XCCloudService.CacheService;
using XCCloudService.Common;
using XCCloudService.DBService.BLL;
using XCCloudService.Model.CustomModel.XCCloud;

namespace XXCloudService.Api.XCCloud
{
    /// <summary>
    /// GameWatch 的摘要说明
    /// </summary>
    public class GameWatch : ApiBase
    {
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCManaUserHelperToken, SysIdAndVersionNo = false)]
        public object addGameWatch(Dictionary<string, object> dicParas)
        {
            string errMsg = string.Empty;
            string url = string.Empty;
            string fileType = string.Empty;
            List<string> fileTypeList = null;
            List<UploadFileResultModel> resultList = new List<UploadFileResultModel>();
            XCManaUserHelperTokenModel userTokenModel = (XCManaUserHelperTokenModel)(dicParas[Constant.XCManaUserHelperToken]);
            string clientType = dicParas.ContainsKey("clientType") ? dicParas["clientType"].ToString() : string.Empty;
            int headIndex = Convert.ToInt32(dicParas.ContainsKey("headIndex") ? dicParas["headIndex"].ToString() : "0");//机头Id
            int inCoin = Convert.ToInt32(dicParas.ContainsKey("inCoin") ? dicParas["inCoin"].ToString() : "0");//投币码表
            int prizeCount = Convert.ToInt32(dicParas.ContainsKey("prizeCount") ? dicParas["prizeCount"].ToString() : "0");//中奖码表
            int outCoin = Convert.ToInt32(dicParas.ContainsKey("outCoin") ? dicParas["outCoin"].ToString() : "0");//出币码表
            int outLottery = Convert.ToInt32(dicParas.ContainsKey("outLottery") ? dicParas["outLottery"].ToString() : "0");//出票码表
            decimal goodPrice = Convert.ToDecimal(dicParas.ContainsKey("goodPrice") ? dicParas["goodPrice"].ToString() : "0");//礼品价值

            //验证文件数量，系统默认一次最多上传4个文件（最多3张图片和一个视频）
            if (HttpContext.Current.Request.Files.Count > 3)
            {
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "上传文件过多（最多3张图片和一个视频）");
            }
            //验证上传文件集合中，是否存在不合法的文件
            if (!FileUploadBusiness.CheckFormatAndSize(HttpContext.Current.Request.Files, ref fileTypeList, out errMsg))
            {
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, errMsg);
            }
 
            for(int i = 0; i < HttpContext.Current.Request.Files.Count;i++)
            {
                if (!FileUploadBusiness.SaveFile(HttpContext.Current.Request.Files[i], clientType,out fileType, out url, out errMsg))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, errMsg);
                }
                else
                {
                    resultList.Add(new UploadFileResultModel(fileType, url));
                }
            }

            //保存数据
            string storedProcedure = "AddGameWatch";
            SqlParameter[] parameters = new SqlParameter[17];
            parameters[0] = new SqlParameter("@StoreId", SqlDbType.VarChar);
            parameters[0].Value = userTokenModel.StoreId;
            parameters[1] = new SqlParameter("@HeadIndex", SqlDbType.Int);
            parameters[1].Value = headIndex;
            parameters[2] = new SqlParameter("@UserId", SqlDbType.Int);
            parameters[2].Value = userTokenModel.UserId;
            parameters[3] = new SqlParameter("@InCoin", SqlDbType.Int);
            parameters[3].Value = inCoin;
            parameters[4] = new SqlParameter("@PrizeCount", SqlDbType.Int);
            parameters[4].Value = prizeCount;
            parameters[5] = new SqlParameter("@OutCoin", SqlDbType.Int);
            parameters[5].Value = outCoin;
            parameters[6] = new SqlParameter("@OutLottery", SqlDbType.Int);
            parameters[6].Value = outLottery;
            parameters[7] = new SqlParameter("@MediaUrl1", SqlDbType.VarChar);
            parameters[7].Value = resultList.Count > 0 ? ((UploadFileResultModel)(resultList[0])).Url:string.Empty;
            parameters[8] = new SqlParameter("@MediaUrl2", SqlDbType.VarChar);
            parameters[8].Value = resultList.Count > 1 ? ((UploadFileResultModel)(resultList[1])).Url : string.Empty;
            parameters[9] = new SqlParameter("@MediaUrl3", SqlDbType.VarChar);
            parameters[9].Value = resultList.Count > 2 ? ((UploadFileResultModel)(resultList[2])).Url : string.Empty;

            parameters[10] = new SqlParameter("@InCoinError", SqlDbType.Int);
            parameters[10].Direction = ParameterDirection.Output;
            parameters[11] = new SqlParameter("@OutCoinError", SqlDbType.Int);
            parameters[11].Direction = ParameterDirection.Output;
            parameters[12] = new SqlParameter("@PrizeError", SqlDbType.Int);
            parameters[12].Direction = ParameterDirection.Output;
            parameters[13] = new SqlParameter("@OutLotteryError", SqlDbType.Int);
            parameters[13].Direction = ParameterDirection.Output;
            parameters[14] = new SqlParameter("@GoodPrice", SqlDbType.Decimal);
            parameters[14].Direction = ParameterDirection.Output;

            parameters[15] = new SqlParameter("@ErrMsg", SqlDbType.VarChar, 200);
            parameters[15].Direction = ParameterDirection.Output;
            parameters[16] = new SqlParameter("@Return", SqlDbType.Int);
            parameters[16].Direction = ParameterDirection.ReturnValue;
            XCCloudBLL.ExecuteStoredProcedureSentence(storedProcedure, parameters);

            if (parameters[16].Value.ToString() == "1")
            {
                var obj = new
                {
                    inCoinError = Convert.ToInt32(parameters[10].Value),
                    outCoinError = Convert.ToInt32(parameters[11].Value),
                    prizeError = Convert.ToInt32(parameters[12].Value),
                    goodPrice = Convert.ToDecimal(parameters[14].Value),
                    outLotteryError = Convert.ToInt32(parameters[13].Value)
                };
                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, obj);
            }
            else
            {
                return new ResponseModel(Return_Code.T, "", Result_Code.F, parameters[15].Value.ToString());
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryGameWatch(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

                string errMsg = string.Empty;
                object[] conditions = dicParas.ContainsKey("conditions") ? (object[])dicParas["conditions"] : null;

                SqlParameter[] parameters = new SqlParameter[0];
                string sqlWhere = string.Empty;

                if (conditions != null && conditions.Length > 0)
                    if (!QueryBLL.GenDynamicSql(conditions, "a.", ref sqlWhere, ref parameters, out errMsg))
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                string sql = @"SELECT a.* from (                                
                                SELECT distinct
                                    a.ID, b.GameName, c.DeviceName, c.SiteName, u.LogName, a.CreateTime, 
                                    a.InCoin, a.InCoinError, a.InCoin2, a.InCoinError2, a.PrizeCount, a.PrizeError, 
                                    a.GoodPrice, a.OutCoin, a.OutCoinError, a.OutLottery, a.OutLotteryError, a.Winner, a.WinnerError
                                FROM
                                	Flw_Game_Watch a
                                LEFT JOIN Data_GameInfo b ON a.GameIndex=b.ID                             
                                LEFT JOIN Base_DeviceInfo c ON a.HeadIndex=c.ID
                                LEFT JOIN Base_UserInfo u ON a.UserID=u.ID                                 
                                WHERE a.MerchID='" + merchId + "' AND a.StoreID='" + storeId + @"'                                
                                ) a WHERE 1=1";
                sql = sql + sqlWhere;
                sql = sql + " ORDER BY a.ID";

                var list = Data_GameInfoService.I.SqlQuery<Flw_Game_WatchList>(sql, parameters).ToList();

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, list);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }
    }
}