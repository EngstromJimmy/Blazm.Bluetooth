# How to use Blazm.Bluetooth

Blazm.Bluetooth makes it easy to connect Blazor to your Bluetooth devices using Web Bluetooth.

Works both Client-side and Server-side.

## Sponsors
Thanks you to much to my sponsors!  
![](https://raw.githubusercontent.com/EngstromJimmy/Blazm.Components/master/Display%20Ads%20Horizontal%20Leaderboard%20728x90%20TOP_RITM0148003.png)


Telerik UI for Blazor – Increase productivity and cut cost in half! Use the Telerik truly native Blazor UI components and high-performing grid to cover any app scenario. [Give it a try for free.](https://www.telerik.com/blazor-ui?utm_source=jimmyengstrom&utm_medium=cpm&utm_campaign=blazor-trial-github-blazmcomp-sponsored-message )


## Getting Started

1. Add Nuget package Blazm.Bluetooth
2. In Program.cs add ```builder.Services.AddBlazmBluetooth();```
3. In the component you want to connect to a device add the Blazm.Bluetooth Namespace
```@using Blazm.Bluetooth```
4. Inject the IBluetoothNavigator (the instance that will communicate with your device)
```@inject IBluetoothNavigator navigator```

Now you are all setup now it is time to connect to a device.

## Connecting to a device

You need to create a query or a filter to filter devices, you could also specify ```AcceptAllDevices``` but that is only recommended while testing.
To take a look at available devices you can use (for Microsoft Edge) edge://bluetooth-internals.
You can query devices using a ServiceId or by name, you also have to specify all the services that you want to access in ```OptionalServices```

To connect to an Andersson (SenSun) scale you need to do the following (check Pages/AnderssonScaleDemo for more info)
Specify the ServiceId and CharacteristicId you want to communicate with.

``` cs
var serviceId = "0000ffb0-0000-1000-8000-00805f9b34fb";
var characteristicId = "0000ffb2-0000-1000-8000-00805f9b34fb";
```

Create a filter

``` cs
var q = new RequestDeviceQuery();
q.Filters.Add(new Filter() { Services = { serviceId } });
```

Request a device

``` cs
var device = await navigator.RequestDeviceAsync(q);
```

This will return a device and it contains an id that you can use to read, write, or set up notifications.
Call the ```SetupNotifyAsync``` to get notifications when the value changes.

``` cs
await device.SetupNotifyAsync(serviceid, characteristics);
device.Notification += Value_Notification;
``` 

and add an event listener that parses the data from the notification.

``` cs
private void Value_Notification(object sender, CharacteristicEventArgs e)
{
    var data = e.Value.ToArray();
    
    // Do something with the data

    StateHasChanged();
}
```

The same thing goes for read and write
``` cs
//Write
await device.WriteValueAsync(serviceid,characteristicsid,value);

//Read
var bytes = await device.ReadValueAsync(serviceid, characteristicId);
```

There is still many scenarios to implement but this should cover the basics.

Please feel free to add ideas /problems/needs you might have or make a PR.
