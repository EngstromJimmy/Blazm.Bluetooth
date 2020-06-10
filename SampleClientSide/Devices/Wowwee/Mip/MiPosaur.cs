using MiPWinRTSDK.Interfaces;
using MiPWinRTSDK.MiP.Models.MiPosaur;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Foundation;

namespace MiPWinRTSDK.MiP
{
    class MiPosaur : MiPBase
    {
        public MiPosaur(DeviceInformation device)
        {
            Device = device;
        }

        public IAsyncAction PlayAnimation(byte animationID, bool enableSound)
        {
            List<byte> bytes = new List<byte>();
            bytes.Add(((byte)animationID));
            bytes.Add(((byte)(enableSound ? 0x00 : 0x01)));
            return SendCommand(0x76, bytes).AsAsyncAction();
        }

        public IAsyncAction SetPosition(PositionEnum position)
        {
            List<byte> bytes = new List<byte>();
            //Direction
            bytes.Add(((byte)position));
            return SendCommand(0x08, bytes).AsAsyncAction();
        }

        //Events

        public event EventHandler<byte> AnimationFinishedRecievedEvent;
        private void OnAnimationFinishedRecieved(byte animationID)
        {
            AnimationFinishedRecievedEvent?.Invoke(this, animationID);
        }

        //Play Animation Finished 0x76
        //Miposour Status	0x79

        internal override void CharacteristicsNotify_ValueChanged(Windows.Devices.Bluetooth.GenericAttributeProfile.GattCharacteristic sender, Windows.Devices.Bluetooth.GenericAttributeProfile.GattValueChangedEventArgs args)
        {
            var data = args.CharacteristicValue.ToArray();
            var hexstring = UTF8Encoding.UTF8.GetString(data, 0, data.Count());
            var bytes = StringToByteArray(hexstring);
            switch (bytes[0])
            {
                case 0x76: //GetCurrentMIPGameMode
                    OnAnimationFinishedRecieved(bytes[1]);
                    break;
            }
        }
    }
}