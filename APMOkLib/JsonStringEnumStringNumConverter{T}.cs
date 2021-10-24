using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace APMOkLib
{
    internal class JsonStringEnumStringNumConverter<T> : JsonConverter<T>
    {
        private class EnumInfo
        {
            public string Name;
            public Enum? EnumValue;
            public ulong RawValue;

            public EnumInfo(string name, Enum? enumValue, ulong rawValue)
            {
                Name = name;
                EnumValue = enumValue;
                RawValue = rawValue;
            }
        }

        private readonly bool _AllowIntegerValues;
        private readonly Type? _UnderlyingType;
        private readonly Type _EnumType;
        private readonly TypeCode _EnumTypeCode;
        private readonly bool _IsFlags;
        private readonly Dictionary<ulong, EnumInfo> _RawToTransformed;
        private readonly Dictionary<string, EnumInfo> _TransformedToRaw;

        public JsonStringEnumStringNumConverter(JsonNamingPolicy? namingPolicy, bool allowIntegerValues, Type? underlyingType)
        {
            Debug.Assert((typeof(T).IsEnum && underlyingType is null) || (Nullable.GetUnderlyingType(typeof(T)) == underlyingType), "Generic type is invalid");

            _AllowIntegerValues = allowIntegerValues;
            _UnderlyingType = underlyingType;
            _EnumType = _UnderlyingType ?? typeof(T);
            _EnumTypeCode = Type.GetTypeCode(_EnumType);
            _IsFlags = _EnumType.IsDefined(typeof(FlagsAttribute), true);

            string[] builtInNames = _EnumType.GetEnumNames();
            Array builtInValues = _EnumType.GetEnumValues();

            _RawToTransformed = new Dictionary<ulong, EnumInfo>();
            _TransformedToRaw = new Dictionary<string, EnumInfo>();

            for (int i = 0; i < builtInNames.Length; i++)
            {
                var enumValue = (Enum?)builtInValues.GetValue(i);
                ulong rawValue = GetEnumValue(enumValue);

                string name = builtInNames[i];
                string transformedName = namingPolicy?.ConvertName(name) ?? name;

                _RawToTransformed[rawValue] = new EnumInfo(transformedName, enumValue, rawValue);
                _TransformedToRaw[transformedName] = new EnumInfo(name, enumValue, rawValue);
            }
        }

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            JsonTokenType token = reader.TokenType;

            if (token == JsonTokenType.String)
            {
                string enumString = reader.GetString() ?? string.Empty;

                if (_IsFlags)
                {
                    ulong calculatedValue = 0UL;
                    string[] flagValues = enumString.Split(",");
                    foreach (string flagValue in flagValues)
                    {
                        (string val11, ulong? val12) = ParseStringEnum(flagValue);

                        // Case sensitive search attempted first.
                        if (_TransformedToRaw.TryGetValue(val11, out EnumInfo? enumInfo1))
                            calculatedValue |= enumInfo1.RawValue;
                        else if (val12 is not null && _TransformedToRaw.TryGetValue(val12.Value.ToString(), out EnumInfo? enumInfo3))
                            calculatedValue |= enumInfo3.RawValue;
                        else
                        {
                            // Case insensitive search attempted second.
                            bool matched = false;
                            foreach (KeyValuePair<string, EnumInfo> enumItem in _TransformedToRaw)
                            {
                                if (string.Equals(enumItem.Key, val11, StringComparison.OrdinalIgnoreCase))
                                {
                                    calculatedValue |= enumItem.Value.RawValue;
                                    matched = true;
                                    break;
                                }
                            }

                            // Search by the numeric part at last
                            if (!matched)
                            {
                                if (val12 is not null && _RawToTransformed.TryGetValue(val12.Value, out EnumInfo? enumInfo4))
                                {
                                    calculatedValue |= enumInfo4.RawValue;
                                    matched = true;
                                }
                            }

                            if (!matched)
                                throw new JsonException($"Unknown flag value {flagValue}.");
                        }
                    }

                    return (T)Enum.ToObject(_EnumType, calculatedValue);
                }

                (string val21, ulong? val22) = ParseStringEnum(enumString);

                // Case sensitive search attempted first.
                if (_TransformedToRaw.TryGetValue(val21, out EnumInfo? enumInfo2))
                    return (T)Enum.ToObject(_EnumType, enumInfo2.RawValue);

                // Case insensitive search attempted second.
                foreach (KeyValuePair<string, EnumInfo> enumItem in _TransformedToRaw)
                {
                    if (string.Equals(enumItem.Key, val21, StringComparison.OrdinalIgnoreCase))
                        return (T)Enum.ToObject(_EnumType, enumItem.Value.RawValue);
                }

                // Search by the numeric part at last
                if (val22 is not null && _RawToTransformed.TryGetValue(val22.Value, out EnumInfo? enumInfo5))
                    return (T)Enum.ToObject(_EnumType, enumInfo5.RawValue);

                throw new JsonException($"Unknown value {enumString}.");
            }

            if (token != JsonTokenType.Number || !_AllowIntegerValues)
                throw new JsonException();

            switch (_EnumTypeCode)
            {
                // Switch cases ordered by expected frequency.
                case TypeCode.Int32:
                    if (reader.TryGetInt32(out int int32))
                        return (T)Enum.ToObject(_EnumType, int32);
                    break;
                case TypeCode.UInt32:
                    if (reader.TryGetUInt32(out uint uint32))
                        return (T)Enum.ToObject(_EnumType, uint32);
                    break;
                case TypeCode.UInt64:
                    if (reader.TryGetUInt64(out ulong uint64))
                        return (T)Enum.ToObject(_EnumType, uint64);
                    break;
                case TypeCode.Int64:
                    if (reader.TryGetInt64(out long int64))
                        return (T)Enum.ToObject(_EnumType, int64);
                    break;
                case TypeCode.SByte:
                    if (reader.TryGetSByte(out sbyte byte8))
                        return (T)Enum.ToObject(_EnumType, byte8);
                    break;
                case TypeCode.Byte:
                    if (reader.TryGetByte(out byte ubyte8))
                        return (T)Enum.ToObject(_EnumType, ubyte8);
                    break;
                case TypeCode.Int16:
                    if (reader.TryGetInt16(out short int16))
                        return (T)Enum.ToObject(_EnumType, int16);
                    break;
                case TypeCode.UInt16:
                    if (reader.TryGetUInt16(out ushort uint16))
                        return (T)Enum.ToObject(_EnumType, uint16);
                    break;
            }

            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            // Note: There is no check for value is null because Json serializer won't call the converter in that case.
            ulong rawValue = GetEnumValue(value!);

            if (_RawToTransformed.TryGetValue(rawValue, out EnumInfo? enumInfo1))
            {
                writer.WriteStringValue($"{enumInfo1.Name} ({enumInfo1.RawValue})");
                return;
            }

            if (_IsFlags)
            {
                ulong calculatedValue = 0UL;

                StringBuilder Builder = new();
                foreach (KeyValuePair<ulong, EnumInfo> enumItem in _RawToTransformed)
                {
                    EnumInfo enumInfo = enumItem.Value;
                    if (!(value as Enum)!.HasFlag(enumInfo.EnumValue!) || enumInfo.RawValue == 0) // Definitions with 'None' should hit the cache case.
                        continue;

                    // Track the value to make sure all bits are represented.
                    calculatedValue |= enumInfo.RawValue;

                    if (Builder.Length > 0)
                        Builder.Append(", ");
                    Builder.Append($"{enumInfo.Name} ({enumInfo.RawValue})");
                }

                if (calculatedValue == rawValue)
                {
                    writer.WriteStringValue(Builder.ToString());
                    return;
                }
            }

            if (!_AllowIntegerValues)
                throw new JsonException();

            switch (_EnumTypeCode)
            {
                case TypeCode.Int32:
                    writer.WriteNumberValue((int)rawValue);
                    break;
                case TypeCode.UInt32:
                    writer.WriteNumberValue((uint)rawValue);
                    break;
                case TypeCode.UInt64:
                    writer.WriteNumberValue(rawValue);
                    break;
                case TypeCode.Int64:
                    writer.WriteNumberValue((long)rawValue);
                    break;
                case TypeCode.Int16:
                    writer.WriteNumberValue((short)rawValue);
                    break;
                case TypeCode.UInt16:
                    writer.WriteNumberValue((ushort)rawValue);
                    break;
                case TypeCode.Byte:
                    writer.WriteNumberValue((byte)rawValue);
                    break;
                case TypeCode.SByte:
                    writer.WriteNumberValue((sbyte)rawValue);
                    break;
                default:
                    throw new JsonException();
            }
        }

        private ulong GetEnumValue(object? value)
        {
            if (value is null)
                return 0UL;

            return _EnumTypeCode switch
            {
                TypeCode.Int32 => (ulong)(int)value,
                TypeCode.UInt32 => (uint)value,
                TypeCode.UInt64 => (ulong)value,
                TypeCode.Int64 => (ulong)(long)value,
                TypeCode.SByte => (ulong)(sbyte)value,
                TypeCode.Byte => (byte)value,
                TypeCode.Int16 => (ulong)(short)value,
                TypeCode.UInt16 => (ushort)value,
                _ => throw new JsonException(),
            };
        }

        internal static (string StringValue, ulong? NumValue) ParseStringEnum(string EnumValue)
        {
            if (EnumValue is null)
                throw new ArgumentNullException(nameof(EnumValue));

            int state = 0;
            StringBuilder sb1 = new(string.Empty);
            StringBuilder sb2 = new(string.Empty);
            foreach (char c in EnumValue.ToCharArray())
            {
                if (state == 0)
                {
                    if (char.IsLetter(c))
                    {
                        sb1.Append(c);
                        state = 1;
                        continue;
                    }
                    if (char.IsDigit(c))
                    {
                        sb2.Append(c);
                        state = 3;
                        continue;
                    }
                }
                if (state == 1)
                {
                    if (char.IsLetterOrDigit(c))
                        sb1.Append(c);
                    else
                        state = 2;
                    continue;
                }
                if (state == 2)
                {
                    if (char.IsDigit(c))
                    {
                        sb2.Append(c);
                        state = 3;
                        continue;
                    }
                }
                if (state == 3)
                {
                    if (char.IsDigit(c))
                        sb2.Append(c);
                    else
                        break;
                }
            }

            ulong? num = null;
            string str1 = sb1.ToString();
            string str2 = sb2.ToString();
            if (ulong.TryParse(str2, out ulong n))
                num = n;

            return (str1, num);
        }
    }
}
