using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XCCloudService.Base;
using XCCloudService.DBService.Model;
using XCCloudService.DBService.BLL;
using XCCloudService.Common;
using XCCloudService.Common.Extensions;
using System.Data.Entity.Validation;
using XXCloudService.Api.XCCloud.Common;
using System.Transactions;
using XCCloudService.BLL.XCCloud;
using XCCloudService.Model.XCCloud;
using XCCloudService.Model.CustomModel.XCCloud;

namespace XCCloudService.Api
{
    /// <summary>
    /// Query 的摘要说明
    /// </summary>
    public class Query : ApiBase
    {
        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object init(Dictionary<string, object> dicParas)
        {
            XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];

            string pageName = string.Empty;
            string processName = string.Empty;
            int userId = 0;

            if (dicParas.ContainsKey("pagename"))
            {
                pageName = dicParas["pagename"].ToString();
            }

            if (dicParas.ContainsKey("processname"))
            {
                processName = dicParas["processname"].ToString();
            }

            if (dicParas.ContainsKey("userid"))
            {
                int.TryParse(userTokenKeyModel.LogId, out userId);
            }

            string errMsg = string.Empty;
            if (string.IsNullOrEmpty(pageName))
            {
                errMsg = "页面名参数不存在";
            }
            else if (string.IsNullOrEmpty(processName))
            {
                errMsg = "功能名参数不存在";
            }

            if (!string.IsNullOrEmpty(errMsg))
            {
                ResponseModel<List<InitModel>> responseModel = new ResponseModel<List<InitModel>>();
                responseModel.Result_Code = Result_Code.F;
                responseModel.Result_Msg = errMsg;
                return responseModel;        
            }
            else
            { 
                List<InitModel> listInitModel = null;
                List<Dict_SystemModel> listDict_SystemModel = null;
                QueryBLL.GetInit(pageName, processName, userId, ref listInitModel, ref listDict_SystemModel);
                ResponseModel<List<InitModel>> responseModel = new ResponseModel<List<InitModel>>();
                responseModel.Result_Data = listInitModel;
                return responseModel;                
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object save(Dictionary<string, object> dicParas)
        {                       
            try
            {
                XCCloudUserTokenModel userTokenKeyModel = (XCCloudUserTokenModel)dicParas[Constant.XCCloudUserTokenModel];

                string errMsg = string.Empty;                
                if (dicParas.GetArray("templateDetails").Validarray("模板条件列表", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var userId = userTokenKeyModel.LogId.Toint(0);
                var templateDetails = dicParas.GetArray("templateDetails");                

                //开启EF事务
                var search_TemplateService = Search_TemplateService.I;
                var search_Template_DetailService = Search_Template_DetailService.I;
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        if (templateDetails != null && templateDetails.Count() >= 0)
                        {
                            foreach (IDictionary<string, object> el in templateDetails)
                            {
                                if (el != null)
                                {
                                    var dicPara = new Dictionary<string, object>(el, StringComparer.OrdinalIgnoreCase);
                                    if (!dicPara.Get("pageName").Nonempty("查询页名", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPara.Get("processName").Nonempty("查询模块名", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPara.Get("fieldName").Nonempty("查询字段名", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPara.Get("title").Nonempty("查询标题", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPara.Get("dataType").Nonempty("查询字段类型", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPara.Get("condition").Validint("查询条件", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPara.Get("width").Validint("字段显示宽度", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPara.Get("showColume").Validint("是否显示列", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                    if (!dicPara.Get("showSearch").Validint("是否显示查询条件", out errMsg))
                                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                                    var id = dicPara.Get("id").Toint(0);
                                    var tempId = dicPara.Get("tempId").Toint(0);
                                    var pageName = dicPara.Get("pageName");
                                    var processName = dicPara.Get("processName");
                                    var fieldName = dicPara.Get("fieldName");
                                    var title = dicPara.Get("title");
                                    var dataType = dicPara.Get("dataType");
                                    var condition = dicPara.Get("condition").Toint();
                                    var width = dicPara.Get("width").Toint();
                                    var showColume = dicPara.Get("showColume").Toint();
                                    var showSearch = dicPara.Get("showSearch").Toint();
                                    var dictId = dicPara.Get("dictId").Toint();

                                    //先检查模板ID是否存在, 不存在则创建新用户查询模板
                                    if (tempId == 0)
                                    {
                                        var search_TemplateModel = search_TemplateService.GetModels(p => p.UserID == userId && p.PageName == pageName && p.ProcessName == processName).FirstOrDefault() ?? new Search_Template();
                                        if (search_TemplateModel.ID == 0)
                                        {
                                            search_TemplateModel.UserID = userId;
                                            search_TemplateModel.PageName = pageName;
                                            search_TemplateModel.ProcessName = processName;
                                            if (!search_TemplateService.Add(search_TemplateModel))
                                            {
                                                errMsg = "保存查询模板信息失败";
                                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                            }
                                        }
                                        
                                        tempId = search_TemplateModel.ID;
                                    }

                                    var search_Template_DetailModel = search_Template_DetailService.GetModels(p => p.ID == id).FirstOrDefault() ?? new Search_Template_Detail();
                                    search_Template_DetailModel.TempID = tempId;
                                    search_Template_DetailModel.FieldName = fieldName;
                                    search_Template_DetailModel.Title = title;
                                    search_Template_DetailModel.Datatype = dataType;
                                    search_Template_DetailModel.Condition = condition;
                                    search_Template_DetailModel.Width = width;
                                    search_Template_DetailModel.ShowColume = showColume;
                                    search_Template_DetailModel.ShowSearch = showSearch;
                                    search_Template_DetailModel.DictID = dictId;
                                    if (id == 0)
                                    {
                                        search_Template_DetailService.AddModel(search_Template_DetailModel);
                                    }
                                    else
                                    {
                                        if (search_Template_DetailModel.ID == 0)
                                        {
                                            errMsg = "该查询模板条件不存在";
                                            return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                        }

                                        search_Template_DetailService.UpdateModel(search_Template_DetailModel);
                                    }
                                }
                                else
                                {
                                    errMsg = "提交数据包含空对象";
                                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                                }
                            }

                            if (!search_Template_DetailService.SaveChanges())
                            {
                                errMsg = "保存查询模板详细信息失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }
                        }                        

                        ts.Complete();

                        return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn);
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
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }         
        }
    }
}