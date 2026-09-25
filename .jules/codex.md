## 2025-02-18 - Architectural Documentation Synchronization

**Observation:** The README.md file contains legacy .NET framework references (e.g., .Net Core 3, .Net 4, .Net core 1/2) and does not reflect current .NET 9.0/10.0+ paradigms. The examples are correct but somewhat simplistic. Furthermore, the library now includes `SplitXY` and `SplitXYZ` functions, and a 3D encode/decode (`Encode(x,y,z)`, `Decode(morton, out x, out y, out z)`) functionality, which are completely undocumented in the README. The hardware acceleration explanation also lacks mention of modern .NET Intrinsics (`X86.Bmi2`) and assumes older framework contexts.

**Strategic Action:** Update README.md to articulate current structural state, including 3D encoding/decoding, the utility of `SplitXY`/`SplitXYZ`, and update the hardware acceleration details to acknowledge modern .NET intrinsic support. Replace legacy testing syntax (Assert.Equal) with a robust structure suitable for demonstration. Update framework references to .NET 8.0/9.0/10.0+.

## 2024-05-18 - Outdated C# Syntax in README.md Examples
**Observation:** Code examples in `README.md` utilized explicit types (`uint`, `ulong`) instead of modern, implicitly-typed variables (`var`). Documentation lacked articulation of the architectural dispatch mechanics and roadmap hypotheses.
**Strategic Action:** Updated code snippets to conform to contemporary .NET 9.0/10.0+ syntax paradigms. Appended "Architectural Execution Flow" section delineating intrinsic dispatching and explicit future roadmap capabilities to prevent epistemological drift.
