
using System;
using Xunit;
using Tedd;

namespace Tedd.MortonEncodingTests
{
    public class SplitTests
    {
        [Theory]
        [InlineData(0u, 1, 0u, 0u)]
        [InlineData(1u, 1, 0u, 1u)]
        [InlineData(2u, 1, 1u, 0u)]
        [InlineData(3u, 1, 1u, 1u)]
        [InlineData(0xFFFFFFFFu, 16, 0xFFFFu, 0xFFFFu)]
        public void SplitXY_Test(UInt32 input, int bits, UInt32 expectedX, UInt32 expectedY)
        {
            MortonEncoding.SplitXY(input, bits, out UInt32 x, out UInt32 y);
            Assert.Equal(expectedX, x);
            Assert.Equal(expectedY, y);
        }

        [Theory]
        [InlineData(0u, 1, 0u, 0u, 0u)]
        [InlineData(1u, 1, 0u, 0u, 1u)]
        [InlineData(2u, 1, 0u, 1u, 0u)]
        [InlineData(3u, 1, 0u, 1u, 1u)]
        [InlineData(4u, 1, 1u, 0u, 0u)]
        [InlineData(5u, 1, 1u, 0u, 1u)]
        [InlineData(6u, 1, 1u, 1u, 0u)]
        [InlineData(7u, 1, 1u, 1u, 1u)]
        [InlineData(0xFFFFFFFFu, 10, 0x3FFu, 0x3FFu, 0x3FFu)]
        public void SplitXYZ_Test(UInt32 input, int bits, UInt32 expectedX, UInt32 expectedY, UInt32 expectedZ)
        {
            MortonEncoding.SplitXYZ(input, bits, out UInt32 x, out UInt32 y, out UInt32 z);
            Assert.Equal(expectedX, x);
            Assert.Equal(expectedY, y);
            Assert.Equal(expectedZ, z);
        }
    }
}
