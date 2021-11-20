using Microsoft.JSInterop;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blazm.Bluetooth
{
  public class BluetoothNavigator : IBluetoothNavigator
  {
    private readonly Lazy<Task<IJSObjectReference>> moduleTask;
    public BluetoothNavigator(IJSRuntime jsRuntime)
    {
      moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
         "import", "./_content/Blazm.Bluetooth/Blazm.Bluetooth.js").AsTask());
    }

    public async Task<Device> RequestDeviceAsync(RequestDeviceQuery query)
    {
      string json = JsonConvert.SerializeObject(query,
                      Formatting.None,
                      new JsonSerializerSettings
                      {
                        NullValueHandling = NullValueHandling.Ignore
                      });
      var module = await moduleTask.Value;
      var device = await module.InvokeAsync<Device>("requestDevice", json);
      device.InitDevice(this);
      return device;
    }

    #region WriteValueAsync
    public async Task WriteValueAsync(string deviceId, Guid serviceId, Guid characteristicId, byte[] value)
    {
      await WriteValueAsync(deviceId, serviceId.ToString(), characteristicId.ToString(), value);
    }

    public async Task WriteValueAsync(string deviceId, Guid serviceId, string characteristicId, byte[] value)
    {
      await WriteValueAsync(deviceId, serviceId.ToString(), characteristicId, value);
    }

    public async Task WriteValueAsync(string deviceId, string serviceId, Guid characteristicId, byte[] value)
    {
      await WriteValueAsync(deviceId, serviceId, characteristicId.ToString(), value);
    }

    public async Task WriteValueAsync(string deviceId, string serviceId, string characteristicId, byte[] value)
    {
      var bytes = value.Select(v => (uint)v).ToArray();
      var module = await moduleTask.Value;
      await module.InvokeVoidAsync("writeValue", deviceId, serviceId.ToLowerInvariant(), characteristicId.ToLowerInvariant(), bytes);
    }
    #endregion

    #region ReadValueAsync
    public async Task<byte[]> ReadValueAsync(string deviceId, string serviceId, string characteristicId)
    {
      var module = await moduleTask.Value;
      var value = await module.InvokeAsync<uint[]>("readValue", deviceId, serviceId.ToLowerInvariant(), characteristicId.ToLowerInvariant());
      return value.Select(v => (byte)(v & 0xFF)).ToArray();
    }

    public async Task<byte[]> ReadValueAsync(string deviceId, Guid serviceId, string characteristicId)
    {
      return await ReadValueAsync(deviceId, serviceId.ToString(), characteristicId);
    }

    public async Task<byte[]> ReadValueAsync(string deviceId, string serviceId, Guid characteristicId)
    {
      return await ReadValueAsync(deviceId, serviceId, characteristicId.ToString());
    }

    public async Task<byte[]> ReadValueAsync(string deviceId, Guid serviceId, Guid characteristicId)
    {
      return await ReadValueAsync(deviceId, serviceId.ToString(), characteristicId.ToString());
    }
    #endregion

    #region SetupNotifyAsync
    private List<DotNetObjectReference<NotificationHandler>> NotificationHandlers = new();
    public async Task SetupNotifyAsync(Device device, string serviceId, string characteristicId)
    {
      var handler = DotNetObjectReference.Create(new NotificationHandler(this));
      NotificationHandlers.Add(handler);
      var module = await moduleTask.Value;
      await module.InvokeVoidAsync("setupNotify", device.Id, serviceId.ToLowerInvariant(), characteristicId.ToLowerInvariant(), handler);
    }
    #endregion

    public void TriggerNotification(CharacteristicEventArgs args)
    {
      Notification?.Invoke(this, args);
    }

    public event EventHandler<CharacteristicEventArgs> Notification;
  }

  public class NotificationHandler
  {
    IBluetoothNavigator bluetoothNavigator;
    public NotificationHandler(IBluetoothNavigator bluetoothNavigator)
    {
      this.bluetoothNavigator = bluetoothNavigator;
    }

    [JSInvokable]
    public void HandleCharacteristicValueChanged(Guid serviceGuid, Guid characteristicGuid, uint[] value)
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
