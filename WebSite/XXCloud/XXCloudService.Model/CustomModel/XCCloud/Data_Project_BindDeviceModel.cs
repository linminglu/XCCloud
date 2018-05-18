using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.Common.Enum;
using XCCloudService.Common;

namespace XCCloudService.Model.CustomModel.XCCloud
{
    public class Data_Project_BindDeviceList
    {
        /// <summary>
        /// ID
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// 设备编号
        /// </summary>
        public int? DeviceID { get; set; }
        /// <summary>
        /// 设备名称
        /// </summary>
        public string DeviceName { get; set; }
        /// <summary>
        /// 设备类别
        /// </summary>
        public string DeviceTypeStr { get; set; }     
        /// <summary>
        /// 工作方式
        /// </summary>
        public int? WorkType { get; set; }
        /// <summary>
        /// 工作方式
        /// </summary>
        public string WorkTypeStr { get { return ((ProjectBindDeviceWorkType?)WorkType).GetDescription(); } set { } }
        /// <summary>
        /// 校验顺序
        /// </summary>
        public int? AdjOrder { get; set; }
        /// <summary>
        /// 校验顺序
        /// </summary>
        public string AdjOrderStr { get { return AdjOrder == 1 ? "开启" : AdjOrder == 0 ? "关闭" : string.Empty; } set { } }
        /// <summary>
        /// 人脸识别
        /// </summary>
        public int? ReadFace { get; set; }
        /// <summary>
        /// 人脸识别
        /// </summary>
        public string ReadFaceStr { get { return ReadFace == 1 ? "是" : ReadFace == 0 ? "否" : string.Empty; } set { } }
        /// <summary>
        /// 刷卡识别
        /// </summary>
        public int? ReadCard { get; set; }
        /// <summary>
        /// 刷卡识别
        /// </summary>
        public string ReadCardStr { get { return ReadCard == 1 ? "是" : ReadCard == 0 ? "否" : string.Empty; } set { } }
        /// <summary>
        /// 二维码识别
        /// </summary>
        public int? ReadQRCode { get; set; }
        /// <summary>
        /// 二维码识别
        /// </summary>
        public string ReadQRCodeStr { get { return ReadQRCode == 1 ? "是" : ReadQRCode == 0 ? "否" : string.Empty; } set { } }
        /// <summary>
        /// 现金使用
        /// </summary>
        public int? AllowCash { get; set; }
        /// <summary>
        /// 现金使用
        /// </summary>
        public string AllowCashStr { get { return AllowCash == 1 ? "是" : AllowCash == 0 ? "否" : string.Empty; } set { } }
        
    }
}
