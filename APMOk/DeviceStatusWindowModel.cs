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

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                return;
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
