using APMOkLib;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace APMOk.Models
{
    public class APMValueProperty : JsonToString, INotifyPropertyChanged, IEquatable<APMValueProperty>
    {
        public APMValueProperty()
        { }

        public APMValueProperty(uint onMains, uint onBatteries, uint current)
        {
            _onMains = onMains;
            _onBatteries = onBatteries;
            _current = current;
        }

        [JsonIgnore]
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

        [JsonIgnore]
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

        [JsonIgnore]
        private uint _current;
        public uint Current
        {
            get => _current;
            set
            {
                if (value != _current)
                {
                    _current = value;
                    NotifyPropertyChanged(nameof(Current));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            if (string.IsNullOrEmpty(propertyName))
                return;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool Equals(APMValueProperty? other)
        {
            if (ReferenceEquals(this, other))
                return true;
            return other is not null && _onMains ==  other.OnMains && _onBatteries == other._onBatteries && _current == other._current;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as APMValueProperty);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_onMains, _onBatteries, _current);
        }
    }
}
