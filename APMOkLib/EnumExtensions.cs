using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace APMOkLib;

public static class EnumExtensions
{
    public static T? GetAttribute<T>(this Enum value) where T : Attribute
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        var type = value.GetType();
        var memberInfo = type.GetMember(value.ToString());
        var attributes = memberInfo[0].GetCustomAttributes(typeof(T), false);

        if (attributes is null)
            return default;

        return attributes.Length > 0
          ? (T)attributes[0]
          : null;
    }

    public static string? EnumMember(this Enum value)
    {
        if (value is null)
            return default;

        var attribute = value.GetAttribute<EnumMemberAttribute>();

        return attribute is null ? value.ToString() : attribute.Value;
    }

    public static T? FindByEnumMember<T>(string enumMember) where T : Enum
    {
        if (enumMember is null)
            return default;

        foreach (Enum value in typeof(T).GetEnumValues())
        {
            if (value.EnumMember() == enumMember)
                return (T)value;
        }
        return default;
    }

    public static string? XmlEnum(this Enum value)
    {
        if (value is null)
            return null;

        var attribute = value.GetAttribute<XmlEnumAttribute>();
        return attribute is null ? value.ToString() : attribute.Name;
    }

    public static string? DisplayEnum(this Enum value)
    {
        if (value is null)
            return null;

        var attribute = value.GetAttribute<DisplayAttribute>();
        return attribute is null ? value.ToString() : attribute.Name;
    }

    public static bool NotMapped(this Enum value)
    {
        if (value is null)
            return false;

        var attribute = value.GetAttribute<NotMappedAttribute>();
        return attribute != null;
    }
}
