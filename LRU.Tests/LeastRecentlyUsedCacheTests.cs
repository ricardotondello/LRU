using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

[assembly: ExcludeFromCodeCoverage]
namespace LRU.Tests
{
    public class LeastRecentlyUsedCacheTests
    {
        [Test]
        [TestCase(null)]
        [TestCase(-1)]
        [TestCase(0)]
        public void Should_Return_ArgumentException_When_Capacity_Is_Invalid(int capacity)
        {
            var act = () => new LeastRecentlyUsedCache<int, int>(capacity);

            act.Should().Throw<ArgumentException>()
                .WithMessage("Must be greater than 0. (Parameter 'capacity')");
        }

        [Test]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(100)]
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


        public static IEnumerable<TestCaseData> ExpectedValueTest()
        {
            yield return new TestCaseData(2, new[,]
            {
                { 1, 1 }, { 2, 2 }, { 3, 3 }, { 4, 4 }
            }, 3, 3);

            yield return new TestCaseData(50, new[,]
            {
                { 10, 90 }, { 20, 77 }, { 33, 5 }, { 35, 44 }, {45, 89}
            }, 45, 89);

            //repeated data test
            yield return new TestCaseData(2, new[,]
            {
                { 1, 1 }, { 2, 2 }, { 3, 3 }, { 4, 4 }, { 4, 4 }, { 4, 4 }, { 4, 4 }, { 4, 4 }
            }, 3, 3);

        }

        [Test]
        [TestCaseSource(nameof(ExpectedValueTest))]
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

        [Test]
        [TestCase(2)]
        public void Should_Remove_Item_From_Cache(int capacity)
        {
            var cache = new LeastRecentlyUsedCache<int, decimal>(capacity) { { 1, 1 }, { 2, 2 } };

            cache.Remove(2);

            var result = cache.TryGetValue(2, out _);

            result.Should().BeFalse();

            var result2 = cache.TryGetValue(1, out _);
            result2.Should().BeTrue();
        }

        [Test]
        [TestCase(2)]
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
        }

        [Test]
        [TestCase(4)]
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

        [Test]
        [TestCase(4)]
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
        
        [Test]
        public void Should_GetEnumeratorOnIEnumerable_Items()
        {
            IEnumerable AsWeakEnumerable(IEnumerable source)
            {
                foreach (object o in source)
                {
                    yield return o;
                }
            }
            var cache = new LeastRecentlyUsedCache<int, int>(1);

            IEnumerable enumerableCache = AsWeakEnumerable(cache);
            var result = enumerableCache.Cast<KeyValuePair<int, int>>().Take(1).ToArray();
            result.GetEnumerator().MoveNext();
            result.GetEnumerator().Should().NotBeNull();
                
        }
        
        [Test]
        public void Should_Throw_ArgumentNullException_When_CopyTo_Is_Called_With_Null_Argument()
        {
            var cache = new LeastRecentlyUsedCache<int, int>(1)
            {
                { 1, 9 }
            };
            
            var act = () => cache.CopyTo(null!, 0);

            act.Should().Throw<ArgumentNullException>();
        }

        [Test]
        [TestCase(-1)]
        [TestCase(2)]
        public void Should_Throw_ArgumentOutOfRangeException_When_CopyTo_Is_Called_With_Invalid_IndexArray(int invalidIndex)
        {
            var cache = new LeastRecentlyUsedCache<int, int>(1)
            {
                { 1, 9 }
            };

            var arrayResult = new[]
            {
                new KeyValuePair<int, int>(1,2)
            };

            var act = () => cache.CopyTo(arrayResult, invalidIndex);

            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Test]
        [TestCase(0)]
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
                new KeyValuePair<int, int>(1,2)
            };

            var act = () => cache.CopyTo(arrayResult, arrayIndex);

            act.Should().Throw<ArgumentException>().WithMessage("Not enough elements after arrayIndex in the destination array.");
        }

        [Test]
        [TestCase(4)]
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

            arrayResult.All(x => cache.Keys.Contains(x.Key) && cache.Values.Contains(x.Value)).Should().BeTrue();

        }
        
        [Test]
        public void Should_IsReadOnly_Be_False()
        {
            var cache = new LeastRecentlyUsedCache<int, int>(1) {new KeyValuePair<int, int>(1, 1)};
            cache.IsReadOnly.Should().BeFalse();
        }
        
        [Test]
        public void Should_ReturnValues_When_Get_Or_Set_The_Element_With_Specified_Key()
        {
            var cache = new LeastRecentlyUsedCache<int, int>(1) {new KeyValuePair<int, int>(1, 1)};
            
            cache[1] = 2;
            cache[1].Should().Be(2);
        }
        
        [Test]
        [TestCase(2)]
        public void Should_Not_Remove_Item_From_Cache(int capacity)
        {
            var cache = new LeastRecentlyUsedCache<int, decimal>(capacity) { { 1, 1 }, { 2, 2 } };
            var key = new KeyValuePair<int, decimal>(3, 3m);
            var result = cache.Remove(key);
            result.Should().BeFalse();
        }
    }
}