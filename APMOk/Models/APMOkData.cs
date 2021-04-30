using APMData;
using APMData.Proto;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace APMOk
{
    public class APMOkData : JsonToString, INotifyPropertyChanged
    {
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

        private bool _connectFailure;
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
    }
}
