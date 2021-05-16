using System.Collections.Generic;
using System.Windows.Controls;

namespace APMOk.Code
{
    public static class ItemCollectionExtensions
    {
        public static void AddRange(this ItemCollection collection, IEnumerable<Control> items)
        {
            foreach (var item in items)
                collection.Add(item);
        }
    }
}
