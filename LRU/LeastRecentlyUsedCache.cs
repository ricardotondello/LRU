/*
 * This is a thread safe caching strategy implementation of Least Recently Used (LRU).
 * It defines the policy to evict elements from the cache to make room for new elements when the cache is full,
 * meaning it discards the least recently used items first.
 */

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LRU.Helpers;

namespace LRU;

public class LeastRecentlyUsedCache<TKey, TValue> : IDictionary<TKey, TValue>
{
    private readonly int _capacity;
    private readonly IDictionary<TKey, LinkedNode<TKey, TValue>> _cacheValue;
    private readonly object _lockObj = new();
    private LinkedNode<TKey, TValue> _head;
    private LinkedNode<TKey, TValue> _tail;

    /// <summary>
    /// A LRU based dictionary.
    /// Always the least recently used item will be removed once max capacity reached.
    /// </summary>
    /// <param name="capacity">The initial number of elements that the <see
    /// cref="ConcurrentDictionary{TKey,TValue}"/>
    /// can contain.</param>
    /// <param name="concurrencyLevel">The estimated number of threads that will update the
    /// <see cref="ConcurrentDictionary{TKey,TValue}"/> concurrently.</param>
    public LeastRecentlyUsedCache(int capacity, int concurrencyLevel = 0)
    {
        if (concurrencyLevel <= 0)
        {
            ThreadPool.GetMaxThreads(out concurrencyLevel, out _);
        }

        _capacity = capacity > 0
            ? capacity
            : throw new ArgumentException("Must be greater than 0.", nameof(capacity));
        _cacheValue = new ConcurrentDictionary<TKey, LinkedNode<TKey, TValue>>(concurrencyLevel, _capacity);
        _head = null;
        _tail = null;
        Count = 0;
    }
        
    #region IDictionary Implementations

    public int Count { get; private set; }

    public bool IsReadOnly => false;

    public bool TryGetValue(TKey key, out TValue value)
    {
        value = default;

        if (!_cacheValue.TryGetValue(key, out var entry))
        {
            return false;
        }

        MoveToHead(entry);

        lock (entry)
        {
            value = entry.Value;
        }

        return true;
    }

    public void Add(TKey key, TValue value) => Put(key, value);

    public void Add(KeyValuePair<TKey, TValue> item) => Put(item.Key, item.Value);

    public void Clear()
    {
        lock (_lockObj)
        {
            _cacheValue.Clear();
            _head = null;
            _tail = null;
            Count = 0;
        }
    }

    public TValue this[TKey key]
    {
        get => TryGetValue(key, out var value) ? value : default;
        set => Put(key, value);
    }

    public ICollection<TKey> Keys => _cacheValue.Keys;
    public ICollection<TValue> Values => _cacheValue.Values.Select(x => x.Value).ToList();

    public bool Contains(KeyValuePair<TKey, TValue> item) =>
        _cacheValue.TryGetValue(item.Key, out var obj) && obj.Value.Equals(item.Value);

    public bool ContainsKey(TKey key) => _cacheValue.ContainsKey(key);

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        if (array == null)
        {
            throw new ArgumentNullException(nameof(array));
        }

        if (arrayIndex < 0 || arrayIndex > array.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        }

        if (array.Length - arrayIndex < Count)
        {
            throw new ArgumentException("Not enough elements after arrayIndex in the destination array.");
        }

        lock (_lockObj)
        {
            var index = 0;
            foreach (var keyValuePair in _cacheValue)
            {
                array[arrayIndex + index++] =
                    new KeyValuePair<TKey, TValue>(keyValuePair.Key, keyValuePair.Value.Value);
            }
        }
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() =>
        _cacheValue.Select(x => new KeyValuePair<TKey, TValue>(x.Key, x.Value.Value)).GetEnumerator();

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
        
    private void MoveToHead(LinkedNode<TKey, TValue> entry)
    {
        if (entry == _head)
        {
            return;
        }

        lock (_lockObj)
        {
            RemoveFromLink(entry);
            AddToHead(entry);
        }
    }

    private void RemoveFromLink(LinkedNode<TKey, TValue> entry)
    {
        var next = entry.Next;
        var prev = entry.Prev;

        if (null != next)
        {
            next.Prev = entry.Prev;
        }

        if (null != prev)
        {
            prev.Next = entry.Next;
        }

        if (_head == entry)
        {
            _head = next;
        }

        if (_tail == entry)
        {
            _tail = prev;
        }
    }

    private void AddToHead(LinkedNode<TKey, TValue> entry)
    {
        entry.Prev = null;
        entry.Next = _head;

        if (null != _head)
        {
            _head.Prev = entry;
        }

        _head = entry;
    }

    private void Put(TKey key, TValue value)
    {
        if (!_cacheValue.TryGetValue(key, out var entry))
        {
            lock (_lockObj)
            {
                if (!_cacheValue.TryGetValue(key, out entry))
                {
                    if (IsFull)
                    {
                        entry = _tail;
                        _cacheValue.Remove(_tail.Key);

                        entry.Key = key;
                        entry.Value = value;
                    }
                    else
                    {
                        ++Count;
                        entry = new LinkedNode<TKey, TValue>
                        {
                            Key = key,
                            Value = value
                        };
                    }

                    _cacheValue.Add(key, entry);
                }
            }
        }
        else
        {
            lock (entry)
            {
                entry.Value = value;
            }
        }

        MoveToHead(entry);

        _tail ??= _head;
    }

    private bool RemoveByKey(TKey key)
    {
        if (!_cacheValue.TryGetValue(key, out var entry))
        {
            return false;
        }

        lock (_lockObj)
        {
            RemoveFromLink(entry);
            _cacheValue.Remove(entry.Key);
            --Count;
        }

        return true;
    }

    private bool IsFull => Count == _capacity;

    #endregion
}