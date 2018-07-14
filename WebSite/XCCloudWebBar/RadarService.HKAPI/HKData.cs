using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadarService.HKAPI
{
    public class HKData
    {
        public enum 业务码
        {
            操作成功 = 100,
            卡未绑定用户 = 101,
            用户认证身份_通过认证 = 110,
            用户认证身份_未通过认证 = 111,
            充值业务_允许充值 = 120,
            充值业务_禁止充值 = 121,
            充值业务_充值记录成功 = 122,
            充值业务_充值记录失败 = 123,
            卡类业务_注销卡成功 = 130,
            卡类业务_注销卡失败 = 131,
            卡类业务_绑定卡成功 = 132,
            卡类业务_绑定卡失败_已经绑定其他用户 = 133,
            卡类业务_绑定卡失败_未找到用户信息 = 134,
            积分业务_代币换积分成功 = 140,
            积分业务_代币换积分失败 = 141,
            设备业务_设备不存在 = 150,
            设备业务_设备已被绑定 = 151,
        }
        /// <summary>
        /// 身份证数据结果
        /// </summary>
        public class IDCard
        {
            public string 身份证号 { get; set; }
            public string 姓名 { get; set; }
            public string 性别 { get; set; }
            public string 名族 { get; set; }
            public string 生日 { get; set; }
            public string 住址 { get; set; }
            public string 签发机关 { get; set; }
            public string 发证日期 { get; set; }
            public string 有效期限 { get; set; }
        }

        public class 实名认证结构
        {
            public string shopId { get; set; }
            public string caller { get; set; }
            public string realName { get; set; }
            public string idCard { get; set; }
            public string phone { get; set; }
        }

        public class 实名认证应答
        {
            public 处理结果 data { get; set; }
            public string statusCode { get; set; }
            public string statusMsg { get; set; }
        }

        public class 兑换应答
        {
            public object data { get; set; }
            public string statusCode { get; set; }
            public string statusMsg { get; set; }
        }

        public class 处理结果
        {
            public string code { get; set; }
            public string description { get; set; }
            public string qrCode { get; set; }
            public string sceneUrl { get; set; }
        }

        public class 开卡请求
        {
            public string shopId { get; set; }
            public string cardId { get; set; }
            public string cardName { get; set; }
            public string realName { get; set; }
            public string idCard { get; set; }
            public string phone { get; set; }
        }

        public class 注销卡请求
        {
            public string shopId { get; set; }
            public string cardId { get; set; }
        }

        public class 充值通知
        {
            public string shopId { get; set; }
            public string cardId { get; set; }
            public string amount { get; set; }
        }

        public class 线上积分兑换
        {
            public string shopId { get; set; }
            public string cardId { get; set; }
            public string typeValue { get; set; }
            public string amount { get; set; }
        }

        public class 设备注册
        {
            public string shopId { get; set; }
            public string device { get; set; }
            public string deviceId { get; set; }
            public string Name { get; set; }
            public string machineSn { get; set; }
            public string machineName { get; set; }
            public string deviceType { get; set; }
            public string cost { get; set; }
        }

        public class 设备状态通知
        {
            public string shopId { get; set; }
            public string cardId { get; set; }
            public string sn { get; set; }
            public string status { get; set; }
        }

        public class 设备中奖通知
        {
            public string orderID { get; set; }
            public string type { get; set; }
            public string num { get; set; }
            public string shopid { get; set; }
            public string caller { get; set; }
            public string sn { get; set; }
        }
    }
}
