using System;
using Tedd;
using Xunit;

namespace Tedd.MortonEncodingTests
{
    public sealed class DispatcherAndFallbackTests
    {
        [Fact]
        public void DeterministicRandomCoordinates_MatchReferenceAndFallback()
        {
            UInt32 state = 0xC001D00Du;
            for (var iteration = 0; iteration < 8192; iteration++)
            {
                var x = ReferenceMorton.NextUInt32(ref state);
                var y = ReferenceMorton.NextUInt32(ref state);
                var z = ReferenceMorton.NextUInt32(ref state);

                var expected2D32 = ReferenceMorton.Encode2D32(x, y);
                Assert.Equal(expected2D32, MortonEncoding.Encode(x, y));
                Assert.Equal(expected2D32, MortonEncoding.EncodeFallback(x, y));

                var expected3D32 = ReferenceMorton.Encode3D32(x, y, z);
                Assert.Equal(expected3D32, MortonEncoding.Encode(x, y, z));
                Assert.Equal(expected3D32, MortonEncoding.EncodeFallback(x, y, z));

                var expected2D64 = ReferenceMorton.Encode2D64(x, y);
                Assert.Equal(expected2D64, MortonEncoding.Encode64(x, y));
                Assert.Equal(expected2D64, MortonEncoding.Encode64Fallback(x, y));

                var expected3D64 = ReferenceMorton.Encode3D64(x, y, z);
                Assert.Equal(expected3D64, MortonEncoding.Encode64(x, y, z));
                Assert.Equal(expected3D64, MortonEncoding.Encode64Fallback(x, y, z));
            }
        }

        [Fact]
        public void ThirtyTwoBit2D_HighInputBitsAreIgnoredWithoutAliasing()
        {
            const UInt32 xLow = 0xA55Au;
            const UInt32 yLow = 0x5AA5u;
            var expected = ReferenceMorton.Encode2D32(xLow, yLow);

            for (var bit = 16; bit < 32; bit++)
            {
                var highBit = 1u << bit;
                Assert.Equal(expected, MortonEncoding.Encode(xLow | highBit, yLow));
                Assert.Equal(expected, MortonEncoding.Encode(xLow, yLow | highBit));
                Assert.Equal(expected, MortonEncoding.EncodeFallback(xLow | highBit, yLow));
                Assert.Equal(expected, MortonEncoding.EncodeFallback(xLow, yLow | highBit));
            }
        }

        [Fact]
        public void ThirtyTwoBit3D_HighInputBitsAreIgnoredWithoutAliasing()
        {
            const UInt32 xLow = 0x155u;
            const UInt32 yLow = 0x2AAu;
            const UInt32 zLow = 0x3C3u;
            var expected = ReferenceMorton.Encode3D32(xLow, yLow, zLow);

            for (var bit = 10; bit < 32; bit++)
            {
                var highBit = 1u << bit;
                Assert.Equal(expected, MortonEncoding.Encode(xLow | highBit, yLow, zLow));
                Assert.Equal(expected, MortonEncoding.Encode(xLow, yLow | highBit, zLow));
                Assert.Equal(expected, MortonEncoding.Encode(xLow, yLow, zLow | highBit));
                Assert.Equal(expected, MortonEncoding.EncodeFallback(xLow | highBit, yLow, zLow));
                Assert.Equal(expected, MortonEncoding.EncodeFallback(xLow, yLow | highBit, zLow));
                Assert.Equal(expected, MortonEncoding.EncodeFallback(xLow, yLow, zLow | highBit));
            }
        }

        [Fact]
        public void SixtyFourBit3D_HighInputBitsAreIgnoredWithoutAliasing()
        {
            const UInt32 xLow = 0x15555u;
            const UInt32 yLow = 0x0AAAAu;
            const UInt32 zLow = 0x1C3C3u;
            var expected = ReferenceMorton.Encode3D64(xLow, yLow, zLow);

            for (var bit = 21; bit < 32; bit++)
            {
                var highBit = 1u << bit;
                Assert.Equal(expected, MortonEncoding.Encode64(xLow | highBit, yLow, zLow));
                Assert.Equal(expected, MortonEncoding.Encode64(xLow, yLow | highBit, zLow));
                Assert.Equal(expected, MortonEncoding.Encode64(xLow, yLow, zLow | highBit));
                Assert.Equal(expected, MortonEncoding.Encode64Fallback(xLow | highBit, yLow, zLow));
                Assert.Equal(expected, MortonEncoding.Encode64Fallback(xLow, yLow | highBit, zLow));
                Assert.Equal(expected, MortonEncoding.Encode64Fallback(xLow, yLow, zLow | highBit));
            }
        }

        [Fact]
        public void DispatchersAndFallbacksProduceIdenticalDecodedCoordinates()
        {
            UInt32 state = 0x8BADF00Du;
            for (var iteration = 0; iteration < 4096; iteration++)
            {
                var morton32 = ReferenceMorton.NextUInt32(ref state);
                MortonEncoding.Decode(morton32, out var x2D32, out var y2D32);
                MortonEncoding.DecodeFallback(morton32, out var fallbackX2D32, out var fallbackY2D32);
                Assert.Equal(fallbackX2D32, x2D32);
                Assert.Equal(fallbackY2D32, y2D32);

                MortonEncoding.Decode(morton32, out var x3D32, out var y3D32, out var z3D32);
                MortonEncoding.DecodeFallback(
                    morton32,
                    out var fallbackX3D32,
                    out var fallbackY3D32,
                    out var fallbackZ3D32);
                Assert.Equal(fallbackX3D32, x3D32);
                Assert.Equal(fallbackY3D32, y3D32);
                Assert.Equal(fallbackZ3D32, z3D32);

                var morton64 = ReferenceMorton.NextUInt64(ref state);
                MortonEncoding.Decode64(morton64, out var x2D64, out var y2D64);
                MortonEncoding.Decode64Fallback(
                    morton64,
                    out var fallbackX2D64,
                    out var fallbackY2D64);
                Assert.Equal(fallbackX2D64, x2D64);
                Assert.Equal(fallbackY2D64, y2D64);

                MortonEncoding.Decode64(morton64, out var x3D64, out var y3D64, out var z3D64);
                MortonEncoding.Decode64Fallback(
                    morton64,
                    out var fallbackX3D64,
                    out var fallbackY3D64,
                    out var fallbackZ3D64);
                Assert.Equal(fallbackX3D64, x3D64);
                Assert.Equal(fallbackY3D64, y3D64);
                Assert.Equal(fallbackZ3D64, z3D64);
            }
        }
    }
}
