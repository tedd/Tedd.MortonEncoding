## 2024-08-02 - Dependency Drift and Missing Metadata

**Observation:** Test projects `Tedd.MortonEncoding.Tests` and `Tedd.MortonEncoding.DotNet4Tests` use severely outdated NuGet packages (`Microsoft.NET.Test.Sdk`, `xunit`, `coverlet.collector`), which cause a transitive vulnerability (`Newtonsoft.Json` 9.0.1). The main project `Tedd.MortonEncoding` is missing a `<PackageReadmeFile>` which causes warning during packing. Also, the conditional compilation regex for `INTRINSIC` excludes `net10.0`.

**Strategic Action:** Update test dependencies to the latest stable versions. Add `README.md` to `Tedd.MortonEncoding.csproj` metadata. Fix the MSBuild Regex for the `INTRINSIC` symbol to include `net10.0+`.
