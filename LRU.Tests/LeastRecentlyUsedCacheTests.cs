﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
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
        var act = () => new LeastRecentlyUsedCache<int, int>(capacity);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Must be greater than 0. (Parameter 'capacity')");
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

        validPosition.Should().BeGreaterThan(0);

        var result = cache.TryGetValue(capacity + 1, out _);

        result.Should().BeFalse();
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

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(2)]
    public void Should_Remove_Item_From_Cache(int capacity)
    {
        var cache = new LeastRecentlyUsedCache<int, decimal>(capacity) { { 1, 1 }, { 2, 2 } };

        cache.Remove(2);

        var result = cache.TryGetValue(2, out _);

        result.Should().BeFalse();

        var result2 = cache.TryGetValue(1, out _);
        result2.Should().BeTrue();
    }

    [Theory]
    [InlineData(2)]
    public void Should_Contains_Item(int capacity)
    {
        var cache = new LeastRecentlyUsedCache<int, int>(capacity)
        {
            { 1, 1 },
            { 2, 2 },
            { 3, 3 },
            { 4, 4 }
        };

        cache.Contains(new KeyValuePair<int, int>(3, 3)).Should().BeTrue();
        cache.ContainsKey(4).Should().BeTrue();
        cache.ContainsKey(2).Should().BeFalse();
        cache.Contains(new KeyValuePair<int, int>(5, 5)).Should().BeFalse();
    }

    [Theory]
    [InlineData(4)]
    public void Should_Clear_Items_From_Cache(int capacity)
    {
        var cache = new LeastRecentlyUsedCache<int, int>(capacity)
        {
            { 1, 1 },
            { 2, 2 },
            { 3, 3 },
            { 4, 4 }
        };

        cache.Count.Should().Be(4);
        cache.Clear();
        cache.Count.Should().Be(0);
    }

    [Theory]
    [InlineData(4)]
    public void Should_GetEnumerator_Items(int capacity)
    {
        var cache = new LeastRecentlyUsedCache<int, int>(capacity)
        {
            { 1, 9 },
            { 2, 8 },
            { 3, 7 },
            { 4, 6 }
        };

        var enumeratorValue = cache.GetEnumerator();
        enumeratorValue.MoveNext();
        enumeratorValue.Current.Key.Should().Be(1);
        enumeratorValue.Current.Value.Should().Be(9);

        enumeratorValue.MoveNext();
        enumeratorValue.Current.Key.Should().Be(2);
        enumeratorValue.Current.Value.Should().Be(8);
        enumeratorValue.Dispose();
    }

    [Fact]
    public void Should_GetEnumeratorOnIEnumerable_Items()
    {
        var cache = new LeastRecentlyUsedCache<int, int>(1);
        if (cache == null) throw new ArgumentNullException(nameof(cache));

        var enumerableCache = AsWeakEnumerable(cache);
        var result = enumerableCache.Cast<KeyValuePair<int, int>>().Take(1).ToArray();
        result.GetEnumerator().MoveNext();
        result.GetEnumerator().Should().NotBeNull();
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
        var cache = new LeastRecentlyUsedCache<int, int>(1)
        {
            { 1, 9 }
        };

        var act = () => cache.CopyTo(null!, 0);

        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void Should_Throw_ArgumentOutOfRangeException_When_CopyTo_Is_Called_With_Invalid_IndexArray(int invalidIndex)
    {
        var cache = new LeastRecentlyUsedCache<int, int>(1)
        {
            { 1, 9 }
        };

        var arrayResult = new[]
        {
            new KeyValuePair<int, int>(1, 2)
        };

        var act = () => cache.CopyTo(arrayResult, invalidIndex);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(0)]
    public void Should_Throw_ArgumentException_When_CopyTo_Items_Does_Not_Fit_In_Array(int arrayIndex)
    {
        var cache = new LeastRecentlyUsedCache<int, int>(4)
        {
            { 1, 9 },
            { 2, 9 },
            { 3, 9 },
            { 4, 9 }
        };

        var arrayResult = new[]
        {
            new KeyValuePair<int, int>(1, 2)
        };

        var act = () => cache.CopyTo(arrayResult, arrayIndex);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Not enough elements after arrayIndex in the destination array.");
    }

    [Theory]
    [InlineData(4)]
    public void Should_CopyTo_Array(int capacity)
    {
        var cache = new LeastRecentlyUsedCache<int, int>(capacity)
        {
            { 1, 9 },
            { 2, 8 },
            { 3, 7 },
            { 4, 6 }
        };

        var arrayResult = new KeyValuePair<int, int>[4];
        cache.CopyTo(arrayResult, 0);

        arrayResult.All(x => cache.ContainsKey(x.Key) && cache.Values.Contains(x.Value)).Should().BeTrue();
    }

    [Fact]
    public void Should_IsReadOnly_Be_False()
    {
        var cache = new LeastRecentlyUsedCache<int, int>(1) { new(1, 1) };
        if (cache == null) throw new ArgumentNullException(nameof(cache));
        cache.IsReadOnly.Should().BeFalse();
    }

    [Fact]
    public void Should_ReturnValues_When_Get_Or_Set_The_Element_With_Specified_Key()
    {
        var cache = new LeastRecentlyUsedCache<int, int>(1) { new(1, 1) };

        cache[1] = 2;
        cache[1].Should().Be(2);
    }

    [Fact]
    public void Should_ReturnDefaultValue_When_Get_The_Element_Is_Not_Present()
    {
        var cache = new LeastRecentlyUsedCache<int, int>(1) { new(1, 1) };

        cache[10].Should().Be(default);
    }

    [Theory]
    [InlineData(2)]
    public void Should_Not_Remove_Item_From_Cache(int capacity)
    {
        var cache = new LeastRecentlyUsedCache<int, decimal>(capacity) { { 1, 1 }, { 2, 2 } };
        var key = new KeyValuePair<int, decimal>(3, 3m);
        var result = cache.Remove(key);
        result.Should().BeFalse();
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
                }
            ))
            .ToArray();

        await Task.WhenAll(tasks);

        cache.TryGetValue(key, out var result).Should().BeTrue();
        result.Should().Be(resultForTestingThreadSafe);
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
        var cache = new LeastRecentlyUsedCache<int, int>(1) { new(1, 1) };

        cache.Keys.Should().BeEquivalentTo(new List<int> { 1 });
    }

    public class LruTestData : TheoryData<int, int[,], int, int>
    {
        public LruTestData()
        {
            Add(
                2, new[,]
                {
                    { 1, 1 }, { 2, 2 }, { 3, 3 }, { 4, 4 }
                },
                3, 3
            );

            Add(
                50, new[,]
                {
                    { 10, 90 }, { 20, 77 }, { 33, 5 }, { 35, 44 }, { 45, 89 }
                },
                45, 89
            );
            //repeated data test
            Add(
                2, new[,]
                {
                    { 1, 1 }, { 2, 2 }, { 3, 3 }, { 4, 4 }, { 4, 4 }, { 4, 4 }, { 4, 4 }, { 4, 4 }
                },
                3, 3
            );
        }
    }
}