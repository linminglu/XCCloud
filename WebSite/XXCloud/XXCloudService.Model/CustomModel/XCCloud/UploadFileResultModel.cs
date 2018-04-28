using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudService.Model.CustomModel.XCCloud
{
    public class UploadFileResultModel
    {
        public UploadFileResultModel(string fileType,string url)
        {
            this.FileType = fileType;
            this.Url = url;
        }

        public string FileType { set; get; }

        public string Url { set; get; }
    }
}
