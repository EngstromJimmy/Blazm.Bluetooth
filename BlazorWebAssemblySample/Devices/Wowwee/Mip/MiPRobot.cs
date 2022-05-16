using Blazm.Bluetooth;
using MiPWinRTSDK.MiP.Models;
using MiPWinRTSDK.MiP.Models.MiP;
using System.Drawing;
using System.Text;

namespace MiPWinRTSDK.MiP;

class MiPRobot : MiPBase
{
    public MiPRobot(IBluetoothNavigator navigator)
    {
        base.Navigator = navigator;
    }

    public override async void Connect()
    {
        //try
        //{
        var q = new RequestDeviceQuery();
        //q.Filters.Add(new Filter() { Name = "WowWee-MiP-27244" });
        q.Filters.Add(new Filter() { Name = "Miposaur-17704" });
        q.OptionalServices.Add("battery_service");
        q.OptionalServices.Add(WriteServiceGuid); //Write service
        q.OptionalServices.Add(ReadServiceGuid); //Read service

        try
        {
            Device = await Navigator.RequestDeviceAsync(q);
            Console.WriteLine(Device);

            await Device.SetupNotifyAsync(ReadServiceGuid, ReadCharacteristicGuid);
            Device.Notification += Value_Notification;

            base.Connect();
        }
        catch { }
    }

    private void Value_Notification(object sender, CharacteristicEventArgs e)
    {
        var data = e.Value.ToArray();
        var hexstring = UTF8Encoding.UTF8.GetString(data, 0, data.Count());
        var bytes = StringToByteArray(hexstring);

        switch (bytes[0])
        {
            case 0x82: //GetCurrentMIPGameMode
                GameMode = (GameModeEmum)Enum.Parse(typeof(GameModeEmum), bytes[1].ToString());
                OnGameModeRecieved(GameMode);
                break;
            case 0x03: //Receive IR Dongle code
                OnIRDongleCodeRecievedEvent(bytes.ToList());
                break;
            case 0x79: //Request MIP status  
                BatteryLevel = bytes[1];
                Status = (StatusEnum)Enum.Parse(typeof(StatusEnum), bytes[2].ToString());
                OnMiPStatusRecieved(new MiPStatus() { BatteryLevel = BatteryLevel, Status = Status });
                break;
            case 0x81: //Request weight update
                Weight = bytes[1];
                OnWeightUpdateRecieved(Weight);
                break;
            case 0x83: //RequestChestLED
                ChestLEDColor = Color.FromArgb(255, bytes[1], bytes[2], bytes[3]);
                if (bytes.Count() >= 5)
                {
                    ChestLEDTimeOn = bytes[4];
                }
                if (bytes.Count() == 6)
                {
                    ChestLEDTimeOff = bytes[5];
                }

                OnChestLEDRecieved(new ChestLED() { Color = ChestLEDColor, TimeOn = ChestLEDTimeOn, TimeOff = ChestLEDTimeOff });
                break;
            case 0x8b:// RequestHeadLED
                Light1 = (LightEnum)Enum.Parse(typeof(LightEnum), bytes[1].ToString());
                Light2 = (LightEnum)Enum.Parse(typeof(LightEnum), bytes[2].ToString());
                Light3 = (LightEnum)Enum.Parse(typeof(LightEnum), bytes[3].ToString());
                Light4 = (LightEnum)Enum.Parse(typeof(LightEnum), bytes[4].ToString());
                OnHeadLEDRecieved(new HeadLED() { Light1 = Light1, Light2 = Light2, Light3 = Light3, Light4 = Light4 });
                break;
            case 0x0A: //GestureDetect
                var detectedGesture = (GestureEnum)Enum.Parse(typeof(GestureEnum), bytes[1].ToString());
                OnGestureRecieved(detectedGesture);
                break;
            case 0x0D: //RadarMode
                RadarMode = (GestureRadarEnum)Enum.Parse(typeof(GestureRadarEnum), bytes[1].ToString());
                OnRadarModeRecieved(RadarMode);
                break;
            case 0x0C: //Radar Response
                byte response = bytes[1];
                OnRadarRecieved(response);
                break;
            case 0x0F: //Mip Detection Status
                MiPDetectionMode = bytes[1];
                MiPDetectionIRTxPower = bytes[2];
                OnMiPDetectionStatusRecieved(new MiPDetectionStatus() { Mode = MiPDetectionMode, IRTxPower = MiPDetectionIRTxPower });
                break;
            case 0x04: //Mip Detected  
                var mipsettingNumber = bytes[1];
                OnMipDetectedRecieved(mipsettingNumber);
                break;
            case 0x1A: //Shake Detected 
                OnShakeDetectedRecieved();
                break;
            case 0x11: //IR Control Status 
                IRControlStatus = bytes[1];
                OnIRControlStatusRecieved(IRControlStatus);
                break;
            case 0xFA: //Sleep
                OnSleepRecieved();
                break;
            case 0x13: //MIP User Or Other Eeprom Data
                var address = bytes[1];
                var userEepromdata = bytes[2];
                OnEePromDataRecieved(new EepromData() { Address = address, Data = userEepromdata });
                break;
            case 0x14: //Mip Software Version 
                MiPVersion = string.Format("{0}.{1}.{2}.{3}", Convert.ToInt32(bytes[1]), Convert.ToInt32(bytes[2]), Convert.ToInt32(bytes[3]), Convert.ToInt32(bytes[4]));
                OnSoftwareVersionRecieved(MiPVersion);
                break;
            case 0x19: //Mip Hardware Info 
                VoiceChipVersion = bytes[1];
                HardwareVersion = bytes[2];
                OnMiPHardwareVersionRecieved(new MiPHardwareVersion() { VoiceChipVersion = VoiceChipVersion, HardwareVersion = HardwareVersion });
                break;
            case 0x16: //GetVolume
                Volume = bytes[1];
                OnVolumeRecieved(Volume);
                break;
            case 0x1d: //Clap times
                var times = bytes[1];
                OnClapTimesRecieved(times);
                break;
            case 0x1f: //Clap Status  
                var isEnabled = bytes[1] == 0x00 ? false : true;
                int delayTime = (bytes[2] << 8) & bytes[3];
                OnClapStatusRecieved(new ClapStatus() { Enabled = isEnabled, DelayTime = delayTime });
                break;
            default:
                //Value_Notification(bytes);
                break;
        }
    }

    private byte volume = 7;
    private byte hardwareVersion;
    private byte voiceChipVersion;
    private string miPVersion;
    private byte iRControlStatus;
    private byte mipDetectionIRTxPower;
    private byte mipDetectionMode;
    private GestureRadarEnum radarMode;
    private LightEnum light4;
    private LightEnum light3;
    private LightEnum light2;
    private LightEnum light1;

    private byte weight;
    private byte batteryLevel;
    private StatusEnum status;
    private GameModeEmum gameMode;
    public GameModeEmum GameMode
    {
        get
        {
            return gameMode;
        }
        set
        {
            gameMode = value;
            RaisePropertyChanged();
        }
    }
    public StatusEnum Status
    {
        get
        {
            return status;
        }
        private set
        {
            status = value;
            RaisePropertyChanged();
        }
    }
    public byte BatteryLevel
    {
        get
        {
            return batteryLevel;
        }
        private set
        {
            batteryLevel = value;
            RaisePropertyChanged();
        }
    }
    public byte Weight
    {
        get
        {
            return weight;
        }
        private set
        {
            weight = value;
            RaisePropertyChanged();
        }
    }

    public LightEnum Light1
    {
        get
        {
            return light1;
        }
        set
        {
            light1 = value;
            RaisePropertyChanged();
        }
    }
    public LightEnum Light2
    {
        get
        {
            return light2;
        }
        set
        {
            light2 = value;
            RaisePropertyChanged();
        }
    }
    public LightEnum Light3
    {
        get
        {
            return light3;
        }
        set
        {
            light3 = value;
            RaisePropertyChanged();
        }
    }
    public LightEnum Light4
    {
        get
        {
            return light4;
        }
        set
        {
            light4 = value;
            RaisePropertyChanged();
        }
    }

    public GestureRadarEnum RadarMode
    {
        get
        {
            return radarMode;
        }
        private set
        {
            radarMode = value;
            RaisePropertyChanged();
        }
    }

    public byte MiPDetectionMode
    {
        get
        {
            return mipDetectionMode;
        }
        private set
        {
            mipDetectionMode = value;
            RaisePropertyChanged();
        }
    }
    public byte MiPDetectionIRTxPower
    {
        get
        {
            return mipDetectionIRTxPower;
        }
        private set
        {
            mipDetectionIRTxPower = value;
            RaisePropertyChanged();
        }
    }

    public byte IRControlStatus
    {
        get
        {
            return iRControlStatus;
        }
        set
        {
            iRControlStatus = value;
            RaisePropertyChanged();
        }
    }

    public string MiPVersion
    {
        get
        {
            return miPVersion;
        }
        private set
        {
            miPVersion = value;
            RaisePropertyChanged();
        }
    }
    public byte VoiceChipVersion
    {
        get
        {
            return voiceChipVersion;
        }
        private set
        {
            voiceChipVersion = value;
            RaisePropertyChanged();
        }
    }
    public byte HardwareVersion
    {
        get
        {
            return hardwareVersion;
        }
        private set
        {
            hardwareVersion = value;
            RaisePropertyChanged();
        }
    }

    public byte Volume
    {
        get
        {
            return volume;
        }
        private set
        {
            volume = value;
            RaisePropertyChanged();
        }
    }

    public async Task SetPosition(PositionEnum position)
    {
        List<byte> bytes = new List<byte>();
        //Direction
        bytes.Add(((byte)position));
        await SendCommand(0x08, bytes);
    }

    public async Task SetGameMode(GameModeEmum gamemode)
    {
        List<byte> bytes = new List<byte>();
        bytes.Add((byte)gamemode);
        await SendCommand(0x76, bytes);
    }

    public async Task GetCurrentMIPGameMode()
    {
        List<byte> bytes = new List<byte>();
        await SendCommand(0x82, bytes);
    }

    public async Task MIPGetUp(GetUpEnum getUp)
    {
        List<byte> bytes = new List<byte>();
        bytes.Add((byte)getUp);
        await SendCommand(0x23, bytes);
    }

    public async Task RequestWeightUpdate()
    {
        List<byte> bytes = new List<byte>();

        await SendCommand(0x81, bytes);
    }

    public async Task ReadOdometer()
    {
        List<byte> bytes = new List<byte>();
        await SendCommand(0x85, bytes);
    }

    public async Task ResetOdometer()
    {
        List<byte> bytes = new List<byte>();
        await SendCommand(0x86, bytes);
    }
    public async Task SetGestureOrRadarMode(GestureRadarEnum mode)
    {
        List<byte> bytes = new List<byte>();
        bytes.Add((byte)mode);
        await SendCommand(0x0C, bytes);
    }

    public async Task GetRadarMode()
    {
        List<byte> bytes = new List<byte>();
        await SendCommand(0x0D, bytes);
    }

    public async Task SetMiPDetectionMode(byte idNumber, byte irTxPower)
    {
        List<byte> bytes = new List<byte>();
        bytes.Add((byte)idNumber);
        bytes.Add((byte)irTxPower);
        await SendCommand(0x0E, bytes);
    }

    public async Task RequestMiPDetectionMode()
    {
        List<byte> bytes = new List<byte>();
        await SendCommand(0x0F, bytes);
    }

    public async Task IRRemoteControlEnabled(bool enabled)
    {
        List<byte> bytes = new List<byte>();
        bytes.Add((byte)(enabled ? 0x01 : 0x00));
        await SendCommand(0x10, bytes);
    }

    public async Task RequestIRControlEnabled()
    {
        List<byte> bytes = new List<byte>();
        await SendCommand(0x11, bytes);
    }

    public async Task ForceBLEDisconnect()
    {
        List<byte> bytes = new List<byte>();
        await SendCommand(0xFC, bytes);
    }

    public async Task GetSoftwareVersion()
    {
        List<byte> bytes = new List<byte>();
        await SendCommand(0x14, bytes);
    }

    public async Task SetVolume(byte volume)
    {
        List<byte> bytes = new List<byte>();
        bytes.Add((byte)volume);
        await SendCommand(0x15, bytes);
    }

    /// <summary>
    /// </summary>
    /// <param name="byte1">IR data bit23 ~bit16       </param>
    /// <param name="byte2">IR data bit15 ~bit8  </param>
    /// <param name="byte3">IR data bit7 ~bit0</param>
    /// <param name="byte4">IR data bit7 ~bit0 </param>
    /// <param name="irdDataNumbers">Data numbers(1~32):e.g.BYTE5=0x08 means BYTE4 is useful. </param>
    /// <param name="irTxPower">IR Tx power(1~120)(About 1cm ~300cm)</param>
    /// <returns></returns>
    public async Task SendIRDongleCode(byte byte1, byte byte2, byte byte3, byte byte4, byte irdDataNumbers, byte irTxPower)
    {
        List<byte> bytes = new List<byte>();
        bytes.Add((byte)byte1);
        bytes.Add((byte)byte2);
        bytes.Add((byte)byte3);
        bytes.Add((byte)byte4);
        bytes.Add((byte)irdDataNumbers);
        bytes.Add((byte)irTxPower);
        await SendCommand(0x8C, bytes);
    }

    public async Task ClapEnabled(bool enabled)
    {
        List<byte> bytes = new List<byte>();
        bytes.Add((byte)(enabled ? 0x01 : 0x00));
        await SendCommand(0x1e, bytes);
    }
    public async Task RequestClapEnabled()
    {
        List<byte> bytes = new List<byte>();
        await SendCommand(0x1f, bytes);
    }

    public event EventHandler<GameModeEmum> GameModeRecievedEvent;
    private void OnGameModeRecieved(GameModeEmum gameMode)
    {
        GameModeRecievedEvent?.Invoke(this, gameMode);
    }

    public event EventHandler<IList<byte>> IRDongleCodeRecievedEvent;
    private void OnIRDongleCodeRecievedEvent(IList<byte> irDongleCode)
    {
        IRDongleCodeRecievedEvent?.Invoke(this, irDongleCode);
    }

    public event EventHandler<MiPStatus> MiPStatusRecievedEvent;
    private void OnMiPStatusRecieved(MiPStatus status)
    {
        MiPStatusRecievedEvent?.Invoke(this, status);
    }

    public event EventHandler<byte> WeightUpdateRecievedEvent;
    private void OnWeightUpdateRecieved(byte weight)
    {
        WeightUpdateRecievedEvent?.Invoke(this, weight);
    }

    public event EventHandler<HeadLED> HeadLEDRecievedEvent;
    private void OnHeadLEDRecieved(HeadLED headLED)
    {
        HeadLEDRecievedEvent?.Invoke(this, headLED);
    }

    public event EventHandler<GestureEnum> GestureRecievedEvent;
    private void OnGestureRecieved(GestureEnum gesture)
    {
        GestureRecievedEvent?.Invoke(this, gesture);
    }

    public event EventHandler<GestureRadarEnum> RadarModeRecievedEvent;
    private void OnRadarModeRecieved(GestureRadarEnum gestureRadarMode)
    {
        RadarModeRecievedEvent?.Invoke(this, gestureRadarMode);
    }

    public event EventHandler<byte> RadarRecievedEvent;
    private void OnRadarRecieved(byte radar)
    {
        RadarRecievedEvent?.Invoke(this, radar);
    }

    public event EventHandler<MiPDetectionStatus> MiPDetectionStatusRecievedEvent;
    private void OnMiPDetectionStatusRecieved(MiPDetectionStatus mipDetection)
    {
        MiPDetectionStatusRecievedEvent?.Invoke(this, mipDetection);
    }

    public event EventHandler<byte> MipDetectedRecievedEvent;
    private void OnMipDetectedRecieved(byte mipSettingsnumber)
    {
        MipDetectedRecievedEvent?.Invoke(this, mipSettingsnumber);
    }

    public event EventHandler<byte> IRControlStatusRecievedEvent;
    private void OnIRControlStatusRecieved(byte irControlStatus)
    {
        IRControlStatusRecievedEvent?.Invoke(this, IRControlStatus);
    }

    public event EventHandler<object> SleepRecievedEvent;
    private void OnSleepRecieved()
    {
        SleepRecievedEvent?.Invoke(this, null);
    }

    public event EventHandler<EepromData> EePromDataRecievedEvent;
    private void OnEePromDataRecieved(EepromData eepromData)
    {
        EePromDataRecievedEvent?.Invoke(this, eepromData);
    }

    public event EventHandler<string> SoftwareVersionRecievedEvent;
    private void OnSoftwareVersionRecieved(string version)
    {
        SoftwareVersionRecievedEvent?.Invoke(this, version);
    }

    public event EventHandler<MiPHardwareVersion> MiPHardwareVersionRecievedEvent;
    private void OnMiPHardwareVersionRecieved(MiPHardwareVersion hardwareVersion)
    {
        MiPHardwareVersionRecievedEvent?.Invoke(this, hardwareVersion);
    }

    public event EventHandler<byte> VolumeRecievedEvent;
    private void OnVolumeRecieved(byte volume)
    {
        VolumeRecievedEvent?.Invoke(this, volume);
    }

    public event EventHandler<byte> ClapTimesRecievedEvent;
    private void OnClapTimesRecieved(byte claps)
    {
        ClapTimesRecievedEvent?.Invoke(this, claps);
    }

    public event EventHandler<ClapStatus> ClapStatusRecievedEvent;
    private void OnClapStatusRecieved(ClapStatus clapStatus)
    {
        ClapStatusRecievedEvent?.Invoke(this, clapStatus);
    }
    //internal override void CharacteristicsNotify_ValueChanged(Windows.Devices.Bluetooth.GenericAttributeProfile.GattCharacteristic sender, Windows.Devices.Bluetooth.GenericAttributeProfile.GattValueChangedEventArgs args)
    //{
    //    var data = args.CharacteristicValue.ToArray();
    //    var hexstring = UTF8Encoding.UTF8.GetString(data, 0, data.Count());
    //    var bytes = StringToByteArray(hexstring);
    //    switch (bytes[0])
    //    {
    //        case 0x82: //GetCurrentMIPGameMode
    //            GameMode = (GameModeEmum)Enum.Parse(typeof(GameModeEmum), bytes[1].ToString());
    //            OnGameModeRecieved(GameMode);
    //            break;
    //        case 0x03: //Receive IR Dongle code
    //            OnIRDongleCodeRecievedEvent(bytes.ToList());
    //            break;
    //        case 0x79: //Request MIP status  
    //            BatteryLevel = bytes[1];
    //            Status = (StatusEnum)Enum.Parse(typeof(StatusEnum), bytes[2].ToString());
    //            OnMiPStatusRecieved(new MiPStatus() { BatteryLevel = BatteryLevel, Status = Status });
    //            break;
    //        case 0x81: //Request weight update
    //            Weight = bytes[1];
    //            OnWeightUpdateRecieved(Weight);
    //            break;
    //        //case 0x83: //RequestChestLED
    //        //    ChestLEDColor = Color.FromArgb(255, bytes[1], bytes[2], bytes[3]);
    //        //    if (bytes.Count() >= 5)
    //        //    {
    //        //        ChestLEDTimeOn = bytes[4];
    //        //    }
    //        //    if (bytes.Count() == 6)
    //        //    {
    //        //        ChestLEDTimeOff = bytes[5];
    //        //    }

    //        //    OnChestLEDRecieved(new ChestLED() { Color = ChestLEDColor, TimeOn = ChestLEDTimeOn, TimeOff = ChestLEDTimeOff });
    //        //    break;
    //        case 0x8b:// RequestHeadLED
    //            Light1 = (LightEnum)Enum.Parse(typeof(LightEnum), bytes[1].ToString());
    //            Light2 = (LightEnum)Enum.Parse(typeof(LightEnum), bytes[2].ToString());
    //            Light3 = (LightEnum)Enum.Parse(typeof(LightEnum), bytes[3].ToString());
    //            Light4 = (LightEnum)Enum.Parse(typeof(LightEnum), bytes[4].ToString());
    //            OnHeadLEDRecieved(new HeadLED() { Light1 = Light1, Light2 = Light2, Light3 = Light3, Light4 = Light4 });
    //            break;
    //        case 0x0A: //GestureDetect
    //            var detectedGesture = (GestureEnum)Enum.Parse(typeof(GestureEnum), bytes[1].ToString());
    //            OnGestureRecieved(detectedGesture);
    //            break;
    //        case 0x0D: //RadarMode
    //            RadarMode = (GestureRadarEnum)Enum.Parse(typeof(GestureRadarEnum), bytes[1].ToString());
    //            OnRadarModeRecieved(RadarMode);
    //            break;
    //        case 0x0C: //Radar Response
    //            byte response = bytes[1];
    //            OnRadarRecieved(response);
    //            break;
    //        case 0x0F: //Mip Detection Status
    //            MiPDetectionMode = bytes[1];
    //            MiPDetectionIRTxPower = bytes[2];
    //            OnMiPDetectionStatusRecieved(new MiPDetectionStatus() { Mode = MiPDetectionMode, IRTxPower = MiPDetectionIRTxPower });
    //            break;
    //        case 0x04: //Mip Detected  
    //            var mipsettingNumber = bytes[1];
    //            OnMipDetectedRecieved(mipsettingNumber);
    //            break;
    //        //case 0x1A: //Shake Detected 
    //        //    OnShakeDetectedRecieved();
    //        //    break;
    //        case 0x11: //IR Control Status 
    //            IRControlStatus = bytes[1];
    //            OnIRControlStatusRecieved(IRControlStatus);
    //            break;
    //        case 0xFA: //Sleep
    //            OnSleepRecieved();
    //            break;
    //        case 0x13: //MIP User Or Other Eeprom Data
    //            var address = bytes[1];
    //            var userEepromdata = bytes[2];
    //            OnEePromDataRecieved(new EepromData() { Address = address, Data = userEepromdata });
    //            break;
    //        case 0x14: //Mip Software Version 
    //            MiPVersion = string.Format("{0}.{1}.{2}.{3}", Convert.ToInt32(bytes[1]), Convert.ToInt32(bytes[2]), Convert.ToInt32(bytes[3]), Convert.ToInt32(bytes[4]));
    //            OnSoftwareVersionRecieved(MiPVersion);
    //            break;
    //        case 0x19: //Mip Hardware Info 
    //            VoiceChipVersion = bytes[1];
    //            HardwareVersion = bytes[2];
    //            OnMiPHardwareVersionRecieved(new MiPHardwareVersion() { VoiceChipVersion = VoiceChipVersion, HardwareVersion = HardwareVersion });
    //            break;
    //        case 0x16: //GetVolume
    //            Volume = bytes[1];
    //            OnVolumeRecieved(Volume);
    //            break;
    //        case 0x1d: //Clap times
    //            var times = bytes[1];
    //            OnClapTimesRecieved(times);
    //            break;
    //        case 0x1f: //Clap Status  
    //            var isEnabled = bytes[1] == 0x00 ? false : true;
    //            int delayTime = (bytes[2] << 8) & bytes[3];
    //            OnClapStatusRecieved(new ClapStatus() { Enabled = isEnabled, DelayTime = delayTime });
    //            break;
    //       default:
    //                base.CharacteristicsNotify_ValueChanged(sender,args);
    //            break;
    //    }

    //}

}
