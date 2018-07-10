﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using XCCloudService.Base;
using XCCloudService.BLL.CommonBLL;
using XCCloudService.BLL.XCCloud;
using XCCloudService.Common;
using XCCloudService.Common.Extensions;
using XCCloudService.DBService.BLL;
using XCCloudService.Model.CustomModel.XCCloud;

namespace XXCloudService.Api.XCCloud
{
    [Authorize(Roles = "StoreUser")]
    /// <summary>
    /// MemberManage 的摘要说明
    /// </summary>
    public class MemberManage : ApiBase
    {
        /// <summary>
        /// 会员档案查询
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryMemberInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                //string merchId = "100016";
                //string storeId = "100016420111001";

                string errMsg = string.Empty;
                object[] conditions = dicParas.ContainsKey("conditions") ? (object[])dicParas["conditions"] : null;

                SqlParameter[] parameters = new SqlParameter[2];
                parameters[0] = new SqlParameter("@MerchID", merchId);
                parameters[1] = new SqlParameter("@StoreID", storeId);

                string sqlWhere = string.Empty;
                if (conditions != null && conditions.Length > 0)
                {
                    if (!QueryBLL.GenDynamicSql(conditions, "a.", ref sqlWhere, ref parameters, out errMsg))
                    {
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                }

                string storedProcedure = "QueryMemberInfo";
                Array.Resize(ref parameters, parameters.Length + 1);
                parameters[parameters.Length - 1] = new SqlParameter("@SqlWhere", sqlWhere);
                Array.Resize(ref parameters, parameters.Length + 1);
                parameters[parameters.Length - 1] = new SqlParameter("@Result", SqlDbType.Int);
                parameters[parameters.Length - 1].Direction = ParameterDirection.Output;

                System.Data.DataSet ds = XCCloudBLL.GetStoredProcedureSentence(storedProcedure, parameters);
                if (parameters[parameters.Length - 1].Value.ToString() != "1")
                {
                    errMsg = "查询会员档案数据失败";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }
                                
                if (ds.Tables.Count > 1)
                {
                    var jsonArr = new {
                        table1 = Utils.DataTableToJson(ds.Tables[0]),
                        table2 = ds.Tables[1].Rows.Cast<DataRow>().ToDictionary(x => x[0].ToString(), x => x[1].ToString())
                    };

                    return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, jsonArr);
                }

                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, "查询结果数据失败");
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 会员档案-押金
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryMemberDepositInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

                string errMsg = string.Empty;
                if(!dicParas.Get("id").Nonempty("卡ID", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var id = dicParas.Get("id");

                var data_Member_CardService = Data_Member_CardService.I;
                if (!data_Member_CardService.Any(p=>p.ID.Equals(id, StringComparison.OrdinalIgnoreCase)))
                {
                    errMsg = "该卡信息不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var data_Member_CardModel = data_Member_CardService.GetModels(p=>p.ID.Equals(id, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if(data_Member_CardModel.CardType == 0) //如果是主卡, 查出所有附属卡（包括主卡)
                {
                    var data_Member_Card = data_Member_CardService.GetModels(p => p.ParentCard.Equals(id, StringComparison.OrdinalIgnoreCase))
                                           .Select(o => new
                                           {
                                               ICCardID = o.ICCardID,
                                               CardType = "附属卡",
                                               Deposit = o.Deposit,
                                               CardName = o.CardName,
                                               CardBirthDay = o.CardBirthDay,
                                               CardSex = o.CardSex == 0 ? "男" : o.CardSex == 1 ? "女" : string.Empty
                                           }).ToList();
                    data_Member_Card.Insert(0, new{
                        ICCardID = data_Member_CardModel.ICCardID,
                        CardType = "主卡",
                        Deposit = data_Member_CardModel.Deposit,
                        CardName = data_Member_CardModel.CardName,
                        CardBirthDay = data_Member_CardModel.CardBirthDay,
                        CardSex = data_Member_CardModel.CardSex == 0 ? "男" : data_Member_CardModel.CardSex == 1 ? "女" : string.Empty
                    });

                    return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, data_Member_Card);
                }
                else
                {
                    var data_Member_Card = new List<Object>();
                    data_Member_Card.Add(new
                    {
                        ICCardID = data_Member_CardModel.ICCardID,
                        CardType = data_Member_CardModel.CardType == 1 ? "附属卡" : string.Empty,
                        Deposit = data_Member_CardModel.Deposit,
                        CardName = data_Member_CardModel.CardName,
                        CardBirthDay = data_Member_CardModel.CardBirthDay,
                        CardSex = data_Member_CardModel.CardSex == 0 ? "男" : data_Member_CardModel.CardSex == 1 ? "女" : string.Empty
                    });

                    return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, data_Member_Card);
                }
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }
    }
}