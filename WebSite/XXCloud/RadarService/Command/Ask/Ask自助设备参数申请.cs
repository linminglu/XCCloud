using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using RadarService.Info;


namespace RadarService.Command.Ask
{
    public class Ask自助设备参数申请
    {
        public byte 机头地址 { get; set; }
        public byte 设备类型 { get; set; }
        public byte 马达配置 { get; set; }
        /// <summary>
        /// 1 TM1639 4位*2行 2 TM1629 6位*2行
        /// </summary>
        public byte 数码管类型 { get; set; }
        public byte 马达1比例 { get; set; }
        public byte 马达2比例 { get; set; }
        public UInt16 存币箱最大存币数 { get; set; }
        public byte 是否允许打印 { get; set; }
        public byte SSR电平 { get; set; }
        public string 本店卡校验密码 { get; set; }

        public bool isSuccess = false;
        public Ask自助设备参数申请(DeviceInfo.机头信息 device)
        {
            机头地址 = Convert.ToByte(device.机头短地址, 16);
            switch (device.类型)
            {
                case DeviceInfo.设备类型.售币机:
                    设备类型 = 0x01;
                    break;
                case DeviceInfo.设备类型.存币机:
                    设备类型 = 0x02;
                    break;
                case DeviceInfo.设备类型.提币机:
                    设备类型 = 0x03;
                    break;
                case DeviceInfo.设备类型.碎票机:
                    设备类型 = 0x04;
                    break;
                case DeviceInfo.设备类型.投币机:
                    设备类型 = 0x05;
                    break;
            }
            string v = "000000";
            v += device.扩展参数.马达1启用 ? "1" : "0";
            v += device.扩展参数.马达2启用 ? "1" : "0";
            马达配置 = Convert.ToByte(v, 2);
            数码管类型 = 0;   //已弃用
            马达1比例 = (byte)device.扩展参数.马达1出币比例;
            马达2比例 = (byte)device.扩展参数.马达1出币比例;
            存币箱最大存币数 = Convert.ToUInt16(device.扩展参数.最大储币数量);
            本店卡校验密码 = PublicHelper.SystemDefiner.StorePassword;
            是否允许打印 = 0;
            SSR电平 = (byte)device.扩展参数.SSR电平;
        }
    }
}
