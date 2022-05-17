using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace Blazm.Bluetooth;

[Serializable]
public class Device
{
    private IBluetoothNavigator _webBluetoothNavigator;
    public void InitDevice(IBluetoothNavigator bluetoothNavigator)
    {
        _webBluetoothNavigator = bluetoothNavigator;
    }

    public string Id { get; set; }
    public string Name { get; set; }

    public async Task WriteValueAsync(string serviceId, string characteristicId, byte[] value)
    {
        await _webBluetoothNavigator.WriteValueAsync(this.Id, serviceId, characteristicId, value);
    }

    public async Task<byte[]> ReadValueAsync(string serviceId, string characteristicId)
    {
        return await _webBluetoothNavigator.ReadValueAsync(this.Id, serviceId, characteristicId);
    }

    public async Task SetupNotifyAsync(string serviceId, string characteristicId)
    {
        await _webBluetoothNavigator.SetupNotifyAsync(this, serviceId, characteristicId);
    }

    [JSInvokable]
    public void HandleCharacteristicValueChanged(string serviceId, string characteristicsId, byte[] data)
    {
        Notification?.Invoke(this, new CharacteristicEventArgs() { CharacteristicId = Guid.Parse(characteristicsId), ServiceId = Guid.Parse(serviceId), Value = data });
    }
    public event EventHandler<CharacteristicEventArgs> Notification;
}
