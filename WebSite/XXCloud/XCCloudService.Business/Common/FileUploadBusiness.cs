using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using XCCloudService.Common;

namespace XCCloudService.Business.Common
{
    public class FileUploadBusiness
    {
        private static List<string> GetUploadFileFormat()
        {
            return CommonConfig.FileFormat.Split(',').ToList<string>();
        }

        /// <summary>
        /// 是否存在不合法的文件大小
        /// </summary>
        /// <returns></returns>
        public static bool IsExistInvalidSzie(HttpFileCollection hfc)
        {
            foreach (HttpPostedFile hf in hfc)
            {
                if (!IsEffectiveFileFormat(hf.FileName))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool CheckFormatAndSize(HttpFileCollection hfc,ref List<string> fileTyeList,out string errMsg)
        {
            string extName = string.Empty;
            errMsg = string.Empty;
            fileTyeList = new List<string>();
            string fileType = string.Empty;
            for (int i = 0; i < hfc.Count;i++ )
            {
                if (!IsEffectiveFileFormat(hfc[i].FileName, out extName))
                {
                    extName = "文件格式不合法";
                    return false;
                }

                if (!IsEffectiveFileSize((int)(hfc[i].InputStream.Length / 1024), extName, out fileType))
                {
                    extName = "文件大小不合法";
                    return false;
                }

                fileTyeList.Add(fileType);
            }
            return true;
        }

        private static bool IsEffectiveFileFormat(string fileName,out string extName)
        {
            List<string> fileFormatList = GetUploadFileFormat();
            string empExtName = Utils.GetExtName(fileName);
            bool b =  fileFormatList.Where<string>(p => p == empExtName).Count() > 0;
            extName = empExtName;
            return b;
        }

        private static bool IsEffectiveFileFormat(string fileName)
        {
            List<string> fileFormatList = GetUploadFileFormat();
            string empExtName = Utils.GetExtName(fileName);
            bool b = fileFormatList.Where<string>(p => p == empExtName).Count() > 0;
            return b;
        }

        /// <summary>
        /// 是否有效文件大小
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        private static bool IsEffectiveFileSize(int size,string extName,out string fileType)
        {
            fileType = GetFileFormatType(extName);
            int defaultSize = GetFileDefaultSize(fileType);
            return size <= defaultSize;
        }


        /// <summary>
        /// 获取上传文件的默认大小
        /// </summary>
        /// <param name="fileType"></param>
        /// <returns></returns>
        private static int GetFileDefaultSize(string fileType)
        {
            if(fileType == Constant.ImageFileType)
            {
                return 512;
            }
            else if(fileType == Constant.ImageFileType)
            {
                return 3072;
            }
            else
            {
                return 0;
            }
        }

        public static string GetFileFormatType(string extName)
        {
            switch (extName)
            {
                case "bmp": return Constant.ImageFileType;
                case "gif": return Constant.ImageFileType;
                case "png": return Constant.ImageFileType;
                case "jpg": return Constant.ImageFileType;
                case "mp3": return Constant.MoveFileType;
                case "mp4": return Constant.MoveFileType;
                default: return "Other";
            }
        }

        public static string GetClientTypeFolder(string clientType)
        {
            switch (clientType)
            {
                case "1": return "xchelper";
                default: return "Other";
            }
        }

        public static bool SaveFile(HttpPostedFile hpf,string clientType,out string fileType,out string url,out string errMsg)
        {
            errMsg = string.Empty;
            fileType = string.Empty;
            url = "";
            if (hpf == null)
            {
                errMsg = "未找到文件内容";
                return false;
            }

            string extName = string.Empty;
            if (!IsEffectiveFileFormat(hpf.FileName,out extName))
            {
                errMsg = "不支持该文件格式";
                return false;
            }

            fileType = GetFileFormatType(extName);

            string folder = string.Empty;
            string filePath = string.Empty;
            string fileName = string.Empty;
            string clientTypeFolder = string.Empty;
            if (CreateFilePath(clientType, extName, out folder, out filePath,out clientTypeFolder, out fileName))
            {
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                hpf.SaveAs(filePath);
                url = string.Format("{0}/{1}/{2}.{3}", GetHost(fileType),GetClientTypeFolder(clientType), fileName,extName);
                return true;
            }
            else
            {
                errMsg = "上传失败";
                return false;
            }
        }

        private static string GetHost(string fileType)
        {
            if (fileType == Constant.ImageFileType)
            {
                return CommonConfig.ImageFileHost;
            }
            else if (fileType == Constant.MoveFileType)
            {
                return CommonConfig.MovieFileHost;
            }
            else
            {
                throw new Exception("文件类型出错");
            }
        }


        private static bool CreateFilePath(string clientType, string extName, out string folder, out string filePath,out string clientTypeFolder, out string fileName)
        {
            int count = 0;
            folder = string.Empty;
            filePath = string.Empty;
            clientTypeFolder = string.Empty;
            fileName = string.Empty;
            bool isExistFile = true;
            while (count <= 3 && isExistFile)
            {
                GetFileName(clientType, extName, out folder, out filePath, out clientTypeFolder, out fileName);
                isExistFile = IsExistFile(filePath);
                count++;
            }
            return !isExistFile;
        }

        private static void GetFileName(string clientType, string extName, out string folder, out string filePath, out string clientTypeFolder, out string fileName)
        {
            clientTypeFolder = GetClientTypeFolder(clientType);
            string fileTypeFolder = GetFileFormatType(extName);
            string dateFolder = System.DateTime.Now.ToString("yyyyMMdd");
            fileName = string.Format("{0}{1}", System.DateTime.Now.ToString("yyyyMMddHHmmssfff"), new Random().Next(0, 999999).ToString().PadLeft(6, '0'));
            folder = string.Format("{0}/{1}/{2}/{3}", CommonConfig.FileServerPhysicsPath, fileTypeFolder, clientTypeFolder, dateFolder);
            filePath = string.Format("{0}/{1}.{2}", folder, fileName,extName);
        }

        private static bool IsExistFile(string filePath)
        {
            FileInfo file = new FileInfo(filePath);
            if (file.Exists)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
