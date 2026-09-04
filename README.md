# Tedd.MortonEncoding

Allocation-free Morton encoding and decoding (Z-order curves) for .NET.

[NuGet](https://www.nuget.org/packages/Tedd.MortonEncoding/) · [Benchmark methodology](https://github.com/tedd/Tedd.MortonEncoding/blob/main/src/Tedd.MortonEncoding.Benchmarks/BENCHMARKS.md)

## Install

```shell
dotnet add package Tedd.MortonEncoding --version 1.1.0
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

uint code2D = MortonEncoding.Encode(x, y);
MortonEncoding.Decode(code2D, out uint decodedX, out uint decodedY);

uint x32 = 4_000_000_000;
uint y32 = 3_000_000_000;

ulong code2D64 = MortonEncoding.Encode64(x32, y32);
MortonEncoding.Decode64(code2D64, out uint decodedX32, out uint decodedY32);

uint x21 = 1_000_000;
uint y21 = 1_500_000;
uint z21 = 2_000_000;

ulong code3D64 = MortonEncoding.Encode64(x21, y21, z21);
MortonEncoding.Decode64(code3D64, out uint decodedX21, out uint decodedY21, out uint decodedZ21);
```

The equivalent 3D 32-bit overloads are `Encode(uint, uint, uint)` and `Decode(uint, out uint, out uint, out uint)`.

## Execution paths

On `netcoreapp3.0`, `net8.0`, and `net10.0`, the dispatcher uses x86/x64 BMI2 `PDEP` and `PEXT` instructions when the processor supports them. Other processors and the `net461` / `netstandard` assets use shift-and-mask software implementations.

`EncodeFallback`, `DecodeFallback`, `Encode64Fallback`, and `Decode64Fallback` expose the deterministic software path for environments that require it and for comparative benchmarks. All methods are thread-safe and allocate no managed memory.

## Contiguous-field helpers

`SplitXY` and `SplitXYZ` split a scalar containing adjacent, equally sized fields. They do not decode an interleaved Morton code.

```csharp
MortonEncoding.SplitXY(0b_1010_0011u, 4, out uint high, out uint low);
// high == 0b1010; low == 0b0011
```

Valid practical widths are 0-16 bits for `SplitXY` and 0-10 bits for `SplitXYZ`.

## Target frameworks

The package provides assets for .NET Framework 4.6.1, .NET Standard 1.0 and 2.0, .NET Core 3.0, .NET 8, and .NET 10. The lowest compatible assets retain broad support; the concrete .NET Core and modern .NET assets enable runtime intrinsics.

## Build, test, and benchmark

```shell
dotnet restore src/Tedd.MortonEncoding.sln
dotnet test src/Tedd.MortonEncoding.sln --configuration Release
dotnet run --project src/Tedd.MortonEncoding.Benchmarks -c Release -f net10.0 -- --filter '*'
```

The benchmark project retains the published 1.0.1 implementation as an archive and compares it with the current dispatch and software paths. See [BENCHMARKS.md](https://github.com/tedd/Tedd.MortonEncoding/blob/main/src/Tedd.MortonEncoding.Benchmarks/BENCHMARKS.md) for hypotheses, controls, and interpretation.

## Credits

The shift-and-mask constants derive from the bit-interleaving techniques described by Jeroen Baert in [Morton encoding/decoding through bit interleaving](https://www.forceflow.be/2013/10/07/morton-encodingdecoding-through-bit-interleaving-implementations/). Julien Bilalte identified the BMI2 `PDEP` / `PEXT` approach.

## License

[MIT](https://github.com/tedd/Tedd.MortonEncoding/blob/main/LICENSE)
