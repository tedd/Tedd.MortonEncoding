using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System;

namespace Tedd.Benchmarks
{
    [MemoryDiagnoser]
    public class EncodingBenchmark
    {
        private uint _x;
        private uint _y;
        private uint _z;
        private uint _encoded2d;
        private uint _encoded3d;

        [GlobalSetup]
        public void Setup()
        {
            var rand = new Random(42);
            _x = (uint)rand.Next(0, 65536);
            _y = (uint)rand.Next(0, 65536);
            _z = (uint)rand.Next(0, 1024);
            _encoded2d = Tedd.Legacy.MortonEncoding.Encode(_x, _y);
            _encoded3d = Tedd.Legacy.MortonEncoding.Encode(_x, _y, _z);
        }

        [Benchmark(Baseline = true)]
        public uint LegacyEncode2D() => Tedd.Legacy.MortonEncoding.Encode(_x, _y);

        [Benchmark]
        public uint OptimizedEncode2D() => Tedd.MortonEncoding.Encode(_x, _y);

        [Benchmark]
        public uint LegacyEncode3D() => Tedd.Legacy.MortonEncoding.Encode(_x, _y, _z);

        [Benchmark]
        public uint OptimizedEncode3D() => Tedd.MortonEncoding.Encode(_x, _y, _z);

        [Benchmark]
        public void LegacyDecode2D() => Tedd.Legacy.MortonEncoding.Decode(_encoded2d, out _, out _);

        [Benchmark]
        public void OptimizedDecode2D() => Tedd.MortonEncoding.Decode(_encoded2d, out _, out _);

        [Benchmark]
        public void LegacyDecode3D() => Tedd.Legacy.MortonEncoding.Decode(_encoded3d, out _, out _, out _);

        [Benchmark]
        public void OptimizedDecode3D() => Tedd.MortonEncoding.Decode(_encoded3d, out _, out _, out _);
    }
}
