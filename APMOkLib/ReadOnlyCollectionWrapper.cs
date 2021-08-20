using System.Collections.Generic;

namespace APMOkLib
{
    public static class ReadOnlyCollectionWrapper
    {
        public static IReadOnlyCollection<T> AsReadOnly<T>(this ICollection<T> collection)
        {
            return new ReadOnlyCollectionWrapper<T>(collection);
        }
    }
}
