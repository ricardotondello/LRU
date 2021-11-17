using BenchmarkDotNet.Attributes;

namespace LRU.Benchmark
{
    [MemoryDiagnoser]
    public class LeastRecentlyUsedCacheBenchmark
    {
        [Benchmark]
        public void Promote()
        {
            var cache = new LeastRecentlyUsedCache<int, int>(30);
            for (var i = 1; i < 10_000; i++)
            {
                cache.Add(i, 1);
            }

            for (var i = 1; i < 10_000; i++)
            {
                cache.TryGetValue(i, out var result);
            }

        }
    }
}

