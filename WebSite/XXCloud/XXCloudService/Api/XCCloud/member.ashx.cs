using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using XCCloudService.Base;
using XCCloudService.BLL.CommonBLL;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
using XCCloudService.BLL.XCCloud;
using XCCloudService.Common;
using XCCloudService.Model.CustomModel.XCCloud;
using XCCloudService.Model.CustomModel.XCCloud.Member;
using XCCloudService.Model.XCCloud;
using System.IO;
using Microsoft.SqlServer.Server;
using XCCloudService.Common.Enum;
using XCCloudService.Common.Extensions;
using System.Transactions;
using System.Data.Entity.Validation;
using XXCloudService.Api.XCCloud.Common;
using XCCloudService.Business.Common;
using Newtonsoft.Json;
using XCCloudService.CacheService;
using XCCloudService.Business.XCCloud;

namespace XCCloudService.Api.XCCloud
{    
    /// <summary>
    /// member 的摘要说明
    /// </summary>
    public class Member : ApiBase
    {
        #region "登录"

            public object login(Dictionary<string, object> dicParas)
            {
                string errMsg = string.Empty;
                string mobile = dicParas.ContainsKey("mobile") ? dicParas["mobile"].ToString() : string.Empty;
                string password = dicParas.ContainsKey("password") ? dicParas["password"].ToString() : string.Empty;

                if (!checkLoginParas(dicParas, out errMsg))
                {
                    ResponseModel responseModel = new ResponseModel(Return_Code.T, "", Result_Code.F, errMsg);
                    return responseModel;
                }

                //获取会员信息
                IBase_MemberInfoService memberService = BLLContainer.Resolve<IBase_MemberInfoService>();
                var member = memberService.GetModels(p => p.Mobile.Equals(mobile, StringComparison.OrdinalIgnoreCase)).LastOrDefault<Base_MemberInfo>();
                if (member == null)
                {
                    ResponseModel responseModel = new ResponseModel(Return_Code.T, "", Result_Code.F, "会员信息不存在");
                    return responseModel;
                }
                else
                {
                    if (member.UserPassword.Equals(password))
                    {
                        ResponseModel responseModel = new ResponseModel(Return_Code.T, "", Result_Code.T, "");
                        return responseModel;
                    }
                    else
                    {
                        ResponseModel responseModel = new ResponseModel(Return_Code.T, "", Result_Code.F, "用户名或密码错");
                        return responseModel;
                    }
                }
            }

            /// <summary>
            /// 验证登录信息
            /// </summary>
            /// <param name="dicParas"></param>
            /// <param name="errMsg"></param>
            /// <returns></returns>
            private bool checkLoginParas(Dictionary<string, object> dicParas, out string errMsg)
            {
                errMsg = string.Empty;
                string mobile = dicParas.ContainsKey("mobile") ? dicParas["mobile"].ToString() : string.Empty;
                string password = dicParas.ContainsKey("password") ? dicParas["password"].ToString() : string.Empty;

                if (!Utils.CheckMobile(mobile))
                {
                    errMsg = "手机号码参数不正确";
                    return false;
                }

                if (string.IsNullOrEmpty(password))
                {
                    errMsg = "用户名参数不能为空";
                    return false;
                }

                if (password.Length > 20)
                {
                    errMsg = "用户名参数长度不能超过20个字符";
                    return false;
                }

                return true;
            }

        #endregion


        #region "验证会员是否需要注册"

            [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
            public object checkOpenCard(Dictionary<string, object> dicParas)
            {
                XCCloudUserTokenModel userTokenModel = (XCCloudUserTokenModel)(dicParas[Constant.XCCloudUserTokenModel]);
                TokenDataModel userTokenDataModel = (TokenDataModel)(userTokenModel.DataModel);
                string storeId = dicParas.ContainsKey("storeId") ? dicParas["storeId"].ToString() : string.Empty;
                string mobile = dicParas.ContainsKey("mobile") ? dicParas["mobile"].ToString() : string.Empty;
                if (!userTokenDataModel.StoreID.Equals(storeId))
                {
                    ResponseModel responseModel = new ResponseModel(Return_Code.T, "", Result_Code.F, "门店信息不正确");
                    return responseModel;
                }

                string storedProcedure = "CheckStoreCanOpenCard";
                SqlParameter[] sqlParameter = new SqlParameter[4];
                sqlParameter[0] = new SqlParameter("@StoreId", SqlDbType.VarChar, 15);
                sqlParameter[0].Value = storeId;
                sqlParameter[1] = new SqlParameter("@Mobile", SqlDbType.VarChar, 11);
                sqlParameter[1].Value = mobile;
                sqlParameter[2] = new SqlParameter("@ErrMsg", SqlDbType.VarChar, 200);
                sqlParameter[2].Direction = ParameterDirection.Output;
                sqlParameter[3] = new SqlParameter("@Return", SqlDbType.Int);
                sqlParameter[3].Direction = ParameterDirection.ReturnValue;
                XCCloudBLL.ExecuteStoredProcedureSentence(storedProcedure, sqlParameter);
                if (sqlParameter[3].Value.ToString() == "1")
                {
                    return new ResponseModel(Return_Code.T, "", Result_Code.T, "");
                }
                else
                {
                    return new ResponseModel(Return_Code.T, "", Result_Code.F, sqlParameter[2].Value.ToString());
                }
            }

        #endregion

        /// <summary>
        /// 注册会员
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken,SysIdAndVersionNo=false)]
        public object register(Dictionary<string, object> dicParas)
        {
            XCCloudUserTokenModel userTokenModel = (XCCloudUserTokenModel)(dicParas[Constant.XCCloudUserTokenModel]);
            TokenDataModel userTokenDataModel = (TokenDataModel)(userTokenModel.DataModel);
            string errMsg = string.Empty;
            string storeId = dicParas.ContainsKey("storeId") ? dicParas["storeId"].ToString() : string.Empty;
            string mobile = dicParas.ContainsKey("mobile") ? dicParas["mobile"].ToString() : string.Empty;
            string wechat = dicParas.ContainsKey("wechat") ? dicParas["wechat"].ToString() : string.Empty;
            string qq = dicParas.ContainsKey("qq") ? dicParas["qq"].ToString() : string.Empty;
            string imme = dicParas.ContainsKey("imme") ? dicParas["imme"].ToString() : string.Empty;
            string cardShape = dicParas.ContainsKey("cardShape") ? dicParas["cardShape"].ToString() : string.Empty;
            string memberName = dicParas.ContainsKey("memberName") ? dicParas["memberName"].ToString() : string.Empty;
            string birthday = dicParas.ContainsKey("birthday") ? dicParas["birthday"].ToString() : string.Empty;
            string gender = dicParas.ContainsKey("gender") ? dicParas["gender"].ToString() : string.Empty;
            string identityCard = dicParas.ContainsKey("identityCard") ? dicParas["identityCard"].ToString() : string.Empty;
            string email = dicParas.ContainsKey("email") ? dicParas["email"].ToString() : string.Empty;
            string leftHandCode = dicParas.ContainsKey("leftHandCode") ? dicParas["leftHandCode"].ToString() : string.Empty;
            string rightHandCode = dicParas.ContainsKey("rightHandCode") ? dicParas["rightHandCode"].ToString() : string.Empty;
            string photo = dicParas.ContainsKey("photo") ? dicParas["photo"].ToString() : string.Empty;
            string memberLevelId = dicParas.ContainsKey("memberLevelId") ? dicParas["memberLevelId"].ToString() : string.Empty;     
            string foodId = dicParas.ContainsKey("foodId") ? dicParas["foodId"].ToString() : string.Empty;
            string payCount = dicParas.ContainsKey("payCount") ? dicParas["payCount"].ToString() : string.Empty;
            string realPay = dicParas.ContainsKey("realPay") ? dicParas["realPay"].ToString() : string.Empty;
            string freePay = dicParas.ContainsKey("freePay") ? dicParas["freePay"].ToString() : string.Empty;
            string repeatCode = dicParas.ContainsKey("repeatCode") ? dicParas["repeatCode"].ToString() : string.Empty;     
            string icCardId = dicParas.ContainsKey("icCardId") ? dicParas["icCardId"].ToString() : string.Empty;
            string workStation = dicParas.ContainsKey("workStation") ? dicParas["workStation"].ToString() : string.Empty;
            string note = dicParas.ContainsKey("note") ? dicParas["note"].ToString() : string.Empty;
            string deposit = dicParas.ContainsKey("deposit") ? dicParas["deposit"].ToString() : string.Empty;
            string payType = dicParas.ContainsKey("payType") ? dicParas["payType"].ToString() : string.Empty;
            string saleCoinType = dicParas.ContainsKey("saleCoinType") ? dicParas["saleCoinType"].ToString() : string.Empty;


            if (!checkRegisterParas(dicParas,out errMsg))
            {
                ResponseModel responseModel = new ResponseModel(Return_Code.T, "", Result_Code.F, errMsg);
                return responseModel;
            }

            string storedProcedure = "RegisterMember";
            String[] Ary = new String[] { 
                "数据0", "数据1", "数据2", "数据3", "数据4", 
                "数据5", "数据6", "数据7", "数据8", "数据9",
                "数据10", "数据11", "数据12", "数据13", "数据14",
                "数据15", "数据16", "数据17", "数据18", "数据19",
                "数据20", "数据21", "数据22", "数据23", "数据24",
                "数据25", "数据26", "数据27"
            };
                
            List<object> listParas = new List<object>();
            listParas.Add(storeId);//StoreId
            listParas.Add(mobile);//Mobile
            listParas.Add(wechat);//WeChat
            listParas.Add(qq);//QQ
            listParas.Add(imme);//IMME

            listParas.Add(int.Parse(cardShape));//CardShape
            listParas.Add(memberName);//MemberName
            listParas.Add("888888");//MemberPassword
            listParas.Add(birthday);//Birthday
            listParas.Add(gender);//Gender

            listParas.Add(identityCard);//IdentityCard
            listParas.Add(email);//EMail
            listParas.Add(leftHandCode);//LeftHandCode
            listParas.Add(rightHandCode);//RightHandCode
            listParas.Add(photo);//Photo

            listParas.Add(int.Parse(memberLevelId));//MemberLevelId 
            listParas.Add(int.Parse(foodId));//FoodId
            listParas.Add(decimal.Parse(payCount));//payCount
            listParas.Add(decimal.Parse(realPay));//realPay
            listParas.Add(decimal.Parse(freePay));//freePay

            listParas.Add(int.Parse(repeatCode));//repeatCode 
            listParas.Add(int.Parse(icCardId));//icCardId
            listParas.Add(workStation);//workStation
            listParas.Add(int.Parse(userTokenModel.LogId));//UserId
            listParas.Add(decimal.Parse(deposit));//deposit

            listParas.Add((long)0);//icCardUID
            listParas.Add(int.Parse(payType));//payType
            listParas.Add(int.Parse(saleCoinType));//saleCoinType


            List<SqlDataRecord> listSqlDataRecord = new List<SqlDataRecord>();
            SqlMetaData[] MetaDataArr = new SqlMetaData[] { 
                new SqlMetaData("StoreId", SqlDbType.VarChar,15), 
                new SqlMetaData("Mobile", SqlDbType.VarChar,20),
                new SqlMetaData("WeChat", SqlDbType.VarChar,64),
                new SqlMetaData("QQ", SqlDbType.VarChar,64),
                new SqlMetaData("IMME", SqlDbType.VarChar,64),

                new SqlMetaData("CardShape", SqlDbType.Int),
                new SqlMetaData("MemberName", SqlDbType.VarChar,50),
                new SqlMetaData("MemberPassword", SqlDbType.VarChar,20),
                new SqlMetaData("Birthday", SqlDbType.VarChar,16),
                new SqlMetaData("Gender", SqlDbType.VarChar,1),

                new SqlMetaData("IdentityCard", SqlDbType.VarChar,50),
                new SqlMetaData("EMail", SqlDbType.VarChar,50),
                new SqlMetaData("LeftHandCode", SqlDbType.VarChar,5000),
                new SqlMetaData("RightHandCode", SqlDbType.VarChar,5000),
                new SqlMetaData("Photo", SqlDbType.VarChar,100),

                new SqlMetaData("MemberLevelId", SqlDbType.Int),
                new SqlMetaData("FoodId", SqlDbType.Int),
                new SqlMetaData("PayCount", SqlDbType.Decimal),
                new SqlMetaData("RealPay", SqlDbType.Decimal),
                new SqlMetaData("FreePay", SqlDbType.Decimal),

                new SqlMetaData("RepeatCode", SqlDbType.Int),
                new SqlMetaData("ICCardId", SqlDbType.Int),
                new SqlMetaData("WorkStation", SqlDbType.VarChar,50),
                new SqlMetaData("UserId", SqlDbType.Int),
                new SqlMetaData("Deposit", SqlDbType.Decimal),

                new SqlMetaData("ICCardUID", SqlDbType.BigInt),
                new SqlMetaData("PayType", SqlDbType.Int),
                new SqlMetaData("SaleCoinType", SqlDbType.Int)
            };

            var record = new SqlDataRecord(MetaDataArr);
            for (int i = 0; i < Ary.Length; i++)
            {  
                record.SetValue(i, listParas[i]);    
            }
            listSqlDataRecord.Add(record);
      
            SqlParameter[] sqlParameter = new SqlParameter[4];
            sqlParameter[0] = new SqlParameter("@RegisterMember", SqlDbType.Structured);
            sqlParameter[0].Value = listSqlDataRecord;
            sqlParameter[1] = new SqlParameter("@Note", SqlDbType.Text);
            sqlParameter[1].Value = note;
            sqlParameter[2] = new SqlParameter("@ErrMsg", SqlDbType.VarChar,200);
            sqlParameter[2].Direction = ParameterDirection.Output;
            sqlParameter[3] = new SqlParameter("@Result", SqlDbType.Int);
            sqlParameter[3].Direction = ParameterDirection.Output;
            System.Data.DataSet ds = XCCloudBLL.GetStoredProcedureSentence(storedProcedure, sqlParameter);
            if (sqlParameter[3].Value.ToString() == "1")
            {
                var baseMemberModel = Utils.GetModelList<MemberBaseModel>(ds.Tables[0]).ToList()[0];
                return ResponseModelFactory<MemberBaseModel>.CreateModel(isSignKeyReturn, baseMemberModel);
            }
            else
            {
                return new ResponseModel(Return_Code.T, "", Result_Code.F, sqlParameter[2].Value.ToString());
            }
        }


        /// <summary>
        /// 退出会员
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object exit(Dictionary<string, object> dicParas)
        {
            return null;
        }

        /// <summary>
        /// 验证注册参数
        /// </summary>
        /// <returns></returns>
        private bool checkRegisterParas(Dictionary<string, object> dicParas,out string errMsg)
        {
            errMsg = string.Empty;
            string storeId = dicParas.ContainsKey("storeId") ? dicParas["storeId"].ToString() : string.Empty;
            string mobile = dicParas.ContainsKey("mobile") ? dicParas["mobile"].ToString() : string.Empty;
            string wechat = dicParas.ContainsKey("wechat") ? dicParas["wechat"].ToString() : string.Empty;
            string qq = dicParas.ContainsKey("qq") ? dicParas["qq"].ToString() : string.Empty;
            string imme = dicParas.ContainsKey("imme") ? dicParas["imme"].ToString() : string.Empty;
            string cardShape = dicParas.ContainsKey("cardShape") ? dicParas["cardShape"].ToString() : string.Empty;
            string memberName = dicParas.ContainsKey("memberName") ? dicParas["memberName"].ToString() : string.Empty;
            string birthday = dicParas.ContainsKey("birthday") ? dicParas["birthday"].ToString() : string.Empty;
            string gender = dicParas.ContainsKey("gender") ? dicParas["gender"].ToString() : string.Empty;
            string identityCard = dicParas.ContainsKey("identityCard") ? dicParas["identityCard"].ToString() : string.Empty;
            string email = dicParas.ContainsKey("email") ? dicParas["email"].ToString() : string.Empty;
            string leftHandCode = dicParas.ContainsKey("leftHandCode") ? dicParas["leftHandCode"].ToString() : string.Empty;
            string rightHandCode = dicParas.ContainsKey("rightHandCode") ? dicParas["rightHandCode"].ToString() : string.Empty;
            string photo = dicParas.ContainsKey("photo") ? dicParas["photo"].ToString() : string.Empty;
            string memberLevelId = dicParas.ContainsKey("memberLevelId") ? dicParas["memberLevelId"].ToString() : string.Empty;
            string foodId = dicParas.ContainsKey("foodId") ? dicParas["foodId"].ToString() : string.Empty;
            string payCount = dicParas.ContainsKey("payCount") ? dicParas["payCount"].ToString() : string.Empty;
            string realPay = dicParas.ContainsKey("realPay") ? dicParas["realPay"].ToString() : string.Empty;
            string freePay = dicParas.ContainsKey("freePay") ? dicParas["freePay"].ToString() : string.Empty;
            string repeatCode = dicParas.ContainsKey("repeatCode") ? dicParas["repeatCode"].ToString() : string.Empty;
            string icCardId = dicParas.ContainsKey("icCardId") ? dicParas["icCardId"].ToString() : string.Empty;
            string workStation = dicParas.ContainsKey("workStation") ? dicParas["workStation"].ToString() : string.Empty;
            string note = dicParas.ContainsKey("note") ? dicParas["note"].ToString() : string.Empty;
            string deposit = dicParas.ContainsKey("deposit") ? dicParas["deposit"].ToString() : string.Empty;
            string payType = dicParas.ContainsKey("payType") ? dicParas["payType"].ToString() : string.Empty;
            string saleCoinType = dicParas.ContainsKey("saleCoinType") ? dicParas["saleCoinType"].ToString() : string.Empty;

            if (string.IsNullOrEmpty(mobile))
            {
                errMsg = "会员手机号码不能为空";
                return false;
            }

            if (!Utils.CheckMobile(mobile))
            {
                errMsg = "会员手机号码格式不正确";
                return false;
            }

            if (string.IsNullOrEmpty(memberName))
            {
                errMsg = "会员姓名不能为空";
                return false;
            }

            if (string.IsNullOrEmpty(birthday))
            {
                errMsg = "会员生日不能为空";
                return false;
            }

            if (string.IsNullOrEmpty(birthday))
            {
                errMsg = "会员生日不能为空";
                return false;
            }

            if (string.IsNullOrEmpty(gender))
            {
                errMsg = "会员性别不能为空";
                return false;
            }

            if (!gender.Equals("0") && !gender.Equals("1"))
            {
                errMsg = "会员性别无效";
                return false;
            }

            if (string.IsNullOrEmpty(identityCard))
            {
                errMsg = "证件号码不能为空";
                return false;
            }

            if (string.IsNullOrEmpty(memberLevelId))
            {
                errMsg = "会员级别不能为空";
                return false;
            }

            if (string.IsNullOrEmpty(icCardId))
            {
                errMsg = "IC卡卡号不能为空";
                return false;
            }

            if (string.IsNullOrEmpty(repeatCode))
            {
                errMsg = "IC卡重复码不能为空";
                return false;
            }

            if (!payType.Equals("0") && !payType.Equals("1") && !payType.Equals("2") && !payType.Equals("3"))
            {
                errMsg = "支付类型无效";
                return false;
            }

            if (!saleCoinType.Equals("1") && !saleCoinType.Equals("2") && !saleCoinType.Equals("3") )
            {
                errMsg = "售币类型无效";
                return false;
            }

            if (!Utils.IsNumeric(payCount) || decimal.Parse(payCount) < 0)
            {
                errMsg = "应付金额无效";
                return false;
            }

            if (!Utils.IsNumeric(realPay) || decimal.Parse(realPay) < 0)
            {
                errMsg = "实付金额无效";
                return false;
            }

            if (!Utils.IsNumeric(freePay) || decimal.Parse(freePay) < 0 )
            {
                errMsg = "免费金额无效";
                return false;
            }

            if (decimal.Parse(payCount) - decimal.Parse(freePay) != decimal.Parse(realPay))
            {
                errMsg = "实付金额无效";
                return false;
            }

            //0 普通卡;1 数字手环;2其他
            if (!cardShape.Equals("0") && !cardShape.Equals("1") && !cardShape.Equals("2"))
            {
                errMsg = "卡类型无效";
                return false;
            }

            return true;
        }

        /// <summary>
        /// 获取会员等级
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object getMemberLevel(Dictionary<string, object> dicParas)
        {
            XCCloudUserTokenModel userTokenModel = (XCCloudUserTokenModel)(dicParas[Constant.XCCloudUserTokenModel]);
            TokenDataModel userTokenDataModel = (TokenDataModel)(userTokenModel.DataModel);

            string storedProcedure = "GetMemberLevel";
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@StoreId", userTokenDataModel.StoreID);
            System.Data.DataSet ds = XCCloudBLL.GetStoredProcedureSentence(storedProcedure, parameters);
            List<Data_MemberLevelModel> list = Utils.GetModelList<Data_MemberLevelModel>(ds.Tables[0]);

            return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, list);
        }


        /// <summary>
        /// 获取用户短信码
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object getUserMobileSMSCode(Dictionary<string, object> dicParas)
        {
            string errMsg = string.Empty;
            string mobile = dicParas.ContainsKey("mobile") ? dicParas["mobile"].ToString() : string.Empty;
            if(!Utils.CheckMobile(mobile))
            {
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "手机号无效");
            }

            if (!SMSCodeBusiness.SendSMSCode(mobile, "2", out errMsg))
            {
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, errMsg);
            }

            return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.T, "");
        }



        /// <summary>
        /// 获取会员
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken,SysIdAndVersionNo=false)]
        public object getMemberRepeatCode(Dictionary<string, object> dicParas)
        {
            XCCloudUserTokenModel userTokenModel = (XCCloudUserTokenModel)(dicParas[Constant.XCCloudUserTokenModel]);
            string storeId = dicParas.ContainsKey("storeId") ? dicParas["storeId"].ToString() : string.Empty;
            string icCardId = dicParas.ContainsKey("icCardId") ? dicParas["icCardId"].ToString() : string.Empty;

            if (string.IsNullOrEmpty(storeId))
            {
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "门店号不能为空");
            }

            IBase_StoreInfoService base_StoreInfoService = BLLContainer.Resolve<IBase_StoreInfoService>();            
            if (!base_StoreInfoService.Any(p => p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase)))
            {
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "门店号无效");
            }            

            if (string.IsNullOrEmpty(icCardId))
            {
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "会员卡号无效");
            }
            //获取会员信息
            string storedProcedure = "GetMemberRepeatCode";
            SqlParameter[] parameters = new SqlParameter[5];
            parameters[0] = new SqlParameter("@ICCardID", icCardId);
            parameters[1] = new SqlParameter("@StoreID", storeId);
            parameters[2] = new SqlParameter("@RepeatCode", SqlDbType.Int);
            parameters[2].Direction = System.Data.ParameterDirection.Output;
            parameters[3] = new SqlParameter("@ErrMsg", SqlDbType.VarChar, 200);
            parameters[3].Direction = System.Data.ParameterDirection.Output;
            parameters[4] = new SqlParameter("@Return", SqlDbType.Int);
            parameters[4].Direction = System.Data.ParameterDirection.ReturnValue;
            XCCloudBLL.ExecuteStoredProcedureSentence(storedProcedure, parameters);
            if (parameters[4].Value.ToString() == "1")
            {
                var obj = new {
                    repeatCode = parameters[2].Value.ToString()
                };
                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, obj);
            }
            else
            {
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "会员卡无效");
            }    
        }

        /// <summary>
        /// 获取会员
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object getMember(Dictionary<string, object> dicParas)
        {
            XCCloudUserTokenModel userTokenModel = (XCCloudUserTokenModel)(dicParas[Constant.XCCloudUserTokenModel]);
            TokenDataModel userTokenDataModel = (TokenDataModel)(userTokenModel.DataModel);

            string icCardId = dicParas.ContainsKey("icCardId") ? dicParas["icCardId"].ToString() : string.Empty;

            if (string.IsNullOrEmpty(icCardId))
            {
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "会员卡号无效");
            }

            string storedProcedure = "GetMember";
            SqlParameter[] parameters = new SqlParameter[4];
            parameters[0] = new SqlParameter("@ICCardID", icCardId);
            parameters[1] = new SqlParameter("@StoreID", userTokenDataModel.StoreID);
            parameters[2] = new SqlParameter("@Result",SqlDbType.Int);
            parameters[2].Direction = System.Data.ParameterDirection.Output;
            parameters[3] = new SqlParameter("@ErrMsg", SqlDbType.VarChar,200);
            parameters[3].Direction = System.Data.ParameterDirection.Output;
            System.Data.DataSet ds = XCCloudBLL.GetStoredProcedureSentence(storedProcedure, parameters);
            if (parameters[2].Value.ToString() == "1")
            {
                var memberBaseModel = Utils.GetModelList<MemberBaseModel>(ds.Tables[0]).ToList()[0];
                var memberBalanceModelList = Utils.GetModelList<MemberBalanceModel>(ds.Tables[1]).ToList();
                MemberModel model = new MemberModel(memberBaseModel, memberBalanceModelList);
                return ResponseModelFactory<MemberModel>.CreateModel(isSignKeyReturn, model);
            }
            return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "会员信息不存在");
        }

        [Authorize(Roles = "MerchUser,StoreUser")]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetMemberLevelDic(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

                var memberLevel = Data_MemberLevelService.I.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && p.State == 1).Select(o => new
                {
                    MemberLevelID = o.MemberLevelID,
                    MemberLevelName = o.MemberLevelName
                }).Distinct();
                
                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, memberLevel);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [Authorize(Roles = "MerchUser")]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetMemberDic(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

                var linq = from a in Base_StoreInfoService.N.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && (p.StoreState == (int)StoreState.Open || p.StoreState == (int)StoreState.Valid))
                           join b in Data_Member_Card_StoreService.N.GetModels() on a.StoreID equals b.StoreID
                           join c in Data_Member_CardService.N.GetModels() on (b.CardID + "") equals c.ID
                           join d in Base_MemberInfoService.N.GetModels() on c.MemberID equals d.ID
                           select new
                           {
                               MemberID = d.ID,
                               MemberName = d.UserName
                           };

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, linq);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [Authorize(Roles = "MerchUser")]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryMemberLevel(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string memberLevelID = dicParas.ContainsKey("memberLevelID") ? dicParas["memberLevelID"].ToString() : string.Empty;
                string memberLevelName = dicParas.ContainsKey("memberLevelName") ? dicParas["memberLevelName"].ToString() : string.Empty;
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;


                IData_MemberLevelService data_MemberLevelService = Data_MemberLevelService.I;
                var query = data_MemberLevelService.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && p.State == 1);
                if (!string.IsNullOrEmpty(memberLevelID))
                {
                    var iMemberLevelID = Convert.ToInt32(memberLevelID);
                    query = query.Where(w => w.MemberLevelID == iMemberLevelID);
                }

                if (!string.IsNullOrEmpty(memberLevelName))
                {
                    query = query.Where(w => w.MemberLevelName.Contains(memberLevelName));
                }

                IDict_SystemService dict_SystemService = Dict_SystemService.I;
                int BirthdayFreeId = dict_SystemService.GetModels(p => p.DictKey.Equals("生日送币方式") && p.PID == 0).FirstOrDefault().ID;
                int FreeTypeId = dict_SystemService.GetModels(p => p.DictKey.Equals("送币方式") && p.PID == 0).FirstOrDefault().ID;
                var linq = query.ToList().Select(o => new
                {
                    MemberLevelModel = o,
                    BirthdayFree = dict_SystemService.GetModels(p => p.PID == BirthdayFreeId),
                    FreeType = dict_SystemService.GetModels(p => p.PID == FreeTypeId)
                }).Select(oo => new
                {
                    MemberLevelID = oo.MemberLevelModel.MemberLevelID,
                    MemberLevelName = oo.MemberLevelModel.MemberLevelName,
                    OpenFee = oo.MemberLevelModel.OpenFee,
                    Deposit = oo.MemberLevelModel.Deposit,
                    Validday = oo.MemberLevelModel.Validday,
                    ContinueFee = oo.MemberLevelModel.ContinueFee,
                    UpdateUsePoint = oo.MemberLevelModel.UpdateUsePoint,
                    ConsumeTotle = oo.MemberLevelModel.ConsumeTotle,                   
                    BirthdayFreeStr = oo.BirthdayFree.Any(s => s.DictValue.Equals(oo.MemberLevelModel.BirthdayFree + "")) ? oo.BirthdayFree.Single(s => s.DictValue.Equals(oo.MemberLevelModel.BirthdayFree + "")).DictKey : string.Empty,
                    FreeTypeStr = oo.FreeType.Any(s => s.DictValue.Equals(oo.MemberLevelModel.FreeType + "")) ? oo.FreeType.Single(s => s.DictValue.Equals(oo.MemberLevelModel.FreeType + "")).DictKey : string.Empty
                });

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, linq);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [Authorize(Roles = "MerchUser")]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object SaveMemberLevel(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

                string errMsg = string.Empty;
                string memberLevelID = dicParas.ContainsKey("memberLevelID") ? (dicParas["memberLevelID"] + "") : string.Empty;
                string memberLevelName = dicParas.ContainsKey("memberLevelName") ? (dicParas["memberLevelName"] + "") : string.Empty;
                string coverUrl = dicParas.ContainsKey("coverUrl") ? (dicParas["coverUrl"] + "") : string.Empty;                
                object[] memberLevelFoods = dicParas.ContainsKey("memberLevelFoods") ? (object[])dicParas["memberLevelFoods"] : null;
                object[] memberLevelFrees = dicParas.ContainsKey("memberLevelFrees") ? (object[])dicParas["memberLevelFrees"] : null;
                object[] memberLevelBalanceCharge = dicParas.ContainsKey("memberLevelBalanceCharge") ? (object[])dicParas["memberLevelBalanceCharge"] : null;
                object[] memberLevelBalance = dicParas.ContainsKey("memberLevelBalance") ? (object[])dicParas["memberLevelBalance"] : null;

                if (string.IsNullOrEmpty(memberLevelName))
                {
                    errMsg = "会员级别名称不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (string.IsNullOrEmpty(coverUrl))
                {
                    errMsg = "会员卡封面地址不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                int iMemberLevelID = 0;
                int.TryParse(memberLevelID, out iMemberLevelID);

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        IData_MemberLevelService data_MemberLevelService = Data_MemberLevelService.I;
                        if (data_MemberLevelService.Any(a => a.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) &&
                            a.MemberLevelName.Equals(memberLevelName, StringComparison.OrdinalIgnoreCase) && a.MemberLevelID != iMemberLevelID && a.State == 1))
                        {
                            errMsg = "该会员级别名称已存在";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        var data_MemberLevel = data_MemberLevelService.GetModels(p => p.MemberLevelID == iMemberLevelID).FirstOrDefault() ?? new Data_MemberLevel();
                        Utils.GetModel(dicParas, ref data_MemberLevel);
                        if (data_MemberLevel.MemberLevelID == 0)
                        {
                            //新增
                            data_MemberLevel.MerchID = merchId;
                            data_MemberLevel.State = 1;
                            if (!data_MemberLevelService.Add(data_MemberLevel))
                            {
                                errMsg = "添加会员级别失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }
                        else
                        {
                            //修改
                            if (!data_MemberLevelService.Update(data_MemberLevel))
                            {
                                errMsg = "修改会员级别失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }

                        iMemberLevelID = data_MemberLevel.MemberLevelID;

                        //保存开卡套餐
                        if (memberLevelFoods != null && memberLevelFoods.Count() >= 0)
                        {
                            //先删除，后添加
                            IData_MemberLevel_FoodService data_MemberLevel_FoodService = Data_MemberLevel_FoodService.I;
                            foreach (var model in data_MemberLevel_FoodService.GetModels(p => p.MemberLevelID == iMemberLevelID))
                            {
                                data_MemberLevel_FoodService.DeleteModel(model);
                            }

                            foreach (IDictionary<string, object> el in memberLevelFoods)
                            {
                                if (el != null)
                                {
                                    var dicPara = new Dictionary<string, object>(el, StringComparer.OrdinalIgnoreCase);
                                    string foodId = dicPara.ContainsKey("foodId") ? (dicPara["foodId"] + "") : string.Empty;
                                    if (string.IsNullOrEmpty(foodId))
                                    {
                                        errMsg = "套餐ID不能为空";
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    }

                                    var data_MemberLevel_Food = new Data_MemberLevel_Food();
                                    data_MemberLevel_Food.MemberLevelID = iMemberLevelID;
                                    data_MemberLevel_Food.FoodID = Convert.ToInt32(foodId);
                                    data_MemberLevel_FoodService.AddModel(data_MemberLevel_Food);
                                }
                                else
                                {
                                    errMsg = "提交数据包含空对象";
                                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                }
                            }

                            if (!data_MemberLevel_FoodService.SaveChanges())
                            {
                                errMsg = "保存会员级别开卡套餐信息失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }

                        //保存商品兑换折扣规则
                        if (memberLevelBalance != null && memberLevelBalance.Count() >= 0)
                        {
                            //先删除，后添加
                            foreach (var model in Data_MemberLevel_BalanceService.I.GetModels(p => p.MemberLevelID == iMemberLevelID))
                            {
                                Data_MemberLevel_BalanceService.I.DeleteModel(model);
                            }

                            var memberLevelBalanceList = new List<Data_MemberLevel_Balance>();
                            foreach (IDictionary<string, object> el in memberLevelBalance)
                            {
                                if (el != null)
                                {
                                    var dicPara = new Dictionary<string, object>(el, StringComparer.OrdinalIgnoreCase);
                                    var balanceIndex = dicPara.Get("balanceIndex").Toint();
                                    var chargeOff = dicPara.Get("chargeOff").Toint();
                                    var needAuthor = dicPara.Get("needAuthor").Toint();
                                    var maxSaveCount = dicPara.Get("maxSaveCount").Toint();
                                    var maxUplife = dicPara.Get("maxUplife").Toint();

                                    var data_MemberLevel_Balance = new Data_MemberLevel_Balance();
                                    data_MemberLevel_Balance.MemberLevelID = iMemberLevelID;
                                    data_MemberLevel_Balance.MerchID = merchId;
                                    data_MemberLevel_Balance.BalanceIndex = balanceIndex;
                                    data_MemberLevel_Balance.NeedAuthor = needAuthor;
                                    data_MemberLevel_Balance.ChargeOFF = chargeOff;
                                    data_MemberLevel_Balance.MaxSaveCount = maxSaveCount;
                                    data_MemberLevel_Balance.MaxUplife = maxUplife;
                                    memberLevelBalanceList.Add(data_MemberLevel_Balance);
                                    Data_MemberLevel_BalanceService.I.AddModel(data_MemberLevel_Balance);
                                }
                                else
                                {
                                    errMsg = "提交数据包含空对象";
                                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                }
                            }

                            //同一类型的兑换折扣规则必须唯一 
                            if (memberLevelBalanceList.GroupBy(g => g.BalanceIndex).Select(o => new { Count = o.Count() }).Any(p => p.Count > 1))
                            {
                                errMsg = "同一类型的兑换折扣规则必须唯一";
                                return false;
                            }

                            if (!Data_MemberLevel_BalanceService.I.SaveChanges())
                            {
                                errMsg = "保存商品兑换折扣规则信息失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }

                        //保存预赠币规则
                        if (memberLevelFrees != null && memberLevelFrees.Count() >= 0)
                        {
                            //先删除，后添加
                            IData_MemberLevelFreeService data_MemberLevelFreeService = Data_MemberLevelFreeService.I;
                            foreach (var model in data_MemberLevelFreeService.GetModels(p => p.MemberLevelID == iMemberLevelID))
                            {
                                data_MemberLevelFreeService.DeleteModel(model);
                            }

                            foreach (IDictionary<string, object> el in memberLevelFrees)
                            {
                                if (el != null)
                                {
                                    var dicPara = new Dictionary<string, object>(el, StringComparer.OrdinalIgnoreCase);
                                    string chargeTotal = dicPara.ContainsKey("chargeTotal") ? (dicPara["chargeTotal"] + "") : string.Empty;
                                    string freeBalanceType = dicPara.ContainsKey("freeBalanceType") ? (dicPara["freeBalanceType"] + "") : string.Empty;
                                    string freeCount = dicPara.ContainsKey("freeCount") ? (dicPara["freeCount"] + "") : string.Empty;
                                    string minSpaceDays = dicPara.ContainsKey("minSpaceDays") ? (dicPara["minSpaceDays"] + "") : string.Empty;
                                    string onceFreeCount = dicPara.ContainsKey("onceFreeCount") ? (dicPara["onceFreeCount"] + "") : string.Empty;
                                    string expireDays = dicPara.ContainsKey("expireDays") ? (dicPara["expireDays"] + "") : string.Empty;
                                    
                                    var data_MemberLevelFree = new Data_MemberLevelFree();
                                    data_MemberLevelFree.MemberLevelID = iMemberLevelID;
                                    data_MemberLevelFree.MerchID = merchId;
                                    data_MemberLevelFree.ChargeTotal = ObjectExt.Todecimal(chargeTotal);
                                    data_MemberLevelFree.FreeBalanceType = ObjectExt.Toint(freeBalanceType);
                                    data_MemberLevelFree.FreeCount = ObjectExt.Toint(freeCount);
                                    data_MemberLevelFree.MinSpaceDays = ObjectExt.Toint(minSpaceDays);
                                    data_MemberLevelFree.OnceFreeCount = ObjectExt.Toint(onceFreeCount);
                                    data_MemberLevelFree.ExpireDays = ObjectExt.Toint(expireDays);
                                    data_MemberLevelFreeService.AddModel(data_MemberLevelFree);
                                }
                                else
                                {
                                    errMsg = "提交数据包含空对象";
                                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                }
                            }

                            if (!data_MemberLevelFreeService.SaveChanges())
                            {
                                errMsg = "保存会员级别预赠币规则信息失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }

                        //保存会员余额互换规则
                        if (memberLevelBalanceCharge != null && memberLevelBalanceCharge.Count() >= 0)
                        {
                            //先删除，后添加
                            foreach (var model in Data_MemberLevel_BalanceChargeService.I.GetModels(p => p.MemberLevelID == iMemberLevelID))
                            {
                                Data_MemberLevel_BalanceChargeService.I.DeleteModel(model);
                            }

                            var chargeRuleList = new List<Data_MemberLevel_BalanceCharge>();
                            foreach (IDictionary<string, object> el in memberLevelBalanceCharge)
                            {
                                if (el != null)
                                {
                                    var dicPara = new Dictionary<string, object>(el, StringComparer.OrdinalIgnoreCase);
                                    if (!dicPara.Get("sourceBalanceIndex").Validint("原类型", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPara.Get("sourceCount").Validintnozero("原数量", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPara.Get("targetBalanceIndex").Validint("兑换类型", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPara.Get("targetCount").Validintnozero("兑换数量", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (dicPara.Get("sourceBalanceIndex").Toint() == dicPara.Get("targetBalanceIndex").Toint())
                                    {
                                        errMsg = "原类型不能与兑换类型相同";
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    }

                                    var sourceBalanceIndex = dicPara.Get("sourceBalanceIndex").Toint();
                                    var sourceCount = dicPara.Get("sourceCount").Toint();
                                    var targetBalanceIndex = dicPara.Get("targetBalanceIndex").Toint();
                                    var targetCount = dicPara.Get("targetCount").Toint();

                                    var data_MemberLevel_BalanceCharge = new Data_MemberLevel_BalanceCharge();
                                    data_MemberLevel_BalanceCharge.MemberLevelID = iMemberLevelID;
                                    data_MemberLevel_BalanceCharge.MerchID = merchId;
                                    data_MemberLevel_BalanceCharge.SourceBalanceIndex = sourceBalanceIndex;
                                    data_MemberLevel_BalanceCharge.SourceCount = sourceCount;
                                    data_MemberLevel_BalanceCharge.TargetBalanceIndex = targetBalanceIndex;
                                    data_MemberLevel_BalanceCharge.TargetCount = targetCount;
                                    chargeRuleList.Add(data_MemberLevel_BalanceCharge);
                                    Data_MemberLevel_BalanceChargeService.I.AddModel(data_MemberLevel_BalanceCharge);                                    
                                }
                                else
                                {
                                    errMsg = "提交数据包含空对象";
                                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                }
                            }

                            //同一类型的兑换规则必须唯一 
                            if (chargeRuleList.GroupBy(g => new { g.SourceBalanceIndex, g.TargetBalanceIndex }).Select(o => new { Count = o.Count() }).Any(p => p.Count > 1))
                            {
                                errMsg = "同一类型的兑换规则必须唯一";
                                return false;
                            }

                            if (!Data_MemberLevel_BalanceChargeService.I.SaveChanges())
                            {
                                errMsg = "保存会员余额兑换规则信息失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }                        

                        ts.Complete();
                    }
                    catch (DbEntityValidationException e)
                    {
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, e.EntityValidationErrors.ToErrors());
                    }
                    catch (Exception ex)
                    {
                        errMsg = ex.Message;
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                }
                
                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [Authorize(Roles = "MerchUser")]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetMemberLevelInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string memberLevelID = dicParas.ContainsKey("memberLevelID") ? dicParas["memberLevelID"].ToString() : string.Empty;

                if (string.IsNullOrEmpty(memberLevelID))
                {
                    errMsg = "会员级别ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                int iMemberLevelID = Convert.ToInt32(memberLevelID);
                IData_MemberLevelService data_MemberLevelService = Data_MemberLevelService.I;
                if (!data_MemberLevelService.Any(a => a.MemberLevelID == iMemberLevelID))
                {
                    errMsg = "该会员级别不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var data_MemberLevelModel = data_MemberLevelService.GetModels(p => p.MemberLevelID == iMemberLevelID).FirstOrDefault();

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, data_MemberLevelModel);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 获取绑定开卡套餐
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [Authorize(Roles = "MerchUser")]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetMemberLevelFoodList(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string memberLevelID = dicParas.ContainsKey("memberLevelID") ? dicParas["memberLevelID"].ToString() : string.Empty;

                if (string.IsNullOrEmpty(memberLevelID))
                {
                    errMsg = "会员级别ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                int iMemberLevelID = Convert.ToInt32(memberLevelID);              
                var linq = from a in Data_MemberLevel_FoodService.N.GetModels(p => p.MemberLevelID == iMemberLevelID)
                           join b in Data_FoodInfoService.N.GetModels() on a.FoodID equals b.FoodID
                           select new { 
                               FoodID = b.FoodID,
                               FoodName = b.FoodName
                           };

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, linq);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 获取预赠币规则
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [Authorize(Roles = "MerchUser")]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetMemberLevelFreeList(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string memberLevelID = dicParas.ContainsKey("memberLevelID") ? dicParas["memberLevelID"].ToString() : string.Empty;

                if (string.IsNullOrEmpty(memberLevelID))
                {
                    errMsg = "会员级别ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                int iMemberLevelID = Convert.ToInt32(memberLevelID);
                var linq = Data_MemberLevelFreeService.I.GetModels(p => p.MemberLevelID == iMemberLevelID);

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, linq);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 获取兑换币规则
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [Authorize(Roles = "MerchUser")]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetMemberLevelBalanceCharge(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string memberLevelID = dicParas.ContainsKey("memberLevelID") ? dicParas["memberLevelID"].ToString() : string.Empty;

                if (string.IsNullOrEmpty(memberLevelID))
                {
                    errMsg = "会员级别ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                int iMemberLevelID = Convert.ToInt32(memberLevelID);
                var linq = from a in Data_MemberLevel_BalanceChargeService.N.GetModels(p => p.MemberLevelID == iMemberLevelID)
                           join b in Dict_BalanceTypeService.N.GetModels(p => p.State == 1) on a.SourceBalanceIndex equals b.ID into b1
                           from b in b1.DefaultIfEmpty()
                           join c in Dict_BalanceTypeService.N.GetModels(p => p.State == 1) on a.TargetBalanceIndex equals c.ID into c1
                           from c in c1.DefaultIfEmpty()
                           select new
                           {
                               SourceBalanceIndex = a.SourceBalanceIndex,
                               SourceBalanceStr = b != null ? b.TypeName : string.Empty,
                               SourceCount = a.SourceCount,
                               TargetBalanceIndex = a.TargetBalanceIndex,
                               TargetBalanceStr = c != null ? c.TypeName : string.Empty,
                               TargetCount = a.TargetCount
                           };

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, linq);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        /// <summary>
        /// 获取兑换折扣规则
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [Authorize(Roles = "MerchUser")]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetMemberLevelBalance(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string memberLevelID = dicParas.ContainsKey("memberLevelID") ? dicParas["memberLevelID"].ToString() : string.Empty;

                if (string.IsNullOrEmpty(memberLevelID))
                {
                    errMsg = "会员级别ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                int iMemberLevelID = Convert.ToInt32(memberLevelID);
                var linq = from a in Data_MemberLevel_BalanceService.N.GetModels(p => p.MemberLevelID == iMemberLevelID)
                           join b in Dict_BalanceTypeService.N.GetModels(p => p.State == 1) on a.BalanceIndex equals b.ID into b1
                           from b in b1.DefaultIfEmpty()                           
                           select new
                           {
                               BalanceIndex = a.BalanceIndex,
                               BalanceStr = b != null ? b.TypeName : string.Empty,
                               ChargeOFF = a.ChargeOFF,
                               NeedAuthor = a.NeedAuthor,
                               MaxSaveCount = a.MaxSaveCount,
                               MaxUplife = a.MaxUplife
                           };

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, linq);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [Authorize(Roles = "MerchUser")]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object UploadCardCover(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                Dictionary<string, string> imageInfo = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                List<string> imageUrls = null;
                if (!Utils.UploadImageFile("/XCCloud/MemberLevel/", out imageUrls, out errMsg))
                {
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                imageInfo.Add("CardUIURL", imageUrls.FirstOrDefault());

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, imageInfo);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [Authorize(Roles = "MerchUser")]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object DelMemberLevelInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string memberLevelID = dicParas.ContainsKey("memberLevelID") ? dicParas["memberLevelID"].ToString() : string.Empty;

                if (string.IsNullOrEmpty(memberLevelID))
                {
                    errMsg = "会员级别ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                int iMemberLevelID = Convert.ToInt32(memberLevelID);                

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        IData_MemberLevelService data_MemberLevelService = Data_MemberLevelService.I;
                        if (!data_MemberLevelService.Any(a => a.MemberLevelID == iMemberLevelID))
                        {
                            errMsg = "该会员级别不存在";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        var data_MemberLevel = data_MemberLevelService.GetModels(p => p.MemberLevelID == iMemberLevelID).FirstOrDefault();
                        data_MemberLevel.State = 0;
                        if (!data_MemberLevelService.Update(data_MemberLevel))
                        {
                            errMsg = "删除会员级别失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        //IData_MemberLevel_FoodService data_MemberLevel_FoodService = Data_MemberLevel_FoodService.I;
                        //foreach (var model in data_MemberLevel_FoodService.GetModels(p => p.MemberLevelID == iMemberLevelID))
                        //{
                        //    data_MemberLevel_FoodService.DeleteModel(model);
                        //}

                        //if (!data_MemberLevel_FoodService.SaveChanges())
                        //{
                        //    errMsg = "删除会员级别开卡套餐信息失败";
                        //    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        //}

                        //IData_MemberLevelFreeService data_MemberLevelFreeService = Data_MemberLevelFreeService.I;
                        //foreach (var model in data_MemberLevelFreeService.GetModels(p => p.MemberLevelID == iMemberLevelID))
                        //{
                        //    data_MemberLevelFreeService.DeleteModel(model);
                        //}

                        //if (!data_MemberLevelFreeService.SaveChanges())
                        //{
                        //    errMsg = "删除会员级别预赠币规则信息失败";
                        //    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        //}

                        ts.Complete();
                    }
                    catch (Exception ex)
                    {
                        errMsg = ex.Message;
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                }

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        #region 查询会员卡信息

        #region 按手机号码查询会员主卡列表
        /// <summary>
        /// 按手机号码查询会员主卡列表
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [Authorize(Roles = "StoreUser")]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object getCardListByPhone(Dictionary<string, object> dicParas)
        {
            try
            {
                string mobile = dicParas.ContainsKey("mobile") ? dicParas["mobile"].ToString() : string.Empty;
                string code = dicParas.ContainsKey("code") ? dicParas["code"].ToString().Trim() : "";
                if (string.IsNullOrEmpty(mobile))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "手机号码无效");
                }

                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                string merchID = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

                if (CommonConfig.IsVerifyCode)
                {
                    //验证码校验
                    string TempCode = RedisCacheHelper.StringGet(CommonConfig.PrefixKey + mobile);
                    if (!TempCode.Equals(code, StringComparison.OrdinalIgnoreCase))
                    {
                        return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "验证码错误");
                    }
                }

                var query = from a in Base_MemberInfoService.N.GetModels(t => t.Mobile == mobile)
                            join b in Data_Member_CardService.N.GetModels(t => t.MerchID == merchID && t.CardType == 0 && t.CardStatus == 1) on a.ID equals b.MemberID
                            join c in Data_Member_Card_StoreService.N.GetModels(t=>t.StoreID == storeId) on b.ID equals c.CardID
                            select new
                            {
                                CardId = b.ID,
                                ICCardNo = b.ICCardID,
                                LevelId = b.MemberLevelID,
                                EndDate = b.EndDate
                            };

                var list = query.ToList().Select(t => new
                {
                    CardId = t.CardId,
                    ICCardNo = t.ICCardNo,
                    LevelName = Data_MemberLevelService.I.GetModels(m => m.MemberLevelID == t.LevelId).FirstOrDefault().MemberLevelName,
                    EndDate = t.EndDate.Value.ToString("yyyy-MM-dd")
                }).ToList();
                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, list);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        private string GetLockState(int islock)
        {
            return "";
        }
        #endregion

        #region 按身份证号查询会员主卡列表
        /// <summary>
        /// 按身份证号查询会员主卡列表
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [Authorize(Roles = "StoreUser")]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object getCardListByIDnumber(Dictionary<string, object> dicParas)
        {
            try
            {
                string idCardNo = dicParas.ContainsKey("idCardNo") ? dicParas["idCardNo"].ToString() : string.Empty;
                if (string.IsNullOrEmpty(idCardNo))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "身份证号码无效");
                }

                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                string merchID = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

                var query = from a in Base_MemberInfoService.N.GetModels(t => t.IDCard == idCardNo)
                            join b in Data_Member_CardService.N.GetModels(t => t.MerchID == merchID && t.CardType == 0 && t.CardStatus == 1) on a.ID equals b.MemberID
                            join c in Data_Member_Card_StoreService.N.GetModels(t => t.StoreID == storeId) on b.ID equals c.CardID
                            select new
                            {
                                CardId = b.ID,
                                ICCardNo = b.ICCardID,
                                LevelId = b.MemberLevelID,
                                EndDate = b.EndDate
                            };

                var list = query.ToList().Select(t => new
                {
                    CardId = t.CardId,
                    ICCardNo = t.ICCardNo,
                    LevelName = Data_MemberLevelService.I.GetModels(m => m.MemberLevelID == t.LevelId).FirstOrDefault().MemberLevelName,
                    EndDate = t.EndDate.Value.ToString("yyyy-MM-dd")
                }).ToList();
                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, list);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }
        #endregion

        #region 按会员卡查询会员及卡信息
        /// <summary>
        /// 按会员卡查询会员及卡信息
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [Authorize(Roles = "StoreUser")]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object getMenberCardInfoByICCardId(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string icCardId = dicParas.ContainsKey("icCardId") ? dicParas["icCardId"].ToString() : string.Empty;
                if (string.IsNullOrEmpty(icCardId))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "会员卡无效");
                }

                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                string merchID = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

                //判断会员卡是否存在或是否存在多张相同卡号的会员卡
                if (!MemberBusiness.ExistsCardByICCardId(icCardId, merchID, storeId, out errMsg))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, errMsg);
                }

                //查询卡实体信息
                Data_Member_Card card = Data_Member_CardService.I.GetModels(t => t.ICCardID == icCardId).FirstOrDefault();
                if (card == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "会员卡不存在");
                }

                //查询会员实体
                Base_MemberInfo member = Base_MemberInfoService.I.GetModels(t => t.ID == card.MemberID).FirstOrDefault();
                if (member == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "会员信息无效");
                }

                MemberCardInfoViewModel model = new MemberCardInfoViewModel();
                model.CardId = card.ID;
                model.ICCardId = card.ICCardID;
                //会员信息
                model.MemberInfo = new CardMemberInfoModel()
                {
                    UserName = member.UserName ?? "",
                    Birthday = member.Birthday.HasValue ? member.Birthday.Value.ToString("yyyy-MM-dd") : "",
                    IDCardNo = member.IDCard ?? "",
                    Mobile = member.Mobile ?? "",
                    Gender = member.Gender.HasValue ? member.Gender.Value == 0 ? "男" : "女" : "",
                    Avatar = member.Photo ?? ""
                };

                //余额
                List<MemberBalanceExchangeRateModel> memberBalance = XCCloudService.Business.XCCloud.MemberBusiness.GetMemberBalanceAndExchangeRate(storeId, card.ICCardID);
                if (memberBalance != null && memberBalance.Count > 0)
                {
                    model.MemberBalances = memberBalance.Select(t => new BalanceModel
                    {
                        BalanceIndex = t.BalanceIndex,
                        BalanceName = t.TypeName,
                        Quantity = t.Total
                    }).ToList();
                }

                //卡权限
                CardPurviewModel purview = new CardPurviewModel();
                purview.AllowIn = card.AllowIn.Value;
                purview.AllowOut = card.AllowOut.Value;
                purview.CardStatus = card.CardStatus.Value;

                var cardRight = from a in Data_Card_RightService.N.GetModels(t => t.CardID == card.ID)
                                  join b in Data_Card_Right_StoreListService.N.GetModels(t => t.StoreID == storeId) on a.ID equals b.CardRightID
                                  select new
                                  {
                                      AllowExitCoin = a.AllowExitCoin,
                                      AllowSaleCoin = a.AllowSaleCoin,
                                      AllowSaveCoin = a.AllowSaveCoin,
                                      AllowFreeCoin = a.AllowFreeCoin
                                  };

                var cardPurview = cardRight.ToList().FirstOrDefault();

                if (cardPurview != null)
                {
                    purview.AllowExitCoin = cardPurview.AllowExitCoin.Value;
                    purview.AllowSaleCoin = cardPurview.AllowSaleCoin.Value;
                    purview.AllowSaveCoin = cardPurview.AllowSaveCoin.Value;
                    purview.AllowFreeCoin = cardPurview.AllowFreeCoin.Value;
                }
                model.CardPurview = purview;

                //附属卡
                model.ChildCardList = Data_Member_CardService.I.GetModels(t => t.ParentCard == card.ID && t.CardType == 1).Select(t => new
                {
                    ChildCardId = t.ID,
                    ChildICCardId = t.ICCardID,
                    CardShape = t.CardShape,
                    Deposit = t.Deposit,
                    //EndDate = t.EndDate,
                    UserName = t.CardName,
                    Birthday = t.CardBirthDay,
                    Gender = t.CardSex
                }).ToList().Select(t => new ChildCardModel()
                {
                    ChildCardId = t.ChildCardId,
                    ChildICCardId = t.ChildICCardId,
                    CardShape = "普通卡",
                    Deposit = t.Deposit.Value.ToString(),
                    //EndDate = t.EndDate.Value.ToString("yyyy-MM-dd"),
                    ChildCardMemberInfo = new ChildCardMemberInfoModel()
                    {
                        UserName = t.UserName,
                        Birthday = t.Birthday.HasValue ? t.Birthday.Value.ToString("yyyy-MM-dd") : "",
                        Gender = t.Gender.HasValue ? t.Gender.Value == 0 ? "男" : "女" : "",
                        Avatar = ""
                    }
                }).ToList();

                return ResponseModelFactory<MemberCardInfoViewModel>.CreateModel(isSignKeyReturn, model);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }
        #endregion 

        #endregion

        #region 退款/退卡

        #region 获取会员余额类别及兑换比例
        /// <summary>
        /// 获取会员余额类别及兑换比例
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [Authorize(Roles = "StoreUser")]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetMemberBalanceAndExchangeRate(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string ICCardId = dicParas.ContainsKey("iccardId") ? dicParas["iccardId"].ToString() : string.Empty;
                if (string.IsNullOrEmpty(ICCardId))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "会员卡号无效");
                }

                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchID = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

                //判断会员卡是否存在或是否存在多张相同卡号的会员卡
                if(!MemberBusiness.ExistsCardByICCardId(ICCardId, merchID, storeId, out errMsg))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, errMsg);
                }

                List<MemberBalanceExchangeRateModel> memberBalance = XCCloudService.Business.XCCloud.MemberBusiness.GetMemberBalanceAndExchangeRate(storeId, ICCardId);
                if (memberBalance.Count > 0)
                {
                    return ResponseModelFactory<List<MemberBalanceExchangeRateModel>>.CreateModel(isSignKeyReturn, memberBalance);
                }
                else
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "无数据");
                }
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        } 
        #endregion

        #region 验证吧台退款信息
        /// <summary>
        /// 验证吧台退款信息
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [Authorize(Roles = "StoreUser")]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object BackCard(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string ICCardId = dicParas.ContainsKey("iccardId") ? dicParas["iccardId"].ToString().Trim() : string.Empty;
                //退款方式： 1 仅退款 2 退款又退卡
                string backType = dicParas.ContainsKey("backType") ? dicParas["backType"].ToString().Trim() : string.Empty;
                if (string.IsNullOrEmpty(backType))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "退款方式无效");
                }

                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchID = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;

                //判断会员卡是否存在或是否存在多张相同卡号的会员卡
                if (!MemberBusiness.ExistsCardByICCardId(ICCardId, merchID, storeId, out errMsg))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, errMsg);
                }

                Data_Member_Card memberCard = Data_Member_CardService.I.GetModels(t => t.ICCardID == ICCardId).FirstOrDefault();
                if (string.IsNullOrEmpty(ICCardId) || memberCard == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "会员卡号无效");
                }

                if (backType == "1" && memberCard.ParentCard != "0")
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "附属卡不能退款");
                }

                //获取会员余额及兑换规则比列
                List<MemberBalanceExchangeRateModel> memberBalanceExchange = XCCloudService.Business.XCCloud.MemberBusiness.GetMemberBalanceAndExchangeRate(storeId, ICCardId);
                if (memberBalanceExchange.Count == 0)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "该会员无可兑换余额");
                }

                //可退款集合
                memberBalanceExchange = memberBalanceExchange.Where(t => t.ExchangeRate != 0).ToList();

                //退款结果集合
                List<BalanceExchangeDataModel> backList = new List<BalanceExchangeDataModel>();
                //退卡结果集合
                List<CardDepositDataModel> cardDepositList = new List<CardDepositDataModel>();
                //仅退款
                if (backType == "1")
                {
                    string exchangeData = dicParas.ContainsKey("exchangeData") ? dicParas["exchangeData"].ToString() : string.Empty;
                    if (string.IsNullOrEmpty(exchangeData))
                    {
                        return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "兑换数据无效");
                    }

                    List<BalanceExchangeDataModel> ExchangeList = JsonConvert.DeserializeObject<List<BalanceExchangeDataModel>>(exchangeData);

                    foreach (var item in ExchangeList)
                    {
                        MemberBalanceExchangeRateModel memberBalance = memberBalanceExchange.FirstOrDefault(t => t.BalanceIndex == item.BalanceIndex);
                        if (memberBalance != null)
                        {
                            if (item.ExchangeQty > memberBalance.Balance)
                            {
                                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "兑换数量超出可兑数量");
                            }

                            //计算小数位，0位小数：按exchangeVal*1计算后再舍弃， 1位小数： 按exchangeVal*10计算后再舍弃， 2位小数： 按exchangeVal*100计算后再舍弃，以此类推
                            //int decimalNumber = Convert.ToInt32("1".PadRight(memberBalance.DecimalNumber + 1, '0'));
                            decimal decimalNumber = Convert.ToDecimal(Math.Pow(10, memberBalance.DecimalNumber));

                            //兑换价值 = 兑换数量 * 兑换比例
                            decimal exchangeVal = item.ExchangeQty * memberBalance.ExchangeRate;
                            //按小数位升位
                            exchangeVal = exchangeVal * decimalNumber;

                            //小数舍弃方式：0 全部舍弃 只取整数部分，1 全部保留 有任何小数都进位，2 四舍五入
                            switch (memberBalance.AddingType)
                            {
                                case 0:
                                    exchangeVal = Math.Floor(exchangeVal); break;
                                case 1:
                                    exchangeVal = Math.Ceiling(exchangeVal); break;
                                case 2:
                                    exchangeVal = Math.Round(exchangeVal, MidpointRounding.AwayFromZero); break;
                            }

                            //按小数位降位
                            exchangeVal = exchangeVal / decimalNumber;

                            //判断兑换价值与前端传过来的是否相等
                            if (Convert.ToDecimal(item.ExchangeValue) != exchangeVal)
                            {
                                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "兑换价值错误");
                            }

                            BalanceExchangeDataModel model = new BalanceExchangeDataModel();
                            model.BalanceIndex = item.BalanceIndex;
                            model.TypeName = memberBalance.TypeName;
                            model.ExchangeQty = item.ExchangeQty;
                            model.ExchangeValue = exchangeVal;
                            backList.Add(model);
                        }
                    }
                }
                else if (backType == "2") //退款又退卡
                {
                    //当前为主卡时计算退款
                    if (memberCard.ParentCard == "0")
                    {
                        foreach (var item in memberBalanceExchange)
                        {
                            //计算小数位，0位小数：按exchangeVal*1计算后再舍弃， 1位小数： 按exchangeVal*10计算后再舍弃， 2位小数： 按exchangeVal*100计算后再舍弃，以此类推
                            decimal decimalNumber = Convert.ToDecimal(Math.Pow(10, item.DecimalNumber));

                            //退款又退卡，余额全部退掉
                            //兑换价值 = 兑换数量 * 兑换比例
                            decimal exchangeVal = item.Balance * item.ExchangeRate;
                            //按小数位升位
                            exchangeVal = exchangeVal * decimalNumber;

                            //小数舍弃方式：0 全部舍弃 只取整数部分，1 全部保留 有任何小数都进位，2 四舍五入
                            switch (item.AddingType)
                            {
                                case 0:
                                    exchangeVal = Math.Floor(exchangeVal); break;
                                case 1:
                                    exchangeVal = Math.Ceiling(exchangeVal); break;
                                case 2:
                                    exchangeVal = Math.Round(exchangeVal, MidpointRounding.AwayFromZero); break;
                            }

                            //按小数位降位
                            exchangeVal = exchangeVal / decimalNumber;

                            BalanceExchangeDataModel model = new BalanceExchangeDataModel();
                            model.BalanceIndex = item.BalanceIndex;
                            model.TypeName = item.TypeName;
                            model.ExchangeQty = item.Balance;
                            model.ExchangeValue = exchangeVal;
                            backList.Add(model);
                        }

                        //计算所有附属卡及当前主卡
                        List<Data_Member_Card> cardList = Data_Member_CardService.I.GetModels(t => t.ParentCard + "" == memberCard.ID || t.ID == memberCard.ID).ToList();
                        foreach (var item in cardList)
                        {
                            CardDepositDataModel cardModel = new CardDepositDataModel();
                            cardModel.CardId = item.ID;
                            cardModel.ICCardID = item.ICCardID;
                            cardModel.CardType = item.ParentCard == "0" ? 0 : 1;
                            cardModel.Deposit = item.Deposit.Value;
                            cardDepositList.Add(cardModel);
                        }
                    }
                    //当前为附属卡时，只能退附属卡押金
                    else
                    {
                        CardDepositDataModel cardModel = new CardDepositDataModel();
                        cardModel.CardId = memberCard.ID;
                        cardModel.ICCardID = memberCard.ICCardID;
                        cardModel.CardType = 1;
                        cardModel.Deposit = memberCard.Deposit.Value;
                        cardDepositList.Add(cardModel);
                    }
                }

                BackDataModel backModel = new BackDataModel();
                backModel.BackTotal = backList.Sum(t => t.ExchangeValue) + cardDepositList.Sum(t => t.Deposit);
                backModel.BalanceExchanges = backList;
                backModel.CardDeposits = cardDepositList;

                return ResponseModelFactory<BackDataModel>.CreateModel(isSignKeyReturn, backModel);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        } 
        #endregion

        #region 确认退款/退卡
        /// <summary>
        /// 确认退款/退卡
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [Authorize(Roles = "StoreUser")]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object ConfirmBackCard(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string ICCardId = dicParas.ContainsKey("ICCardId") ? dicParas["ICCardId"].ToString().Trim() : string.Empty;
                //退款方式： 1 仅退款 2 退款又退卡
                string backType = dicParas.ContainsKey("backType") ? dicParas["backType"].ToString().Trim() : string.Empty;
                if (string.IsNullOrEmpty(backType))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "退款方式无效");
                }

                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                string merchID = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string workStation = (userTokenKeyModel.DataModel as TokenDataModel).WorkStation;
                int userId = userTokenKeyModel.LogId.Toint(0);

                //判断会员卡是否存在或是否存在多张相同卡号的会员卡
                if (!MemberBusiness.ExistsCardByICCardId(ICCardId, merchID, storeId, out errMsg))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, errMsg);
                }

                Data_Member_Card memberCard = Data_Member_CardService.I.GetModels(t => t.ICCardID == ICCardId).FirstOrDefault();
                if (string.IsNullOrEmpty(ICCardId) || memberCard == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "会员卡号无效");
                }

                if (backType == "1" && memberCard.ParentCard != "0")
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "附属卡不能退款");
                }

                //获取会员余额及兑换规则比列
                List<MemberBalanceExchangeRateModel> memberBalanceExchange = XCCloudService.Business.XCCloud.MemberBusiness.GetMemberBalanceAndExchangeRate(storeId, ICCardId);

                //可退款集合
                //memberBalanceExchange = memberBalanceExchange.Where(t => t.ExchangeRate != 0).ToList();

                //退款集合
                List<BalanceExchangeDataModel> backList = new List<BalanceExchangeDataModel>();
                //退卡集合
                List<CardDepositDataModel> cardDepositList = new List<CardDepositDataModel>();

                string balanceExchanges = dicParas.ContainsKey("balanceExchanges") ? dicParas["balanceExchanges"].ToString().Trim() : string.Empty;
                if (!string.IsNullOrEmpty(balanceExchanges))
                {
                    backList = JsonConvert.DeserializeObject<List<BalanceExchangeDataModel>>(balanceExchanges);
                }

                string cardDeposits = dicParas.ContainsKey("cardDeposits") ? dicParas["cardDeposits"].ToString().Trim() : string.Empty;
                if (!string.IsNullOrEmpty(cardDeposits))
                {
                    cardDepositList = JsonConvert.DeserializeObject<List<CardDepositDataModel>>(cardDeposits);
                }

                using (TransactionScope ts = new TransactionScope(TransactionScopeOption.RequiresNew))
                {
                    //当前班次
                    Flw_Schedule schedule = Flw_ScheduleService.I.GetModels(t => t.StoreID == storeId && t.State == 1).FirstOrDefault();
                    if (schedule == null)
                    {
                        return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "当前班次为空，不能进行退款操作");
                    }

                    //主卡退款退卡流水号
                    string backSerialNo = RedisCacheHelper.CreateStoreSerialNo(storeId);

                    //储值金退款总和
                    decimal backTotal = 0m;
                    //退款明细说明
                    string strCardExitNote = string.Empty;

                    //退卡
                    if (cardDepositList.Count > 0)
                    {
                        foreach (var item in cardDepositList)
                        {
                            Data_Member_Card card = Data_Member_CardService.I.GetModels(t => t.ID == item.CardId).FirstOrDefault();
                            card.CardStatus = 0;
                            //修改卡状态为不可用
                            if (!Data_Member_CardService.I.Update(card, false))
                            {
                                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "退卡失败");
                            }
                            //退附属卡
                            if (card.ParentCard != "0")
                            {
                                //写入附属卡退卡记录
                                Flw_MemberCard_Exit mce = new Flw_MemberCard_Exit();
                                mce.OrderID = RedisCacheHelper.CreateStoreSerialNo(storeId);
                                mce.MerchID = merchID;
                                mce.StoreID = storeId;
                                mce.MemberID = card.MemberID;
                                mce.OperateType = 1;
                                mce.CardID = card.ICCardID;
                                mce.Deposit = card.Deposit;
                                mce.ExitMoney = 0;
                                mce.WorkStation = workStation;
                                mce.UserID = userId;
                                mce.ScheduldID = schedule.ID.ToString();
                                mce.CheckDate = schedule.CheckDate;
                                mce.Note = "附属卡退卡";

                                strCardExitNote += string.Format("附属卡号：{0}，卡押金：{1}；" + Environment.NewLine, card.ICCardID, card.Deposit);
                                if (!Flw_MemberCard_ExitService.I.Add(mce, false))
                                {
                                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "附属卡退卡失败");
                                }
                            }
                        }
                    }
                    //退款
                    if (backList.Count > 0)
                    {
                        foreach (var item in backList)
                        {
                            //源余额种类实体
                            Data_Card_Balance cardBalance = Data_Card_BalanceService.I.GetModels(b => b.BalanceIndex == item.BalanceIndex && b.CardIndex == memberCard.ID).FirstOrDefault();
                            MemberBalanceExchangeRateModel exchange = memberBalanceExchange.FirstOrDefault(t => t.BalanceIndex == item.BalanceIndex);

                            //其他余额种类兑换储值金
                            if (exchange.BalanceIndex != exchange.TargetBalanceIndex)
                            {
                                //判断要扣除的数量是否大于当前余额数量
                                if (item.ExchangeQty > cardBalance.Balance)
                                {
                                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "退款失败");
                                }

                                //源余额减少
                                cardBalance.Balance = cardBalance.Balance - item.ExchangeQty;
                                if (!Data_Card_BalanceService.I.Update(cardBalance, false))
                                {
                                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "余额更新失败");
                                }

                                //目标余额种类实体(储值金)
                                Data_Card_Balance targetBalance = Data_Card_BalanceService.I.GetModels(b => b.BalanceIndex == exchange.TargetBalanceIndex && b.CardIndex == memberCard.ID).FirstOrDefault();
                                //储值金增加
                                targetBalance.Balance += item.ExchangeValue;
                                if (!Data_Card_BalanceService.I.Update(targetBalance, false))
                                {
                                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "余额更新失败");
                                }

                                //增加会员卡余额互转记录
                                Flw_MemberCard_BalanceCharge balanceChange = new Flw_MemberCard_BalanceCharge();
                                balanceChange.ID = RedisCacheHelper.CreateStoreSerialNo(storeId);
                                balanceChange.MerchID = merchID;
                                balanceChange.StoreID = storeId;
                                balanceChange.MemberID = memberCard.MemberID;
                                balanceChange.SourceBalanceIndex = item.BalanceIndex;
                                balanceChange.SourceCount = item.ExchangeQty;
                                balanceChange.SourceRemain = cardBalance.Balance;
                                balanceChange.TargetBalanceIndex = exchange.TargetBalanceIndex;
                                balanceChange.TargetCount = item.ExchangeValue;
                                balanceChange.TargetRemain = targetBalance.Balance;
                                balanceChange.OpTime = DateTime.Now;
                                balanceChange.OpUserID = userId;
                                balanceChange.ScheduleID = schedule.ID;
                                balanceChange.Workstation = workStation;
                                balanceChange.CheckDate = schedule.CheckDate;
                                balanceChange.ExitID = backSerialNo;///***主卡退卡记录ID***
                                balanceChange.Note = string.Format("{0}兑换{1}", exchange.TypeName, exchange.TargetTypeName);
                                if (!Flw_MemberCard_BalanceChargeService.I.Add(balanceChange, false))
                                {
                                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, string.Format("{0}兑换{1}失败", exchange.TypeName, exchange.TargetTypeName));
                                }
                                strCardExitNote += string.Format("退{0}：{1}，价值{2}：{3}；" + Environment.NewLine, exchange.TypeName, item.ExchangeQty, exchange.TargetTypeName, item.ExchangeValue);
                            }
                            else
                            {
                                strCardExitNote += string.Format("退{0}：{1}；" + Environment.NewLine, exchange.TargetTypeName, item.ExchangeValue);
                            }
                            //总退款额累加
                            backTotal += item.ExchangeValue;
                        }

                        //商户映射到储值金的币种类
                        var balanceTypes = Dict_BalanceTypeService.I.GetModels(t => t.MerchID == merchID && t.State == 1 && t.MappingType == 4).ToList();
                        //当前用户余额集合
                        var balances = Data_Card_BalanceService.I.GetModels(t => t.MerchID == merchID && t.CardIndex == memberCard.ID).ToList();

                        //储值金
                        Data_Card_Balance exitBalance = balances.Where(t => balanceTypes.Any(b => b.ID == t.BalanceIndex)).FirstOrDefault();
                        //剩余储值金余额
                        decimal remainMoney = exitBalance.Balance.Value - backTotal;
                        exitBalance.Balance = remainMoney;
                        if (remainMoney < 0 || !Data_Card_BalanceService.I.Update(exitBalance, false))
                        {
                            return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "余额更新失败");
                        }

                        //主卡退卡、退款信息
                        Flw_MemberCard_Exit memberParentCardExit = new Flw_MemberCard_Exit();
                        memberParentCardExit.OrderID = backSerialNo;
                        memberParentCardExit.MerchID = merchID;
                        memberParentCardExit.StoreID = storeId;
                        memberParentCardExit.MemberID = memberCard.MemberID;
                        memberParentCardExit.OperateType = Convert.ToInt32(backType);
                        memberParentCardExit.CardID = memberCard.ICCardID;
                        memberParentCardExit.Deposit = backType == "0" ? memberCard.Deposit : 0;
                        memberParentCardExit.ExitMoney = backTotal;
                        memberParentCardExit.RemainMoney = remainMoney; //储值金余额
                        memberParentCardExit.WorkStation = workStation;
                        memberParentCardExit.UserID = userId;
                        memberParentCardExit.ScheduldID = schedule.ID.ToString();
                        memberParentCardExit.CheckDate = schedule.CheckDate;
                        memberParentCardExit.Note = strCardExitNote;
                        if (!Flw_MemberCard_ExitService.I.Add(memberParentCardExit, false))
                        {
                            return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "主卡退款失败");
                        }
                    }

                    //读取门店退卡配置，退卡时余额是否清零
                    var storeExitCardConfig = Data_ParametersService.I.GetModels(t => t.StoreID == storeId && t.System == "chkForceClearBalanceOnExitCard").FirstOrDefault();
                    //当退卡类型为退卡又退款时，还要判断是否将不能兑换为储值金的余额（如积分、彩票等）清零
                    if (backType == "2" && storeExitCardConfig != null && storeExitCardConfig.ParameterValue == "1")
                    {
                        List<MemberBalanceExchangeRateModel> query = memberBalanceExchange.Where(t => t.ExchangeRate == 0).ToList();
                        foreach (var item in query)
                        {
                            Data_Card_Balance cardBalance = Data_Card_BalanceService.I.GetModels(b => b.BalanceIndex == item.BalanceIndex && b.CardIndex == memberCard.ID).FirstOrDefault();
                            if (cardBalance.Balance > 0)
                            {
                                cardBalance.Balance = 0;
                                if (!Data_Card_BalanceService.I.Update(cardBalance, false))
                                {
                                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, string.Format("{0}清零失败", item.TypeName));
                                }
                            }
                        }
                    }

                    ts.Complete();
                }
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.T, "");
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        } 
        #endregion 
        #endregion

        #region 补卡、换卡、续期

        #region 创建补卡/换卡订单
        /// <summary>
        /// 创建补卡/换卡订单
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [Authorize(Roles = "StoreUser")]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object CreateRechangeCardOrder(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string oldCardIndexId = dicParas.ContainsKey("oldCardIndexId") ? dicParas["oldCardIndexId"].ToString() : string.Empty;
                string operateType = dicParas.ContainsKey("OperateType") ? dicParas["OperateType"].ToString() : string.Empty;
                if (string.IsNullOrEmpty(oldCardIndexId))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "会员卡ID为空");
                }
                string operate = string.Empty;
                if (operateType == "0")
                {
                    operate = "换卡";
                }
                else if (operateType == "1")
                {
                    operate = "补卡";
                }
                else
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "操作无效");
                }

                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                string merchID = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string workStation = (userTokenKeyModel.DataModel as TokenDataModel).WorkStation;
                int userId = userTokenKeyModel.LogId.Toint(0);

                Data_Member_Card oldCard = Data_Member_CardService.I.GetModels(t => t.ID == oldCardIndexId).FirstOrDefault();
                if (oldCard == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "会员卡不存在");
                }
                //判断状态
                if (oldCard.CardStatus != 1)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, string.Format("当前会员卡不可用，不能进行{0}操作", operate));
                }
                //判断是否为电子卡
                if (oldCard.ICCardID.Length == 32)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, string.Format("电子卡不能进行{0}操作", operate));
                }
                //判断当前卡能否在当前门店使用
                var storeUseCards = Data_Member_Card_StoreService.I.GetModels(t => t.CardID == oldCard.ID && t.StoreID == storeId).FirstOrDefault();
                if (storeUseCards == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, string.Format("此会员卡不能在当前门店使用，不能进行{0}操作", operate));
                }

                //当前班次
                Flw_Schedule schedule = Flw_ScheduleService.I.GetModels(t => t.StoreID == storeId && t.State == 1).FirstOrDefault();
                if (schedule == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, string.Format("当前班次为空，不能进行{0}操作", operate));
                }
                decimal? OpFree = 0m; //总费用
                string orderId = RedisCacheHelper.CreateStoreSerialNo(storeId);
                using (TransactionScope ts = new TransactionScope(TransactionScopeOption.RequiresNew))
                {
                    string saleId = RedisCacheHelper.CreateStoreSerialNo(storeId);

                    Data_MemberLevel memberLevel = Data_MemberLevelService.I.GetModels(t => t.MemberLevelID == oldCard.MemberLevelID).FirstOrDefault();

                    OpFree = operateType == "1" ? (memberLevel.RechargeFee.HasValue ? memberLevel.RechargeFee : 0) : (memberLevel.ChangeFee.HasValue ? memberLevel.ChangeFee : 0);

                    //创建订单
                    Flw_Order order = new Flw_Order();
                    order.OrderID = orderId;
                    order.StoreID = storeId;
                    order.FoodCount = 1;
                    order.GoodCount = 1;
                    order.MemberID = oldCard.MemberID;
                    order.CardID = oldCard.ID;
                    order.OrderSource = 0;
                    order.CreateTime = DateTime.Now;
                    order.PayCount = OpFree;
                    order.FreePay = 0;
                    order.UserID = userId;
                    order.ScheduleID = schedule.ID;
                    order.WorkStation = workStation;
                    order.OrderStatus = 1;
                    order.Note = operate;
                    if (!Flw_OrderService.I.Add(order))
                    {
                        return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, string.Format("创建{0}订单失败", operate));
                    }

                    //创建订单明细
                    Flw_Order_Detail orderDetail = new Flw_Order_Detail();
                    orderDetail.ID = RedisCacheHelper.CreateStoreSerialNo(storeId);
                    //orderDetail.mer
                    orderDetail.OrderFlwID = orderId;
                    orderDetail.GoodsCount = 1;
                    if (!Flw_Order_DetailService.I.Add(orderDetail))
                    {
                        return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, string.Format("创建订单明细失败", operate));
                    }
                    ts.Complete();
                }

                var result = new
                {
                    OrderNo = orderId,
                    OpFree = OpFree.Value.ToString("0.00")
                };
                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, result);
            }
            catch (DbEntityValidationException ex)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, ex.Message);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        } 
        #endregion

        #region 补卡、换卡
        /// <summary>
        /// 补卡、换卡
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [Authorize(Roles = "StoreUser")]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object SupplementCard(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string oldCardIndexId = dicParas.ContainsKey("oldCardIndexId") ? dicParas["oldCardIndexId"].ToString() : string.Empty;
                string newCardNo = dicParas.ContainsKey("newCardNo") ? dicParas["newCardNo"].ToString() : string.Empty;
                string repeatCode = dicParas.ContainsKey("repeatCode") ? dicParas["repeatCode"].ToString() : string.Empty;
                string operateType = dicParas.ContainsKey("OperateType") ? dicParas["OperateType"].ToString() : string.Empty;
                if (string.IsNullOrEmpty(oldCardIndexId))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "会员卡ID为空");
                }
                if (string.IsNullOrEmpty(newCardNo))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "新会员卡号为空");
                }
                int iRepeatCode = repeatCode.Toint(0);
                if (iRepeatCode == 0)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "卡动态密码无效");
                }
                string operate = string.Empty;
                if (operateType == "0")
                {
                    operate = "换卡";
                }
                else if (operateType == "1")
                {
                    operate = "补卡";
                }
                else
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "操作无效");
                }

                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                string merchID = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string workStation = (userTokenKeyModel.DataModel as TokenDataModel).WorkStation;
                int userId = userTokenKeyModel.LogId.Toint(0);

                Data_Member_Card oldCard = Data_Member_CardService.I.GetModels(t => t.ID == oldCardIndexId).FirstOrDefault();
                if (oldCard == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "会员卡不存在");
                }
                ////判断密码
                //if(!oldCard.CardPassword.Equals(cardPwd))
                //{
                //    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "卡密码错误");
                //}
                //判断状态
                if(oldCard.CardStatus != 1)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, string.Format("当前会员卡不可用，不能进行{0}操作", operate));
                }
                //判断是否为电子卡
                if(oldCard.ICCardID.Length == 32)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, string.Format("电子卡不能进行{0}操作", operate));
                }
                //判断当前卡能否在当前门店使用
                var storeUseCards = Data_Member_Card_StoreService.I.GetModels(t => t.CardID == oldCard.ID && t.StoreID == storeId).FirstOrDefault();
                if(storeUseCards == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, string.Format("此会员卡不能在当前门店使用，不能进行{0}操作", operate));
                }

                //验证新卡号在当前门店是否存在
                var verifyCard = Data_Member_CardService.I.GetModels(t => t.ICCardID == newCardNo).FirstOrDefault();
                if (verifyCard != null)
                {
                    var verifyCardStore = Data_Member_Card_StoreService.I.GetModels(t => t.StoreID == storeId && t.CardID == verifyCard.ID).FirstOrDefault();
                    if (verifyCardStore != null)
                    {
                        return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, string.Format("新会员卡已存在，请使用新的空白卡进行{0}", operate));
                    }
                }

                //当前班次
                Flw_Schedule schedule = Flw_ScheduleService.I.GetModels(t => t.StoreID == storeId && t.State == 1).FirstOrDefault();
                if (schedule == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, string.Format("当前班次为空，不能进行{0}操作", operate));
                }
                decimal? OpFree = 0m; //总费用
                using (TransactionScope ts = new TransactionScope(TransactionScopeOption.RequiresNew))
                {
                    //创建新卡
                    Data_Member_Card newCard = new Data_Member_Card();
                    newCard.ID = RedisCacheHelper.CreateStoreSerialNo(storeId);
                    newCard.MerchID = oldCard.MerchID;
                    newCard.StoreID = storeId;
                    newCard.ICCardID = newCardNo;
                    newCard.ParentCard = oldCard.ParentCard;
                    newCard.JoinChannel = oldCard.JoinChannel;
                    newCard.CardPassword = oldCard.CardPassword;
                    newCard.CardType = oldCard.CardType;
                    newCard.CardShape = oldCard.CardShape;
                    newCard.CardName = oldCard.CardName;
                    newCard.CardSex = oldCard.CardSex;
                    newCard.CardBirthDay = oldCard.CardBirthDay;
                    newCard.FaceReadID = oldCard.FaceReadID;
                    newCard.CardLimit = oldCard.CardLimit;
                    newCard.AllowIn = oldCard.AllowIn;
                    newCard.AllowOut = oldCard.AllowOut;
                    newCard.MemberID = oldCard.MemberID;
                    newCard.MemberLevelID = oldCard.MemberLevelID;
                    newCard.CreateTime = oldCard.CreateTime;
                    newCard.EndDate = oldCard.EndDate;
                    newCard.LastStore = storeId;
                    newCard.UpdateTime = DateTime.Now;
                    newCard.Deposit = oldCard.Deposit;
                    newCard.UID = oldCard.UID;
                    newCard.IsLock = oldCard.IsLock;
                    //newCard.RepeatCode = new Random(Guid.NewGuid().GetHashCode()).Next(1, 256);
                    newCard.RepeatCode = iRepeatCode;
                    newCard.CardStatus = 1;
                    newCard.OrderID = oldCard.OrderID;
                    if (!Data_Member_CardService.I.Add(newCard, false))
                    {
                        return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "创建新卡失败");
                    }

                    //更新会员卡有效门店
                    var cardStoreList = Data_Member_Card_StoreService.I.GetModels(t => t.CardID == oldCard.ID);
                    foreach (var item in cardStoreList)
                    {
                        item.CardID = newCard.ID;
                        if (!Data_Member_Card_StoreService.I.Update(item, false))
                        {
                            return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "新会员卡与门店关联失败");
                        }
                    }

                    //如果旧卡是主卡：将旧卡余额的索引关联到新卡索引
                    // 还要将此主卡的附属卡的ParentCard关联到新卡
                    if(oldCard.CardType == 0)
                    {
                        //当前主卡的附属卡
                        var childs = Data_Member_CardService.I.GetModels(t => t.ParentCard == oldCard.ID);
                        foreach (var item in childs)
                        {
                            item.ParentCard = newCard.ID;
                            if (!Data_Member_CardService.I.Update(item, false))
                            {
                                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "更新附属卡信息失败");
                            }
                        }

                        //将旧卡正价余额中的卡索引改为新卡ID
                        var balances = Data_Card_BalanceService.I.GetModels(t => t.CardIndex == oldCard.ID);
                        foreach (var item in balances)
                        {
                            item.CardIndex = newCard.ID;
                            if(!Data_Card_BalanceService.I.Update(item, false))
                            {
                                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "更新卡余额失败");
                            }
                        }
                        //将旧卡赠送余额中的卡索引改为新卡ID
                        var balanceFrees = Data_Card_Balance_FreeService.I.GetModels(t => t.CardIndex == oldCard.ID);
                        foreach (var item in balanceFrees)
                        {
                            item.CardIndex = newCard.ID;
                            if (!Data_Card_Balance_FreeService.I.Update(item, false))
                            {
                                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "更新卡余额失败");
                            }
                        }
                    }

                    //旧卡状态改为不可用
                    oldCard.CardStatus = 0;
                    bool ret = Data_Member_CardService.I.Update(oldCard, false);
                    if (!ret)
                    {
                        return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, operate + "失败");
                    }

                    string saleId = RedisCacheHelper.CreateStoreSerialNo(storeId);

                    Data_MemberLevel memberLevel = Data_MemberLevelService.I.GetModels(t => t.MemberLevelID == oldCard.MemberLevelID).FirstOrDefault();

                    //会员补卡/换卡记录
                    Flw_MemberCard_Change cardChange = new Flw_MemberCard_Change();
                    cardChange.OrderID = RedisCacheHelper.CreateStoreSerialNo(storeId);
                    cardChange.MerchID = merchID;
                    cardChange.StoreID = storeId;
                    cardChange.MemberID = oldCard.MemberID;
                    cardChange.OperateType = Convert.ToInt32(operateType);
                    cardChange.OldCardID = oldCard.ICCardID;
                    cardChange.NewCardID = newCardNo;
                    cardChange.FoodSaleID = saleId;
                    cardChange.OpFee = operateType == "1" ? (memberLevel.RechargeFee.HasValue ? memberLevel.RechargeFee : 0) : (memberLevel.ChangeFee.HasValue ? memberLevel.ChangeFee : 0);
                    cardChange.OpStoreID = storeId;
                    cardChange.WorkStation = workStation;
                    cardChange.UserID = userId;
                    cardChange.ScheduldID = schedule.ID.ToString();
                    cardChange.CheckDate = schedule.CheckDate;
                    cardChange.Note = operate;
                    if (!Flw_MemberCard_ChangeService.I.Add(cardChange, false))
                    {
                        return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, string.Format("写入{0}记录失败", operate));
                    }

                    OpFree = cardChange.OpFee;

                    //增加销售流水
                    Flw_Food_Sale sale = new Flw_Food_Sale();
                    sale.ID = saleId;
                    sale.MerchID = merchID;
                    sale.StoreID = storeId;
                    sale.FlowType = 0;//单品
                    sale.SingleType = operateType == "0" ? 5 : 4;
                    sale.FoodID = cardChange.OrderID;
                    sale.SaleCount = 1;
                    sale.MemberLevelID = oldCard.MemberLevelID;
                    sale.Deposit = 0;
                    if (operateType == "1")
                    {
                        sale.ReissueFee = cardChange.OpFee;
                        sale.TotalMoney = sale.ReissueFee;
                    }
                    else
                    {
                        sale.ChangeFee = cardChange.OpFee;
                        sale.TotalMoney = sale.ChangeFee;
                    }
                    sale.BuyFoodType = 0;
                    sale.Note = operate;
                    if (!Flw_Food_SaleService.I.Add(sale))
                    {
                        return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, string.Format("创建{0}套餐销售记录失败", operate));
                    }

                    ts.Complete();
                }

                var result = new
                {
                    NewICCardId = newCardNo,
                    OpFree = OpFree.Value.ToString("0.00")
                };
                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, result);
            }
            catch (DbEntityValidationException ex)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, ex.Message);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        } 
        #endregion 

        #region 续卡
        /// <summary>
        /// 续卡
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [Authorize(Roles = "StoreUser")]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object RenewCard(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string ICCardId = dicParas.ContainsKey("ICCardId") ? dicParas["ICCardId"].ToString() : string.Empty;
                if (string.IsNullOrEmpty(ICCardId))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "会员卡号无效");
                }

                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                string merchID = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string workStation = (userTokenKeyModel.DataModel as TokenDataModel).WorkStation;
                int userId = userTokenKeyModel.LogId.Toint(0);

                Data_Member_Card memberCard = Data_Member_CardService.I.GetModels(t => t.MerchID == merchID && t.ICCardID == ICCardId).FirstOrDefault();
                if (memberCard == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "会员卡不存在");
                }

                //当前班次
                Flw_Schedule schedule = Flw_ScheduleService.I.GetModels(t => t.StoreID == storeId && t.State == 1).FirstOrDefault();
                if (schedule == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "当前班次为空，不能进行续卡操作");
                }

                Data_MemberLevel memberLevel = Data_MemberLevelService.I.GetModels(t => t.MemberLevelID == memberCard.MemberLevelID).FirstOrDefault();
                DateTime? oldEndDate = memberCard.EndDate;
                decimal? OpFree = 0m;
                using (TransactionScope ts = new TransactionScope(TransactionScopeOption.RequiresNew))
                {
                    memberCard.LastStore = storeId;
                    memberCard.UpdateTime = DateTime.Now;
                    memberCard.RepeatCode = new Random(Guid.NewGuid().GetHashCode()).Next(1, 256);
                    //更新会员卡到期时间
                    memberCard.EndDate = memberCard.EndDate.HasValue ? memberCard.EndDate.Value.AddDays(memberLevel.Validday.Value) : DateTime.Now.AddDays(memberLevel.Validday.Value);
                    bool ret = Data_Member_CardService.I.Update(memberCard, false);
                    if (!ret)
                    {
                        return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "续卡失败");
                    }

                    string saleId = RedisCacheHelper.CreateStoreSerialNo(storeId);

                    //会员补卡记录
                    Flw_MemberCard_Renew cardRenew = new Flw_MemberCard_Renew();
                    cardRenew.OrderID = RedisCacheHelper.CreateStoreSerialNo(storeId);
                    cardRenew.MerchID = merchID;
                    cardRenew.StoreID = storeId;
                    cardRenew.MemberID = memberCard.MemberID;
                    cardRenew.CardID = ICCardId;
                    cardRenew.OldEndDate = oldEndDate;
                    cardRenew.NewEndDate = memberCard.EndDate;
                    cardRenew.FoodSaleID = saleId;
                    cardRenew.RenewFee = memberLevel.ContinueFee.HasValue ? memberLevel.ContinueFee.Value : 0;
                    cardRenew.WorkStation = workStation;
                    cardRenew.UserID = userId;
                    cardRenew.ScheduldID = schedule.ID.ToString();
                    cardRenew.CheckDate = schedule.CheckDate;
                    cardRenew.Note = "续卡";
                    if (!Flw_MemberCard_RenewService.I.Add(cardRenew, false))
                    {
                        return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "续卡失败");
                    }

                    OpFree = cardRenew.RenewFee;

                    //增加销售流水
                    Flw_Food_Sale sale = new Flw_Food_Sale();
                    sale.ID = saleId;
                    sale.MerchID = merchID;
                    sale.StoreID = storeId;
                    sale.FlowType = 0;//单品
                    sale.SingleType = 3;
                    sale.FoodID = cardRenew.OrderID;
                    sale.SaleCount = 1;
                    sale.MemberLevelID = memberCard.MemberLevelID;
                    sale.Deposit = 0;
                    sale.RenewFee = OpFree;
                    sale.TotalMoney = sale.RenewFee;
                    sale.BuyFoodType = 0;
                    sale.Note = "续卡";
                    if (!Flw_Food_SaleService.I.Add(sale))
                    {
                        return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "续卡失败");
                    }

                    ts.Complete();
                }

                var result = new
                {
                    ICCardId = ICCardId,
                    OpFree = OpFree.Value.ToString("0.00"),
                    EndDate = memberCard.EndDate.Value.ToString("yyyy-MM-dd")
                };
                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, result);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }
        #endregion 
        #endregion

        #region 余额币种兑换

        #region 获取会员余额兑换列表
        /// <summary>
        /// 获取会员余额兑换列表
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [Authorize(Roles = "StoreUser")]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object getBalanceExchangeList(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string ICCardId = dicParas.ContainsKey("iccardId") ? dicParas["iccardId"].ToString() : string.Empty;
                if (string.IsNullOrEmpty(ICCardId))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "会员卡号无效");
                }

                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                string merchID = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string workStation = (userTokenKeyModel.DataModel as TokenDataModel).WorkStation;
                int userId = userTokenKeyModel.LogId.Toint(0);

                Data_Member_Card currCard = Data_Member_CardService.I.GetModels(t => t.ICCardID == ICCardId && t.MerchID == merchID).FirstOrDefault();
                if (currCard == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "会员卡不存在");
                }

                //当前会员卡可用余额列表
                var cardBalances = from a in Data_Card_BalanceService.N.GetModels(t => t.CardIndex == currCard.ID)
                                   join c in Data_Card_Balance_StoreListService.N.GetModels(t => t.StoreID == storeId) on a.ID equals c.CardBalanceID
                                   join b in Data_Card_Balance_FreeService.N.GetModels(t => t.CardIndex == currCard.ID) on a.BalanceIndex equals b.BalanceIndex into b1
                                   from balance in b1.DefaultIfEmpty()
                                   join d in Dict_BalanceTypeService.N.GetModels(t => t.State == 1 && t.MerchID == merchID) on a.BalanceIndex equals d.ID
                                   select new SourceBalanceModel
                                   {
                                       BalanceId = a.ID,
                                       BalanceIndex = a.BalanceIndex.Value,
                                       BalanceName = d.TypeName,
                                       Balance = a.Balance,
                                       BalanceFree = balance == null ? 0 : balance.Balance,
                                       BalanceTotal = a.Balance + (balance == null ? 0 : balance.Balance)
                                   };

                //当前商户、当前会员级别兑换规则列表
                var balanceCharges = from a in Data_MemberLevel_BalanceChargeService.N.GetModels(t => t.MerchID == merchID && t.MemberLevelID == currCard.MemberLevelID)
                                     join b in Dict_BalanceTypeService.N.GetModels(t => t.State == 1 && t.MerchID == merchID) on a.TargetBalanceIndex equals b.ID
                                     select new
                                     {
                                         ExchangeRuleId = a.ID,
                                         SourceBalanceIndex = a.SourceBalanceIndex,
                                         SourceCount = a.SourceCount,
                                         TargetBalanceIndex = a.TargetBalanceIndex,
                                         TargetBalanceName = b.TypeName,
                                         TargetCount = a.TargetCount,
                                         DecimalNumber = b.DecimalNumber,
                                         AddingType = b.AddingType
                                     };


                List<ChargeBalanceModel> chargeBalanceList = new List<ChargeBalanceModel>();

                var balanceList = cardBalances.ToList();
                var charges = balanceCharges.ToList();
                foreach (var item in balanceList)
                {
                    var chargeRule = charges.Where(t => t.SourceBalanceIndex == item.BalanceIndex).Select(t => new TargetChargeModel()
                    {
                        ExchangeRuleId = t.ExchangeRuleId,
                        TargetBalanceName = t.TargetBalanceName,
                        TargetBalanceQty = GetTargetBalanceQuantity(balanceList, t.TargetBalanceIndex.Value),
                        SourceCount = t.SourceCount.HasValue ? t.SourceCount.Value : 1,
                        TargetCount = t.TargetCount.HasValue ? t.TargetCount.Value : 1,
                        DecimalNumber = t.DecimalNumber.Value,
                        AddingType = t.AddingType.Value

                    }).Where(t => t.ExchangeRate != "0").ToList();
                    if (chargeRule.Count > 0)
                    {
                        ChargeBalanceModel model = new ChargeBalanceModel();
                        model.BalanceId = item.BalanceId;
                        model.SourceBalanceName = item.BalanceName;
                        model.BalanceTotal = item.BalanceTotal.HasValue ? item.BalanceTotal.Value.ToString() : "0";
                        model.Balance = item.Balance.HasValue ? item.Balance.Value.ToString() : "0";
                        model.BalanceFree = item.BalanceFree.HasValue ? item.BalanceFree.Value.ToString() : "0";
                        model.TargetChargeList = chargeRule;
                        chargeBalanceList.Add(model);
                    }
                }

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, chargeBalanceList);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        private string GetTargetBalanceQuantity(List<SourceBalanceModel> balanceList, int targetBalanceIndex)
        {
            var item = balanceList.FirstOrDefault(t => t.BalanceIndex == targetBalanceIndex);
            return item == null ? "0" : item.BalanceTotal.Value.ToString();
        }
        #endregion

        #region 余额兑换
        /// <summary>
        /// 余额兑换
        /// </summary>
        /// <param name="dicParas"></param>
        /// <returns></returns>
        [Authorize(Roles = "StoreUser")]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object balanceExchange(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string ICCardId = dicParas.ContainsKey("iccardId") ? dicParas["iccardId"].ToString() : string.Empty;
                string balanceId = dicParas.ContainsKey("balanceId") ? dicParas["balanceId"].ToString() : string.Empty;
                string exchangeQty = dicParas.ContainsKey("exchangeQty") ? dicParas["exchangeQty"].ToString() : string.Empty;
                string exchangeRuleId = dicParas.ContainsKey("exchangeRuleId") ? dicParas["exchangeRuleId"].ToString() : string.Empty;
                if (string.IsNullOrEmpty(ICCardId))
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "会员卡号无效");
                }
                int iExchangeRuleId = exchangeRuleId.Toint(0); //兑换规则ID
                int iExchangeQty = exchangeQty.Toint(0); //兑换数量
                if (iExchangeRuleId == 0 || iExchangeQty == 0)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "兑换参数错误");
                }               

                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string storeId = (userTokenKeyModel.DataModel as TokenDataModel).StoreID;
                string merchID = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                string workStation = (userTokenKeyModel.DataModel as TokenDataModel).WorkStation;
                int userId = userTokenKeyModel.LogId.Toint(0);

                Data_Member_Card currCard = Data_Member_CardService.I.GetModels(t => t.ICCardID == ICCardId && t.MerchID == merchID).FirstOrDefault();
                if (currCard == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "会员卡不存在");
                }

                //判断源余额是否为当前门店合法余额
                //源余额可用门店
                var cardStore = Data_Card_Balance_StoreListService.I.GetModels(t => t.CardBalanceID == balanceId && t.StoreID == storeId).FirstOrDefault();
                //源余额实体
                Data_Card_Balance sourceBalance = Data_Card_BalanceService.I.GetModels(t => t.ID == balanceId && t.CardIndex == currCard.ID && t.MerchID == merchID).FirstOrDefault();
                if (sourceBalance == null || cardStore == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "余额无效");
                }

                //判断兑换数量是否超过当前源余额数量
                if(iExchangeQty > sourceBalance.Balance)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "余额不足，兑换数量不能大于余额数量");
                }

                //判断当前会员卡级别是否存在【源余额】到【目标余额】的兑换规则
                //兑换规则：规则实体的级别必须与卡级别一致、商户必须一致、源余额类型必须一致
                var exchangeRule = Data_MemberLevel_BalanceChargeService.I.GetModels(t => t.ID == iExchangeRuleId).FirstOrDefault();
                if (exchangeRule == null || exchangeRule.MemberLevelID != currCard.MemberLevelID || exchangeRule.MerchID != merchID || exchangeRule.SourceBalanceIndex != sourceBalance.BalanceIndex)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "兑换规则无效");
                }

                //判断目标余额是否为当前门店合法余额
                var balanceTypeByStore = Data_BalanceType_StoreListService.I.GetModels(t => t.MerchID == merchID && t.BalanceIndex == exchangeRule.TargetBalanceIndex && t.StroeID == storeId).FirstOrDefault();
                if (balanceTypeByStore == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "兑换目标当前门店不可用");
                }

                //当前卡在当前门店的目标余额ID集合
                var targetBalanceIds = from a in Data_Card_BalanceService.N.GetModels(t => t.CardIndex == currCard.ID && t.MerchID == merchID && t.BalanceIndex == exchangeRule.TargetBalanceIndex)
                                       join b in Data_Card_Balance_StoreListService.N.GetModels(t => t.StoreID == storeId) on a.ID equals b.CardBalanceID
                                       select new
                                       {
                                           BalanceId = a.ID
                                       };

                //如果当前卡在当前门店没有目标余额 或者 不只一个目标余额，都无法兑换
                if(targetBalanceIds.Count() != 1)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "兑换目标无效");
                }

                string targetBalanceId = targetBalanceIds.FirstOrDefault().BalanceId;
                //获取目标余额实体
                Data_Card_Balance targetBalance = Data_Card_BalanceService.I.GetModels(t => t.ID == targetBalanceId).FirstOrDefault();

                //获取目标余额类别字典表实体，用于计算兑换结果的小数位
                Dict_BalanceType balanceType = Dict_BalanceTypeService.I.GetModels(t => t.MerchID == merchID && t.ID == targetBalance.BalanceIndex).FirstOrDefault();
                if (balanceType == null)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "兑换目标无效");
                }

                //计算兑换比率
                if(!exchangeRule.SourceCount.HasValue || exchangeRule.SourceCount.Value == 0 || !exchangeRule.TargetCount.HasValue || exchangeRule.TargetCount.Value == 0)
                {
                    return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "兑换规则错误");
                }
                decimal rate = exchangeRule.TargetCount.Value / (decimal)exchangeRule.SourceCount.Value;
                rate = Math.Round(rate, 2, MidpointRounding.AwayFromZero);

                //计算小数位，0位小数：按exchangeVal*1计算后再舍弃， 1位小数： 按exchangeVal*10计算后再舍弃， 2位小数： 按exchangeVal*100计算后再舍弃，以此类推
                //int decimalNumber = Convert.ToInt32("1".PadRight(memberBalance.DecimalNumber + 1, '0'));
                decimal decimalNumber = Convert.ToDecimal(Math.Pow(10, balanceType.DecimalNumber.HasValue ? balanceType.DecimalNumber.Value : 0));

                //兑换价值 = 兑换数量 * 兑换比例
                decimal exchangeVal = iExchangeQty * rate;
                //按小数位升位
                exchangeVal = exchangeVal * decimalNumber;

                //小数舍弃方式：0 全部舍弃 只取整数部分，1 全部保留 有任何小数都进位，2 四舍五入
                switch (balanceType.AddingType)
                {
                    case 0:
                        exchangeVal = Math.Floor(exchangeVal); break;
                    case 1:
                        exchangeVal = Math.Ceiling(exchangeVal); break;
                    case 2:
                        exchangeVal = Math.Round(exchangeVal, MidpointRounding.AwayFromZero); break;
                    default: 
                        exchangeVal = Math.Floor(exchangeVal); break; //默认全部舍弃
                }

                //按小数位降位，得到真实兑换价值
                exchangeVal = exchangeVal / decimalNumber;
                
                //更新余额
                using (TransactionScope ts = new TransactionScope(TransactionScopeOption.RequiresNew))
                {
                    //源余额减少
                    sourceBalance.Balance -= iExchangeQty;
                    if (!Data_Card_BalanceService.I.Update(sourceBalance, false))
                    {
                        return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "操作失败，扣除余额不成功");
                    }
                    //目标余额增加
                    targetBalance.Balance += exchangeVal;
                    if (!Data_Card_BalanceService.I.Update(targetBalance, false))
                    {
                        return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "操作失败，增加余额不成功");
                    }
                    ts.Complete();
                }

                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.T, "");
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        } 
        #endregion

        #endregion
    }
}