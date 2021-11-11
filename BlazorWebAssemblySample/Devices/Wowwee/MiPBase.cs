using Blazm.Bluetooth;
using MiPWinRTSDK.MiP.Models;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace MiPWinRTSDK;

class MiPBase : INotifyPropertyChanged
{
    protected IBluetoothNavigator Navigator { get; set; }

    public Device Device { get; set; }

    #region Properties
    private byte chestLEDTimeOff;
    private byte chestLEDTimeOn;
    private Color chestLEDColor;
    public Color ChestLEDColor
    {
        get
        {
            return chestLEDColor;
        }
        set
        {
            chestLEDColor = value;
            RaisePropertyChanged();
        }
    }
    public byte ChestLEDTimeOn
    {
        get
        {
            return chestLEDTimeOn;
        }
        set
        {
            chestLEDTimeOn = value;
            RaisePropertyChanged();
        }
    }
    public byte ChestLEDTimeOff
    {
        get
        {
            return chestLEDTimeOff;
        }
        set
        {
            chestLEDTimeOff = value;
            RaisePropertyChanged();
        }
    }
    #endregion




    public async Task DistanceDrive(DirectionEnum direction, byte distance, TurnEnum turn, int angle)
    {
        List<byte> bytes = new List<byte>();

        bytes.Add((byte)direction);
        bytes.Add(distance);
        bytes.Add((byte)turn);
        byte highbyte = (byte)((angle >> 8) & 0xff);
        byte lowbyte = (byte)(angle & 0xff);
        bytes.Add(highbyte);
        bytes.Add(lowbyte);
        await SendCommand(0x70, bytes);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="speed">Speed (0~30)</param>
    /// <param name="timeInMiliseconds">Time in 7ms intervals (0~255) </param>
    /// <returns></returns>
    public async Task DriveForwardWithTime(byte speed, byte timeInMiliseconds)
    {
        List<byte> bytes = new List<byte>();
        byte time = (byte)(timeInMiliseconds);
        bytes.Add(speed);
        bytes.Add(time);
        await SendCommand(0x71, bytes);
    }

    public async Task DriveBackwardWithTime(byte speed, byte timeInMiliseconds)
    {
        List<byte> bytes = new List<byte>();
        byte time = (byte)(timeInMiliseconds / 7);
        bytes.Add(speed);
        bytes.Add(time);
        await SendCommand(0x72, bytes);
    }


    public async Task TurnLeftByAngle(byte speed, int angle)
    {
        List<byte> bytes = new List<byte>();
        bytes.Add((byte)(angle / 5));
        bytes.Add(speed);
        await SendCommand(0x73, bytes);
    }
    public async Task TurnRightByAngle(byte speed, int angle)
    {
        List<byte> bytes = new List<byte>();
        bytes.Add((byte)(angle / 5));
        bytes.Add(speed);
        await SendCommand(0x74, bytes);
    }

    public async Task ContinuousDrive(byte direction, byte spin)
    {
        List<byte> bytes = new List<byte>();
        bytes.Add(direction);
        bytes.Add(spin);
        await SendCommand(0x78, bytes);
    }

    public async Task DelayTimeBetweenTwoClaps(int delay)
    {
        List<byte> bytes = new List<byte>();
        byte highbyte = (byte)((delay >> 8) & 0xff);
        byte lowbyte = (byte)(delay & 0xff);
        await SendCommand(0x20, bytes);
    }

    internal async Task SendCommand(byte command, List<byte> commandBytes)
    {
        //if (this.Device== BluetoothConnectionStatus.Connected)
        //{
        var serviceGuid = "0000ffe5-0000-1000-8000-00805f9b34fb";
        var characteristicGuid = "0000ffe9-0000-1000-8000-00805f9b34fb";

        //Add command to the beginning
        commandBytes.Insert(0, command);

        await Navigator.WriteValueAsync(Device.Id, serviceGuid, characteristicGuid, commandBytes.ToArray());

        //await this.Device.WriteValueAsync(serviceGuid, characteristicGuid, commandBytes.ToArray());
        //}
    }

    public async Task RequestChestLED()
    {
        List<byte> bytes = new List<byte>();
        await SendCommand(0x83, bytes);
    }


    public async Task SetChestLED(byte red, byte green, byte blue)
    {
        List<byte> bytes = new List<byte>();
        bytes.Add((byte)red);
        bytes.Add((byte)green);
        bytes.Add((byte)blue);
        await SendCommand(0x84, bytes);
    }


    public async Task FlashChestLED(byte red, byte green, byte blue, byte timeOn, byte timeOff)
    {
        List<byte> bytes = new List<byte>();
        bytes.Add((byte)red);
        bytes.Add((byte)green);
        bytes.Add((byte)blue);
        bytes.Add((byte)timeOn);
        bytes.Add((byte)timeOff);
        await SendCommand(0x89, bytes);
    }


    public async Task SetHeadLED(LightEnum light1, LightEnum light2, LightEnum light3, LightEnum light4)
    {
        List<byte> bytes = new List<byte>();
        bytes.Add((byte)light1);
        bytes.Add((byte)light2);
        bytes.Add((byte)light3);
        bytes.Add((byte)light4);
        await SendCommand(0x8a, bytes);
    }


    public async Task RequestHeadLED()
    {
        List<byte> bytes = new List<byte>();
        await SendCommand(0x8b, bytes);
    }

    public async Task SetUserData(byte address, byte data)
    {
        List<byte> bytes = new List<byte>();
        bytes.Add(address);
        bytes.Add(data);
        await SendCommand(0x12, bytes);
    }

    public async Task GetUserOrOtherEepromData(byte address)
    {
        List<byte> bytes = new List<byte>();
        bytes.Add((byte)address);
        await SendCommand(0x13, bytes);
    }

    public async Task GetHardwareInfo()
    {
        List<byte> bytes = new List<byte>();
        await SendCommand(0x19, bytes);
    }

    public async Task GetVolume()
    {
        List<byte> bytes = new List<byte>();
        await SendCommand(0x16, bytes);
    }

    public async Task PlaySound(MiPSound[] sounds, byte numberOfTimesToRepeat)
    {
        List<byte> bytes = new List<byte>();
        foreach (var s in sounds)
        {
            if (s.Volume == null)
                bytes.Add(s.Sound.Value);
            else
                bytes.Add((byte)(s.Volume.Value + 0xf7));
        }
        bytes.Add(numberOfTimesToRepeat);
        await SendCommand(0x06, bytes);
    }

    public async Task Sleep()
    {
        List<byte> bytes = new List<byte>();
        await SendCommand(0xFA, bytes);
    }

    public async Task DisconnectApp()
    {
        List<byte> bytes = new List<byte>();
        await SendCommand(0xFE, bytes);
    }
    public async Task Stop()
    {
        List<byte> bytes = new List<byte>();
        await SendCommand(0x77, bytes);
    }


    public async Task RequestStatus()
    {
        List<byte> bytes = new List<byte>();
        await SendCommand(0x79, bytes);
    }


    #region Recieve
    //GattCharacteristic CharacteristicsNotify { get; set; }

    public virtual void Connect()
    {
            
    }
        

    public byte[] StringToByteArray(string hex)
    {
        return Enumerable.Range(0, hex.Length)
                            .Where(x => x % 2 == 0)
                            .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                            .ToArray();
    }
    #endregion

    #region Events    
    public event EventHandler<object> ShakeDetectedRecievedEvent;
    protected void OnShakeDetectedRecieved()
    {
        ShakeDetectedRecievedEvent?.Invoke(this, null);
    }

    public event EventHandler<ChestLED> ChestLEDRecievedEvent;
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnChestLEDRecieved(ChestLED chestLED)
    {
        ChestLEDRecievedEvent?.Invoke(this, chestLED);
    }
    #endregion

    internal void RaisePropertyChanged([CallerMemberName] string caller = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(caller));
    }
}

