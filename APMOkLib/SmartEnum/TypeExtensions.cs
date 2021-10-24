using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace APMOkLib.SmartEnum
{
    internal static class TypeExtensions
    {
        public static IEnumerable<TFieldType> GetFieldsOfType<TFieldType>(this Type type)
        {
            foreach (var t in type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(p => type.IsAssignableFrom(p.FieldType)))
            {
                var t1 = (TFieldType?)t?.GetValue(null);
                if (t1 is not null)
                    yield return t1;
            }
        }
    }
}
