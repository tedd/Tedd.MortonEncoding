# Tedd.MortonEncoding

Allocation-free Morton encoding and decoding (Z-order curves) for .NET.

[Website](https://tedd.no/Tedd.MortonEncoding/) · [NuGet](https://www.nuget.org/packages/Tedd.MortonEncoding/) · [Benchmark methodology](https://github.com/tedd/Tedd.MortonEncoding/blob/main/src/Tedd.MortonEncoding.Benchmarks/BENCHMARKS.md)

## Install

```shell
dotnet add package Tedd.MortonEncoding
```

## Capacity

| API | Dimensions | Coordinate capacity | Morton code |
| --- | ---: | ---: | ---: |
| `Encode` / `Decode` | 2D | 16 bits per axis | `uint` |
| `Encode` / `Decode` | 3D | 10 bits per axis | `uint` |
| `Encode64` / `Decode64` | 2D | 32 bits per axis | `ulong` |
| `Encode64` / `Decode64` | 3D | 21 bits per axis | `ulong` (bit 63 unused) |

Encoding places X in bit positions 0, 2, 4, ... for 2D, or 0, 3, 6, ... for 3D. Y and Z occupy the subsequent lanes.

The hot-path encoders deliberately mask unsupported high coordinate bits rather than validating or throwing. For example, 32-bit 3D encoding uses `x & 0x3FF`; 64-bit 3D encoding uses `x & 0x1FFFFF`.

## Usage

```csharp
using Tedd;

uint x = 10;
uint y = 20;

// 2D 32-bit Encode/Decode
uint code2D = MortonEncoding.Encode(x, y);
MortonEncoding.Decode(code2D, out var decodedX, out var decodedY);

// 2D 64-bit Encode/Decode
ulong code2D64 = MortonEncoding.Encode64(4_000_000_000, 3_000_000_000);
MortonEncoding.Decode64(code2D64, out var decodedX32, out var decodedY32);

// 3D 32-bit Encode/Decode
uint code3D = MortonEncoding.Encode(100, 200, 300);
MortonEncoding.Decode(code3D, out var decodedX10, out var decodedY10, out var decodedZ10);

// 3D 64-bit Encode/Decode
ulong code3D64 = MortonEncoding.Encode64(1_000_000, 1_500_000, 2_000_000);
MortonEncoding.Decode64(code3D64, out var decodedX21, out var decodedY21, out var decodedZ21);
```

## Architectural Execution Flow

The framework employs a dynamic dispatch mechanism, segregating hardware-accelerated processing from deterministic software fallbacks. This architecture optimally leverages modern processor capabilities while guaranteeing ubiquitous compatibility.

**Established Capabilities:**
On modern frameworks (`net8.0` and `net10.0`), the system utilizes runtime intrinsics (`System.Runtime.Intrinsics.X86.Bmi2`). Specifically, it relies on the `PDEP` (Parallel Bit Deposit) and `PEXT` (Parallel Bit Extract) instructions, if the underlying processor architecture supports them.

For platforms devoid of `BMI2` support, and for legacy runtime environments (`net461`, `netstandard`), the system defaults to a robust, highly optimized shift-and-mask software fallback mechanism.

`EncodeFallback`, `DecodeFallback`, `Encode64Fallback`, and `Decode64Fallback` explicitly bypass the dispatch logic, forcing execution of the shift-and-mask logic. These pathways are primarily utilized for comparative benchmarking and for environments mandating deterministic algorithmic execution irrespective of processor architecture.

**Future Hypotheses:**
Future architectural enhancements may involve extended vectorization utilizing advanced SIMD instruction sets (e.g., AVX-512) for bulk encoding/decoding operations, though this remains an unverified hypothesis currently under evaluation.

## Contiguous-field helpers

`SplitXY` and `SplitXYZ` delineate a scalar integer into adjacent, equally sized fields. Note: These methods do not perform Morton decoding; they execute a straightforward structural decomposition of contiguous bits.

```csharp
// Delineate into two 4-bit fields
MortonEncoding.SplitXY(0b_1010_0011u, 4, out var highXY, out var lowXY);
// highXY == 0b1010; lowXY == 0b0011

// Delineate into three 2-bit fields
MortonEncoding.SplitXYZ(0b_11_10_01u, 2, out var highXYZ, out var midXYZ, out var lowXYZ);
// highXYZ == 0b11; midXYZ == 0b10; lowXYZ == 0b01
```

Valid operational widths range from 0-16 bits for `SplitXY` and 0-10 bits for `SplitXYZ`.

## Target frameworks

The package provides a comprehensive compatibility matrix encompassing .NET Framework 4.6.1, .NET Standard 1.0 and 2.0, .NET Core 3.0, .NET 8.0, and .NET 10.0. Assets targeting contemporary runtimes activate the hardware acceleration pathways, whereas legacy assets ensure maximum environmental interoperability.

## Build, test, and benchmark

```shell
dotnet restore src/Tedd.MortonEncoding.sln
dotnet test src/Tedd.MortonEncoding.sln --configuration Release
dotnet run --project src/Tedd.MortonEncoding.Benchmarks -c Release -f net10.0 -- --filter '*'
```

The benchmark infrastructure retains the original 1.0.1 implementation as a baseline for comparative analysis against the current dispatch logic and the software fallback paths. Refer to [BENCHMARKS.md](https://github.com/tedd/Tedd.MortonEncoding/blob/main/src/Tedd.MortonEncoding.Benchmarks/BENCHMARKS.md) for empirical methodologies and performance insights.

## Credits

The shift-and-mask algorithmic constants originate from the bit-interleaving paradigms detailed by Jeroen Baert in [Morton encoding/decoding through bit interleaving](https://www.forceflow.be/2013/10/07/morton-encodingdecoding-through-bit-interleaving-implementations/). The `BMI2` `PDEP` / `PEXT` implementation strategy was formulated by Julien Bilalte.

## License

[GNU Lesser General Public License 2.1](https://github.com/tedd/Tedd.MortonEncoding/blob/main/LICENSE)