using System;
using Xunit;
using Tedd;

namespace Tedd.MortonEncodingTests
{
    public class NullAndBoundaryTests
    {
        [Theory]
        [InlineData(0x55555555u, 0xAAAAAAAAu)]
        [InlineData(0xAAAAAAAAu, 0x55555555u)]
        [InlineData(0x0F0F0F0Fu, 0xF0F0F0F0u)]
        [InlineData(0x00FF00FFu, 0xFF00FF00u)]
        [InlineData(0x0000FFFFu, 0xFFFF0000u)]
        public void EncodeDecode_2D_Patterns_Fallback_Test(UInt32 x, UInt32 y)
        {
            UInt32 validX = x & 0xFFFF;
            UInt32 validY = y & 0xFFFF;
            UInt32 encoded = MortonEncoding.EncodeFallback(validX, validY);
            MortonEncoding.DecodeFallback(encoded, out UInt32 decodedX, out UInt32 decodedY);
            Assert.Equal(validX, decodedX);
            Assert.Equal(validY, decodedY);
        }

        [Theory]
        [InlineData(0x09249249u, 0x12492492u, 0x24924924u)]
        [InlineData(0x12492492u, 0x24924924u, 0x09249249u)]
        [InlineData(0x24924924u, 0x09249249u, 0x12492492u)]
        public void EncodeDecode_3D_Patterns_Fallback_Test(UInt32 x, UInt32 y, UInt32 z)
        {
            UInt32 validX = x & 0x3FF;
            UInt32 validY = y & 0x3FF;
            UInt32 validZ = z & 0x3FF;

            UInt32 encoded = MortonEncoding.EncodeFallback(validX, validY, validZ);
            MortonEncoding.DecodeFallback(encoded, out UInt32 decodedX, out UInt32 decodedY, out UInt32 decodedZ);

            Assert.Equal(validX, decodedX);
            Assert.Equal(validY, decodedY);
            Assert.Equal(validZ, decodedZ);
        }
    }
}
