using System;

namespace Tedd.MortonEncodingTests
{
    /// <summary>
    /// Deliberately simple test oracle. These loop-based implementations share no
    /// bit-spreading constants or algorithms with the production implementation.
    /// </summary>
    internal static class ReferenceMorton
    {
        internal const UInt32 CoordinateMask2D32 = 0x0000FFFFu;
        internal const UInt32 CoordinateMask3D32 = 0x000003FFu;
        internal const UInt32 CoordinateMask3D64 = 0x001FFFFFu;

        internal static UInt32 Encode2D32(UInt32 x, UInt32 y)
        {
            UInt32 morton = 0;
            for (var bit = 0; bit < 16; bit++)
            {
                morton |= ((x >> bit) & 1u) << (bit * 2);
                morton |= ((y >> bit) & 1u) << ((bit * 2) + 1);
            }

            return morton;
        }

        internal static UInt32 Encode3D32(UInt32 x, UInt32 y, UInt32 z)
        {
            UInt32 morton = 0;
            for (var bit = 0; bit < 10; bit++)
            {
                morton |= ((x >> bit) & 1u) << (bit * 3);
                morton |= ((y >> bit) & 1u) << ((bit * 3) + 1);
                morton |= ((z >> bit) & 1u) << ((bit * 3) + 2);
            }

            return morton;
        }

        internal static UInt64 Encode2D64(UInt32 x, UInt32 y)
        {
            UInt64 morton = 0;
            for (var bit = 0; bit < 32; bit++)
            {
                morton |= ((UInt64)((x >> bit) & 1u)) << (bit * 2);
                morton |= ((UInt64)((y >> bit) & 1u)) << ((bit * 2) + 1);
            }

            return morton;
        }

        internal static UInt64 Encode3D64(UInt32 x, UInt32 y, UInt32 z)
        {
            UInt64 morton = 0;
            for (var bit = 0; bit < 21; bit++)
            {
                morton |= ((UInt64)((x >> bit) & 1u)) << (bit * 3);
                morton |= ((UInt64)((y >> bit) & 1u)) << ((bit * 3) + 1);
                morton |= ((UInt64)((z >> bit) & 1u)) << ((bit * 3) + 2);
            }

            return morton;
        }

        internal static void Decode2D32(UInt32 morton, out UInt32 x, out UInt32 y)
        {
            x = 0;
            y = 0;
            for (var bit = 0; bit < 16; bit++)
            {
                x |= ((morton >> (bit * 2)) & 1u) << bit;
                y |= ((morton >> ((bit * 2) + 1)) & 1u) << bit;
            }
        }

        internal static void Decode3D32(UInt32 morton, out UInt32 x, out UInt32 y, out UInt32 z)
        {
            x = 0;
            y = 0;
            z = 0;
            for (var bit = 0; bit < 10; bit++)
            {
                x |= ((morton >> (bit * 3)) & 1u) << bit;
                y |= ((morton >> ((bit * 3) + 1)) & 1u) << bit;
                z |= ((morton >> ((bit * 3) + 2)) & 1u) << bit;
            }
        }

        internal static void Decode2D64(UInt64 morton, out UInt32 x, out UInt32 y)
        {
            x = 0;
            y = 0;
            for (var bit = 0; bit < 32; bit++)
            {
                x |= (UInt32)((morton >> (bit * 2)) & 1ul) << bit;
                y |= (UInt32)((morton >> ((bit * 2) + 1)) & 1ul) << bit;
            }
        }

        internal static void Decode3D64(UInt64 morton, out UInt32 x, out UInt32 y, out UInt32 z)
        {
            x = 0;
            y = 0;
            z = 0;
            for (var bit = 0; bit < 21; bit++)
            {
                x |= (UInt32)((morton >> (bit * 3)) & 1ul) << bit;
                y |= (UInt32)((morton >> ((bit * 3) + 1)) & 1ul) << bit;
                z |= (UInt32)((morton >> ((bit * 3) + 2)) & 1ul) << bit;
            }
        }

        internal static UInt32 NextUInt32(ref UInt32 state)
        {
            // xorshift32: deterministic on every target framework and runtime.
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            return state;
        }

        internal static UInt64 NextUInt64(ref UInt32 state)
        {
            var low = NextUInt32(ref state);
            var high = NextUInt32(ref state);
            return low | ((UInt64)high << 32);
        }
    }
}
