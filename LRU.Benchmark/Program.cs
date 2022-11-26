using BenchmarkDotNet.Running;
using LRU.Benchmark;

BenchmarkRunner.Run<LeastRecentlyUsedCacheBenchmark>();
