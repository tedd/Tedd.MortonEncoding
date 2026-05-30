using BenchmarkDotNet.Running;

namespace Tedd.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<SoftwareFallbackBenchmark>();
        }
    }
}
