## 2025-02-18 - Architectural Documentation Synchronization

**Observation:** The README.md file contains legacy .NET framework references (e.g., .Net Core 3, .Net 4, .Net core 1/2) and does not reflect current .NET 9.0/10.0+ paradigms. The examples are correct but somewhat simplistic. Furthermore, the library now includes `SplitXY` and `SplitXYZ` functions, and a 3D encode/decode (`Encode(x,y,z)`, `Decode(morton, out x, out y, out z)`) functionality, which are completely undocumented in the README. The hardware acceleration explanation also lacks mention of modern .NET Intrinsics (`X86.Bmi2`) and assumes older framework contexts.

**Strategic Action:** Update README.md to articulate current structural state, including 3D encoding/decoding, the utility of `SplitXY`/`SplitXYZ`, and update the hardware acceleration details to acknowledge modern .NET intrinsic support. Replace legacy testing syntax (Assert.Equal) with a robust structure suitable for demonstration. Update framework references to .NET 8.0/9.0/10.0+.

## 2025-02-18 - Target Framework Intrinsic Misalignment
**Observation:** The README.md asserted intrinsic hardware acceleration support through .NET 10.0+, but the MSBuild Regex in Tedd.MortonEncoding.csproj (`^(netcoreapp3|net[56789])`) failed to define the `INTRINSIC` constant for the `net10.0` TargetFramework.
**Strategic Action:** Updated the Regular Expression in the .csproj to explicitly capture .NET 10.0 (`^(netcoreapp3|net([5-9]|[1-9][0-9]))`), synchronizing the compiled structural capabilities with the documented architectural claims.
