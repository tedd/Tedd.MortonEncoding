using System;
using Xunit;
using Tedd;

namespace Tedd.MortonEncodingTests
{
    public class MockedIntrinsicsTests
    {
        [Theory]
        [InlineData(UInt32.MaxValue, UInt32.MaxValue)]
        [InlineData(0x80000000u, 0x80000000u)]
        [InlineData(0x7FFFFFFFu, 0x7FFFFFFFu)]
        public void EncodeDecode_2D_Fallback_Test(UInt32 x, UInt32 y)
        {
            UInt32 encoded = MortonEncoding.EncodeFallback(x, y);
            MortonEncoding.DecodeFallback(encoded, out UInt32 decodedX, out UInt32 decodedY);
            Assert.Equal(x & 0xFFFF, decodedX);
            Assert.Equal(y & 0xFFFF, decodedY);
        }

        [Theory]
        [InlineData(UInt32.MaxValue, UInt32.MaxValue, UInt32.MaxValue)]
        [InlineData(0x80000000u, 0x80000000u, 0x80000000u)]
        [InlineData(0x7FFFFFFFu, 0x7FFFFFFFu, 0x7FFFFFFFu)]
        public void EncodeDecode_3D_Fallback_Test(UInt32 x, UInt32 y, UInt32 z)
        {
            UInt32 encoded = MortonEncoding.EncodeFallback(x, y, z);
            MortonEncoding.DecodeFallback(encoded, out UInt32 decodedX, out UInt32 decodedY, out UInt32 decodedZ);
            Assert.Equal(x & 0x3FF, decodedX);
            Assert.Equal(y & 0x3FF, decodedY);
            Assert.Equal(z & 0x3FF, decodedZ);
        }
    }
}
