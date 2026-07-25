## 2025-02-18 - Architectural Documentation Synchronization

**Observation:** The README.md file contains legacy .NET framework references (e.g., .Net Core 3, .Net 4, .Net core 1/2) and does not reflect current .NET 9.0/10.0+ paradigms. The examples are correct but somewhat simplistic. Furthermore, the library now includes `SplitXY` and `SplitXYZ` functions, and a 3D encode/decode (`Encode(x,y,z)`, `Decode(morton, out x, out y, out z)`) functionality, which are completely undocumented in the README. The hardware acceleration explanation also lacks mention of modern .NET Intrinsics (`X86.Bmi2`) and assumes older framework contexts.

**Strategic Action:** Update README.md to articulate current structural state, including 3D encoding/decoding, the utility of `SplitXY`/`SplitXYZ`, and update the hardware acceleration details to acknowledge modern .NET intrinsic support. Replace legacy testing syntax (Assert.Equal) with a robust structure suitable for demonstration. Update framework references to .NET 8.0/9.0/10.0+.
## 2025-02-18 - Documentation Drift regarding Framework Versions and Architectural Speculation

**Observation:** The README.md file exhibited documentation drift by inaccurately claiming intrinsic hardware execution for .NET 10.0+ (due to regex conditional compilation boundaries in the csproj). Additionally, there was a risk of pedagogical friction and speculative assumptions from developers regarding the presence of hierarchical data binding, routed events, or retro-computing DOS-era controls.

**Strategic Action:** Updated the README.md to accurately document the software fallback path for .NET 10.0+. Added an "Internal Mechanics & Boundary Delineation" section to explicitly delineate that the framework does not utilize hierarchical data binding, routed events, or DOS-era controls, constraining operational capability strictly to high-throughput bitwise manipulation.
