using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RadarService.Info
{
    /// <summary>
    /// 机头状态
    /// </summary>
    public static class HeadStatus
    {
        public class 机头状态开关
        {
            public bool 锁定游戏机头 { get; set; }
            public bool 允许从游戏机退币到卡里或机头能打票 { get; set; }
            public bool 机头能上分 { get; set; }
            public bool 读币器故障 { get; set; }
            public bool 打印机过热 { get; set; }
            public bool 打印机过压 { get; set; }
            public bool 打印机缺纸 { get; set; }
            public bool 机头在线状态 { get; set; }
        }

        public class 机头干扰开关
        {
            public bool bit7 { get; set; }
            public bool bit6 { get; set; }
            public bool bit5 { get; set; }
            public bool bit4 { get; set; }
            public bool bit3 { get; set; }
            public bool bit2 { get; set; }
            public bool 高压干扰报警 { get; set; }
            public bool 高频干扰报警 { get; set; }
        }

        /// <summary>
        /// 币属性
        /// </summary>
        public class CoinInfo
        {
            public int 投币数 = 0;
            public int 退币数 = 0;
            public int 盈利数 = 0;
            public int 每天投币上限 = 0;
        }

        public class 机头控制
        {
            public bool 强制停止打票 { get; set; }
            public 机头状态开关 开关 { get; set; }
            public 机头干扰开关 报警 { get; set; }
        }

        static Dictionary<string, 机头控制> 状态列表 = new Dictionary<string, 机头控制>();

        /// <summary>
        /// 更新机头状态
        /// </summary>
        /// <param name="HeadAddress">机头地址</param>
        /// <param name="Status">状态</param>
        public static void Update(string HeadAddress, 机头状态开关 Status)
        {
            if (状态列表.ContainsKey(HeadAddress))
            {
                状态列表[HeadAddress].开关 = Status;
            }
            else
            {
                机头控制 控制 = new 机头控制();
                控制.开关 = Status;
                状态列表.Add(HeadAddress, 控制);
            }
        }
        /// <summary>
        /// 删除机头状态
        /// </summary>
        /// <param name="HeadAddress">机头地址</param>
        public static void Delete(string HeadAddress)
        {
            if (状态列表.ContainsKey(HeadAddress))
            {
                状态列表.Remove(HeadAddress);
            }
        }
        /// <summary>
        /// 获取机头状态
        /// </summary>
        /// <param name="HeadAddress">机头地址</param>
        /// <returns></returns>
        public static 机头控制 Get(string HeadAddress)
        {
            if (状态列表.ContainsKey(HeadAddress))
            {
                return 状态列表[HeadAddress];
            }
            return new 机头控制();
        }
    }
}
