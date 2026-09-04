using System;
using Tedd;
using Xunit;

namespace Tedd.MortonEncodingTests
{
    public sealed class MortonEncoding3DTests
    {
        [Fact]
        public void ThirtyTwoBit_ExhaustiveFiveBitDomain_MatchesReferenceAndRoundTrips()
        {
            for (UInt32 x = 0; x < 32; x++)
            {
                for (UInt32 y = 0; y < 32; y++)
                {
                    for (UInt32 z = 0; z < 32; z++)
                    {
                        var expected = ReferenceMorton.Encode3D32(x, y, z);
                        var encoded = MortonEncoding.Encode(x, y, z);
                        var fallbackEncoded = MortonEncoding.EncodeFallback(x, y, z);

                        Assert.Equal(expected, encoded);
                        Assert.Equal(expected, fallbackEncoded);

                        MortonEncoding.Decode(encoded, out var decodedX, out var decodedY, out var decodedZ);
                        MortonEncoding.DecodeFallback(
                            fallbackEncoded,
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
                }
            }
        }

        [Fact]
        public void SixtyFourBit_ExhaustiveFiveBitDomain_MatchesReferenceAndRoundTrips()
        {
            for (UInt32 x = 0; x < 32; x++)
            {
                for (UInt32 y = 0; y < 32; y++)
                {
                    for (UInt32 z = 0; z < 32; z++)
                    {
                        var expected = ReferenceMorton.Encode3D64(x, y, z);
                        var encoded = MortonEncoding.Encode64(x, y, z);
                        var fallbackEncoded = MortonEncoding.Encode64Fallback(x, y, z);

                        Assert.Equal(expected, encoded);
                        Assert.Equal(expected, fallbackEncoded);

                        MortonEncoding.Decode64(encoded, out var decodedX, out var decodedY, out var decodedZ);
                        MortonEncoding.Decode64Fallback(
                            fallbackEncoded,
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
                }
            }
        }

        [Fact]
        public void ThirtyTwoBit_EachCoordinateBitMapsToItsOwnLane()
        {
            for (var bit = 0; bit < 10; bit++)
            {
                var coordinate = 1u << bit;
                Assert.Equal(1u << (bit * 3), MortonEncoding.Encode(coordinate, 0, 0));
                Assert.Equal(1u << ((bit * 3) + 1), MortonEncoding.Encode(0, coordinate, 0));
                Assert.Equal(1u << ((bit * 3) + 2), MortonEncoding.Encode(0, 0, coordinate));
                Assert.Equal(1u << (bit * 3), MortonEncoding.EncodeFallback(coordinate, 0, 0));
                Assert.Equal(1u << ((bit * 3) + 1), MortonEncoding.EncodeFallback(0, coordinate, 0));
                Assert.Equal(1u << ((bit * 3) + 2), MortonEncoding.EncodeFallback(0, 0, coordinate));
            }
        }

        [Fact]
        public void SixtyFourBit_EachCoordinateBitMapsToItsOwnLane()
        {
            for (var bit = 0; bit < 21; bit++)
            {
                var coordinate = 1u << bit;
                Assert.Equal(1ul << (bit * 3), MortonEncoding.Encode64(coordinate, 0, 0));
                Assert.Equal(1ul << ((bit * 3) + 1), MortonEncoding.Encode64(0, coordinate, 0));
                Assert.Equal(1ul << ((bit * 3) + 2), MortonEncoding.Encode64(0, 0, coordinate));
                Assert.Equal(1ul << (bit * 3), MortonEncoding.Encode64Fallback(coordinate, 0, 0));
                Assert.Equal(1ul << ((bit * 3) + 1), MortonEncoding.Encode64Fallback(0, coordinate, 0));
                Assert.Equal(1ul << ((bit * 3) + 2), MortonEncoding.Encode64Fallback(0, 0, coordinate));
            }
        }

        [Fact]
        public void Decoders_MatchReferenceForArbitraryMortonWords()
        {
            UInt32 state = 0x13579BDFu;
            for (var iteration = 0; iteration < 4096; iteration++)
            {
                var morton32 = ReferenceMorton.NextUInt32(ref state);
                ReferenceMorton.Decode3D32(
                    morton32,
                    out var expectedX32,
                    out var expectedY32,
                    out var expectedZ32);

                MortonEncoding.Decode(morton32, out var x32, out var y32, out var z32);
                MortonEncoding.DecodeFallback(
                    morton32,
                    out var fallbackX32,
                    out var fallbackY32,
                    out var fallbackZ32);
                Assert.Equal(expectedX32, x32);
                Assert.Equal(expectedY32, y32);
                Assert.Equal(expectedZ32, z32);
                Assert.Equal(expectedX32, fallbackX32);
                Assert.Equal(expectedY32, fallbackY32);
                Assert.Equal(expectedZ32, fallbackZ32);

                var morton64 = ReferenceMorton.NextUInt64(ref state);
                ReferenceMorton.Decode3D64(
                    morton64,
                    out var expectedX64,
                    out var expectedY64,
                    out var expectedZ64);

                MortonEncoding.Decode64(morton64, out var x64, out var y64, out var z64);
                MortonEncoding.Decode64Fallback(
                    morton64,
                    out var fallbackX64,
                    out var fallbackY64,
                    out var fallbackZ64);
                Assert.Equal(expectedX64, x64);
                Assert.Equal(expectedY64, y64);
                Assert.Equal(expectedZ64, z64);
                Assert.Equal(expectedX64, fallbackX64);
                Assert.Equal(expectedY64, fallbackY64);
                Assert.Equal(expectedZ64, fallbackZ64);
            }
        }
    }
}
