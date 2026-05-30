# Tedd.MortonEncoding
Efficient z-order curve Morton encoder / decoder for .NET.

"**Z-order**, **Lebesgue curve**, **Morton space filling curve**, **Morton order** or **Morton code** map multidimensional data to one dimension while preserving locality of the data points." [Wikipedia article](https://en.wikipedia.org/wiki/Z-order_curve).

Available as NuGet package: https://www.nuget.org/packages/Tedd.MortonEncoding/

# Architectural Execution Flow
The `Tedd.MortonEncoding` library establishes a strict epistemological interface for multidimensional coordinate transposition. It deterministically interleaves spatial coordinates into a scalar index (encoding) and symmetrically reverses the operation (decoding).

## 1. Spatial Encoding & Decoding (2D / 3D)
The framework natively supports both 2-dimensional (x, y) and 3-dimensional (x, y, z) topologies.

```csharp
using Tedd;

// Define spatial coordinates (2D)
uint x = 0b00000000_00000000;
uint y = 0b00000000_11111111;

// Execute Encoding
uint encoded2D = MortonEncoding.Encode(x, y);

// Execute Decoding
MortonEncoding.Decode(encoded2D, out uint xBack, out uint yBack);

// Define spatial coordinates (3D)
uint x3 = 10;
uint y3 = 20;
uint z3 = 30;

// Execute 3D Encoding
uint encoded3D = MortonEncoding.Encode(x3, y3, z3);

// Execute 3D Decoding
MortonEncoding.Decode(encoded3D, out uint rx, out uint ry, out uint rz);
```

## 2. Component Isolation
The framework exposes utility functions to isolate coordinate sequences utilizing arbitrary bit depths.

```csharp
using Tedd;

uint composite = 0b11111111;
int depth = 4;

// Isolate 2D components based on specified bit depth
MortonEncoding.SplitXY(composite, depth, out uint splitX, out uint splitY);

// Isolate 3D components based on specified bit depth
MortonEncoding.SplitXYZ(composite, depth, out uint sx, out uint sy, out uint sz);
```

# Hardware Acceleration Paradigm
The encoding/decoding operational mechanisms automatically invoke hardware-accelerated processing vectors within modern framework iterations (.NET 5.0 through .NET 10.0+).

## Intrinsic Execution (Modern Environments)
For processors supporting the [BMI2](https://en.wikipedia.org/wiki/Bit_Manipulation_Instruction_Sets#BMI2_%28Bit_Manipulation_Instruction_Set_2%29) instruction set, the library directly executes native CPU operations (`System.Runtime.Intrinsics.X86.Bmi2.ParallelBitDeposit` and `ParallelBitExtract`). This structural execution ensures maximum reciprocal throughput capability.

## Software Fallback (Legacy Environments)
When executing within environments preceding intrinsic integration, or on architectures devoid of BMI2 capability, the system relies on an optimized software sequence consisting of discrete AND, XOR, and SHIFT bitwise operations. This calculated path ensures functional parity while circumventing lookup-table architectures to optimize L1 cache availability.

# Credits
Thanks to Jeroen Baert's [blog entry](https://www.forceflow.be/2013/10/07/morton-encodingdecoding-through-bit-interleaving-implementations/), as well as Julien Bilalte's comment on using BMI2-instructions. The magic numbers and LUT-table helped a lot. LUT-table is used in xUnit tests to verify both BMI2 and manual calculations.
