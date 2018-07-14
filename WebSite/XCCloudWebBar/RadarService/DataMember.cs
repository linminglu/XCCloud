using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadarService
{
    public class DataMember
    {
        public class 电子币投币应答结构
        {
            public string 数字币编号 { get; set; }
            public bool 机头能上分 { get; set; }
            public bool 机头能打票 { get; set; }
            public bool 锁机头 { get; set; }
            public int 币数 { get; set; }
            public int 扣币数 { get; set; }
            public int 发脉冲数 { get; set; }
            public bool 是否将退币上回游戏机 { get; set; }
            public bool 是否正在使用限时送分优惠券 { get; set; }
        }

        public class 电子币出币应答结构
        {
            public bool 机头能上分 { get; set; }
            public bool 机头能打票 { get; set; }
            public bool 锁机头 { get; set; }
            public UInt32 条码 { get; set; }
            public byte 动态密码 { get; set; }
            public bool 超额锁 { get; set; }
            public int 发脉冲数 { get; set; }
            public bool 是否将退币上回游戏机 { get; set; }
            public bool 是否正在使用限时送分优惠券 { get; set; }
        }

        public class IC卡模式进出币应答结构
        {
            public bool 机头能上分 { get; set; }
            public bool 机头能打票 { get; set; }
            public bool 锁机头 { get; set; }
            public int 发脉冲数 { get; set; }
            public int 币余额 { get; set; }
            public bool 是否启用卡片专卡专用 { get; set; }
            public bool 超出当日机头最大净退币上线 { get; set; }
            public bool 是否将退币上回游戏机 { get; set; }
            public bool 是否正在使用限时送分优惠券 { get; set; }
        }
        public class 液晶卡头进出币应答结构
        {
            public bool 机头能上分 { get; set; }
            public bool 机头能打票 { get; set; }
            public bool 锁机头 { get; set; }
            public int 发脉冲数 { get; set; }
            public int 送币数 { get; set; }
            public int 币余额 { get; set; }
            public bool 是否启用卡片专卡专用 { get; set; }
            public bool 超出当日机头最大净退币上线 { get; set; }
            public bool 是否将退币上回游戏机 { get; set; }
            public bool 是否正在使用限时送分优惠券 { get; set; }
        }
    }
}
