## 2025-02-18 - Architectural Documentation Synchronization

**Observation:** The README.md file contains legacy .NET framework references (e.g., .Net Core 3, .Net 4, .Net core 1/2) and does not reflect current .NET 9.0/10.0+ paradigms. The examples are correct but somewhat simplistic. Furthermore, the library now includes `SplitXY` and `SplitXYZ` functions, and a 3D encode/decode (`Encode(x,y,z)`, `Decode(morton, out x, out y, out z)`) functionality, which are completely undocumented in the README. The hardware acceleration explanation also lacks mention of modern .NET Intrinsics (`X86.Bmi2`) and assumes older framework contexts.

**Strategic Action:** Update README.md to articulate current structural state, including 3D encoding/decoding, the utility of `SplitXY`/`SplitXYZ`, and update the hardware acceleration details to acknowledge modern .NET intrinsic support. Replace legacy testing syntax (Assert.Equal) with a robust structure suitable for demonstration. Update framework references to .NET 8.0/9.0/10.0+.

## 2026-10-25 - Architectural Articulation and Syntax Modernization
**Observation:** The README.md usage examples contained outdated C# syntax (`uint` and `ulong` explicit type declarations and `out uint` instead of `out var`) which failed to reflect contemporary .NET 9.0/10.0+ programming paradigms.
**Strategic Action:** Synchronized the documentation examples to use implicitly-typed local variables (`var`) and inline out-variable declarations (`out var`) to ensure epistemological parity with modern .NET conventions. Checked and verified architectural descriptions correctly described the BMI2 hardware intrinsic dispatcher and software fallback without introducing unsupported claims.
