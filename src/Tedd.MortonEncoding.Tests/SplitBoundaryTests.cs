using System;
using Tedd;
using Xunit;

namespace Tedd.MortonEncodingTests
{
    public sealed class SplitBoundaryTests
    {
        [Theory]
        [InlineData(0u, 0, 0u, 0u)]
        [InlineData(0u, 1, 0u, 0u)]
        [InlineData(1u, 1, 0u, 1u)]
        [InlineData(2u, 1, 1u, 0u)]
        [InlineData(3u, 1, 1u, 1u)]
        [InlineData(UInt32.MaxValue, 16, 0xFFFFu, 0xFFFFu)]
        public void SplitXY_HandlesZeroMinimumAndMaximumWidths(
            UInt32 packed,
            Int32 bits,
            UInt32 expectedX,
            UInt32 expectedY)
        {
            MortonEncoding.SplitXY(packed, bits, out var x, out var y);
            Assert.Equal(expectedX, x);
            Assert.Equal(expectedY, y);
        }

        [Theory]
        [InlineData(0u, 0, 0u, 0u, 0u)]
        [InlineData(0u, 1, 0u, 0u, 0u)]
        [InlineData(1u, 1, 0u, 0u, 1u)]
        [InlineData(2u, 1, 0u, 1u, 0u)]
        [InlineData(4u, 1, 1u, 0u, 0u)]
        [InlineData(UInt32.MaxValue, 10, 0x3FFu, 0x3FFu, 0x3FFu)]
        public void SplitXYZ_HandlesZeroMinimumAndMaximumWidths(
            UInt32 packed,
            Int32 bits,
            UInt32 expectedX,
            UInt32 expectedY,
            UInt32 expectedZ)
        {
            MortonEncoding.SplitXYZ(packed, bits, out var x, out var y, out var z);
            Assert.Equal(expectedX, x);
            Assert.Equal(expectedY, y);
            Assert.Equal(expectedZ, z);
        }

        [Fact]
        public void SplitXY_AllValidWidthsPreservePackedFields()
        {
            for (var bits = 1; bits <= 16; bits++)
            {
                var mask = (1u << bits) - 1;
                var expectedX = 0xA55Au & mask;
                var expectedY = 0x5AA5u & mask;
                var packed = (expectedX << bits) | expectedY;

                MortonEncoding.SplitXY(packed, bits, out var x, out var y);
                Assert.Equal(expectedX, x);
                Assert.Equal(expectedY, y);
            }
        }

        [Fact]
        public void SplitXYZ_AllValidWidthsPreservePackedFields()
        {
            for (var bits = 1; bits <= 10; bits++)
            {
                var mask = (1u << bits) - 1;
                var expectedX = 0x155u & mask;
                var expectedY = 0x2AAu & mask;
                var expectedZ = 0x3C3u & mask;
                var packed = (expectedX << (bits * 2)) | (expectedY << bits) | expectedZ;

                MortonEncoding.SplitXYZ(packed, bits, out var x, out var y, out var z);
                Assert.Equal(expectedX, x);
                Assert.Equal(expectedY, y);
                Assert.Equal(expectedZ, z);
            }
        }

        [Fact]
        public void SplitXYZ_IgnoresBitsAboveTheThreePackedFields()
        {
            const UInt32 packedThirtyBits = 0x155u << 20 | 0x2AAu << 10 | 0x3C3u;
            const UInt32 unusedTopBits = 0xC0000000u;

            MortonEncoding.SplitXYZ(packedThirtyBits | unusedTopBits, 10, out var x, out var y, out var z);
            Assert.Equal(0x155u, x);
            Assert.Equal(0x2AAu, y);
            Assert.Equal(0x3C3u, z);
        }
    }
}
