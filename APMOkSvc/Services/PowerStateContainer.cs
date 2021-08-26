using APMData;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace APMOkSvc.Services
{
    /// <summary>
    /// Container for the latest PowerStatus data
    /// DI Lifetime: Singleton
    /// </summary>
    public class PowerStateContainer
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private PowerStateReply _powerState;

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

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (string.IsNullOrEmpty(propertyName))
                return;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
