window.blazmwebbluetooth = {};

var PairedBluetoothDevices = [];
window.blazmwebbluetooth.requestDevice = async (query) => {    
    var objquery = JSON.parse(query);
    console.log(query);

    var device = await navigator.bluetooth.requestDevice(objquery);
    console.log(device);
    await device.gatt.connect();
    device.addEventListener('gattserverdisconnected', onDisconnected);
    PairedBluetoothDevices.push(device);

    return { "Name": device.name, "Id": device.id };    
}


function onDisconnected() {
    console.log('> Bluetooth Device disconnected');
    connect();
}

function connect(bluetoothDevice) {
    exponentialBackoff(3 /* max retries */, 2 /* seconds delay */,
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

window.blazmwebbluetooth.writeValue = async (deviceId,serviceId,characteristicId,value) => {

    var device = PairedBluetoothDevices.filter(function (item) {
        return item.id==deviceId;
    });

    var service = await device[0].gatt.getPrimaryService(serviceId);
    var characteristic = await service.getCharacteristic(characteristicId);
    var b = Uint8Array.from(value);
    await characteristic.writeValue(b);
        
}

window.blazmwebbluetooth.readValue = async (deviceId, serviceId, characteristicId) => {


    var device = PairedBluetoothDevices.filter(function (item) {
        return item.id == deviceId;
    });

    var service = await device[0].gatt.getPrimaryService(serviceId);
    var characteristic = await service.getCharacteristic(characteristicId);

    var value = await characteristic.readValue();
    var uint8Array = new Uint8Array(value.buffer);
    var array = Array.from(uint8Array);
    return array;
}

var NotificationHandler = [];

window.blazmwebbluetooth.setupNotify = async (deviceId, serviceId, characteristicId,notificationHandler) => {
    NotificationHandler=notificationHandler;

    var device = PairedBluetoothDevices.filter(function (item) {
        return item.id == deviceId;
    });

    var service = await device[0].gatt.getPrimaryService(serviceId);
    var characteristic = await service.getCharacteristic(characteristicId);
    await characteristic.startNotifications();
    characteristic.addEventListener('characteristicvaluechanged', handleCharacteristicValueChanged);
    console.log("Characteristics listening");
}

async function handleCharacteristicValueChanged(event) {

    var value = event.target.value;
    
    var uint8Array = new Uint8Array(value.buffer);

    var array = Array.from(uint8Array)
    console.log(JSON.stringify(array));
    await NotificationHandler.invokeMethodAsync('HandleCharacteristicValueChanged', event.target.service.uuid, event.target.uuid, array);
    
}