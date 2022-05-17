# Blazm.Bluetooth demo script 
## Blazm
One of the really cool things with Blazor is that it is possible to make JavaScript calls from .NET as well as making .NET calls from JavaScript.
Blazm.Bluetooth make use of both of these.
1. Open **Blazm.Bluetooth/wwwroot/Blazm.Bluetooth.js**
2. Show the **requestDevice** call
Here we make one of the .NET to JavaScript calls, we will call this method, the web browser UI will let us choose a device followed by returning the selected device.
They are also stored in an array on the JavaScript side so that we can access the device later on.
3. Show connect
When we connect to the device we do a couple of retries
4. Show handleCharacteristicValueChanged
When we subscribe to an event the handleCharacteristicValueChanged will be called when the event occurs.
The last line here **.....invokeMethodAsync** will call back to the .NET code.
5. Open BlueToothnavigator.cs
This is where the magic happens.
We get the JavaScript and load that when we need it (no need to manually add that to the HTML).
Here we have all the methods that mkes JavaScript calls.
6. SetupNotify
I won't go into details when it comes to calling .NET code from JavaScript but basically, we create a **DotNetObjectReference** and send that to the JavaScript and then we can use that object to call methods in .NET.


## Andersson scale
1. Add namespace
    ```csharp
    @using Blazm.Bluetooth
    ```
2. AddBluetoothNavigator
    ```csharp
    @inject IBluetoothNavigator navigator
    ```
3. AddWeightVariable
    ```csharp
    int weight = 0;
    ```
4. ShowWeight
    ```csharp
    @weight
    ```
5. Add filter
Now it's time to connect to the device, I have already added the serviceid and characteristicsid.
    ```csharp
    var q = new RequestDeviceQuery();
    q.Filters.Add(new Filter() { Services = { serviceid } });
    ```
6. Request a device
I have kept the terminology from the JavaScript methods so that it's easier to understand and look up.    
    ```csharp
    var q = new RequestDeviceQuery();
    q.Filters.Add(new Filter() { Services = { serviceid } });
    ```
    In this case, we are using a filter asking for all devices that have the serviceid. It will also automatically grant us access to that service and all the characteristics underneath.

7. Set up notifications
For us to be able to receive notifications we need to set them up. 
We can listen to multiple characteristics, but we will only have one notification event to handle them.
    ```csharp
    await device.SetupNotifyAsync(serviceid, characteristics);
    device.Notification += Value_Notification;
    ```
8. Handle the notification
The last step in all of this is to handle the notification.
    ```csharp
    private void Value_Notification(object sender, CharacteristicEventArgs e)
    {
        var data = e.Value.ToArray();
        weight = (256 * data[4]) + data[5];
        if (data[7] == 1)
        {
            weight *= -1;
        }

        StateHasChanged();
    }
    ```
    We get ServiceId, CharacteristicId and an array of bytes which is the data sent from the device.

## Robosapien
1. Add Robosapien
    ``` Csharp
        var q = new RequestDeviceQuery();
        q.Filters.Add(new Filter() { Name = "Robosapien Blue" });
        q.OptionalServices.Add(serviceId.ToString());
        robosapien = await navigator.RequestDeviceAsync(q);
    ```
First, we get the Robosapien device, int this case we use the device name instead of the serviceId, but for this, to work we also need to supply the ServiceId we want to access otherwise we won't know what to give access to.
1. Show the **Devices/Robosapien.cs  
The Robosapien file is basically an enum with bytes for the different things the Robosapien Blue can do.
2. Send command  
Then it's time to send the command.
    ```csharp
    await robosapien.WriteValueAsync(serviceId, characteristicId, new byte[] { command });
    ```
3. Show the buttons
4. Open **devices/Robosapien.cs** 
This file is basically an enum but is represented as a class.
5. Run the app and connect to the Robosapien.

## MiP / MiPosaur
The **MiP robot** and **MiPosaur** have almost the same robot, same serviceid, and characteristics but different names.
In this example, I have created a base class for both robots, **MiPBase**.
1. Open **Devices/MipBase.cs**
Here we have some helper methods, service ids, and stuff like that.
2. Open **Devices/MipRobot.cs**
Here we have the implementation of the robot, making sure values change, and so on. As you might see from the namespace this is a file I have moved from my WinRT library that I wrote a while back.

This is an example that shows that you can easily create classes that handle the connections and commands.


## Deadpool's head
1. Open **Deadpoolsheaddemo.razor**
This device works just like the others but has a bit more requirements, it needs a keep awake signal every 5 seconds.
This is the only way to keep Deadpool quiet, otherwise, he will just talk all the time.
2. Show the **timer** 
We simply set up a timer and every **5 seconds** we send a keep awake command... or keep quiet I guess command.
3. Show **connect**  
Here we simply set up the timer, get the device, set up notifications, and connect the app.
These are commands I sniffed from my iPhone to figure out.
4. When we send a command we wait for 2 seconds before we update the UI and then start the timer again.

