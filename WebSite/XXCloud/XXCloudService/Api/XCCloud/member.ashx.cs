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
using XCCloudService.Business.XCCloud;
using XCCloudService.Common;
using XCCloudService.Model.CustomModel.XCCloud;
using XCCloudService.Model.CustomModel.XCCloud.Member;
using XCCloudService.Model.XCCloud;
using XCCloudService.Business.XCGameMana;
using XCCloudService.Business.XCGame;
using System.IO;
using Microsoft.SqlServer.Server;
using XCCloudService.Common.Enum;
using XCCloudService.Common.Extensions;
using System.Transactions;

namespace XCCloudService.Api.XCCloud
{
    [Authorize(Roles = "MerchUser")]
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
                StoreIDDataModel userTokenDataModel = (StoreIDDataModel)(userTokenModel.DataModel);
                string storeId = dicParas.ContainsKey("storeId") ? dicParas["storeId"].ToString() : string.Empty;
                string mobile = dicParas.ContainsKey("mobile") ? dicParas["mobile"].ToString() : string.Empty;
                if (!userTokenDataModel.StoreId.Equals(storeId))
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
            StoreIDDataModel userTokenDataModel = (StoreIDDataModel)(userTokenModel.DataModel);
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
                var baseMemberModel = Utils.GetModelList<BaseMemberModel>(ds.Tables[0]).ToList()[0];
                return ResponseModelFactory<BaseMemberModel>.CreateModel(isSignKeyReturn, baseMemberModel);
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
            StoreIDDataModel userTokenDataModel = (StoreIDDataModel)(userTokenModel.DataModel);

            string storedProcedure = "GetMemberLevel";
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@StoreId", userTokenDataModel.StoreId);
            System.Data.DataSet ds = XCCloudBLL.GetStoredProcedureSentence(storedProcedure, parameters);
            List<Data_MemberLevelModel> list = Utils.GetModelList<Data_MemberLevelModel>(ds.Tables[0]);

            return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, list);
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
            StoreIDDataModel userTokenDataModel = (StoreIDDataModel)(userTokenModel.DataModel);

            string icCardId = dicParas.ContainsKey("icCardId") ? dicParas["icCardId"].ToString() : string.Empty;
            string storeId = dicParas.ContainsKey("storeId") ? dicParas["storeId"].ToString() : string.Empty;

            if (string.IsNullOrEmpty(icCardId))
            {
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "会员卡号无效");
            }
            if (string.IsNullOrEmpty(storeId))
            {
                return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "门店号无效");
            }

            string storedProcedure = "GetMember";
            SqlParameter[] parameters = new SqlParameter[4];
            parameters[0] = new SqlParameter("@ICCardID", icCardId);
            parameters[1] = new SqlParameter("@StoreID", storeId);
            parameters[2] = new SqlParameter("@Result",SqlDbType.Int);
            parameters[2].Direction = System.Data.ParameterDirection.Output;
            parameters[3] = new SqlParameter("@ErrMsg", SqlDbType.VarChar,200);
            parameters[3].Direction = System.Data.ParameterDirection.Output;
            System.Data.DataSet ds = XCCloudBLL.GetStoredProcedureSentence(storedProcedure, parameters);
            if (parameters[2].Value.ToString() == "1")
            {
                var baseMemberModel = Utils.GetModelList<BaseMemberModel>(ds.Tables[0]).ToList()[0];
                return ResponseModelFactory<BaseMemberModel>.CreateModel(isSignKeyReturn, baseMemberModel);
            }
            return ResponseModelFactory.CreateModel(isSignKeyReturn, Return_Code.T, "", Result_Code.F, "会员信息不存在");
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetMemberLevelDic(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as MerchDataModel).MerchID;

                Dictionary<int, string> memberLevel = Data_MemberLevelBusiness.Instance.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && p.State == 1).Select(o => new
                {
                    MemberLevelID = o.MemberLevelID,
                    MemberLevelName = o.MemberLevelName
                }).Distinct().ToDictionary(d => d.MemberLevelID, d => d.MemberLevelName);
                
                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, memberLevel);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetMemberDic(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as MerchDataModel).MerchID;

                var linq = from a in Base_StoreInfoBusiness.NewInstance.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && (p.StoreState == (int)StoreState.Open || p.StoreState == (int)StoreState.Valid))
                           join b in Data_Member_Card_StoreBusiness.NewInstance.GetModels() on a.StoreID equals b.StoreID
                           join c in Data_Member_CardBusiness.NewInstance.GetModels() on b.CardID equals c.ID
                           join d in Base_MemberInfoBusiness.NewInstance.GetModels() on c.MemberID equals d.ID
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

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object QueryMemberLevel(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string memberLevelID = dicParas.ContainsKey("memberLevelID") ? dicParas["memberLevelID"].ToString() : string.Empty;
                string memberLevelName = dicParas.ContainsKey("memberLevelName") ? dicParas["memberLevelName"].ToString() : string.Empty;
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as MerchDataModel).MerchID;


                IData_MemberLevelService data_MemberLevelService = Data_MemberLevelBusiness.Instance;
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

                IDict_SystemService dict_SystemService = Dict_SystemBusiness.Instance;
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

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object SaveMemberLevel(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as MerchDataModel).MerchID;

                string errMsg = string.Empty;
                string memberLevelID = dicParas.ContainsKey("memberLevelID") ? (dicParas["memberLevelID"] + "") : string.Empty;
                string memberLevelName = dicParas.ContainsKey("memberLevelName") ? (dicParas["memberLevelName"] + "") : string.Empty;
                string coverUrl = dicParas.ContainsKey("coverUrl") ? (dicParas["coverUrl"] + "") : string.Empty;
                string openFee = dicParas.ContainsKey("openFee") ? (dicParas["openFee"] + "") : string.Empty;
                string deposit = dicParas.ContainsKey("deposit") ? (dicParas["deposit"] + "") : string.Empty;
                string clearPointDays = dicParas.ContainsKey("clearPointDays") ? (dicParas["clearPointDays"] + "") : string.Empty;
                string priceOff = dicParas.ContainsKey("priceOff") ? (dicParas["priceOff"] + "") : string.Empty;
                string validday = dicParas.ContainsKey("validday") ? (dicParas["validday"] + "") : string.Empty;
                string needAuthor = dicParas.ContainsKey("needAuthor") ? (dicParas["needAuthor"] + "") : string.Empty;
                string mustPhone = dicParas.ContainsKey("mustPhone") ? (dicParas["mustPhone"] + "") : string.Empty;
                string mustIDCard = dicParas.ContainsKey("mustIDCard") ? (dicParas["mustIDCard"] + "") : string.Empty;
                string useReadID = dicParas.ContainsKey("useReadID") ? (dicParas["useReadID"] + "") : string.Empty;
                string readFace = dicParas.ContainsKey("readFace") ? (dicParas["readFace"] + "") : string.Empty;
                string readPlam = dicParas.ContainsKey("readPlam") ? (dicParas["readPlam"] + "") : string.Empty;
                string changeFee = dicParas.ContainsKey("changeFee") ? (dicParas["changeFee"] + "") : string.Empty;
                string continueFee = dicParas.ContainsKey("continueFee") ? (dicParas["continueFee"] + "") : string.Empty;
                string continueUsePoint = dicParas.ContainsKey("continueUsePoint") ? (dicParas["continueUsePoint"] + "") : string.Empty;
                string consumeTotle = dicParas.ContainsKey("consumeTotle") ? (dicParas["consumeTotle"] + "") : string.Empty;
                string nonActiveDays = dicParas.ContainsKey("nonActiveDays") ? (dicParas["nonActiveDays"] + "") : string.Empty;
                string updateUsePoint = dicParas.ContainsKey("updateUsePoint") ? (dicParas["updateUsePoint"] + "") : string.Empty;
                string freeRate = dicParas.ContainsKey("freeRate") ? (dicParas["freeRate"] + "") : string.Empty;
                string freeCoin = dicParas.ContainsKey("freeCoin") ? (dicParas["freeCoin"] + "") : string.Empty;
                string freeType = dicParas.ContainsKey("freeType") ? (dicParas["freeType"] + "") : string.Empty;
                string freeNeedWin = dicParas.ContainsKey("freeNeedWin") ? (dicParas["freeNeedWin"] + "") : string.Empty;
                string birthdayFree = dicParas.ContainsKey("birthdayFree") ? (dicParas["birthdayFree"] + "") : string.Empty;
                string birthdayFreeCoin = dicParas.ContainsKey("birthdayFreeCoin") ? (dicParas["birthdayFreeCoin"] + "") : string.Empty;
                string minCoin = dicParas.ContainsKey("minCoin") ? (dicParas["minCoin"] + "") : string.Empty;
                string maxCoin = dicParas.ContainsKey("maxCoin") ? (dicParas["maxCoin"] + "") : string.Empty;
                string allowExitCard = dicParas.ContainsKey("allowExitCard") ? (dicParas["allowExitCard"] + "") : string.Empty;
                string allowExitMoney = dicParas.ContainsKey("allowExitMoney") ? (dicParas["allowExitMoney"] + "") : string.Empty;
                string allowExitCoinToCard = dicParas.ContainsKey("allowExitCoinToCard") ? (dicParas["allowExitCoinToCard"] + "") : string.Empty;
                string lockHead = dicParas.ContainsKey("lockHead") ? (dicParas["lockHead"] + "") : string.Empty;
                object[] memberLevelFoods = dicParas.ContainsKey("memberLevelFoods") ? (object[])dicParas["memberLevelFoods"] : null;
                object[] memberLevelFrees = dicParas.ContainsKey("memberLevelFrees") ? (object[])dicParas["memberLevelFrees"] : null;

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
                        IData_MemberLevelService data_MemberLevelService = Data_MemberLevelBusiness.Instance;
                        if (data_MemberLevelService.Any(a => a.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) &&
                            a.MemberLevelName.Equals(memberLevelName, StringComparison.OrdinalIgnoreCase) && a.MemberLevelID != iMemberLevelID && a.State == 1))
                        {
                            errMsg = "该会员级别名称已存在";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        var data_MemberLevel = data_MemberLevelService.GetModels(p => p.MemberLevelID == iMemberLevelID).FirstOrDefault() ?? new Data_MemberLevel();
                        data_MemberLevel.MemberLevelID = iMemberLevelID;
                        data_MemberLevel.MemberLevelName = memberLevelName;
                        data_MemberLevel.CoverURL = coverUrl;
                        data_MemberLevel.OpenFee = ObjectExt.Todecimal(openFee);
                        data_MemberLevel.Deposit = ObjectExt.Todecimal(deposit);
                        data_MemberLevel.ClearPointDays = ObjectExt.Toint(clearPointDays);
                        data_MemberLevel.PriceOFF = ObjectExt.Todecimal(priceOff);
                        data_MemberLevel.Validday = ObjectExt.Toint(validday);
                        data_MemberLevel.NeedAuthor = ObjectExt.Toint(needAuthor);
                        data_MemberLevel.MustPhone = ObjectExt.Toint(mustPhone);
                        data_MemberLevel.MustIDCard = ObjectExt.Toint(mustIDCard);
                        data_MemberLevel.UseReadID = ObjectExt.Toint(useReadID);
                        data_MemberLevel.ReadFace = ObjectExt.Toint(readFace);
                        data_MemberLevel.ReadPlam = ObjectExt.Toint(readPlam);
                        data_MemberLevel.ChangeFee = ObjectExt.Todecimal(changeFee);
                        data_MemberLevel.ContinueFee = ObjectExt.Todecimal(continueFee);
                        data_MemberLevel.ContinueUsePoint = ObjectExt.Toint(continueUsePoint);
                        data_MemberLevel.ConsumeTotle = ObjectExt.Todecimal(consumeTotle);
                        data_MemberLevel.NonActiveDays = ObjectExt.Toint(nonActiveDays);
                        data_MemberLevel.UpdateUsePoint = ObjectExt.Toint(updateUsePoint);
                        data_MemberLevel.FreeRate = ObjectExt.Toint(freeRate);
                        data_MemberLevel.FreeCoin = ObjectExt.Toint(freeCoin);
                        data_MemberLevel.FreeType = ObjectExt.Toint(freeType);
                        data_MemberLevel.FreeNeedWin = ObjectExt.Toint(freeNeedWin);
                        data_MemberLevel.BirthdayFree = ObjectExt.Toint(birthdayFree);
                        data_MemberLevel.BirthdayFreeCoin = ObjectExt.Toint(birthdayFreeCoin);
                        data_MemberLevel.MinCoin = ObjectExt.Toint(minCoin);
                        data_MemberLevel.MaxCoin = ObjectExt.Toint(maxCoin);
                        data_MemberLevel.AllowExitCard = ObjectExt.Toint(allowExitCard);
                        data_MemberLevel.AllowExitMoney = ObjectExt.Toint(allowExitMoney);
                        data_MemberLevel.AllowExitCoinToCard = ObjectExt.Toint(allowExitCoinToCard);
                        data_MemberLevel.LockHead = ObjectExt.Toint(lockHead);

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
                            IData_MemberLevel_FoodService data_MemberLevel_FoodService = Data_MemberLevel_FoodBusiness.Instance;
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

                        //保存预赠币规则
                        if (memberLevelFrees != null && memberLevelFrees.Count() >= 0)
                        {
                            //先删除，后添加
                            IData_MemberLevelFreeService data_MemberLevelFreeService = Data_MemberLevelFreeBusiness.Instance;
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
                IData_MemberLevelService data_MemberLevelService = Data_MemberLevelBusiness.Instance;
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

                IData_MemberLevel_FoodService data_MemberLevel_FoodService = Data_MemberLevel_FoodBusiness.NewInstance;
                IData_FoodInfoService data_FoodInfoService = Data_FoodInfoBusiness.NewInstance;                
                var linq = from a in data_MemberLevel_FoodService.GetModels(p => p.MemberLevelID == iMemberLevelID)
                           join b in data_FoodInfoService.GetModels(p => p.FoodState == 1 && p.FoodType == (int)FoodType.Member) on a.FoodID equals b.FoodID
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
                IData_MemberLevelFreeService data_MemberLevelFreeService = Data_MemberLevelFreeBusiness.Instance;
                var linq = data_MemberLevelFreeService.GetModels(p => p.MemberLevelID == iMemberLevelID);
                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, linq);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object UploadCardCover(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;

                #region 验证参数

                var file = HttpContext.Current.Request.Files[0];
                if (file == null)
                {
                    errMsg = "未找到图片";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (file.ContentLength > int.Parse(System.Configuration.ConfigurationManager.AppSettings["MaxImageSize"].ToString()))
                {
                    errMsg = "超过图片的最大限制为1M";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                #endregion

                string picturePath = System.Configuration.ConfigurationManager.AppSettings["UploadImageUrl"].ToString() + "/XCCloud/Store/Member/";
                string path = System.Web.HttpContext.Current.Server.MapPath(picturePath);
                //如果不存在就创建file文件夹
                if (Directory.Exists(path) == false)
                {
                    Directory.CreateDirectory(path);
                }

                string fileName = Path.GetFileNameWithoutExtension(file.FileName) + Utils.ConvertDateTimeToLong(DateTime.Now) + Path.GetExtension(file.FileName);

                if (File.Exists(path + fileName))
                {
                    errMsg = "图片名称已存在，请重命名后上传";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                file.SaveAs(path + fileName);

                Dictionary<string, string> dicStoreInfo = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                dicStoreInfo.Add("CardUIURL", picturePath + fileName);
                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, dicStoreInfo);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

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
                        IData_MemberLevelService data_MemberLevelService = Data_MemberLevelBusiness.Instance;
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

                        IData_MemberLevel_FoodService data_MemberLevel_FoodService = Data_MemberLevel_FoodBusiness.Instance;
                        foreach (var model in data_MemberLevel_FoodService.GetModels(p => p.MemberLevelID == iMemberLevelID))
                        {
                            data_MemberLevel_FoodService.DeleteModel(model);
                        }

                        if (!data_MemberLevel_FoodService.SaveChanges())
                        {
                            errMsg = "删除会员级别开卡套餐信息失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        IData_MemberLevelFreeService data_MemberLevelFreeService = Data_MemberLevelFreeBusiness.Instance;
                        foreach (var model in data_MemberLevelFreeService.GetModels(p => p.MemberLevelID == iMemberLevelID))
                        {
                            data_MemberLevelFreeService.DeleteModel(model);
                        }

                        if (!data_MemberLevelFreeService.SaveChanges())
                        {
                            errMsg = "删除会员级别预赠币规则信息失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

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
    }
}