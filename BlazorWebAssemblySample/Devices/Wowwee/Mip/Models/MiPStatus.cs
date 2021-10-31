using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiPWinRTSDK.MiP.Models
{
    public sealed class MiPStatus
    {
        public byte BatteryLevel { get; internal set; }
        public StatusEnum Status { get; internal set; }
    }
}
