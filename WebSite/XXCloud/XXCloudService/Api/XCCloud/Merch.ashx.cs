using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XCCloudService.Base;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;
using XCCloudService.BLL.XCCloud;
using XCCloudService.Common;
using XCCloudService.Model.CustomModel.XCCloud;
using XCCloudService.Model.XCCloud;
using System.Transactions;
using System.Data.SqlClient;
using XCCloudService.BLL.CommonBLL;
using XCCloudService.CacheService;
using XCCloudService.Business;
using XCCloudService.Model.WeiXin.Message;
using XCCloudService.WeiXin.Message;
using XCCloudService.Common.Enum;
using XCCloudService.WeiXin.WeixinOAuth;
using XCCloudService.DAL;
using System.Data.Entity;
using XCCloudService.DBService.BLL;
using XXCloudService.Api.XCCloud.Common;
using XCCloudService.Common.Extensions;

namespace XXCloudService.Api.XCCloud
{
    [Authorize(Roles = "XcUser,XcAdmin,MerchUser")]
    /// <summary>
    /// MerchInfo 的摘要说明
    /// </summary>
    public class Merch : ApiBase
    {
        private bool getAdcode(out string adcode, out string errMsg)
        {
            errMsg = string.Empty;
            adcode = string.Empty;

            //获取客户端IP地址
            var ip = Utils.GetRequestIpAddress();

            //解析维度、精度对应的地址信息
            string json = MapService.GetBaiduMapIPAnalysis(ip);
            IPAnalysisModel iPAnalysisModel = Utils.DataContractJsonDeserializer<IPAnalysisModel>(json);

            if (iPAnalysisModel == null || iPAnalysisModel.Status != 0)
            {
                errMsg = ip + "位置解析出错";
                return false;
            }

            string longitude = iPAnalysisModel.Content.Point.X;
            string latitude = iPAnalysisModel.Content.Point.Y;
            json = MapService.GetBaiduMapCoordinateAnalysis(latitude, longitude);
            CoordinateAnalysisModel coordinateAnalysisModel = Utils.DataContractJsonDeserializer<CoordinateAnalysisModel>(json);
            if (coordinateAnalysisModel == null || coordinateAnalysisModel.Status != 0)
            {
                errMsg = "经度：" + longitude + " 纬度：" + latitude + "位置解析出错";
                return false;
            }

            //获取行政区划代码
            adcode = coordinateAnalysisModel.Result.AddressComponent.Adcode.PadLeft(6, '0');

            //行政区划代码是否存在
            IDict_AreaService dict_AreaService = new Dict_AreaService();
            var ac = adcode;
            var dict_AreaModel = dict_AreaService.GetModels(p => p.ID.ToString().Equals(ac, StringComparison.OrdinalIgnoreCase)).FirstOrDefault<Dict_Area>();
            if (dict_AreaModel == null)
            {
                errMsg = adcode + "行政区划代码无效";
                return false;
            }

            return true;
        }      

        private bool genNo(out string merchId, out string errMsg)
        {
            errMsg = string.Empty;
            merchId = string.Empty;

            string sql = "select max(MerchID) from Base_MerchantInfo where MerchID >= '100010'";
            string lastMerchID = XCCloudBLL.ExecuteScalar(sql);
            var bNo = 0;
            int.TryParse(lastMerchID, out bNo);
            if (bNo >= 899989)
            {
                errMsg = "商户数量已满额，系统最多支持899989个用户";
                return false;
            }

            if (bNo == 0)
            {
                bNo = 100010;
            }
            else
            {
                bNo = bNo + 1;
            }

            merchId = bNo.ToString();
            return true;
        }        

        private void NewPasswordMessagePush(string openId, string userName, string password)
        {
            string errMsg = string.Empty;
            MerchNewPasswordDataModel dataModel = new MerchNewPasswordDataModel(userName, password);
            if (MessageMana.PushMessage(WeiXinMesageType.MerchNewPassword, openId, dataModel, out errMsg))
            {
                LogHelper.SaveLog(TxtLogType.WeiXin, TxtLogContentType.Common, TxtLogFileType.Day, "true");
            }
            else
            {
                LogHelper.SaveLog(TxtLogType.WeiXin, TxtLogContentType.Common, TxtLogFileType.Day, errMsg);
            }
        }

        private void ResetPasswordMessagePush(string openId, string userName, string password)
        {
            string errMsg = string.Empty;
            MerchResetPasswordDataModel dataModel = new MerchResetPasswordDataModel(userName, password);
            if (MessageMana.PushMessage(WeiXinMesageType.MerchResetPassword, openId, dataModel, out errMsg))
            {
                LogHelper.SaveLog(TxtLogType.WeiXin, TxtLogContentType.Common, TxtLogFileType.Day, "true");
            }
            else
            {
                LogHelper.SaveLog(TxtLogType.WeiXin, TxtLogContentType.Common, TxtLogFileType.Day, errMsg);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetMerchList(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                object[] conditions = dicParas.ContainsKey("conditions") ? (object[])dicParas["conditions"] : null;
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];

                //代理商仅能查所属商户，普通商户或大客户只能查自己，莘宸管理员可以查所有商户
                SqlParameter[] parameters = new SqlParameter[0];
                string sqlWhere = string.Empty;
                if (userTokenKeyModel.LogType == (int)RoleType.MerchUser)
                {
                    var merchIdDataModel = userTokenKeyModel.DataModel as TokenDataModel;
                    Array.Resize(ref parameters, parameters.Length + 1);                    
                    if (merchIdDataModel.MerchType == (int)MerchType.Agent)
                    {
                        parameters[parameters.Length - 1] = new SqlParameter("@logId", userTokenKeyModel.LogId);
                        sqlWhere = sqlWhere + " and a.CreateUserID=@logId";
                    }
                    else
                    {
                        parameters[parameters.Length - 1] = new SqlParameter("@merchId", merchIdDataModel.MerchID);
                        sqlWhere = sqlWhere + " and a.MerchID=@merchId";
                    }
                }

                if (conditions != null && conditions.Length > 0)
                {
                    if (!QueryBLL.GenDynamicSql(conditions, "a.", ref sqlWhere, ref parameters, out errMsg))
                    {
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                }

                string sql = @"select a.MerchID, a.MerchName, a.Mobil, b.DictKey as MerchTypeStr, a.MerchAccount, d.DictKey as AllowCreateSubStr, a.AllowCreateCount, c.DictKey as MerchStatusStr from Base_MerchantInfo a " +
                    " left join (select b.* from Dict_System a inner join Dict_System b on a.ID=b.PID where a.DictKey='商户类别' and a.PID=0) b on convert(varchar, a.MerchType)=b.DictValue " +
                    " left join (select b.* from Dict_System a inner join Dict_System b on a.ID=b.PID where a.DictKey='商户状态' and a.PID=0) c on convert(varchar, a.MerchStatus)=c.DictValue " +
                    " left join (select b.* from Dict_System a inner join Dict_System b on a.ID=b.PID where a.DictKey='创建子账号' and a.PID=0) d on convert(varchar, a.AllowCreateSub)=d.DictValue " +
                    " where 1=1 ";
                sql = sql + sqlWhere;
                var dbContext = DbContextFactory.CreateByModelNamespace(typeof(Base_MerchantInfo).Namespace);
                var base_MerchantInfo = dbContext.Database.SqlQuery<Base_MerchantInfoListModel>(sql, parameters).ToList();

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, base_MerchantInfo);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object AddMerch(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string merchId = string.Empty;
                string merchType = dicParas.ContainsKey("merchType") ? dicParas["merchType"].ToString() : string.Empty;
                string merchTag = dicParas.ContainsKey("merchTag") ? dicParas["merchTag"].ToString() : string.Empty; 
                string merchStatus = dicParas.ContainsKey("merchStatus") ? dicParas["merchStatus"].ToString() : string.Empty;
                string merchAccount = dicParas.ContainsKey("merchAccount") ? dicParas["merchAccount"].ToString() : string.Empty;
                string merchName = dicParas.ContainsKey("merchName") ? dicParas["merchName"].ToString() : string.Empty;
                string openId = dicParas.ContainsKey("openId") ? dicParas["openId"].ToString() : string.Empty;
                string unionId = dicParas.ContainsKey("unionId") ? dicParas["unionId"].ToString() : string.Empty;
                string mobil = dicParas.ContainsKey("mobil") ? dicParas["mobil"].ToString() : string.Empty;
                string allowCreateSub = dicParas.ContainsKey("allowCreateSub") ? dicParas["allowCreateSub"].ToString() : string.Empty;
                string allowCreateCount = dicParas.ContainsKey("allowCreateCount") ? dicParas["allowCreateCount"].ToString() : string.Empty;
                string comment = dicParas.ContainsKey("comment") ? dicParas["comment"].ToString() : string.Empty;
                object[] merchFunction = dicParas.ContainsKey("merchFunction") ? (object[])dicParas["merchFunction"] : null;
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string createUserId = userTokenKeyModel.LogId;
                int logType = userTokenKeyModel.LogType;                
                
                #region 验证参数
                
                if (!string.IsNullOrEmpty(merchId) && merchId.Length > 6)
                {
                    errMsg = "商户编号不能超过6个字符";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (string.IsNullOrEmpty(merchType))
                {
                    errMsg = "商户类型不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (!Utils.isNumber(merchType))
                {
                    errMsg = "商户类别不是Int类型";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (string.IsNullOrEmpty(merchStatus))
                {
                    errMsg = "商户状态不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (!Utils.isNumber(merchStatus))
                {
                    errMsg = "商户状态不是Int类型";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (string.IsNullOrEmpty(merchTag))
                {
                    errMsg = "商户标签不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (!Utils.isNumber(merchTag))
                {
                    errMsg = "商户标签不是Int类型";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (string.IsNullOrWhiteSpace(merchAccount))
                {
                    errMsg = "商户账号不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (merchAccount.Length > 100)
                {
                    errMsg = "商户账号不能超过100个字符";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (string.IsNullOrWhiteSpace(openId))
                {
                    errMsg = "请选择微信昵称";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (string.IsNullOrWhiteSpace(merchName))
                {
                    errMsg = "负责人名称不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (merchName.Length > 50)
                {
                    errMsg = "负责人名称不能超过50个字符";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }                

                if (string.IsNullOrWhiteSpace(mobil))
                {
                    errMsg = "手机号不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }
                                                
                if (!Utils.CheckMobile(mobil))
                {
                    errMsg = "手机号不正确";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (!string.IsNullOrEmpty(allowCreateSub) && !Utils.isNumber(allowCreateSub))
                {
                    errMsg = "是否允许创建子账号不是Int类型";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (!string.IsNullOrEmpty(allowCreateCount) && !Utils.isNumber(allowCreateCount))
                {
                    errMsg = "账号数量不是Int类型";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (!string.IsNullOrEmpty(comment) && comment.Length > 500)
                {
                    errMsg = "备注不能超过500个字符";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }
                
                //获取用户基本信息
                if (string.IsNullOrEmpty(unionId))
                {
                    if (!TokenMana.GetUnionId(openId, out unionId, out errMsg))
                    {
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                }                

                //生成商户编号
                if (!genNo(out merchId, out errMsg))
                {
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }   
                
                //生成商户密码
                string pwd = Utils.GetCheckCode(6);

                #endregion
                                             
                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        var merchPassword = Utils.MD5(pwd);
                        IBase_MerchantInfoService base_MerchantInfoService = BLLContainer.Resolve<IBase_MerchantInfoService>();
                        if (base_MerchantInfoService.Any(p => !p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && p.MerchAccount.Equals(merchAccount, StringComparison.OrdinalIgnoreCase)))
                        {
                            errMsg = "账号名称已使用";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }
                        
                        var base_MerchantInfo = new Base_MerchantInfo();
                        base_MerchantInfo.MerchID = merchId;
                        base_MerchantInfo.MerchType = Convert.ToInt32(merchType);
                        base_MerchantInfo.MerchStatus = Convert.ToInt32(merchStatus);
                        base_MerchantInfo.MerchAccount = merchAccount;
                        base_MerchantInfo.MerchName = merchName;
                        base_MerchantInfo.Mobil = mobil;
                        base_MerchantInfo.WxOpenID = openId;
                        base_MerchantInfo.WxUnionID = unionId;
                        base_MerchantInfo.MerchPassword = merchPassword;
                        base_MerchantInfo.AllowCreateSub = ObjectExt.Toint(allowCreateSub);
                        base_MerchantInfo.AllowCreateCount = ObjectExt.Toint(allowCreateCount);
                        base_MerchantInfo.CreateUserID = (logType == (int)RoleType.XcUser || logType == (int)RoleType.XcAdmin) ? createUserId : 
                                                            logType == (int)RoleType.MerchUser ? (userTokenKeyModel.DataModel as TokenDataModel).MerchID : string.Empty;
                        base_MerchantInfo.CreateType = (logType == (int)RoleType.XcUser || logType == (int)RoleType.XcAdmin) ? (int)CreateType.Xc : 
                                                            logType == (int)RoleType.MerchUser ? (int)CreateType.Agent : 0;
                        base_MerchantInfo.Comment = comment;
                        base_MerchantInfo.MerchTag = Convert.ToInt32(merchTag);

                        if (!base_MerchantInfoService.Add(base_MerchantInfo))
                        {
                            errMsg = "添加商户信息失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        IBase_UserInfoService base_UserInfoService = BLLContainer.Resolve<IBase_UserInfoService>();
                        if (base_UserInfoService.Any(p => p.LogName.Equals(merchAccount, StringComparison.OrdinalIgnoreCase)))
                        {
                            errMsg = "账号名称已使用";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        var base_UserInfo = new Base_UserInfo();
                        base_UserInfo.LogName = merchAccount;
                        base_UserInfo.RealName = merchName;
                        base_UserInfo.MerchID = merchId;
                        base_UserInfo.OpenID = openId;
                        base_UserInfo.UnionID = unionId;
                        base_UserInfo.LogPassword = merchPassword;
                        base_UserInfo.UserType = Convert.ToInt32(merchType);
                        base_UserInfo.SwitchMerch = 1;
                        base_UserInfo.SwitchStore = 1;
                        base_UserInfo.SwitchWorkstation = 1;
                        if (!base_UserInfoService.Add(base_UserInfo))
                        {
                            errMsg = "添加商户负责人信息失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        if (merchFunction != null && merchFunction.Count() > 0)
                        {
                            var dbContext = DbContextFactory.CreateByModelNamespace(typeof(Base_MerchFunction).Namespace);
                            foreach (IDictionary<string, object> el in merchFunction)
                            {
                                if (el != null)
                                {
                                    var dicPara = new Dictionary<string, object>(el, StringComparer.OrdinalIgnoreCase);
                                    string functionId = dicPara.ContainsKey("functionId") ? dicPara["functionId"].ToString() : string.Empty;
                                    string functionEn = dicPara.ContainsKey("functionEn") ? dicPara["functionEn"].ToString() : string.Empty;
                                    if (string.IsNullOrEmpty(functionId))
                                    {
                                        errMsg = "功能编号不能为空";
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    }
                                    if (!Utils.isNumber(functionId))
                                    {
                                        errMsg = "功能编号不是Int类型";
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    }
                                    if (!string.IsNullOrEmpty(functionEn) && !Utils.isNumber(functionEn))
                                    {
                                        errMsg = "功能启停不是Int类型";
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    }
                                    var base_MerchFunction = new Base_MerchFunction();
                                    base_MerchFunction.MerchID = merchId;
                                    base_MerchFunction.FunctionID = Convert.ToInt32(functionId);
                                    base_MerchFunction.FunctionEN = !string.IsNullOrEmpty(functionEn) ? Convert.ToInt32(functionEn) : default(int?);
                                    dbContext.Entry(base_MerchFunction).State = EntityState.Added;
                                }
                                else
                                {
                                    errMsg = "提交数据包含空对象";
                                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                }
                            }

                            if (dbContext.SaveChanges() < 0)
                            {
                                errMsg = "保存商户功能菜单信息失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }

                        #region 初始化连锁店余额通用设置
                        //初始化连锁店余额通用设置
                        var base_ChainRuleContext = DbContextFactory.CreateByModelNamespace(typeof(Base_ChainRule).Namespace);
                        if (base_ChainRuleContext.Set<Base_ChainRule>().Any(a => a.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase)))
                        {
                            errMsg = "初始化连锁店余额通用设置异常，该商户ID已存在";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }
                        foreach (ChainStoreRuleType item in Enum.GetValues(typeof(ChainStoreRuleType)))
                        {
                            var base_ChainRuleModel = new Base_ChainRule();
                            base_ChainRuleModel.GroupName = item.ToString() + "余额通用";
                            base_ChainRuleModel.MerchID = merchId;
                            base_ChainRuleModel.RuleType = (int)item;
                            base_ChainRuleContext.Entry(base_ChainRuleModel).State = EntityState.Added;
                        }
                        if (base_ChainRuleContext.SaveChanges() < 0)
                        {
                            errMsg = "初始化连锁店余额通用设置失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }
                        #endregion

                        ts.Complete();
                    }
                    catch (Exception ex)
                    {
                        errMsg = ex.Message;
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }                    
                }
                
                //发送消息密码到商户负责人微信公众号，消息模板“莘宸商户注册密码”
                NewPasswordMessagePush(openId, merchAccount, pwd);

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object EditMerch(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string merchId = dicParas.ContainsKey("merchId") ? dicParas["merchId"].ToString() : string.Empty;
                string merchType = dicParas.ContainsKey("merchType") ? dicParas["merchType"].ToString() : string.Empty;
                string merchTag = dicParas.ContainsKey("merchTag") ? dicParas["merchTag"].ToString() : string.Empty; 
                string merchStatus = dicParas.ContainsKey("merchStatus") ? dicParas["merchStatus"].ToString() : string.Empty;
                string merchAccount = dicParas.ContainsKey("merchAccount") ? dicParas["merchAccount"].ToString() : string.Empty;
                string merchName = dicParas.ContainsKey("merchName") ? dicParas["merchName"].ToString() : string.Empty;
                string openId = dicParas.ContainsKey("openId") ? dicParas["openId"].ToString() : string.Empty;
                string unionId = dicParas.ContainsKey("unionId") ? dicParas["unionId"].ToString() : string.Empty;
                string mobil = dicParas.ContainsKey("mobil") ? dicParas["mobil"].ToString() : string.Empty;
                string allowCreateSub = dicParas.ContainsKey("allowCreateSub") ? dicParas["allowCreateSub"].ToString() : string.Empty;
                string allowCreateCount = dicParas.ContainsKey("allowCreateCount") ? dicParas["allowCreateCount"].ToString() : string.Empty;
                string comment = dicParas.ContainsKey("comment") ? dicParas["comment"].ToString() : string.Empty;
                object[] merchFunction = dicParas.ContainsKey("merchFunction") ? (object[])dicParas["merchFunction"] : null;
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string createUserId = userTokenKeyModel.LogId;
                int logType = userTokenKeyModel.LogType;                

                #region 验证参数

                if (string.IsNullOrEmpty(merchId))
                {
                    errMsg = "商户编号不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (string.IsNullOrWhiteSpace(merchType))
                {
                    errMsg = "商户类型不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (!Utils.isNumber(merchType))
                {
                    errMsg = "商户类别不是Int类型";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (string.IsNullOrWhiteSpace(merchStatus))
                {
                    errMsg = "商户状态不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (!Utils.isNumber(merchStatus))
                {
                    errMsg = "商户状态不是Int类型";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (string.IsNullOrEmpty(merchTag))
                {
                    errMsg = "商户标签不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (!Utils.isNumber(merchTag))
                {
                    errMsg = "商户标签不是Int类型";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (string.IsNullOrWhiteSpace(merchAccount))
                {
                    errMsg = "商户账号不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (merchAccount.Length > 100)
                {
                    errMsg = "商户账号不能超过100个字符";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }                

                if (string.IsNullOrWhiteSpace(openId))
                {
                    errMsg = "请选择微信昵称";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (string.IsNullOrWhiteSpace(merchName))
                {
                    errMsg = "负责人名称不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (merchName.Length > 50)
                {
                    errMsg = "负责人名称不能超过50个字符";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (string.IsNullOrWhiteSpace(mobil))
                {
                    errMsg = "手机号不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (!Utils.CheckMobile(mobil))
                {
                    errMsg = "手机号不正确";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }
                                                                
                if (!string.IsNullOrEmpty(allowCreateSub) && !Utils.isNumber(allowCreateSub))
                {
                    errMsg = "是否允许创建子账号不是Int类型";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (!string.IsNullOrEmpty(allowCreateCount) && !Utils.isNumber(allowCreateCount))
                {
                    errMsg = "账号数量不是Int类型";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (!string.IsNullOrEmpty(comment) && comment.Length > 500)
                {
                    errMsg = "备注不能超过500个字符";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                //获取用户基本信息
                if (string.IsNullOrEmpty(unionId))
                {
                    if (!TokenMana.GetUnionId(openId, out unionId, out errMsg))
                    {
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                }
                                
                #endregion

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        IBase_MerchantInfoService base_MerchantInfoService = BLLContainer.Resolve<IBase_MerchantInfoService>();
                        if (base_MerchantInfoService.Any(p => !p.MerchID.Equals(merchId) && p.MerchAccount.Equals(merchAccount, StringComparison.OrdinalIgnoreCase)))
                        {
                            errMsg = "账号名称已使用";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        var base_MerchantInfo = base_MerchantInfoService.GetModels(p => p.MerchID.Equals(merchId)).FirstOrDefault();
                        var merchAccount_old = base_MerchantInfo.MerchAccount;
                        IBase_UserInfoService base_UserInfoService = BLLContainer.Resolve<IBase_UserInfoService>();                       
                        if (base_UserInfoService.Any(p => p.LogName.Equals(merchAccount_old, StringComparison.OrdinalIgnoreCase)))
                        {
                            if (base_UserInfoService.Any(p => !p.LogName.Equals(merchAccount_old, StringComparison.OrdinalIgnoreCase) && p.LogName.Equals(merchAccount, StringComparison.OrdinalIgnoreCase)))
                            {
                                errMsg = "商户负责人名称已注册";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            var base_UserInfo = base_UserInfoService.GetModels(p => p.LogName.Equals(merchAccount_old, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                            base_UserInfo.LogName = merchAccount;
                            if (!base_UserInfoService.Update(base_UserInfo))
                            {
                                errMsg = "修改商户负责人信息失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }
                        else
                        {
                            if (base_UserInfoService.Any(p => p.LogName.Equals(merchAccount, StringComparison.OrdinalIgnoreCase)))
                            {
                                errMsg = "商户负责人名称已注册";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            var base_UserInfo = new Base_UserInfo();
                            base_UserInfo.LogName = merchAccount;
                            base_UserInfo.RealName = merchName;
                            base_UserInfo.MerchID = merchId;
                            base_UserInfo.OpenID = openId;
                            base_UserInfo.UnionID = unionId;
                            base_UserInfo.LogPassword = base_MerchantInfo.MerchPassword;
                            base_UserInfo.UserType = Convert.ToInt32(merchType);
                            base_UserInfo.SwitchMerch = 1;
                            base_UserInfo.SwitchStore = 1;
                            base_UserInfo.SwitchWorkstation = 1;
                            if (!base_UserInfoService.Add(base_UserInfo))
                            {
                                errMsg = "添加商户负责人信息失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }
                        
                        base_MerchantInfo.MerchType = Convert.ToInt32(merchType);
                        base_MerchantInfo.MerchStatus = Convert.ToInt32(merchStatus);
                        base_MerchantInfo.MerchAccount = merchAccount;
                        base_MerchantInfo.MerchName = merchName;
                        base_MerchantInfo.Mobil = mobil;
                        base_MerchantInfo.WxOpenID = openId;
                        base_MerchantInfo.WxUnionID = unionId;
                        base_MerchantInfo.AllowCreateSub = ObjectExt.Toint(allowCreateSub);
                        base_MerchantInfo.AllowCreateCount = ObjectExt.Toint(allowCreateCount);
                        base_MerchantInfo.CreateUserID = (logType == (int)RoleType.XcUser || logType == (int)RoleType.XcAdmin) ? createUserId :
                                                            logType == (int)RoleType.MerchUser ? (userTokenKeyModel.DataModel as TokenDataModel).MerchID : string.Empty;
                        base_MerchantInfo.CreateType = (logType == (int)RoleType.XcUser || logType == (int)RoleType.XcAdmin) ? (int)CreateType.Xc :
                                                            logType == (int)RoleType.MerchUser ? (int)CreateType.Agent : 0;
                        base_MerchantInfo.Comment = comment;
                        base_MerchantInfo.MerchTag = Convert.ToInt32(merchTag);
                        if (!base_MerchantInfoService.Update(base_MerchantInfo))
                        {
                            errMsg = "修改商户信息失败";
                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                        }

                        if (merchFunction != null && merchFunction.Count() >= 0)
                        {
                            //先删除已有数据，后添加
                            var dbContext = DbContextFactory.CreateByModelNamespace(typeof(Base_MerchFunction).Namespace);
                            var base_MerchFunctionList = dbContext.Set<Base_MerchFunction>().Where(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase)).ToList();
                            foreach (var base_MerchFunction in base_MerchFunctionList)
                            {
                                dbContext.Entry(base_MerchFunction).State = EntityState.Deleted;
                            }


                            foreach (IDictionary<string, object> el in merchFunction)
                            {
                                if (el != null)
                                {
                                    var dicPara = new Dictionary<string, object>(el, StringComparer.OrdinalIgnoreCase);
                                    string functionId = dicPara.ContainsKey("functionId") ? dicPara["functionId"].ToString() : string.Empty;
                                    string functionEn = dicPara.ContainsKey("functionEn") ? dicPara["functionEn"].ToString() : string.Empty;
                                    if (string.IsNullOrEmpty(functionId))
                                    {
                                        errMsg = "功能编号不能为空";
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    }
                                    if (!Utils.isNumber(functionId))
                                    {
                                        errMsg = "功能编号不是Int类型";
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    }
                                    if (!string.IsNullOrEmpty(functionEn) && !Utils.isNumber(functionEn))
                                    {
                                        errMsg = "功能启停不是Int类型";
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    }
                                    var base_MerchFunction = new Base_MerchFunction();
                                    base_MerchFunction.MerchID = merchId;
                                    base_MerchFunction.FunctionID = Convert.ToInt32(functionId);
                                    base_MerchFunction.FunctionEN = !string.IsNullOrEmpty(functionEn) ? Convert.ToInt32(functionEn) : default(int?);
                                    dbContext.Entry(base_MerchFunction).State = EntityState.Added;
                                }
                                else
                                {
                                    errMsg = "提交数据包含空对象";
                                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                }
                            }

                            if (dbContext.SaveChanges() < 0)
                            {
                                errMsg = "保存商户功能菜单信息失败";
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

        [Authorize(Merches = "Agent")]
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]        
        public object DelMerch(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string merchId = dicParas.ContainsKey("merchId") ? dicParas["merchId"].ToString() : string.Empty;                
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string createUserId = userTokenKeyModel.LogId;
                
                #region 验证参数

                if (string.IsNullOrWhiteSpace(merchId))
                {
                    errMsg = "商户编号不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                if (!string.IsNullOrEmpty(merchId) && merchId.Length > 11)
                {
                    errMsg = "商户编号不能超过11个字符";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                #endregion
                
                IBase_MerchantInfoService base_MerchantInfoService = BLLContainer.Resolve<IBase_MerchantInfoService>();
                if (base_MerchantInfoService.Any(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase)))
                {
                    var base_MerchantInfoModel = base_MerchantInfoService.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    if (base_MerchantInfoModel.CreateType == (int)CreateType.Agent && !base_MerchantInfoModel.CreateUserID.Equals(createUserId, StringComparison.OrdinalIgnoreCase)) //代理商创建
                    {
                        errMsg = "该商户只有所属代理商能删除";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }

                    base_MerchantInfoModel.MerchStatus = (int)MerchState.Stop;
                    if (!base_MerchantInfoService.Update(base_MerchantInfoModel))
                    {
                        errMsg = "删除商户信息失败";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                }
                else
                {
                    errMsg = "该商户信息不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }
                   
                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetMerchInfo(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string merchId = dicParas.ContainsKey("merchId") ? dicParas["merchId"].ToString() : string.Empty;
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];

                #region 商户信息和功能菜单

                //返回商户信息
                string sql = "select * from Base_MerchantInfo where MerchID=@MerchID";
                SqlParameter[] parameters = new SqlParameter[1];
                if (string.IsNullOrEmpty(merchId))
                {
                    
                    parameters[0] = new SqlParameter("@MerchID", (userTokenKeyModel.DataModel as TokenDataModel).MerchID);
                }
                else
                {
                    parameters[0] = new SqlParameter("@MerchID", merchId);
                }

                System.Data.DataSet ds = XCCloudBLL.ExecuteQuerySentence(sql, parameters);
                if (ds.Tables.Count != 1)
                {
                    errMsg = "获取数据异常";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                //返回功能菜单信息
                var base_MerchInfoModel = Utils.GetModelList<Base_MerchInfoModel>(ds.Tables[0]).FirstOrDefault() ?? new Base_MerchInfoModel();
                sql = " exec  SelectMerchFunction @MerchID";
                parameters = new SqlParameter[1];
                if (string.IsNullOrEmpty(merchId))
                {
                    parameters[0] = new SqlParameter("@MerchID", (userTokenKeyModel.DataModel as TokenDataModel).MerchID);
                }
                else
                {
                    parameters[0] = new SqlParameter("@MerchID", merchId);
                }
                                                
                ds = XCCloudBLL.ExecuteQuerySentence(sql, parameters);
                if (ds.Tables.Count != 1)
                {
                    errMsg = "获取数据异常";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                //实例化一个根节点
                Base_MerchFunctionModel rootRoot = new Base_MerchFunctionModel();
                rootRoot.ParentID = 0;
                TreeHelper.LoopToAppendChildren(Utils.GetModelList<Base_MerchFunctionModel>(ds.Tables[0]), rootRoot);
                base_MerchInfoModel.MerchFunction = rootRoot.Children;

                #endregion

                if (string.IsNullOrEmpty(merchId))
                {
                    #region 商户类型列表

                    sql = " exec  SP_DictionaryNodes @MerchID,@DictKey,@RootID output ";
                    parameters = new SqlParameter[3];
                    parameters[0] = new SqlParameter("@MerchID", merchId);
                    parameters[1] = new SqlParameter("@DictKey", "商户类别");
                    parameters[2] = new SqlParameter("@RootID", 0);
                    parameters[2].Direction = System.Data.ParameterDirection.Output;
                    ds = XCCloudBLL.ExecuteQuerySentence(sql, parameters);
                    if (ds.Tables.Count == 0)
                    {
                        errMsg = "没有找到节点信息";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }

                    var dictionaryResponse = Utils.GetModelList<DictionaryResponseModel>(ds.Tables[0]);

                    //代理商不能创建代理商
                    if (base_MerchInfoModel.MerchType == (int)MerchType.Agent)
                    {
                        dictionaryResponse = dictionaryResponse.Where(p => !p.DictValue.Equals(Convert.ToString(MerchType.Agent), StringComparison.OrdinalIgnoreCase)).ToList();
                    }

                    //实例化一个根节点
                    int rootId = 0;
                    int.TryParse(parameters[2].Value.ToString(), out rootId);
                    var rootRoot2 = new DictionaryResponseModel();
                    rootRoot2.ID = rootId;
                    TreeHelper.LoopToAppendChildren(dictionaryResponse, rootRoot2);
                    base_MerchInfoModel.MerchTypes = rootRoot2.Children;

                    #endregion                    
                }               

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, base_MerchInfoModel);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetMerchAlipay(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;                
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = dicParas.ContainsKey("merchId") ? dicParas["merchId"].ToString() : string.Empty;
                if (userTokenKeyModel.LogType == (int)RoleType.MerchUser)
                {
                    merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;
                }
                else
                {
                    if (string.IsNullOrEmpty(merchId))
                    {
                        errMsg = "商户ID参数不能为空";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                }

                IBase_StoreInfoService base_StoreInfoService = BLLContainer.Resolve<IBase_StoreInfoService>();
                IData_MerchAlipay_ShopService data_MerchAlipay_ShopService = BLLContainer.Resolve<IData_MerchAlipay_ShopService>();
                IBase_MerchAlipayService base_MerchAlipayService = BLLContainer.Resolve<IBase_MerchAlipayService>();
                if (!base_MerchAlipayService.Any(a => a.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase)))
                {
                    errMsg = "该商户支付宝口碑接口不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                var StoreShops = from a in data_MerchAlipay_ShopService.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase)) 
                                 join b in base_StoreInfoService.GetModels() on a.StoreID equals b.StoreID
                                 select new {
                                    StoreID = a.StoreID,
                                    StoreName = b.StoreName,
                                    ShopID = a.ShopID
                                 };
                var base_MerchAlipay = base_MerchAlipayService.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                var linq = new
                {
                    AppID = base_MerchAlipay.AppID,
                    PrivateKey = base_MerchAlipay.PrivateKey,
                    PublicKey = base_MerchAlipay.PublicKey,
                    Fee = base_MerchAlipay.Fee,
                    StoreShops = StoreShops
                };
                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, linq);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object SaveMerchAlipay(Dictionary<string, object> dicParas)
        {
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];
                string merchId = (userTokenKeyModel.DataModel as TokenDataModel).MerchID;

                string errMsg = string.Empty;
                string appId = dicParas.ContainsKey("appId") ? Convert.ToString(dicParas["appId"]) : string.Empty;                
                string privateKey = dicParas.ContainsKey("privateKey") ? Convert.ToString(dicParas["privateKey"]) : string.Empty;
                string publicKey = dicParas.ContainsKey("publicKey") ? Convert.ToString(dicParas["publicKey"]) : string.Empty;
                string fee = dicParas.ContainsKey("fee") ? Convert.ToString(dicParas["fee"]) : string.Empty;
                object[] storeShops = dicParas.ContainsKey("storeShops") ? (object[])dicParas["storeShops"] : null;

                #region 参数验证
                if(string.IsNullOrEmpty(appId))
                {
                    errMsg = "应用ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }                
                if (string.IsNullOrEmpty(privateKey))
                {
                    errMsg = "私钥不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }
                if (string.IsNullOrEmpty(publicKey))
                {
                    errMsg = "公钥不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }
                if (string.IsNullOrEmpty(fee))
                {
                    errMsg = "交易费率不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }
                if (!Utils.IsDecimal(fee))
                {
                    errMsg = "交易费率格式不正确";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }
                #endregion

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        IBase_MerchAlipayService base_MerchAlipayService = BLLContainer.Resolve<IBase_MerchAlipayService>();
                        var base_MerchAlipay = base_MerchAlipayService.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase)).FirstOrDefault() ?? new Base_MerchAlipay();
                        base_MerchAlipay.AppID = appId;
                        base_MerchAlipay.PrivateKey = privateKey;
                        base_MerchAlipay.PublicKey = publicKey;
                        base_MerchAlipay.Fee = ObjectExt.Todecimal(fee);                        
                        if (base_MerchAlipay.ID == 0)
                        {
                            base_MerchAlipay.MerchID = merchId;
                            if (!base_MerchAlipayService.Add(base_MerchAlipay))
                            {
                                errMsg = "添加商户支付宝口碑配置失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }
                        else
                        {
                            if (!base_MerchAlipayService.Update(base_MerchAlipay))
                            {
                                errMsg = "更新商户支付宝口碑配置失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }

                        if (storeShops != null && storeShops.Count() >= 0)
                        {
                            //先删除，后添加
                            IData_MerchAlipay_ShopService data_MerchAlipay_ShopService = BLLContainer.Resolve<IData_MerchAlipay_ShopService>();
                            foreach (var model in data_MerchAlipay_ShopService.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase)))
                            {
                                data_MerchAlipay_ShopService.DeleteModel(model);
                            }

                            foreach (IDictionary<string, object> el in storeShops)
                            {
                                if (el != null)
                                {
                                    var dicPara = new Dictionary<string, object>(el, StringComparer.OrdinalIgnoreCase);
                                    string storeId = dicPara.ContainsKey("storeId") ? (dicPara["storeId"] + "") : string.Empty;
                                    string shopId = dicPara.ContainsKey("shopId") ? (dicPara["shopId"] + "") : string.Empty;

                                    if (string.IsNullOrEmpty(storeId))
                                    {
                                        errMsg = "门店ID不能为空";
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    }
                                    if (string.IsNullOrEmpty(shopId))
                                    {
                                        errMsg = "口碑店号不能为空";
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    }
                                    
                                    var data_MerchAlipay_Shop = new Data_MerchAlipay_Shop();
                                    data_MerchAlipay_Shop.MerchID = merchId;
                                    data_MerchAlipay_Shop.StoreID = storeId;
                                    data_MerchAlipay_Shop.ShopID = shopId;
                                    data_MerchAlipay_ShopService.AddModel(data_MerchAlipay_Shop);
                                }
                                else
                                {
                                    errMsg = "提交数据包含空对象";
                                    return false;
                                }
                            }

                            if (!data_MerchAlipay_ShopService.SaveChanges())
                            {
                                errMsg = "保存口碑门店列表信息失败";
                                return false;
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
        public object ResetPassword(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                //string mobil = dicParas.ContainsKey("mobil") ? dicParas["mobil"].ToString() : string.Empty;

                //if (string.IsNullOrEmpty(mobil))
                //{
                //    errMsg = "手机号不能为空";
                //    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                //}

                //if (!Utils.CheckMobile(mobil))
                //{
                //    errMsg = "手机号不正确";
                //    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                //}

                //bool isSMSTest = bool.Parse(System.Configuration.ConfigurationManager.AppSettings["isSMSTest"].ToString());
                //string pwd = "123456";
                //if (!isSMSTest)
                //{
                //    //生成6位随机字母数字混合
                //    pwd = Utils.GetCheckCode(6);
                //    ResetPasswordCache.Add(mobil, pwd, CacheExpires.SMSCodeExpires);
                //    if (!SMSBusiness.SendSMSCode("3", mobil, pwd, out errMsg))
                //    {
                //        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                //    }
                //}
                //else
                //{
                //    ResetPasswordCache.Add(mobil, pwd, CacheExpires.SMSCodeExpires);
                //}

                string merchId = dicParas.ContainsKey("merchId") ? dicParas["merchId"].ToString() : string.Empty;
                if (string.IsNullOrEmpty(merchId))
                {
                    errMsg = "商户ID不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }    

                IBase_MerchantInfoService base_MerchantInfoService = BLLContainer.Resolve<IBase_MerchantInfoService>();
                if (!base_MerchantInfoService.Any(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase)))
                {
                    errMsg = "该商户不存在";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }
                                                            
                var base_MerchantInfoModel = base_MerchantInfoService.GetModels(p => p.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                string pwd = Utils.GetCheckCode(6);
                base_MerchantInfoModel.MerchPassword = Utils.MD5(pwd);
                if (!base_MerchantInfoService.Update(base_MerchantInfoModel))
                {
                    errMsg = "重置商户密码失败";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                //推送微信消息
                ResetPasswordMessagePush(base_MerchantInfoModel.WxOpenID, base_MerchantInfoModel.MerchAccount, pwd);
                                
                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }
    }
}