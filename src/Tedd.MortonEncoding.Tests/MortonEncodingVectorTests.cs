using System;
using Tedd;
using Xunit;

namespace Tedd.MortonEncodingTests
{
    public sealed class MortonEncodingVectorTests
    {
        [Theory]
        [InlineData(0u, 0u, 0u)]
        [InlineData(1u, 0u, 1u)]
        [InlineData(0u, 1u, 2u)]
        [InlineData(2u, 0u, 4u)]
        [InlineData(0u, 2u, 8u)]
        [InlineData(0x00FFu, 0u, 0x00005555u)]
        [InlineData(0u, 0x00FFu, 0x0000AAAAu)]
        [InlineData(0xFFFFu, 0u, 0x55555555u)]
        [InlineData(0u, 0xFFFFu, 0xAAAAAAAAu)]
        [InlineData(0xFFFFu, 0xFFFFu, UInt32.MaxValue)]
        public void ThirtyTwoBit2D_KnownVectors(UInt32 x, UInt32 y, UInt32 expected)
        {
            Assert.Equal(expected, MortonEncoding.Encode(x, y));
            Assert.Equal(expected, MortonEncoding.EncodeFallback(x, y));

            MortonEncoding.Decode(expected, out var decodedX, out var decodedY);
            MortonEncoding.DecodeFallback(expected, out var fallbackX, out var fallbackY);
            Assert.Equal(x, decodedX);
            Assert.Equal(y, decodedY);
            Assert.Equal(x, fallbackX);
            Assert.Equal(y, fallbackY);
        }

        [Theory]
        [InlineData(0u, 0u, 0u, 0u)]
        [InlineData(1u, 0u, 0u, 1u)]
        [InlineData(0u, 1u, 0u, 2u)]
        [InlineData(0u, 0u, 1u, 4u)]
        [InlineData(2u, 0u, 0u, 8u)]
        [InlineData(0u, 2u, 0u, 16u)]
        [InlineData(0u, 0u, 2u, 32u)]
        [InlineData(0x3FFu, 0u, 0u, 0x09249249u)]
        [InlineData(0u, 0x3FFu, 0u, 0x12492492u)]
        [InlineData(0u, 0u, 0x3FFu, 0x24924924u)]
        [InlineData(0x3FFu, 0x3FFu, 0x3FFu, 0x3FFFFFFFu)]
        public void ThirtyTwoBit3D_KnownVectors(UInt32 x, UInt32 y, UInt32 z, UInt32 expected)
        {
            Assert.Equal(expected, MortonEncoding.Encode(x, y, z));
            Assert.Equal(expected, MortonEncoding.EncodeFallback(x, y, z));

            MortonEncoding.Decode(expected, out var decodedX, out var decodedY, out var decodedZ);
            MortonEncoding.DecodeFallback(
                expected,
                out var fallbackX,
                out var fallbackY,
                out var fallbackZ);
            Assert.Equal(x, decodedX);
            Assert.Equal(y, decodedY);
            Assert.Equal(z, decodedZ);
            Assert.Equal(x, fallbackX);
            Assert.Equal(y, fallbackY);
            Assert.Equal(z, fallbackZ);
        }

        [Theory]
        [InlineData(0u, 0u, 0ul)]
        [InlineData(1u, 0u, 1ul)]
        [InlineData(0u, 1u, 2ul)]
        [InlineData(0x80000000u, 0u, 0x4000000000000000ul)]
        [InlineData(0u, 0x80000000u, 0x8000000000000000ul)]
        [InlineData(UInt32.MaxValue, 0u, 0x5555555555555555ul)]
        [InlineData(0u, UInt32.MaxValue, 0xAAAAAAAAAAAAAAAAul)]
        [InlineData(UInt32.MaxValue, UInt32.MaxValue, UInt64.MaxValue)]
        public void SixtyFourBit2D_KnownVectors(UInt32 x, UInt32 y, UInt64 expected)
        {
            Assert.Equal(expected, MortonEncoding.Encode64(x, y));
            Assert.Equal(expected, MortonEncoding.Encode64Fallback(x, y));

            MortonEncoding.Decode64(expected, out var decodedX, out var decodedY);
            MortonEncoding.Decode64Fallback(expected, out var fallbackX, out var fallbackY);
            Assert.Equal(x, decodedX);
            Assert.Equal(y, decodedY);
            Assert.Equal(x, fallbackX);
            Assert.Equal(y, fallbackY);
        }

        [Theory]
        [InlineData(0u, 0u, 0u, 0ul)]
        [InlineData(1u, 0u, 0u, 1ul)]
        [InlineData(0u, 1u, 0u, 2ul)]
        [InlineData(0u, 0u, 1u, 4ul)]
        [InlineData(0x00100000u, 0u, 0u, 0x1000000000000000ul)]
        [InlineData(0u, 0x00100000u, 0u, 0x2000000000000000ul)]
        [InlineData(0u, 0u, 0x00100000u, 0x4000000000000000ul)]
        [InlineData(0x001FFFFFu, 0u, 0u, 0x1249249249249249ul)]
        [InlineData(0u, 0x001FFFFFu, 0u, 0x2492492492492492ul)]
        [InlineData(0u, 0u, 0x001FFFFFu, 0x4924924924924924ul)]
        [InlineData(0x001FFFFFu, 0x001FFFFFu, 0x001FFFFFu, 0x7FFFFFFFFFFFFFFFul)]
        public void SixtyFourBit3D_KnownVectors(UInt32 x, UInt32 y, UInt32 z, UInt64 expected)
        {
            Assert.Equal(expected, MortonEncoding.Encode64(x, y, z));
            Assert.Equal(expected, MortonEncoding.Encode64Fallback(x, y, z));

            MortonEncoding.Decode64(expected, out var decodedX, out var decodedY, out var decodedZ);
            MortonEncoding.Decode64Fallback(
                expected,
                out var fallbackX,
                out var fallbackY,
                out var fallbackZ);
            Assert.Equal(x, decodedX);
            Assert.Equal(y, decodedY);
            Assert.Equal(z, decodedZ);
            Assert.Equal(x, fallbackX);
            Assert.Equal(y, fallbackY);
            Assert.Equal(z, fallbackZ);
        }

        [Fact]
        public void ThreeDimensionalDecoders_IgnoreUnusedHighMortonBits()
        {
            MortonEncoding.Decode(UInt32.MaxValue, out var x32, out var y32, out var z32);
            MortonEncoding.DecodeFallback(
                UInt32.MaxValue,
                out var fallbackX32,
                out var fallbackY32,
                out var fallbackZ32);
            Assert.Equal(ReferenceMorton.CoordinateMask3D32, x32);
            Assert.Equal(ReferenceMorton.CoordinateMask3D32, y32);
            Assert.Equal(ReferenceMorton.CoordinateMask3D32, z32);
            Assert.Equal(ReferenceMorton.CoordinateMask3D32, fallbackX32);
            Assert.Equal(ReferenceMorton.CoordinateMask3D32, fallbackY32);
            Assert.Equal(ReferenceMorton.CoordinateMask3D32, fallbackZ32);

            MortonEncoding.Decode64(UInt64.MaxValue, out var x64, out var y64, out var z64);
            MortonEncoding.Decode64Fallback(
                UInt64.MaxValue,
                out var fallbackX64,
                out var fallbackY64,
                out var fallbackZ64);
            Assert.Equal(ReferenceMorton.CoordinateMask3D64, x64);
            Assert.Equal(ReferenceMorton.CoordinateMask3D64, y64);
            Assert.Equal(ReferenceMorton.CoordinateMask3D64, z64);
            Assert.Equal(ReferenceMorton.CoordinateMask3D64, fallbackX64);
            Assert.Equal(ReferenceMorton.CoordinateMask3D64, fallbackY64);
            Assert.Equal(ReferenceMorton.CoordinateMask3D64, fallbackZ64);
        }
    }
}
