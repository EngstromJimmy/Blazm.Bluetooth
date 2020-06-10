using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace Blazm.Bluetooth
{
    [Serializable]
    public class Device
    {
        public string Id { get; set; }
        public string Name { get; set; }

    }
}
