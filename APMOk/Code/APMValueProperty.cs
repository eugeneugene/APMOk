using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace APMOk.Code
{
    public class APMValueProperty : INotifyPropertyChanged, IEquatable<APMValueProperty>
    {
        public APMValueProperty()
        { }

        public APMValueProperty(uint defaultValue, uint currentValue)
        {
            _defaultValue = defaultValue;
            _currentValue = currentValue;
        }

        private uint _defaultValue;
        public uint DefaultValue
        {
            get => _defaultValue;
            set
            {
                if (value != _defaultValue)
                {
                    _defaultValue = value;
                    NotifyPropertyChanged(nameof(DefaultValue));
                }
            }
        }

        private uint _currentValue;
        public uint CurrentValue
        {
            get => _currentValue;
            set
            {
                if (value != _currentValue)
                {
                    _currentValue = value;
                    NotifyPropertyChanged(nameof(CurrentValue));
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
            return _defaultValue == other._defaultValue && _currentValue == other.CurrentValue;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as APMValueProperty);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_defaultValue, _currentValue);
        }
    }
}
