using APMOk.Code;
using APMOkLib;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace APMOk
{
    public class APMOkModel : JsonToString, INotifyPropertyChanged, IDisposable
    {
        private APMData.Proto.DisksReply _systemDiskInfo;
        private APMData.Proto.PowerStateReply _powerState;
        private bool _connectFailure;
        private bool disposedValue;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableConcurrentDictionary<string, APMValueProperty> APMValueDictionary { get; }

        public APMOkModel()
        {
            APMValueDictionary = new();
            APMValueDictionary.PropertyChanged += APMValueDictionaryPropertyChanged;
        }

        public APMData.Proto.DisksReply SystemDiskInfo
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

        public APMData.Proto.PowerStateReply PowerState
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

        private void APMValueDictionaryPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NotifyPropertyChanged(nameof(APMValueDictionary));
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (string.IsNullOrEmpty(propertyName))
                return;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    APMValueDictionary.PropertyChanged -= APMValueDictionaryPropertyChanged;
                }
            }
            disposedValue = true;
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
