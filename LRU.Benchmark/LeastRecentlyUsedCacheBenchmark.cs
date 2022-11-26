using BenchmarkDotNet.Attributes;

namespace LRU.Benchmark
{
    [MemoryDiagnoser]
    public class LeastRecentlyUsedCacheBenchmark
    {
        private static readonly LeastRecentlyUsedCache<int, int> Cache = new (30);
        
        [Benchmark]
        public void Promote()
        {
            for (var i = 1; i < 10_000; i++)
            {
                Cache.Add(i, 1);
            }

            for (var i = 1; i < 10_000; i++)
            {
                Cache.TryGetValue(i, out var result);
            }

        }
    }
}

