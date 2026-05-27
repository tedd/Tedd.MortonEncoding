
using System;
using Xunit;
using Tedd;
using System.Runtime.Intrinsics.X86;
using System.Reflection;

namespace Tedd.MortonEncodingTests
{
    public class MockedIntrinsicsTests
    {
        // Unfortunately, it seems Bmi2.IsSupported is a hardware intrinsic property that we might not be able to easily mock
        // without a framework like Moq or specific environment variables. We can use Reflection if we absolutely have to or just use preprocessor directives.

        // Since we cannot easily control X86.Bmi2.IsSupported during runtime without complex mocking,
        // we can test the methods when it is supported natively (if the machine supports it),
        // or we just acknowledge the 50% branch coverage limitation based on the running hardware.
        // Let's add some more edge case inputs just in case for full parameterization.

        [Theory]
        [InlineData(UInt32.MaxValue, UInt32.MaxValue)]
        [InlineData(0x80000000u, 0x80000000u)]
        [InlineData(0x7FFFFFFFu, 0x7FFFFFFFu)]
        public void EncodeDecode_2D_Boundary_Test(UInt32 x, UInt32 y)
        {
            UInt32 encoded = MortonEncoding.Encode(x, y);
            MortonEncoding.Decode(encoded, out UInt32 decodedX, out UInt32 decodedY);
            // Since max bits used by Encode is 16 bits for x and y, decoded should match the masked input
            Assert.Equal(x & 0xFFFF, decodedX);
            Assert.Equal(y & 0xFFFF, decodedY);
        }

        [Theory]
        [InlineData(UInt32.MaxValue, UInt32.MaxValue, UInt32.MaxValue)]
        [InlineData(0x80000000u, 0x80000000u, 0x80000000u)]
        [InlineData(0x7FFFFFFFu, 0x7FFFFFFFu, 0x7FFFFFFFu)]
        public void EncodeDecode_3D_Boundary_Test(UInt32 x, UInt32 y, UInt32 z)
        {
            UInt32 encoded = MortonEncoding.Encode(x, y, z);
            MortonEncoding.Decode(encoded, out UInt32 decodedX, out UInt32 decodedY, out UInt32 decodedZ);
            // Max bits used by 3D encode is 10 bits per dimension
            Assert.Equal(x & 0x3FF, decodedX);
            Assert.Equal(y & 0x3FF, decodedY);
            Assert.Equal(z & 0x3FF, decodedZ);
        }
    }
}
