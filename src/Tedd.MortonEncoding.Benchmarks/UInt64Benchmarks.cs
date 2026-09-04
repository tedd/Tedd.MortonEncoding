using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Current = Tedd.MortonEncoding;

namespace Tedd.MortonEncodingBenchmarks;

[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class UInt64Benchmarks
{
    private const int BatchSize = 1024;

    private Coordinate2D[] _coordinates2D = null!;
    private Coordinate3D[] _coordinates3D = null!;
    private ulong[] _morton2D = null!;
    private ulong[] _morton3D = null!;

    [GlobalSetup]
    public void Setup()
    {
        _coordinates2D = new Coordinate2D[BatchSize];
        _coordinates3D = new Coordinate3D[BatchSize];
        _morton2D = new ulong[BatchSize];
        _morton3D = new ulong[BatchSize];

        uint state = 0xD1B54A35u;
        for (int i = 0; i < BatchSize; i++)
        {
            uint x2D = Next(ref state);
            uint y2D = Next(ref state);
            uint x3D = Next(ref state) & 0x001FFFFFu;
            uint y3D = Next(ref state) & 0x001FFFFFu;
            uint z3D = Next(ref state) & 0x001FFFFFu;

            _coordinates2D[i] = new Coordinate2D(x2D, y2D);
            _coordinates3D[i] = new Coordinate3D(x3D, y3D, z3D);
            _morton2D[i] = Current.Encode64Fallback(x2D, y2D);
            _morton3D[i] = Current.Encode64Fallback(x3D, y3D, z3D);
        }
    }

    [Benchmark(Baseline = true, OperationsPerInvoke = BatchSize)]
    [BenchmarkCategory("Encode64 2D")]
    public ulong CurrentDispatchEncode2D()
    {
        Coordinate2D[] inputs = _coordinates2D;
        ulong checksum = 0;
        for (int i = 0; i < inputs.Length; i++)
        {
            Coordinate2D input = inputs[i];
            checksum = unchecked(checksum + Current.Encode64(input.X, input.Y));
        }

        return checksum;
    }

    [Benchmark(OperationsPerInvoke = BatchSize)]
    [BenchmarkCategory("Encode64 2D")]
    public ulong CurrentSoftwareEncode2D()
    {
        Coordinate2D[] inputs = _coordinates2D;
        ulong checksum = 0;
        for (int i = 0; i < inputs.Length; i++)
        {
            Coordinate2D input = inputs[i];
            checksum = unchecked(checksum + Current.Encode64Fallback(input.X, input.Y));
        }

        return checksum;
    }

    [Benchmark(Baseline = true, OperationsPerInvoke = BatchSize)]
    [BenchmarkCategory("Decode64 2D")]
    public ulong CurrentDispatchDecode2D()
    {
        ulong[] inputs = _morton2D;
        ulong checksum = 0;
        for (int i = 0; i < inputs.Length; i++)
        {
            Current.Decode64(inputs[i], out uint x, out uint y);
            checksum = unchecked(checksum + Pack2D(x, y));
        }

        return checksum;
    }

    [Benchmark(OperationsPerInvoke = BatchSize)]
    [BenchmarkCategory("Decode64 2D")]
    public ulong CurrentSoftwareDecode2D()
    {
        ulong[] inputs = _morton2D;
        ulong checksum = 0;
        for (int i = 0; i < inputs.Length; i++)
        {
            Current.Decode64Fallback(inputs[i], out uint x, out uint y);
            checksum = unchecked(checksum + Pack2D(x, y));
        }

        return checksum;
    }

    [Benchmark(Baseline = true, OperationsPerInvoke = BatchSize)]
    [BenchmarkCategory("Encode64 3D")]
    public ulong CurrentDispatchEncode3D()
    {
        Coordinate3D[] inputs = _coordinates3D;
        ulong checksum = 0;
        for (int i = 0; i < inputs.Length; i++)
        {
            Coordinate3D input = inputs[i];
            checksum = unchecked(checksum + Current.Encode64(input.X, input.Y, input.Z));
        }

        return checksum;
    }

    [Benchmark(OperationsPerInvoke = BatchSize)]
    [BenchmarkCategory("Encode64 3D")]
    public ulong CurrentSoftwareEncode3D()
    {
        Coordinate3D[] inputs = _coordinates3D;
        ulong checksum = 0;
        for (int i = 0; i < inputs.Length; i++)
        {
            Coordinate3D input = inputs[i];
            checksum = unchecked(checksum + Current.Encode64Fallback(input.X, input.Y, input.Z));
        }

        return checksum;
    }

    [Benchmark(Baseline = true, OperationsPerInvoke = BatchSize)]
    [BenchmarkCategory("Decode64 3D")]
    public ulong CurrentDispatchDecode3D()
    {
        ulong[] inputs = _morton3D;
        ulong checksum = 0;
        for (int i = 0; i < inputs.Length; i++)
        {
            Current.Decode64(inputs[i], out uint x, out uint y, out uint z);
            checksum = unchecked(checksum + Pack3D(x, y, z));
        }

        return checksum;
    }

    [Benchmark(OperationsPerInvoke = BatchSize)]
    [BenchmarkCategory("Decode64 3D")]
    public ulong CurrentSoftwareDecode3D()
    {
        ulong[] inputs = _morton3D;
        ulong checksum = 0;
        for (int i = 0; i < inputs.Length; i++)
        {
            Current.Decode64Fallback(inputs[i], out uint x, out uint y, out uint z);
            checksum = unchecked(checksum + Pack3D(x, y, z));
        }

        return checksum;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Pack2D(uint x, uint y) => x | ((ulong)y << 32);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Pack3D(uint x, uint y, uint z) => x | ((ulong)y << 21) | ((ulong)z << 42);

    private static uint Next(ref uint state)
    {
        state = unchecked((state * 1664525u) + 1013904223u);
        return state;
    }

    private readonly struct Coordinate2D
    {
        public Coordinate2D(uint x, uint y)
        {
            X = x;
            Y = y;
        }

        public uint X { get; }

        public uint Y { get; }
    }

    private readonly struct Coordinate3D
    {
        public Coordinate3D(uint x, uint y, uint z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public uint X { get; }

        public uint Y { get; }

        public uint Z { get; }
    }
}
