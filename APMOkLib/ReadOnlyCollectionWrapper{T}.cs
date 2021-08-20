using System.Collections;
using System.Collections.Generic;

namespace APMOkLib
{
    public class ReadOnlyCollectionWrapper<T> : IReadOnlyCollection<T>
    {
        private readonly ICollection<T> _collection;
        public ReadOnlyCollectionWrapper(ICollection<T> collection)
        {
            _collection = collection;
        }

        public int Count => _collection.Count;

        public IEnumerator<T> GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _collection.GetEnumerator();
        }
    }
}
