using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudWebBar.Model.CustomModel.XCGameManager
{
    public class WxConfigModel
    {
        /// <summary>
        /// 公众号appId
        /// </summary>
        public string AppId { get; set; }

        /// <summary>
        /// 生成签名的时间戳
        /// </summary>
        public string TimeStamp { get; set; }

        /// <summary>
        /// 生成签名的随机串
        /// </summary>
        public string NonceStr { get; set; }

        /// <summary>
        /// 签名
        /// </summary>
        public string Signature { get; set; }
    }
}
