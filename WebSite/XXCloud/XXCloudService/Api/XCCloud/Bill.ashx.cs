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
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using XCCloudService.Common.Extensions;

namespace XXCloudService.Api.XCCloud
{
    [Authorize(Merches = "Normal,Heavy")]
    /// <summary>
    /// Bill 的摘要说明
    /// </summary>
    public class Bill : ApiBase
    {
        //查询方法  
        private List<Data_BillInfo> Search(string title, DateTime? publishDate)
        {
            //为了模拟EF查询，转换为IEnumerable,在EF中此处为数据库上下文的表对象  
            IData_BillInfoService data_BillInfoService = BLLContainer.Resolve<IData_BillInfoService>();
            var result = data_BillInfoService.GetModels();

            /*下列代码不会立即执行查询，而是生成查询计划 
             * 若参数不存在则不添加查询条件，从而可以无限制的添加查询条件 
             */
            if (!string.IsNullOrEmpty(title))
            {
                result = result.Where(p => p.Title.Contains(title));
            }

            if (publishDate != null)
            {
                result = result.Where(p => System.Data.Entity.DbFunctions.DiffDays(p.ReleaseTime, publishDate) == 0);
            }            

            //此时执行查询  
            var final = result.ToList();
            return final;
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object UploadPicture(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;              

                List<string> imageUrls = null;
                if (!Utils.UploadImageFile("/XCCloud/Bill/", out imageUrls, out errMsg))
                {
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                return ResponseModelFactory.CreateAnonymousSuccessModel(isSignKeyReturn, new { PicturePath = imageUrls.FirstOrDefault() });
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object DeletePicture(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                var fileName = dicParas.Get("fileName");

                if (string.IsNullOrEmpty(fileName))
                {
                    errMsg = "图片名称不能为空";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                string picturePath = System.Configuration.ConfigurationManager.AppSettings["UploadImageUrl"].ToString() + "/XCCloud/";
                string path = System.Web.HttpContext.Current.Server.MapPath(picturePath);                
                
                SqlParameter[] parameters = new SqlParameter[1];
                parameters[0] = new SqlParameter("@PicturePath", picturePath + fileName);
                Data_BillInfoService.I.ExecuteSqlCommand("update Data_BillInfo set PicturePath='' where PicturePath=@PicturePath", parameters);

                if (File.Exists(path + fileName))
                {
                    File.Delete(path + fileName);
                }
                
                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object PublishBill(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                if (!dicParas.Get("title").Nonempty("标题", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("publishType").Validint("展示方式", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("promotionType").Validint("活动类别", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (!dicParas.Get("pagePath").Nonempty("活动内容", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                if (dicParas.Get("title").Length > 50)
                {
                    errMsg = "标题不能超过50字";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                }

                int id = dicParas.Get("id").Toint(0);
                var title = dicParas.Get("title");
                var publishType = dicParas.Get("publishType").Toint();
                var promotionType = dicParas.Get("promotionType").Toint();
                var picturePath = dicParas.Get("picturePath");
                var pagePath = dicParas.Get("pagePath");

                

                var data_BillInfoModel = new Data_BillInfo();
                if (id == 0)
                {
                    data_BillInfoModel.Title = title;
                    data_BillInfoModel.PublishType = publishType;
                    data_BillInfoModel.PromotionType = promotionType;
                    data_BillInfoModel.PicturePath = picturePath;
                    data_BillInfoModel.PagePath = pagePath;
                    data_BillInfoModel.State = 1;
                    data_BillInfoModel.Time = DateTime.Now;
                    data_BillInfoModel.ReleaseTime = DateTime.Now;

                    if (Data_BillInfoService.I.Add(data_BillInfoModel))
                    {
                        errMsg = "发布海报失败";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }
                }
                else
                {
                    if (!Data_BillInfoService.I.Any(p => p.ID == id))
                    {
                        errMsg = "该海报不存在";
                        return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                    }

                    data_BillInfoModel = Data_BillInfoService.I.GetModels(p => p.ID == id).FirstOrDefault();
                    data_BillInfoModel.Title = title;
                    data_BillInfoModel.PublishType = publishType;
                    data_BillInfoModel.PromotionType = promotionType;
                    data_BillInfoModel.PicturePath = picturePath;
                    data_BillInfoModel.PagePath = pagePath;
                    data_BillInfoModel.State = 1;
                    data_BillInfoModel.ReleaseTime = DateTime.Now;

                    if (!Data_BillInfoService.I.Update(data_BillInfoModel))
                    {
                        errMsg = "发布海报失败";
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
        public object DeleteBill(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                var idArr = dicParas.GetArray("id");

                if (!idArr.Validarray("海报ID列表", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                //开启EF事务
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        foreach (var id in idArr)
                        {
                            if (!id.Validintnozero("海报ID", out errMsg))
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                            if (!Data_BillInfoService.I.Any(p => p.ID == (int)id))
                            {
                                errMsg = "该海报不存在";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            var data_BillInfoModel = Data_BillInfoService.I.GetModels(p => p.ID == (int)id).FirstOrDefault();
                            if (!Data_BillInfoService.I.Delete(data_BillInfoModel))
                            {
                                errMsg = "删除海报失败";
                                return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);
                            }

                            if (File.Exists(data_BillInfoModel.PicturePath))
                            {
                                File.Delete(data_BillInfoModel.PicturePath);
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
        public object GetBills(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                if(!dicParas.Get("publishDate").Validdate("发布日期", out errMsg))
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg);

                var title = dicParas.Get("title");
                var publishDate = dicParas.Get("publishDate").Todatetime();

                //IData_BillInfoService data_BillInfoService = BLLContainer.Resolve<IData_BillInfoService>();    
                //string sql = "select * from Data_BillInfo where 1=1";
                //if(!string.IsNullOrEmpty(title))
                //{
                //    sql = sql + " and title=@title";
                //}
                //if(!string.IsNullOrEmpty(publishDate))
                //{
                //    sql = sql + " and DATEDIFF(day,releasetime,@publishDate)=0";
                //}
                //SqlParameter[] parameters = new SqlParameter[2];
                //parameters[0] = new SqlParameter("@title", title);
                //parameters[1] = new SqlParameter("@publishDate", string.IsNullOrEmpty(publishDate) ? new DateTime(1900, 1, 1) : Convert.ToDateTime(publishDate));
                //var data_BillInfo = data_BillInfoService.SqlQuery(sql, parameters).ToList<Data_BillInfo>();

                var data_BillInfo = Search(title, publishDate);

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, data_BillInfo);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.XCCloudUserCacheToken, SysIdAndVersionNo = false)]
        public object GetPictures(Dictionary<string, object> dicParas)
        {
            try
            {
                string sql = "select * from Data_BillInfo";
                var data_BillInfo = Data_BillInfoService.I.SqlQuery(sql).ToList().GroupBy(p => p.PicturePath).Select(g => g.Key).ToList();

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, data_BillInfo);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        private object ToAnonymousObject(int code = 1, string msg = "", string src = "", string title = null)
        {
            return new
            {
                code = code, //0表示成功，其它失败
                msg = msg,   //提示信息 //一般上传失败后返回
                data = new
                {
                    src = src,    //图片路径
                    title = title //可选
                }
            };
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MethodToken, SysIdAndVersionNo = false)]
        public object UploadPictureInRich(Dictionary<string, object> dicParas)
        {            
            try
            {
                string errMsg = string.Empty;                                           

                List<string> imageUrls = null;
                if (!Utils.UploadImageFile("/XCCloud/Bill/", out imageUrls, out errMsg))
                {
                    return ToAnonymousObject(msg: errMsg);
                }

                return ToAnonymousObject(code: 0, src: imageUrls.FirstOrDefault());
            }
            catch (Exception e)
            {
                return ToAnonymousObject(msg: e.Message);
            }
        }
    }
}