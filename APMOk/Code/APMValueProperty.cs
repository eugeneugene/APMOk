using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace APMOk.Code
{
    public class APMValueProperty : INotifyPropertyChanged, IEquatable<APMValueProperty>
    {
        public APMValueProperty()
        { }

        public APMValueProperty(uint onMains, uint onBatteries)
        {
            _onMains = onMains;
            _onBatteries = onBatteries;
        }

        private uint _onMains;
        public uint OnMains
        {
            get => _onMains;
            set
            {
                if (value != _onMains)
                {
                    _onMains = value;
                    NotifyPropertyChanged(nameof(OnMains));
                }
            }
        }

        private uint _onBatteries;
        public uint OnBatteries
        {
            get => _onBatteries;
            set
            {
                if (value != _onBatteries)
                {
                    _onBatteries = value;
                    NotifyPropertyChanged(nameof(OnBatteries));
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

        public bool Equals(APMValueProperty other)
        {
            if (ReferenceEquals(this, other))
                return true;
            return _onMains == other.OnMains;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as APMValueProperty);
        }

        public override int GetHashCode()
        {
            return _onMains.GetHashCode();
        }
    }
}
