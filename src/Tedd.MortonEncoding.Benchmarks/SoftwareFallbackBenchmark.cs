using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System;

namespace Tedd.Benchmarks
{
    [MemoryDiagnoser]
    public class SoftwareFallbackBenchmark
    {
        private uint _x;
        private uint _y;

        [GlobalSetup]
        public void Setup()
        {
            var rand = new Random(42);
            _x = (uint)rand.Next(0, 65535);
            _y = (uint)rand.Next(0, 65535);
        }

        [Benchmark(Baseline = true)]
        public uint LegacyEncode2D()
        {
            uint x = _x;
            uint y = _y;
            x = (x | x << 16) & 0x0000FFFF;
            x = (x | x << 8) & 0x00FF00FF;
            x = (x | x << 4) & 0x0F0F0F0F;
            x = (x | x << 2) & 0x33333333;
            x = (x | x << 1) & 0x55555555;

            y = (y | y << 16) & 0x0000FFFF;
            y = (y | y << 8) & 0x00FF00FF;
            y = (y | y << 4) & 0x0F0F0F0F;
            y = (y | y << 2) & 0x33333333;
            y = (y | y << 1) & 0x55555555;

            return x | (y << 1);
        }

        [Benchmark]
        public uint OptimizedEncode2D()
        {
            uint x = _x;
            uint y = _y;
            x = x & 0x0000FFFF;
            x = (x | x << 8) & 0x00FF00FF;
            x = (x | x << 4) & 0x0F0F0F0F;
            x = (x | x << 2) & 0x33333333;
            x = (x | x << 1) & 0x55555555;

            y = y & 0x0000FFFF;
            y = (y | y << 8) & 0x00FF00FF;
            y = (y | y << 4) & 0x0F0F0F0F;
            y = (y | y << 2) & 0x33333333;
            y = (y | y << 1) & 0x55555555;

            return x | (y << 1);
        }
    }
}
