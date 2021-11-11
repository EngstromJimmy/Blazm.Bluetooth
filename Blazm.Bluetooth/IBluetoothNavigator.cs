using System;
using System.Threading.Tasks;

namespace Blazm.Bluetooth;

public interface IBluetoothNavigator
{
  Task<Device> RequestDeviceAsync(RequestDeviceQuery query);
  Task WriteValueAsync(string deviceId, Guid serviceId, Guid characteristicId, byte[] value);
  Task WriteValueAsync(string deviceId, Guid serviceId, string characteristicId, byte[] value);
  Task WriteValueAsync(string deviceId, string serviceId, Guid characteristicId, byte[] value);
  Task WriteValueAsync(string deviceId, string serviceId, string characteristicId,  byte[] value);
  Task<byte[]> ReadValueAsync(string deviceId, string serviceId, string characteristicId);
  Task<byte[]> ReadValueAsync(string deviceId, Guid serviceId, string characteristicId);
  Task<byte[]> ReadValueAsync(string deviceId, string serviceId, Guid characteristicId);
  Task<byte[]> ReadValueAsync(string deviceId, Guid serviceId, Guid characteristicId);
  Task SetupNotifyAsync(Device device, string serviceId, string characteristicId);
  void TriggerNotification(CharacteristicEventArgs args);
  event EventHandler<CharacteristicEventArgs> Notification;
}