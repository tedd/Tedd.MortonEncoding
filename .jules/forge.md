## 2025-02-18 - Dependency Update

**Observation:** Found outdated test dependencies:
- Tedd.MortonEncoding.Tests used outdated xunit, coverlet.collector, and Microsoft.NET.Test.Sdk.
- Tedd.MortonEncoding.DotNet4Tests targeted net461 but Microsoft.NET.Test.Sdk 18.x requires newer target frameworks or suppression.

**Strategic Action:** Updated Tedd.MortonEncoding.Tests dependencies to latest versions. Changed Tedd.MortonEncoding.DotNet4Tests TargetFramework from net461 to net462, updated Microsoft.NET.Test.Sdk to 18.10.1, added SuppressTfmSupportBuildErrors to suppress warnings, and pinned xunit.runner.visualstudio to 2.8.2 since newer versions drop net462 support.
