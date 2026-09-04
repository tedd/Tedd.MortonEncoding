using System;
using Tedd;
using Xunit;

namespace Tedd.MortonEncodingTests
{
    public sealed class MortonEncoding2DTests
    {
        [Fact]
        public void ThirtyTwoBit_ExhaustiveByteDomain_MatchesReferenceAndRoundTrips()
        {
            for (UInt32 x = 0; x <= Byte.MaxValue; x++)
            {
                for (UInt32 y = 0; y <= Byte.MaxValue; y++)
                {
                    var expected = ReferenceMorton.Encode2D32(x, y);
                    var encoded = MortonEncoding.Encode(x, y);
                    var fallbackEncoded = MortonEncoding.EncodeFallback(x, y);

                    Assert.Equal(expected, encoded);
                    Assert.Equal(expected, fallbackEncoded);

                    MortonEncoding.Decode(encoded, out var decodedX, out var decodedY);
                    MortonEncoding.DecodeFallback(fallbackEncoded, out var fallbackX, out var fallbackY);
                    Assert.Equal(x, decodedX);
                    Assert.Equal(y, decodedY);
                    Assert.Equal(x, fallbackX);
                    Assert.Equal(y, fallbackY);
                }
            }
        }

        [Fact]
        public void SixtyFourBit_ExhaustiveByteDomain_MatchesReferenceAndRoundTrips()
        {
            for (UInt32 x = 0; x <= Byte.MaxValue; x++)
            {
                for (UInt32 y = 0; y <= Byte.MaxValue; y++)
                {
                    var expected = ReferenceMorton.Encode2D64(x, y);
                    var encoded = MortonEncoding.Encode64(x, y);
                    var fallbackEncoded = MortonEncoding.Encode64Fallback(x, y);

                    Assert.Equal(expected, encoded);
                    Assert.Equal(expected, fallbackEncoded);

                    MortonEncoding.Decode64(encoded, out var decodedX, out var decodedY);
                    MortonEncoding.Decode64Fallback(fallbackEncoded, out var fallbackX, out var fallbackY);
                    Assert.Equal(x, decodedX);
                    Assert.Equal(y, decodedY);
                    Assert.Equal(x, fallbackX);
                    Assert.Equal(y, fallbackY);
                }
            }
        }

        [Fact]
        public void ThirtyTwoBit_EachCoordinateBitMapsToItsOwnLane()
        {
            for (var bit = 0; bit < 16; bit++)
            {
                var coordinate = 1u << bit;
                Assert.Equal(1u << (bit * 2), MortonEncoding.Encode(coordinate, 0));
                Assert.Equal(1u << ((bit * 2) + 1), MortonEncoding.Encode(0, coordinate));
                Assert.Equal(1u << (bit * 2), MortonEncoding.EncodeFallback(coordinate, 0));
                Assert.Equal(1u << ((bit * 2) + 1), MortonEncoding.EncodeFallback(0, coordinate));
            }
        }

        [Fact]
        public void SixtyFourBit_EachCoordinateBitMapsToItsOwnLane()
        {
            for (var bit = 0; bit < 32; bit++)
            {
                var coordinate = 1u << bit;
                Assert.Equal(1ul << (bit * 2), MortonEncoding.Encode64(coordinate, 0));
                Assert.Equal(1ul << ((bit * 2) + 1), MortonEncoding.Encode64(0, coordinate));
                Assert.Equal(1ul << (bit * 2), MortonEncoding.Encode64Fallback(coordinate, 0));
                Assert.Equal(1ul << ((bit * 2) + 1), MortonEncoding.Encode64Fallback(0, coordinate));
            }
        }

        [Fact]
        public void Decoders_MatchReferenceForArbitraryMortonWords()
        {
            UInt32 state = 0xBADC0FFEu;
            for (var iteration = 0; iteration < 4096; iteration++)
            {
                var morton32 = ReferenceMorton.NextUInt32(ref state);
                ReferenceMorton.Decode2D32(morton32, out var expectedX32, out var expectedY32);

                MortonEncoding.Decode(morton32, out var x32, out var y32);
                MortonEncoding.DecodeFallback(morton32, out var fallbackX32, out var fallbackY32);
                Assert.Equal(expectedX32, x32);
                Assert.Equal(expectedY32, y32);
                Assert.Equal(expectedX32, fallbackX32);
                Assert.Equal(expectedY32, fallbackY32);

                var morton64 = ReferenceMorton.NextUInt64(ref state);
                ReferenceMorton.Decode2D64(morton64, out var expectedX64, out var expectedY64);

                MortonEncoding.Decode64(morton64, out var x64, out var y64);
                MortonEncoding.Decode64Fallback(morton64, out var fallbackX64, out var fallbackY64);
                Assert.Equal(expectedX64, x64);
                Assert.Equal(expectedY64, y64);
                Assert.Equal(expectedX64, fallbackX64);
                Assert.Equal(expectedY64, fallbackY64);
            }
        }
    }
}
