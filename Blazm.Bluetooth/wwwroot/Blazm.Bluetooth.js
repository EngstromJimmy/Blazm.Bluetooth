var PairedBluetoothDevices = [];
export async function requestDevice(query)
{
    var objquery = JSON.parse(query);
    console.log(query);

    var device = await navigator.bluetooth.requestDevice(objquery);
    await device.gatt.connect();
    device.addEventListener('gattserverdisconnected', onDisconnected);
    PairedBluetoothDevices.push(device);
    return { "Name": device.name, "Id": device.id };    
}


export async function  onDisconnected(arg) {
    console.log(arg.srcElement);
    console.log('> Bluetooth Device disconnected');
    connect(arg.srcElement);
}

function connect(bluetoothDevice) {
    exponentialBackoff(3 /* max retries */, 1 /* seconds delay */,
        function toTry() {
            time('Connecting to Bluetooth Device... ');
            return bluetoothDevice.gatt.connect();
        },
        function success() {
            console.log('> Bluetooth Device connected. Try disconnect it now.');
        },
        function fail() {
            time('Failed to reconnect.');
        });
}

function exponentialBackoff(max, delay, toTry, success, fail) {
    toTry().then(result => success(result))
        .catch(_ => {
            if (max === 0) {
                return fail();
            }
            time('Retrying in ' + delay + 's... (' + max + ' tries left)');
            setTimeout(function () {
                exponentialBackoff(--max, delay * 2, toTry, success, fail);
            }, delay * 1000);
        });
}

function time(text) {
    console.log('[' + new Date().toJSON().substr(11, 8) + '] ' + text);
}

function getDevice(deviceId) {
    var device = PairedBluetoothDevices.filter(function (item) {
        return item.id == deviceId;
    });
    return device[0];
}

export async function writeValue(deviceId, serviceId, characteristicId, value)
{
    var device = getDevice(deviceId);
    console.log(device);
    if (device.gatt.connected) {
        var service = await device.gatt.getPrimaryService(serviceId);
        var characteristic = await service.getCharacteristic(characteristicId);
        var b = Uint8Array.from(value);
        await characteristic.writeValue(b);
    }
    else
    {
        await sleep(1000);
        await writeValue(deviceId, serviceId, characteristicId, value);
    }
}

function sleep(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
}

export async function readValue(deviceId, serviceId, characteristicId)
{
    var device = getDevice(deviceId);

    var service = await device.gatt.getPrimaryService(serviceId);
    var characteristic = await service.getCharacteristic(characteristicId);

    var value = await characteristic.readValue();
    var uint8Array = new Uint8Array(value.buffer);
    var array = Array.from(uint8Array);
    return array;
}

var NotificationHandler = [];

export async function setupNotify(deviceId, serviceId, characteristicId, notificationHandler)
{
    console.log("Setting up");
    var device = getDevice(deviceId);
    device.NotificationHandler = notificationHandler;
    var service = await device.gatt.getPrimaryService(serviceId);
    var characteristic = await service.getCharacteristic(characteristicId);
    await characteristic.startNotifications();
    characteristic.addEventListener('characteristicvaluechanged', handleCharacteristicValueChanged);
    console.log("Characteristics listening");
}

async function handleCharacteristicValueChanged(event) {
    var value = event.target.value;
    var deviceId = event.target.service.device.id;
    var uint8Array = new Uint8Array(value.buffer);
    var device = getDevice(deviceId);
    await device.NotificationHandler.invokeMethodAsync('HandleCharacteristicValueChanged', event.target.service.uuid, event.target.uuid, uint8Array);
}