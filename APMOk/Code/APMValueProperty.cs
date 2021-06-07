using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace APMOk.Code
{
    public class APMValueProperty : INotifyPropertyChanged, IEquatable<APMValueProperty>
    {
        public APMValueProperty()
        { }

        public APMValueProperty(int userValue, int currentValue)
        {
            _userValue = userValue;
            _currentValue = currentValue;
        }

        private int _userValue;
        public int UserValue
        {
            get => _userValue;
            set
            {
                if (value != _userValue)
                {
                    _userValue = value;
                    NotifyPropertyChanged(nameof(UserValue));
                }
            }
        }

        private int _currentValue;
        public int CurrentValue
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
            return _userValue == other.UserValue;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as APMValueProperty);
        }

        public override int GetHashCode()
        {
            return _userValue.GetHashCode();
        }
    }
}
