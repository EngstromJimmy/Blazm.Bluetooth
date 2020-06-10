using Microsoft.JSInterop;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Blazm.Bluetooth
{
    public class BluetoothNavigator
    {
        private IJSRuntime jsRuntime;
        public BluetoothNavigator(IJSRuntime jsRuntime)
        {
            this.jsRuntime = jsRuntime;
        }

        public async Task<Device> RequestDeviceAsync(RequestDeviceQuery query)
        {
            string json=JsonConvert.SerializeObject(query,
                            Newtonsoft.Json.Formatting.None,
                            new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore
                            });

            return await jsRuntime.InvokeAsync<Device>("blazmwebbluetooth.requestDevice",json);
        }

        public async Task WriteValueAsync(string deviceId, string serviceId, string characteristicId,  byte[] value)
        {
            var bytes = value.Select(v => (uint)v).ToArray();
            await jsRuntime.InvokeVoidAsync("blazmwebbluetooth.writeValue", deviceId, serviceId, characteristicId, bytes);
        }

        public async Task<byte[]> ReadValueAsync(string deviceId, string serviceId, string characteristicId)
        {
            var value= await jsRuntime.InvokeAsync<uint[]>("blazmwebbluetooth.readValue", deviceId, serviceId, characteristicId);
            return value.Select(v => (byte)(v & 0xFF)).ToArray();
        }

        private DotNetObjectReference<NotificationHandler> NotificationHandler;
        public async Task SetupNotifyAsync(string id, string serviceGuid, string characteristicGuid)
        {
            if (NotificationHandler == null)
            {
                NotificationHandler = DotNetObjectReference.Create(new NotificationHandler(this));
            }

            await jsRuntime.InvokeVoidAsync("blazmwebbluetooth.setupNotify", id, serviceGuid, characteristicGuid, NotificationHandler);
        }


        public void TriggerNotification(CharacteristicEventArgs args)
        {
            Notification?.Invoke(this, args);
        }
        public event EventHandler<CharacteristicEventArgs> Notification;
    }


    public class NotificationHandler
    {

        BluetoothNavigator bluetoothNavigator;
        public NotificationHandler(BluetoothNavigator bluetoothNavigator)
        {
            this.bluetoothNavigator = bluetoothNavigator;
        }

        [JSInvokable]
        public void HandleCharacteristicValueChanged(Guid serviceGuid, Guid characteristicGuid,uint[] value)
        {
            byte[] byteArray = value.Select(u => (byte)(u & 0xff)).ToArray();

            CharacteristicEventArgs args = new CharacteristicEventArgs();
            args.ServiceId = serviceGuid;
            args.CharacteristicId = characteristicGuid;
            args.Value = byteArray;
            bluetoothNavigator.TriggerNotification(args);
        }
    }

    public class CharacteristicEventArgs : EventArgs
    {
        public Guid ServiceId { get; set; }
        public Guid CharacteristicId { get; set; }
        public byte[] Value { get; set; }
    }
}
