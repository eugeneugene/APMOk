using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace APMData
{
    public class ObservableDictionary<T1, T2> : IDictionary<T1, T2>, INotifyPropertyChanged, INotifyCollectionChanged
    {
        /// <summary>
        /// Constructor initializes internal data
        /// </summary>
        public ObservableDictionary()
        {
            _nameValues = new Dictionary<T1, T2>();
        }

        /// <summary>
        ///     Adds a key/value pair to the ContentLocatorPart.  If a value for the key already
        ///     exists, the old value is overwritten by the new value.
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="val">value</param>
        /// <exception cref="ArgumentNullException">key or val is null</exception>
        /// <exception cref="ArgumentException">a value for key is already present in the locator part</exception>
        public void Add(T1 key, T2 val)
        {
            if (key == null || val == null)
                throw new ArgumentNullException(key == null ? "key" : "val");

            _nameValues.Add(key, val);
            var index = _nameValues.Keys.ToList().IndexOf(key);

            OnCountPropertyChanged();
            OnIndexerPropertyChanged();
            OnCollectionChanged(NotifyCollectionChangedAction.Add, val, index);
        }

        /// <summary>
        ///     Removes all name/value pairs from the ContentLocatorPart.
        /// </summary>
        public void Clear()
        {
            int count = _nameValues.Count;

            if (count > 0)
            {
                _nameValues.Clear();

                OnCountPropertyChanged();
                OnIndexerPropertyChanged();
                OnCollectionReset();
            }
        }

        /// <summary>
        ///     Returns whether or not a value of the key exists in this ContentLocatorPart.
        /// </summary>
        /// <param name="key">the key to check for</param>
        /// <returns>true - yes, false - no</returns>
        public bool ContainsKey(T1 key)
        {
            return _nameValues.ContainsKey(key);
        }

        /// <summary>
        ///     Removes the key and its value from the ContentLocatorPart.
        /// </summary>
        /// <param name="key">key to be removed</param>
        /// <returns>true - the key was found in the ContentLocatorPart, false o- it wasn't</returns>
        public bool Remove(T1 key)
        {
            if (_nameValues.TryGetValue(key, out var value))
            {
                var index = _nameValues.Keys.ToList().IndexOf(key);
                OnCountPropertyChanged();
                OnIndexerPropertyChanged();
                OnCollectionChanged(NotifyCollectionChangedAction.Remove, value, index);

                return true;
            }
            return false;
        }

        /// <summary>
        ///     Returns an enumerator for the key/value pairs in this ContentLocatorPart.
        /// </summary>
        /// <returns>an enumerator for the key/value pairs; never returns null</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _nameValues.GetEnumerator();
        }

        /// <summary>
        ///     Returns an enumerator for the key/value pairs in this ContentLocatorPart.
        /// </summary>
        /// <returns>an enumerator for the key/value pairs; never returns null</returns>
        public IEnumerator<KeyValuePair<T1, T2>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<T1, T2>>)_nameValues).GetEnumerator();
        }

        public bool TryGetValue(T1 key, out T2 value)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            return _nameValues.TryGetValue(key, out value);
        }

        void ICollection<KeyValuePair<T1, T2>>.Add(KeyValuePair<T1, T2> pair)
        {
            ((ICollection<KeyValuePair<T1, T2>>)_nameValues).Add(pair);
        }

        bool ICollection<KeyValuePair<T1, T2>>.Contains(KeyValuePair<T1, T2> pair)
        {
            return ((ICollection<KeyValuePair<T1, T2>>)_nameValues).Contains(pair);
        }

        bool ICollection<KeyValuePair<T1, T2>>.Remove(KeyValuePair<T1, T2> pair)
        {
            return ((ICollection<KeyValuePair<T1, T2>>)_nameValues).Remove(pair);
        }

        void ICollection<KeyValuePair<T1, T2>>.CopyTo(KeyValuePair<T1, T2>[] target, int startIndex)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (startIndex < 0 || startIndex > target.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            ((ICollection<KeyValuePair<T1, T2>>)_nameValues).CopyTo(target, startIndex);
        }

        /// <summary>
        ///     The number of name/value pairs in this ContentLocatorPart.
        /// </summary>
        /// <value>count of name/value pairs</value>
        public int Count
        {
            get => _nameValues.Count;
        }

        /// <summary>
        ///     Indexer provides lookup of values by key.  Gets or sets the value
        ///     in the ContentLocatorPart for the specified key.  If the key does not exist
        ///     in the ContentLocatorPart,
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>the value stored in this locator part for key</returns>
        public T2 this[T1 key]
        {
            get
            {
                if (key == null)
                    throw new ArgumentNullException(nameof(key));

                _nameValues.TryGetValue(key, out T2 value);
                return value;
            }
            set
            {
                if (key == null)
                    throw new ArgumentNullException(nameof(key));

                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                _nameValues.TryGetValue(key, out T2 oldValue);

                // If the new value is actually different, then we add it and fire
                // a change notification
                if (oldValue == null || !oldValue.Equals(value))
                {
                    _nameValues[key] = value;
                    var index = _nameValues.Keys.ToList().IndexOf(key);

                    OnIndexerPropertyChanged();
                    if (oldValue == null)
                        OnCollectionChanged(NotifyCollectionChangedAction.Add, value, index);
                    else
                        OnCollectionChanged(NotifyCollectionChangedAction.Replace, oldValue, value, index);
                }
            }
        }

        public bool IsReadOnly
        {
            get => false;
        }

        /// <summary>
        ///     Returns a collection of all the keys in this ContentLocatorPart.
        /// </summary>
        /// <value>keys</value>
        public ICollection<T1> Keys
        {
            get => _nameValues.Keys;
        }

        /// <summary>
        ///     Returns a collection of all the values in this ContentLocatorPart.
        /// </summary>
        /// <value>values</value>
        public ICollection<T2> Values
        {
            get => _nameValues.Values;
        }

        /// <summary>
        /// PropertyChanged event (per <see cref="INotifyPropertyChanged" />).
        /// </summary>
        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add => PropertyChanged += value;
            remove => PropertyChanged -= value;
        }

        /// <summary>
        /// Occurs when the collection changes, either by adding or removing an item.
        /// </summary>
        /// <remarks>
        /// see <seealso cref="INotifyCollectionChanged"/>
        /// </remarks>
        [field: NonSerialized]
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Raises a PropertyChanged event (per <see cref="INotifyPropertyChanged" />).
        /// </summary>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        /// <summary>
        /// PropertyChanged event (per <see cref="INotifyPropertyChanged" />).
        /// </summary>
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Notify the owner this ContentLocatorPart has changed.
        /// </summary>
        //private void FireDictionaryChanged()
        //{
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        //}

        /// <summary>
        /// Raise CollectionChanged event to any listeners.
        /// Properties/methods modifying this ObservableCollection will raise
        /// a collection changed event through this virtual method.
        /// </summary>
        /// <remarks>
        /// When overriding this method, either call its base implementation
        /// or call <see cref="BlockReentrancy"/> to guard against reentrant collection changes.
        /// </remarks>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Helper to raise a PropertyChanged event for the Count property
        /// </summary>
        private void OnCountPropertyChanged() => OnPropertyChanged(EventArgsCache.CountPropertyChanged);

        /// <summary>
        /// Helper to raise a PropertyChanged event for the Indexer property
        /// </summary>
        private void OnIndexerPropertyChanged() => OnPropertyChanged(EventArgsCache.IndexerPropertyChanged);

        /// <summary>
        /// Helper to raise CollectionChanged event to any listeners
        /// </summary>
        private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
        }

        /// <summary>
        /// Helper to raise CollectionChanged event to any listeners
        /// </summary>
        //private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index, int oldIndex)
        //{
        //    OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index, oldIndex));
        //}

        /// <summary>
        /// Helper to raise CollectionChanged event to any listeners
        /// </summary>
        private void OnCollectionChanged(NotifyCollectionChangedAction action, object oldItem, object newItem, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index));
        }

        /// <summary>
        /// Helper to raise CollectionChanged event with action == Reset to any listeners
        /// </summary>
        private void OnCollectionReset() => OnCollectionChanged(EventArgsCache.ResetCollectionChanged);

        /// <summary>
        ///     The internal data structure.
        /// </summary>
        private readonly Dictionary<T1, T2> _nameValues;

        internal static class EventArgsCache
        {
            internal static readonly PropertyChangedEventArgs CountPropertyChanged = new("Count");
            internal static readonly PropertyChangedEventArgs IndexerPropertyChanged = new("Item[]");
            internal static readonly NotifyCollectionChangedEventArgs ResetCollectionChanged = new(NotifyCollectionChangedAction.Reset);
        }
    }
}
