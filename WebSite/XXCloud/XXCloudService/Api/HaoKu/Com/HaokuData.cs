using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace XXCloudService.Api.HaoKu.Com
{
    public class HaokuData
    {
        #region 公共请求参数
        /// <summary>
        /// 公共请求参数
        /// </summary>
        public class CommonParameter
        {
            /// <summary>
            /// 店铺ID ，无需加密
            /// </summary>
            public string shopId { get; set; }
            /// <summary>
            /// 供应商 ，无需加密
            /// </summary>
            public string caller { get; set; }
        } 
        #endregion

        #region 实名认证 - 认证用户信息
        #region 实名认证请求
        /// <summary>
        /// 实名认证请求
        /// </summary>
        public class MemberVerify : CommonParameter
        {            
            /// <summary>
            /// 真实姓名
            /// </summary>
            public string realName { get; set; }
            /// <summary>
            /// 身份证号码
            /// </summary>
            public string idCard { get; set; }
            /// <summary>
            /// 手机号码
            /// </summary>
            public string phone { get; set; }
        } 
        #endregion
        #endregion

        #region 开/绑/注销卡请求参数
        /// <summary>
        /// 开/绑/注销卡请求参数
        /// </summary>
        public class CardInfo : CommonParameter
        {
            /// <summary>
            /// 卡ID ，需加密
            /// </summary>
            public string cardId { get; set; }
            /// <summary>
            /// 卡名称 ，需加密
            /// </summary>
            public string cardName { get; set; }
            /// <summary>
            /// 真实姓名 ，需加密
            /// </summary>
            public string realName { get; set; }
            /// <summary>
            /// 身份证号码 ，需加密
            /// </summary>
            public string idCard { get; set; }
            /// <summary>
            /// 手机号码 ，需加密
            /// </summary>
            public string phone { get; set; }
        }
        #endregion

        #region 充值
        /// <summary>
        /// 充值
        /// </summary>
        public class ChargeData : CommonParameter
        {
            /// <summary>
            /// 卡ID ，需加密
            /// </summary>
            public string cardId { get; set; }
            /// <summary>
            /// 充值金额 ，需加密, 单位为分
            /// </summary>
            public string amount { get; set; }
        }
        #endregion

        #region 代币换积分
        /// <summary>
        /// 代币换积分
        /// </summary>
        public class ChargePointData : CommonParameter
        {
            /// <summary>
            /// 卡ID ，需加密
            /// </summary>
            public string cardId { get; set; }
            /// <summary>
            /// 会员ID ，需加密
            /// </summary>
            public string memberId { get; set; }

            /// <summary>
            /// 类型 说明： 1 积分 , 2 彩票 , 3 代币 , 4 现金 ，需加密
            /// </summary>
            public string type { get; set; }

            /// <summary>
            /// 金额 ，需加密,单位为分
            /// </summary>
            public string amount { get; set; }
        }
        #endregion

        #region 设备绑定
        /// <summary>
        /// 设备绑定
        /// </summary>
        public class DeviceData : CommonParameter
        {
            /// <summary>
            /// 卡头编码 ，卡头编号，请保证店铺内唯一,需加密
            /// </summary>
            public string sn { get; set; }
            /// <summary>
            /// 卡头名称 ，需加密
            /// </summary>
            public string name { get; set; }

            /// <summary>
            /// 游戏机编码 ，游戏机编号，请保证店铺内唯一,需加密
            /// </summary>
            public string machineSn { get; set; }

            /// <summary>
            /// 游戏机名称 ，游戏机名称需加密
            /// </summary>
            public string machineName { get; set; }

            /// <summary>
            /// 游戏机类型 说明： 1 娃娃机设备 , 2 积分设备 , 3 售币设备 , 4 游乐设备 , 5 点票机 ，需加密
            /// </summary>
            public string deviceType { get; set; }

            /// <summary>
            /// 卡头标识 ，卡头唯一识别标识,需加密
            /// </summary>
            public string dopCode { get; set; }

            /// <summary>
            /// 单次耗币 ，需加密
            /// </summary>
            public string cost { get; set; }

            /// <summary>
            /// 币与积分的兑换比例 ，如果有,需加密
            /// </summary>
            public string point { get; set; }
        }
        #endregion

        #region 设备状态更新
        /// <summary>
        /// 设备状态更新
        /// </summary>
        public class DeviceStatusData : CommonParameter
        {
            /// <summary>
            /// 卡头编码 ，卡头编号，请保证店铺内唯一,需加密
            /// </summary>
            public string sn { get; set; }
            /// <summary>
            /// 设备状态 说明： 1 在线 , 2 离线 ，需加密
            /// </summary>
            public string status { get; set; }
        }
        #endregion

        #region 设备出奖
        /// <summary>
        /// 设备出奖
        /// </summary>
        public class DevicePrizeData : CommonParameter
        {
            /// <summary>
            /// 来源单号 ，远程投币来源单号,需加密
            /// </summary>
            public string source { get; set; }
            /// <summary>
            /// 奖品类型 说明： 1 游戏币 , 51 礼品 , 52 彩票 , 53 积分 , 54 商城商品 ，需加密
            /// </summary>
            public string type { get; set; }

            /// <summary>
            /// 奖品数量 ，需加密
            /// </summary>
            public string num { get; set; }

            /// <summary>
            /// 出奖流水号 ，同一个远程投币来源单号唯一,需加密
            /// </summary>
            public string sn { get; set; }

            /// <summary>
            /// 扩展字段 ，备用字段，可不填写，如果有需加密
            /// </summary>
            public string ext { get; set; }
        }
        #endregion

        #region 接口请求应答
        #region 通用应答
        /// <summary>
        /// 接口请求应答
        /// </summary>
        public class RequestACK
        {
            public ResultData data { get; set; }
            /// <summary>
            /// 状态码
            /// </summary>
            public int statusCode { get; set; }
            /// <summary>
            /// 消息说明
            /// </summary>
            public string statusMsg { get; set; }
        }

        public class ResultData
        {
            public int code { get; set; }
            public object data { get; set; }
            public string description { get; set; }
        } 
        #endregion

        #region 绑卡应答
        public class BindCardACK
        {
            public BindData data { get; set; }
            /// <summary>
            /// 状态码
            /// </summary>
            public int statusCode { get; set; }
            /// <summary>
            /// 消息说明
            /// </summary>
            public string statusMsg { get; set; }
        }

        public class BindData
        {
            public int code { get; set; }
            public BindDetailData data { get; set; }
            public string description { get; set; }
        }

        public class BindDetailData
        {
            public string id { get; set; }
            public string qrCodeUrl { get; set; }
        } 
        #endregion

        #region 获取卡关联的用户应答
        /// <summary>
        /// 获取卡关联的用户应答
        /// </summary>
        public class MemberListACK
        {
            public MemberListData data { get; set; }
            /// <summary>
            /// 状态码
            /// </summary>
            public int statusCode { get; set; }
            /// <summary>
            /// 消息说明
            /// </summary>
            public string statusMsg { get; set; }
        }

        public class MemberListData
        {
            public List<MemberListDetail> list { get; set; }
            public ResultPage page { get; set; }
        }

        public class MemberListDetail
        {
            public string memberId { get; set; }
            public string nickname { get; set; }
            public string shopId { get; set; }
        }

        public class ResultPage
        {
            public int currCount { get; set; }
            public int currPage { get; set; }
            public int total { get; set; }
            public int totalPage { get; set; }
        }
        #endregion

        #region 设备绑定应答
        public class BindDeviceACK
        {
            public DeviceDetail data { get; set; }
            /// <summary>
            /// 状态码
            /// </summary>
            public int statusCode { get; set; }
            /// <summary>
            /// 消息说明
            /// </summary>
            public string statusMsg { get; set; }
        }

        public class DeviceDetail
        {
            /// <summary>
            /// 单次耗币
            /// </summary>
            public int cost { get; set; }
            /// <summary>
            /// 卡头名称
            /// </summary>
            public string name { get; set; }
            /// <summary>
            /// 二维码图片连接
            /// </summary>
            public string qrCode { get; set; }
            /// <summary>
            /// 生成微信二维码code
            /// </summary>
            public string sceneUrl { get; set; }
            /// <summary>
            /// 好酷门店ID
            /// </summary>
            public string shopId { get; set; }
            /// <summary>
            /// 卡头编码
            /// </summary>
            public string sn { get; set; }
        }
        #endregion
        #endregion
    }

    public class HaokuViewModel
    {
        public class BindViewModel
        {
            public string Id { get; set; }
            public string QRCodeUrl { get; set; }
        }

        public class MemberListViewModel
        {
            public string memberId { get; set; }
            public string nickName { get; set; }
        }
    }
}