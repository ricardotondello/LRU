using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

[assembly: ExcludeFromCodeCoverage]

namespace LRU.Tests;

public class LeastRecentlyUsedCacheTests
{
    private readonly object _lock = new();

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void Should_Return_ArgumentException_When_Capacity_Is_Invalid(int capacity)
    {
        var ex = Assert.Throws<ArgumentException>(() => new LeastRecentlyUsedCache<int, int>(capacity));
        Assert.Equal("Must be greater than 0. (Parameter 'capacity')", ex.Message);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(100)]
    public void Should_Not_Have_More_Than_Capacity(int capacity)
    {
        var cache = new LeastRecentlyUsedCache<int, int>(capacity);

        for (var i = 0; i < capacity + 1; i++)
        {
            cache.Add(i, i + new Random().Next(10));
        }

        cache.TryGetValue(capacity, out var validPosition);

        Assert.True(validPosition > 0);

        var result = cache.TryGetValue(capacity + 1, out _);

        Assert.False(result);
    }

    [Theory]
    [ClassData(typeof(LruTestData))]
    public void Should_Return_Expected_Value(int capacity, int[,] values, int key, int expected)
    {
        var cache = new LeastRecentlyUsedCache<int, int>(capacity);

        for (var i = 0; i < values.GetLength(0); i++)
        {
            cache.Add(values[i, 0], values[i, 1]);
        }

        cache.TryGetValue(key, out var result);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(2)]
    public void Should_Remove_Item_From_Cache(int capacity)
    {
        var cache = new LeastRecentlyUsedCache<int, decimal>(capacity) { { 1, 1 }, { 2, 2 } };

        cache.Remove(2);

        var result = cache.TryGetValue(2, out _);

        Assert.False(result);

        var result2 = cache.TryGetValue(1, out _);
        Assert.True(result2);
    }

    [Theory]
    [InlineData(2)]
    public void Should_Contains_Item(int capacity)
    {
        var cache = new LeastRecentlyUsedCache<int, int>(capacity) { { 1, 1 }, { 2, 2 }, { 3, 3 }, { 4, 4 } };
        
        var contains = cache.Contains(new KeyValuePair<int, int>(3, 3));
        var notContains = cache.Contains(new KeyValuePair<int, int>(5, 5));
        
        Assert.True(contains);
        Assert.True(cache.ContainsKey(4));
        Assert.False(cache.ContainsKey(2));
        Assert.False(notContains);
    }

    [Theory]
    [InlineData(4)]
    public void Should_Clear_Items_From_Cache(int capacity)
    {
        var cache = new LeastRecentlyUsedCache<int, int>(capacity) { { 1, 1 }, { 2, 2 }, { 3, 3 }, { 4, 4 } };

        Assert.Equal(4, cache.Count);
        cache.Clear();
        Assert.Empty(cache);
    }

    [Theory]
    [InlineData(4)]
    public void Should_GetEnumerator_Items(int capacity)
    {
        var cache = new LeastRecentlyUsedCache<int, int>(capacity) { { 1, 9 }, { 2, 8 }, { 3, 7 }, { 4, 6 } };

        var enumeratorValue = cache.GetEnumerator();
        enumeratorValue.MoveNext();
        Assert.Equal(1, enumeratorValue.Current.Key);
        Assert.Equal(9, enumeratorValue.Current.Value);

        enumeratorValue.MoveNext();
        Assert.Equal(2, enumeratorValue.Current.Key);
        Assert.Equal(8, enumeratorValue.Current.Value);
        enumeratorValue.Dispose();
    }

    [Fact]
    public void Should_GetEnumeratorOnIEnumerable_Items()
    {
        var cache = new LeastRecentlyUsedCache<int, int>(1);
        if (cache == null) throw new ArgumentNullException(nameof(cache));

        var enumerableCache = AsWeakEnumerable(cache);
        var result = enumerableCache.Cast<KeyValuePair<int, int>>()
            .Take(1)
            .ToArray();
        result.GetEnumerator()
            .MoveNext();
        Assert.NotNull(result.GetEnumerator());
        return;

        IEnumerable AsWeakEnumerable(IEnumerable source)
        {
            foreach (var o in source)
            {
                yield return o;
            }
        }
    }

    [Fact]
    public void Should_Throw_ArgumentNullException_When_CopyTo_Is_Called_With_Null_Argument()
    {
        var cache = new LeastRecentlyUsedCache<int, int>(1) { { 1, 9 } };

        Assert.Throws<ArgumentNullException>(() => cache.CopyTo(null!, 0));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void Should_Throw_ArgumentOutOfRangeException_When_CopyTo_Is_Called_With_Invalid_IndexArray(int invalidIndex)
    {
        var cache = new LeastRecentlyUsedCache<int, int>(1) { { 1, 9 } };

        var arrayResult = new[] { new KeyValuePair<int, int>(1, 2) };

        Assert.Throws<ArgumentOutOfRangeException>(() => cache.CopyTo(arrayResult, invalidIndex));
    }

    [Theory]
    [InlineData(0)]
    public void Should_Throw_ArgumentException_When_CopyTo_Items_Does_Not_Fit_In_Array(int arrayIndex)
    {
        var cache = new LeastRecentlyUsedCache<int, int>(4) { { 1, 9 }, { 2, 9 }, { 3, 9 }, { 4, 9 } };

        var arrayResult = new[] { new KeyValuePair<int, int>(1, 2) };

        var ex = Assert.Throws<ArgumentException>(() => cache.CopyTo(arrayResult, arrayIndex));
        Assert.Equal("Not enough elements after arrayIndex in the destination array.", ex.Message);
    }

    [Theory]
    [InlineData(4)]
    public void Should_CopyTo_Array(int capacity)
    {
        var cache = new LeastRecentlyUsedCache<int, int>(capacity) { { 1, 9 }, { 2, 8 }, { 3, 7 }, { 4, 6 } };

        var arrayResult = new KeyValuePair<int, int>[4];
        cache.CopyTo(arrayResult, 0);

        Assert.True(arrayResult.All(x => cache.ContainsKey(x.Key) && cache.Values.Contains(x.Value)));
    }

    [Fact]
    public void Should_IsReadOnly_Be_False()
    {
        var cache = new LeastRecentlyUsedCache<int, int>(1) { new KeyValuePair<int, int>(1, 1) };
        if (cache == null)
        {
            throw new ArgumentNullException(nameof(cache));
        }

        Assert.False(cache.IsReadOnly);
    }

    [Fact]
    public void Should_ReturnValues_When_Get_Or_Set_The_Element_With_Specified_Key()
    {
        var cache = new LeastRecentlyUsedCache<int, int>(1) { new KeyValuePair<int, int>(1, 1) };

        cache[1] = 2;
        Assert.Equal(2, cache[1]);
    }

    [Fact]
    public void Should_ReturnDefaultValue_When_Get_The_Element_Is_Not_Present()
    {
        var cache = new LeastRecentlyUsedCache<int, int>(1) { new KeyValuePair<int, int>(1, 1) };

        Assert.Equal(0, cache[10]);
    }

    [Theory]
    [InlineData(2)]
    public void Should_Not_Remove_Item_From_Cache(int capacity)
    {
        var cache = new LeastRecentlyUsedCache<int, decimal>(capacity) { { 1, 1 }, { 2, 2 } };
        var key = new KeyValuePair<int, decimal>(3, 3m);
        var result = cache.Remove(key);
        Assert.False(result);
    }

    [Fact]
    public async Task Should_Cache_Be_Thread_Safe()
    {
        const int key = 1;
        var resultForTestingThreadSafe = 0;
        var cache = new LeastRecentlyUsedCache<int, int>(2);
        var isFirstTime = true;

        var tasks = Enumerable.Range(1, 100)
            .Select(_ => Task.Run(() =>
            {
                var r = new Random();
                cache.Add(key, Set(r.Next(1, int.MaxValue)));
                return Task.CompletedTask;
            }))
            .ToArray();

        await Task.WhenAll(tasks);

        Assert.True(cache.TryGetValue(key, out var result));
        Assert.Equal(resultForTestingThreadSafe, result);
        return;

        int Set(int i)
        {
            lock (_lock)
            {
                if (isFirstTime)
                {
                    isFirstTime = false;
                }

                resultForTestingThreadSafe = i;
            }

            return i;
        }
    }

    [Fact]
    public void Should_ReturnKeys_When_Get_Keys()
    {
        var cache = new LeastRecentlyUsedCache<int, int>(1) { new KeyValuePair<int, int>(1, 1) };

        Assert.Equivalent(new List<int> { 1 }, cache.Keys);
    }

    private class LruTestData : TheoryData<int, int[,], int, int>
    {
        public LruTestData()
        {
            Add(2, new[,] { { 1, 1 }, { 2, 2 }, { 3, 3 }, { 4, 4 } }, 3, 3);

            Add(50, new[,] { { 10, 90 }, { 20, 77 }, { 33, 5 }, { 35, 44 }, { 45, 89 } }, 45, 89);
            //repeated data test
            Add(2, new[,] { { 1, 1 }, { 2, 2 }, { 3, 3 }, { 4, 4 }, { 4, 4 }, { 4, 4 }, { 4, 4 }, { 4, 4 } }, 3, 3);
        }
    }
}