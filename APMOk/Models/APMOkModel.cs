using APMData;
using APMOkLib;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace APMOk.Models
{
    /// <summary>
    /// APM Model
    /// DI Lifetime: Singleton
    /// </summary>
    public class APMOkModel : JsonToString, INotifyPropertyChanged
    {
        private DisksReply _systemDiskInfo;
        private PowerStateReply _powerState;
        private bool _connectFailure;

        public event PropertyChangedEventHandler PropertyChanged;

        private readonly ObservableConcurrentDictionary<string, APMValueProperty> APMValueDictionary = new();

        public APMValueProperty GetAPMValue(string deviceID)
        {
            if (APMValueDictionary.ContainsKey(deviceID))
                return APMValueDictionary[deviceID];
            return null;
        }

        public void UpdateAPMValue(string deviceID, APMValueProperty apmValueProperty)
        {
            if (APMValueDictionary.ContainsKey(deviceID))
            {
                var value = APMValueDictionary[deviceID];
                if (!value.Equals(apmValueProperty))
                {
                    value.OnMains = apmValueProperty.OnMains;
                    value.OnBatteries = apmValueProperty.OnBatteries;
                    value.Current = apmValueProperty.Current;
                    Debug.WriteLine($"{deviceID}, Updated APMValueProperty: {value}");
                    PropertyChanged.Invoke(this, new(nameof(APMValueDictionary)));
                }
            }
            else
            {
                var value = new APMValueProperty(apmValueProperty.OnMains, apmValueProperty.OnBatteries, apmValueProperty.Current);
                APMValueDictionary[deviceID] = value;
                Debug.WriteLine($"{deviceID}, New APMValueProperty: {value}");
                PropertyChanged.Invoke(this, new(nameof(APMValueDictionary)));
            }
        }

        public DisksReply SystemDiskInfo
        {
            get => _systemDiskInfo;
            set
            {
                if (_systemDiskInfo == null || !_systemDiskInfo.Equals(value))
                {
                    _systemDiskInfo = value;
                    NotifyPropertyChanged(nameof(SystemDiskInfo));
                }
            }
        }

        public PowerStateReply PowerState
        {
            get => _powerState;
            set
            {
                if (_powerState == null || !_powerState.Equals(value))
                {
                    _powerState = value;
                    NotifyPropertyChanged(nameof(PowerState));
                }
            }
        }

        public bool ConnectFailure
        {
            get => _connectFailure;
            set
            {
                if (_connectFailure != value)
                {
                    _connectFailure = value;
                    NotifyPropertyChanged(nameof(ConnectFailure));
                }
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (string.IsNullOrEmpty(propertyName))
                return;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
