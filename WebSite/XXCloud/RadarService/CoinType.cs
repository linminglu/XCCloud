using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadarService
{
    public enum CoinType
    {
        实物投币 = 0,
        电子投币 = 1,
        实物退币 = 2,
        电子退币 = 3,
        电子存币 = 4,
        会员卡提币 = 5,
        电子碎票 = 6,
        实物彩票 = 7,
        IC退彩票 = 8,
        远程出币通知 = 9,
        远程存币通知 = 10,
        礼品掉落 = 11,
    }

    public enum HeadAlertType
    {
        打印机故障 = 0,
        打印设置故障 = 1,
        卡头读卡故障 = 2,
        高频干扰报警 = 3,
        高压干扰报警 = 4,
        SSR信号异常 = 5,
        CO信号异常 = 6,
        CO2信号异常 = 7,
        存币箱满报警 = 8,
    }
}
