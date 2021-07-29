/*
 * This is a thread safe caching strategy implementation of Least Recently Used (LRU).
 * It defines the policy to evict elements from the cache to make room for new elements when the cache is full,
 * meaning it discards the least recently used items first.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LRU
{
    public class LeastRecentlyUsedCache<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private readonly int _capacity;
        private readonly LinkedList<TKey> _order;
        private readonly IDictionary<TKey, TValue> _cacheValue;
        private readonly IDictionary<TKey, LinkedListNode<TKey>> _cacheNode;
        private readonly object _lockObj = new ();

        public LeastRecentlyUsedCache(int capacity)
        {
            _capacity = (capacity > 0)
                ? capacity
                : throw new ArgumentException("Must be greater than 0.", nameof(capacity));
            _order = new LinkedList<TKey>();
            _cacheValue = new Dictionary<TKey, TValue>(_capacity);
            _cacheNode = new Dictionary<TKey, LinkedListNode<TKey>>(_capacity);
        }

        #region IDictionary Implementations

        public int Count
        {
            get
            {
                lock (_lockObj)
                {
                    return _cacheValue.Count;
                }
            }
        }
        public bool IsReadOnly => false;

        public void Add(TKey key, TValue value) => Put(key, value);

        public void Add(KeyValuePair<TKey, TValue> item) => Put(item.Key, item.Value);

        public bool TryGetValue(TKey key, out TValue value)
        {
            try
            {
                value = Get(key);
                return true;
            }
            catch (InvalidOperationException)
            {
                value = default;
                return false;
            }
        }

        public void Clear()
        {
            lock (_lockObj)
            {
                _order.Clear();
                _cacheValue.Clear();
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                return Get(key);
            }
            set => Put(key, value);
        }

        public ICollection<TValue> Values
        {
            get
            {
                lock (_lockObj)
                {
                    return _cacheValue.Values.ToList();
                }
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                lock (_lockObj)
                {
                    return _cacheValue.Keys;
                }

            }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            lock (_lockObj)
            {
                return _cacheValue.TryGetValue(item.Key, out var node) && node.Equals(item.Value);
            }
        }

        public bool ContainsKey(TKey key)
        {
            lock (_lockObj)
            {
                return _cacheValue.ContainsKey(key);
            }
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (array.Rank > 1)
                throw new ArgumentException("array is multidimensional.");
            if (arrayIndex < 0 || arrayIndex > array.Length)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (array.Length - arrayIndex < Count)
                throw new ArgumentException("Not enough elements after arrayIndex in the destination array.");

            lock (_lockObj)
            {
                var index = 0;
                foreach (var keyValuePair in _cacheValue)
                {
                    array[arrayIndex + index++] = new KeyValuePair<TKey, TValue>(keyValuePair.Key, keyValuePair.Value);
                }
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            lock (_lockObj)
            {
                return _cacheValue.GetEnumerator();
            }
        }

        public bool Remove(TKey key) => RemoveByKey(key);

        public bool Remove(KeyValuePair<TKey, TValue> item) => RemoveByKey(item.Key);

        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (_lockObj)
            {
                return _cacheValue.GetEnumerator();
            }
        }
        #endregion

        #region Private Methods

        private TValue Get(TKey key)
        {
            lock (_lockObj)
            {
                if (!_cacheValue.ContainsKey(key)) throw new InvalidOperationException();

                Promote(key);
                return _cacheValue[key];
            }
        }

        private void Put(TKey key, TValue value)
        {
            lock (_lockObj)
            {
                if (_cacheValue.ContainsKey(key))
                {
                    Promote(key);
                    _cacheValue[key] = value;
                    return;
                }

                if (_cacheValue.Count == _capacity)
                {
                    RemoveLast();
                }

                AddFirst(key, value);
            }
        }

        private void Promote(TKey key)
        {
            var node = _cacheNode[key];
            _order.Remove(node);
            _order.AddFirst(node);
        }

        private void AddFirst(TKey key, TValue value)
        {
            var node = new LinkedListNode<TKey>(key);
            _cacheValue[key] = value;
            _cacheNode[key] = node;
            _order.AddFirst(node);
        }

        private void RemoveLast()
        {
            var lastNode = _order.Last;
            _cacheValue.Remove(lastNode.Value);
            _cacheNode.Remove(lastNode.Value);
            _order.RemoveLast();
        }

        private bool RemoveByKey(TKey key)
        {
            lock (_lockObj)
            {
                _order.Remove(key);
                _cacheNode.Remove(key);
                return _cacheValue.Remove(key);
            }
        }
        #endregion
    }
}