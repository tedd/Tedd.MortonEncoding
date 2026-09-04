using System;
using System.Runtime.CompilerServices;
#if NETCOREAPP3_0_OR_GREATER
using X86 = System.Runtime.Intrinsics.X86;
#endif

namespace Tedd
{
    /// <summary>
    /// Encodes and decodes two- and three-dimensional unsigned Morton codes.
    /// </summary>
    public static class MortonEncoding
    {
        private const UInt32 EvenBits32 = 0x55555555u;
        private const UInt32 EveryThirdBit32 = 0x09249249u;

        private const UInt64 EvenBits64 = 0x5555555555555555ul;
        private const UInt64 EveryThirdBit64 = 0x1249249249249249ul;

        /// <summary>
        /// Splits two contiguous, equally sized fields from <paramref name="i"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SplitXY(UInt32 i, int bits, out UInt32 x, out UInt32 y)
        {
            UInt32 mask = ((UInt32)1 << bits) - 1;
            x = (i >> bits) & mask;
            y = i & mask;
        }

        /// <summary>
        /// Splits three contiguous, equally sized fields from <paramref name="i"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SplitXYZ(UInt32 i, int bits, out UInt32 x, out UInt32 y, out UInt32 z)
        {
            UInt32 mask = ((UInt32)1 << bits) - 1;
            x = (i >> (bits + bits)) & mask;
            y = (i >> bits) & mask;
            z = i & mask;
        }

        /// <summary>
        /// Encodes the low 16 bits of two coordinates into a 32-bit Morton code.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt32 Encode(UInt32 x, UInt32 y)
        {
#if NETCOREAPP3_0_OR_GREATER
            if (X86.Bmi2.IsSupported)
            {
                return X86.Bmi2.ParallelBitDeposit(x, EvenBits32)
                     | X86.Bmi2.ParallelBitDeposit(y, EvenBits32 << 1);
            }
#endif
            return EncodeFallback(x, y);
        }

        /// <summary>
        /// Encodes the low 16 bits of two coordinates without hardware intrinsics.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt32 EncodeFallback(UInt32 x, UInt32 y)
        {
            return Spread2(x) | (Spread2(y) << 1);
        }

        /// <summary>
        /// Decodes a 32-bit, two-dimensional Morton code.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Decode(UInt32 morton, out UInt32 x, out UInt32 y)
        {
#if NETCOREAPP3_0_OR_GREATER
            if (X86.Bmi2.IsSupported)
            {
                x = X86.Bmi2.ParallelBitExtract(morton, EvenBits32);
                y = X86.Bmi2.ParallelBitExtract(morton, EvenBits32 << 1);
                return;
            }
#endif
            DecodeFallback(morton, out x, out y);
        }

        /// <summary>
        /// Decodes a 32-bit, two-dimensional Morton code without hardware intrinsics.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DecodeFallback(UInt32 morton, out UInt32 x, out UInt32 y)
        {
            x = Compact2(morton);
            y = Compact2(morton >> 1);
        }

        /// <summary>
        /// Encodes the low 10 bits of three coordinates into a 32-bit Morton code.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt32 Encode(UInt32 x, UInt32 y, UInt32 z)
        {
#if NETCOREAPP3_0_OR_GREATER
            if (X86.Bmi2.IsSupported)
            {
                return X86.Bmi2.ParallelBitDeposit(x, EveryThirdBit32)
                     | X86.Bmi2.ParallelBitDeposit(y, EveryThirdBit32 << 1)
                     | X86.Bmi2.ParallelBitDeposit(z, EveryThirdBit32 << 2);
            }
#endif
            return EncodeFallback(x, y, z);
        }

        /// <summary>
        /// Encodes the low 10 bits of three coordinates without hardware intrinsics.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt32 EncodeFallback(UInt32 x, UInt32 y, UInt32 z)
        {
            return Spread3(x) | (Spread3(y) << 1) | (Spread3(z) << 2);
        }

        /// <summary>
        /// Decodes a 32-bit, three-dimensional Morton code.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Decode(UInt32 morton, out UInt32 x, out UInt32 y, out UInt32 z)
        {
#if NETCOREAPP3_0_OR_GREATER
            if (X86.Bmi2.IsSupported)
            {
                x = X86.Bmi2.ParallelBitExtract(morton, EveryThirdBit32);
                y = X86.Bmi2.ParallelBitExtract(morton, EveryThirdBit32 << 1);
                z = X86.Bmi2.ParallelBitExtract(morton, EveryThirdBit32 << 2);
                return;
            }
#endif
            DecodeFallback(morton, out x, out y, out z);
        }

        /// <summary>
        /// Decodes a 32-bit, three-dimensional Morton code without hardware intrinsics.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DecodeFallback(UInt32 morton, out UInt32 x, out UInt32 y, out UInt32 z)
        {
            x = Compact3(morton);
            y = Compact3(morton >> 1);
            z = Compact3(morton >> 2);
        }

        /// <summary>
        /// Encodes two full 32-bit coordinates into a 64-bit Morton code.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt64 Encode64(UInt32 x, UInt32 y)
        {
#if NETCOREAPP3_0_OR_GREATER
            if (X86.Bmi2.X64.IsSupported)
            {
                return X86.Bmi2.X64.ParallelBitDeposit(x, EvenBits64)
                     | X86.Bmi2.X64.ParallelBitDeposit(y, EvenBits64 << 1);
            }
#endif
            return Encode64Fallback(x, y);
        }

        /// <summary>
        /// Encodes two full 32-bit coordinates without hardware intrinsics.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt64 Encode64Fallback(UInt32 x, UInt32 y)
        {
            return Spread2((UInt64)x) | (Spread2((UInt64)y) << 1);
        }

        /// <summary>
        /// Decodes a 64-bit, two-dimensional Morton code.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Decode64(UInt64 morton, out UInt32 x, out UInt32 y)
        {
#if NETCOREAPP3_0_OR_GREATER
            if (X86.Bmi2.X64.IsSupported)
            {
                x = (UInt32)X86.Bmi2.X64.ParallelBitExtract(morton, EvenBits64);
                y = (UInt32)X86.Bmi2.X64.ParallelBitExtract(morton, EvenBits64 << 1);
                return;
            }
#endif
            Decode64Fallback(morton, out x, out y);
        }

        /// <summary>
        /// Decodes a 64-bit, two-dimensional Morton code without hardware intrinsics.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Decode64Fallback(UInt64 morton, out UInt32 x, out UInt32 y)
        {
            x = (UInt32)Compact2(morton);
            y = (UInt32)Compact2(morton >> 1);
        }

        /// <summary>
        /// Encodes the low 21 bits of three coordinates into a 64-bit Morton code.
        /// Bit 63 is unused.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt64 Encode64(UInt32 x, UInt32 y, UInt32 z)
        {
#if NETCOREAPP3_0_OR_GREATER
            if (X86.Bmi2.X64.IsSupported)
            {
                return X86.Bmi2.X64.ParallelBitDeposit(x, EveryThirdBit64)
                     | X86.Bmi2.X64.ParallelBitDeposit(y, EveryThirdBit64 << 1)
                     | X86.Bmi2.X64.ParallelBitDeposit(z, EveryThirdBit64 << 2);
            }
#endif
            return Encode64Fallback(x, y, z);
        }

        /// <summary>
        /// Encodes the low 21 bits of three coordinates without hardware intrinsics.
        /// Bit 63 is unused.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt64 Encode64Fallback(UInt32 x, UInt32 y, UInt32 z)
        {
            return Spread3((UInt64)x) | (Spread3((UInt64)y) << 1) | (Spread3((UInt64)z) << 2);
        }

        /// <summary>
        /// Decodes a 64-bit, three-dimensional Morton code.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Decode64(UInt64 morton, out UInt32 x, out UInt32 y, out UInt32 z)
        {
#if NETCOREAPP3_0_OR_GREATER
            if (X86.Bmi2.X64.IsSupported)
            {
                x = (UInt32)X86.Bmi2.X64.ParallelBitExtract(morton, EveryThirdBit64);
                y = (UInt32)X86.Bmi2.X64.ParallelBitExtract(morton, EveryThirdBit64 << 1);
                z = (UInt32)X86.Bmi2.X64.ParallelBitExtract(morton, EveryThirdBit64 << 2);
                return;
            }
#endif
            Decode64Fallback(morton, out x, out y, out z);
        }

        /// <summary>
        /// Decodes a 64-bit, three-dimensional Morton code without hardware intrinsics.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Decode64Fallback(UInt64 morton, out UInt32 x, out UInt32 y, out UInt32 z)
        {
            x = (UInt32)Compact3(morton);
            y = (UInt32)Compact3(morton >> 1);
            z = (UInt32)Compact3(morton >> 2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UInt32 Spread2(UInt32 value)
        {
            value &= 0x0000FFFFu;
            value = (value | (value << 8)) & 0x00FF00FFu;
            value = (value | (value << 4)) & 0x0F0F0F0Fu;
            value = (value | (value << 2)) & 0x33333333u;
            return (value | (value << 1)) & EvenBits32;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UInt32 Compact2(UInt32 value)
        {
            value &= EvenBits32;
            value = (value ^ (value >> 1)) & 0x33333333u;
            value = (value ^ (value >> 2)) & 0x0F0F0F0Fu;
            value = (value ^ (value >> 4)) & 0x00FF00FFu;
            return (value ^ (value >> 8)) & 0x0000FFFFu;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UInt32 Spread3(UInt32 value)
        {
            value &= 0x000003FFu;
            value = (value | (value << 16)) & 0x030000FFu;
            value = (value | (value << 8)) & 0x0300F00Fu;
            value = (value | (value << 4)) & 0x030C30C3u;
            return (value | (value << 2)) & EveryThirdBit32;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UInt32 Compact3(UInt32 value)
        {
            value &= EveryThirdBit32;
            value = (value ^ (value >> 2)) & 0x030C30C3u;
            value = (value ^ (value >> 4)) & 0x0300F00Fu;
            value = (value ^ (value >> 8)) & 0x030000FFu;
            return (value ^ (value >> 16)) & 0x000003FFu;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UInt64 Spread2(UInt64 value)
        {
            value &= 0x00000000FFFFFFFFul;
            value = (value | (value << 16)) & 0x0000FFFF0000FFFFul;
            value = (value | (value << 8)) & 0x00FF00FF00FF00FFul;
            value = (value | (value << 4)) & 0x0F0F0F0F0F0F0F0Ful;
            value = (value | (value << 2)) & 0x3333333333333333ul;
            return (value | (value << 1)) & EvenBits64;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UInt64 Compact2(UInt64 value)
        {
            value &= EvenBits64;
            value = (value ^ (value >> 1)) & 0x3333333333333333ul;
            value = (value ^ (value >> 2)) & 0x0F0F0F0F0F0F0F0Ful;
            value = (value ^ (value >> 4)) & 0x00FF00FF00FF00FFul;
            value = (value ^ (value >> 8)) & 0x0000FFFF0000FFFFul;
            return (value ^ (value >> 16)) & 0x00000000FFFFFFFFul;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UInt64 Spread3(UInt64 value)
        {
            value &= 0x00000000001FFFFFul;
            value = (value | (value << 32)) & 0x001F00000000FFFFul;
            value = (value | (value << 16)) & 0x001F0000FF0000FFul;
            value = (value | (value << 8)) & 0x100F00F00F00F00Ful;
            value = (value | (value << 4)) & 0x10C30C30C30C30C3ul;
            return (value | (value << 2)) & EveryThirdBit64;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UInt64 Compact3(UInt64 value)
        {
            value &= EveryThirdBit64;
            value = (value ^ (value >> 2)) & 0x10C30C30C30C30C3ul;
            value = (value ^ (value >> 4)) & 0x100F00F00F00F00Ful;
            value = (value ^ (value >> 8)) & 0x001F0000FF0000FFul;
            value = (value ^ (value >> 16)) & 0x001F00000000FFFFul;
            return (value ^ (value >> 32)) & 0x00000000001FFFFFul;
        }
    }
}
