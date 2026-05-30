using BenchmarkDotNet.Running;

namespace Tedd.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<SoftwareFallbackBenchmark>();
        }
    }
}
