using BenchmarkDotNet.Running;

namespace LRU.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<LeastRecentlyUsedCacheBenchmark>();
        }
    }
}
