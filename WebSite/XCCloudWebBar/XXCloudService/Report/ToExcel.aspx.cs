using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using XCCloudWebBar.Common;

namespace XXCloudService.Report
{
    public partial class ToExcel : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                string errMsg = string.Empty;
                string guid = Request["guid"] ?? "";

                object obj = CacheHelper.Get(guid);
                if (obj != null)
                {
                    string filePath = obj.ToString();
                    string fileName = Path.GetFileName(filePath);
                    //检查末尾是否为.xls或xlsx如果不是返回低版本xls
                    if (!(fileName.EndsWith(".xls") || fileName.EndsWith(".xlsx")))
                    {
                        fileName = fileName + ".xls";
                    }
                    ////向客户端发送文件...
                    HttpContext.Current.Response.Clear();
                    HttpContext.Current.Response.Charset = "GB2312";
                    HttpContext.Current.Response.ContentEncoding = System.Text.Encoding.UTF8;
                    // 添加头信息，为"文件下载/另存为"对话框指定默认文件名   
                    HttpContext.Current.Response.AddHeader("Content-Disposition", "attachment; filename=" + HttpUtility.UrlEncode(fileName));
                    // 添加头信息，指定文件大小，让浏览器能够显示下载进度   
                    //HttpContext.Current.Response.AddHeader("Content-Length", file.Length.ToString());
                    // 指定返回的是一个不能被客户端读取的流，必须被下载   
                    //HttpContext.Current.Response.ContentType = "application/ms-excel";
                    HttpContext.Current.Response.ContentType = "application/vnd.ms-excel";
                    // 把文件流发送到客户端   
                    HttpContext.Current.Response.WriteFile(filePath);
                    HttpContext.Current.Response.End();
                }
                CacheHelper.Remove(guid);
            }
            catch (Exception ex)
            {
                LogHelper.SaveLog("错误:" + ex.Message);                
            }
        }
    }
}