using System.Drawing;

namespace MiPWinRTSDK.MiP.Models
{
    public sealed class ChestLED
    {
        public Color Color { get; internal set; }
        public byte TimeOff { get; internal set; }
        public byte TimeOn { get; internal set; }
    }
}