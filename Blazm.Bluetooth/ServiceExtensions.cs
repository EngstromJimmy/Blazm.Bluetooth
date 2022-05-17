using Microsoft.Extensions.DependencyInjection;

namespace Blazm.Bluetooth;

public static class ServiceExtensions
{
    public static IServiceCollection AddBlazmBluetooth(this IServiceCollection services)
    {
        return services.AddTransient<IBluetoothNavigator, BluetoothNavigator>();
    }
}
