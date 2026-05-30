## 2024-05-25 - Hardware Intrinsic Optimizations in Morton Encoding
**Observation:** The `Tedd.MortonEncoding` library uses software fallbacks (`AND`, `XOR`, `SHIFT`) to perform 2D and 3D Morton encoding/decoding when `System.Runtime.Intrinsics.X86.Bmi2` is not supported. The `Encode` and `Decode` logic utilizes `Bmi2.ParallelBitDeposit` and `Bmi2.ParallelBitExtract` when `Bmi2` is available.
The BMI2 intrinsic methods, when measured using BenchmarkDotNet on RyuJIT x86-64-v3 with .NET 10.0, show extreme micro-optimization levels relative to software bitwise implementation (nearly ~0.0 ns and indistinguishable from empty methods due to method inlining and direct CPU instruction mapping).
**Strategic Action:** Continue evaluating other intrinsic acceleration opportunities.

## 2024-05-25 - Redundant Shift and OR Elimination in Software Fallback
**Observation:** In the `Encode(UInt32 x, UInt32 y)` software fallback, operations like `x = (x | x << 16) & 0x0000FFFF` shift bits into the upper 16 positions, but then immediately apply a mask (`0x0000FFFF`) that clears them, making the shift and OR entirely redundant. Removing these operations reduces latency from 3.153 ns to 2.633 ns per benchmark.
**Strategic Action:** Always inspect bitwise operations before applying masks to detect and eliminate dead operations (O(1) Time, O(1) Space).
