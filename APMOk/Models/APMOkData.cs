using APMData;
using APMData.Proto;
using APMOkLib;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace APMOk
{
    public class APMOkData : JsonToString, INotifyPropertyChanged, IDisposable
    {
        public APMOkData()
        {
            _APMValueDictionary.PropertyChanged += APMValueDictionaryPropertyChanged;
        }

        private void APMValueDictionaryPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NotifyPropertyChanged(nameof(APMValueDictionary));
        }

        private SystemDiskInfoReply _systemDiskInfo;
        public SystemDiskInfoReply SystemDiskInfo
        {
            get => _systemDiskInfo;
            set
            {
                if (_systemDiskInfo != value)
                {
                    _systemDiskInfo = value;
                    NotifyPropertyChanged(nameof(SystemDiskInfo));
                }
            }
        }

        private PowerStateReply _powerState;
        public PowerStateReply PowerState
        {
            get => _powerState;
            set
            {
                if (_powerState != value)
                {
                    _powerState = value;
                    NotifyPropertyChanged(nameof(PowerState));
                }
            }
        }

        private readonly ObservableDictionary<string, ushort> _APMValueDictionary = new();
        public ObservableDictionary<string, ushort> APMValueDictionary
        {
            get => _APMValueDictionary;
        }

        private bool _connectFailure;
        private bool disposedValue;

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

        public event PropertyChangedEventHandler PropertyChanged;
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
                    _APMValueDictionary.PropertyChanged -= APMValueDictionaryPropertyChanged;
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
