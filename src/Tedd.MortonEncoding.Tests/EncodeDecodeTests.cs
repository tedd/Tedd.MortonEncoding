
using System;
using Xunit;
using Tedd;

namespace Tedd.MortonEncodingTests
{
    public class EncodeDecodeTests
    {
        [Theory]
        [InlineData(0u, 0u)]
        [InlineData(1u, 1u)]
        [InlineData(0xFFFFu, 0xFFFFu)]
        [InlineData(0xFFFFu, 0u)]
        [InlineData(0u, 0xFFFFu)]
        [InlineData(0xAAAAu, 0x5555u)]
        [InlineData(0x5555u, 0xAAAAu)]
        public void EncodeDecode_2D_Test(UInt32 x, UInt32 y)
        {
            UInt32 encoded = MortonEncoding.Encode(x, y);
            MortonEncoding.Decode(encoded, out UInt32 decodedX, out UInt32 decodedY);
            Assert.Equal(x, decodedX);
            Assert.Equal(y, decodedY);
        }

        [Theory]
        [InlineData(0u, 0u, 0u)]
        [InlineData(1u, 1u, 1u)]
        [InlineData(0x3FFu, 0x3FFu, 0x3FFu)]
        [InlineData(0x3FFu, 0u, 0u)]
        [InlineData(0u, 0x3FFu, 0u)]
        [InlineData(0u, 0u, 0x3FFu)]
        [InlineData(0x2AAu, 0x155u, 0x0AAu)]
        public void EncodeDecode_3D_Test(UInt32 x, UInt32 y, UInt32 z)
        {
            UInt32 encoded = MortonEncoding.Encode(x, y, z);
            MortonEncoding.Decode(encoded, out UInt32 decodedX, out UInt32 decodedY, out UInt32 decodedZ);
            Assert.Equal(x, decodedX);
            Assert.Equal(y, decodedY);
            Assert.Equal(z, decodedZ);
        }
    }
}
