using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadarService.Notify
{
    public enum ActionEnum
    {
        出币 = 1,
        存币 = 2,
        投币 = 6,
        退币 = 7,
    }

    public enum DeviceStatusEnum
    {
        离线 = 0,
        在线 = 1,
        出币中 = 2,
        故障 = 3,
        锁定 = 4
    }
}
