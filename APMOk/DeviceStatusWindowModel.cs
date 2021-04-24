using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace APMOk
{
    public class DeviceStatusWindowModel : INotifyPropertyChanged
    {
        private bool _connected;
        public bool Connected
        {
            get => _connected;
            set
            {
                if (_connected != value)
                {
                    _connected = value;
                    NotifyPropertyChanged(nameof(Connected));
                }
            }
        }

        private PowerState _battery;
        public PowerState Battery
        {
            get => _battery;
            set
            {
                if (_battery != value)
                {
                    _battery = value;
                    NotifyPropertyChanged(nameof(Battery));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (!string.IsNullOrEmpty(propertyName))
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
