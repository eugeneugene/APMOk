using System;
using System.Collections.Generic;
using System.Reflection;

namespace APMOkLib.SmartEnum
{
    public static class SmartEnumExtensions
    {
        public static bool IsSmartEnum(this Type type) =>
            IsSmartEnum(type, out var _);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="genericArguments"></param>
        /// <returns></returns>
        public static bool IsSmartEnum(this Type type, out Type[]? genericArguments)
        {
            if (type is null || type.IsAbstract || type.IsGenericTypeDefinition)
            {
                genericArguments = null;
                return false;
            }

            Type? ntype = type;
            do
            {
                if (ntype!.IsGenericType && ntype.GetGenericTypeDefinition() == typeof(SmartEnum<,>))
                {
                    genericArguments = ntype.GetGenericArguments();
                    return true;
                }

                ntype = ntype.BaseType;
            }
            while (ntype is not null);

            genericArguments = null;
            return false;
        }

        public static bool TryGetValues(this Type type, out IEnumerable<object>? enums)
        {
            Type? ntype = type;
            while (ntype is not null)
            {
                if (ntype.IsGenericType && ntype.GetGenericTypeDefinition() == typeof(SmartEnum<,>))
                {
                    var listPropertyInfo = ntype.GetProperty("List", BindingFlags.Public | BindingFlags.Static);
                    if (listPropertyInfo is null)
                    {
                        enums = null;
                        return false;
                    }

                    enums = (IEnumerable<object>?)listPropertyInfo.GetValue(ntype!, null);
                    return true;
                }
                ntype = ntype.BaseType;
            }

            enums = null;
            return false;
        }
    }
}
