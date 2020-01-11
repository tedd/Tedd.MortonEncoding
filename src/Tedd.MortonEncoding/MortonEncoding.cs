using System;
using System.Runtime.CompilerServices;
#if INTRINSIC
using System.Runtime.InteropServices;
using X86 = System.Runtime.Intrinsics.X86;
#endif

namespace Tedd
{
    public static class MortonEncoding
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SplitXY(UInt32 i, int bits, out UInt32 x, out UInt32 y)
        {
            var mask = ((UInt32)1 << bits) - 1;
            x = (i >> bits) & mask;
            y = i & mask;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SplitXYZ(UInt32 i, int bits, out UInt32 x, out UInt32 y, out UInt32 z)
        {
            var mask = ((UInt32)1 << bits) - 1;
            x = (i >> (bits + bits)) & mask;
            y = (i >> bits) & mask;
            z = i & mask;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt32 Encode(UInt32 x, UInt32 y)
        {
#if INTRINSIC
            if (X86.Bmi2.IsSupported)
                return X86.Bmi2.ParallelBitDeposit(y, 0xAAAAAAAA)
                     | X86.Bmi2.ParallelBitDeposit(x, 0x55555555);
            else
#endif
            {
                //x =  0x0000FFFF;
                x = (x | x << 16) & 0x0000FFFF; // Should only shift one, right? But then we need twice as many operators?
                x = (x | x << 8) & 0x00FF00FF;
                x = (x | x << 4) & 0x0F0F0F0F;
                x = (x | x << 2) & 0x33333333;
                x = (x | x << 1) & 0x55555555;

                y = (y | y << 16) & 0x0000FFFF;
                y = (y | y << 8) & 0x00FF00FF;
                y = (y | y << 4) & 0x0F0F0F0F;
                y = (y | y << 2) & 0x33333333;
                y = (y | y << 1) & 0x55555555;


                return x | (y << 1);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Decode(UInt32 morton, out UInt32 x, out UInt32 y)
        {
#if INTRINSIC
            if (X86.Bmi2.IsSupported)
            {
                x = X86.Bmi2.ParallelBitExtract(morton, 0x55555555);
                y = X86.Bmi2.ParallelBitExtract(morton, 0xAAAAAAAA);
            }
            else
#endif
            {
                x = morton & 0x55555555;
                x = (x ^ (x >> 1)) & 0x33333333;
                x = (x ^ (x >> 2)) & 0x0F0F0F0F;
                x = (x ^ (x >> 4)) & 0x00FF00FF;
                x = (x ^ (x >> 8)) & 0x0000FFFF;

                y = (morton >> 1) & 0x55555555;
                y = (y ^ (y >> 1)) & 0x33333333;
                y = (y ^ (y >> 2)) & 0x0F0F0F0F;
                y = (y ^ (y >> 4)) & 0x00FF00FF;
                y = (y ^ (y >> 8)) & 0x0000FFFF;

            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt32 Encode(UInt32 x, UInt32 y, UInt32 z)
        {
#if INTRINSIC
            if (X86.Bmi2.IsSupported)
                return X86.Bmi2.ParallelBitDeposit(z, 0x24924924)
                     | X86.Bmi2.ParallelBitDeposit(y, 0x12492492)
                     | X86.Bmi2.ParallelBitDeposit(x, 0x09249249);
            else
#endif
            {
                x = (x | (x << 16)) & 0x030000FF;
                x = (x | (x << 8)) & 0x0300F00F;
                x = (x | (x << 4)) & 0x030C30C3;
                x = (x | (x << 2)) & 0x09249249;

                y = (y | (y << 16)) & 0x030000FF;
                y = (y | (y << 8)) & 0x0300F00F;
                y = (y | (y << 4)) & 0x030C30C3;
                y = (y | (y << 2)) & 0x09249249;

                z = (z | (z << 16)) & 0x030000FF;
                z = (z | (z << 8)) & 0x0300F00F;
                z = (z | (z << 4)) & 0x030C30C3;
                z = (z | (z << 2)) & 0x09249249;

                return x | (y << 1) | (z << 2);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Decode(UInt32 morton, out UInt32 x, out UInt32 y, out UInt32 z)
        {
#if INTRINSIC
            if (X86.Bmi2.IsSupported)
            {
                x = X86.Bmi2.ParallelBitExtract(morton, 0x09249249);
                y = X86.Bmi2.ParallelBitExtract(morton, 0x12492492);
                z = X86.Bmi2.ParallelBitExtract(morton, 0x24924924);
            }
            else
#endif
            {
                x = morton & 0x9249249;
                x = (x ^ (x >> 2)) & 0x30c30c3;
                x = (x ^ (x >> 4)) & 0x0300f00f;
                x = (x ^ (x >> 8)) & 0x30000ff;
                x = (x ^ (x >> 16)) & 0x000003ff;

                y = (morton >> 1) & 0x9249249;
                y = (y ^ (y >> 2)) & 0x30c30c3;
                y = (y ^ (y >> 4)) & 0x0300f00f;
                y = (y ^ (y >> 8)) & 0x30000ff;
                y = (y ^ (y >> 16)) & 0x000003ff;

                z = (morton >> 2) & 0x9249249;
                z = (z ^ (z >> 2)) & 0x30c30c3;
                z = (z ^ (z >> 4)) & 0x0300f00f;
                z = (z ^ (z >> 8)) & 0x30000ff;
                z = (z ^ (z >> 16)) & 0x000003ff;

            }
        }

    }
}
