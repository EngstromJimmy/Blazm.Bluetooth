using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiPWinRTSDK.MiP.Models
{
    public sealed class MiPSound
    {
        public byte? Sound { get; set; }
        public byte? Volume { get; set; }
        public byte Delay { get; set; }
    }
}
