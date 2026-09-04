# Performance benchmarks

This project measures amortized Morton encode and decode throughput with BenchmarkDotNet. Each benchmark invocation processes 1,024 values, and OperationsPerInvoke normalizes the reported mean to one encode or decode. The result therefore represents steady-state time per operation over warm, cache-resident inputs; it is not isolated instruction latency.

## Baseline provenance

Tedd.MortonEncodingArchive.V1_0_1.MortonEncoding preserves the six public methods and method bodies published in Tedd.MortonEncoding 1.0.1. The source is corroborated by the published package and release-era commit e93d4a2; only the namespace changes, preventing a collision with the current Tedd.MortonEncoding type.

The repository tag named v1.0.1 is not the archive source. That tag was created later and points to a revision in which the redundant first stage of the 2D software encoder had already been removed. Treating it as the package baseline would produce a historically incorrect comparison.

The published .NET Core 3.x assets used BMI2 dispatch when supported, whereas the other 1.0.1 target assets used software. The archive is compiled for the benchmark's modern runtimes with the same conditional BMI2 branch. V1_0_1Software separately exposes exact copies of the release's software branches so they can be measured explicitly on a BMI2-capable host. It is benchmark instrumentation, not an asserted part of the 1.0.1 API.

Both the archive and benchmark projects are non-packable.

## Hypotheses

1. On an x86/x64 processor with BMI2, the dispatched 32-bit paths should have lower amortized time per operation than their explicit software counterparts. On unsupported hardware, both dispatched versions select software; no hardware advantage should be expected.
2. The current 32-bit 2D fallback should be no slower than the published 1.0.1 software encoder because it removes a redundant shift and OR. A null result remains plausible if the JIT removes those operations itself.
3. Current 32-bit behavior should not regress materially against 1.0.1 for equivalent dispatch and software paths. The 3D algorithms provide a control because the original optimization concerned the 2D encoder.
4. Current 64-bit dispatch should outperform its explicit software fallback on hardware where the 64-bit BMI2 path is supported. This must be evaluated independently for 2D and 3D.
5. .NET 8 and .NET 10 may generate different machine code or dispatch overhead. Results from different target frameworks are separate experiments and should not be merged without retaining runtime identity.

These are falsifiable expectations, not measured conclusions. No benchmark results are committed here.

## Benchmark design

- UInt32Benchmarks groups published-1.0.1 dispatch, current dispatch, published-1.0.1 software, and current software by 2D/3D encode/decode operation.
- UInt64Benchmarks groups current dispatch and current software by 2D/3D encode/decode operation.
- Every category has exactly one baseline and contains only methods with identical input and output semantics.
- Every method traverses the same 1,024-element deterministic dataset and uses the same loop and checksum structure as its peers. Two-dimensional 32-bit inputs use 16 bits per coordinate, three-dimensional 32-bit inputs use 10 bits, two-dimensional 64-bit inputs use all 32 bits, and three-dimensional 64-bit inputs use 21 bits.
- Batch inputs are populated in GlobalSetup, outside measurement. Runtime-filled arrays prevent compile-time constant folding and prevent a repeated scalar operation from being hoisted out of the loop.
- Each operation contributes to a returned ulong checksum. BenchmarkDotNet consumes that return value, so encode results and every decoded coordinate remain observable.
- Batching makes the measured workload substantially larger than the invocation harness overhead. OperationsPerInvoke then reports the elapsed time per encode or decode instead of allowing overhead subtraction to collapse sub-nanosecond BMI2 measurements to zero.
- MemoryDiagnoser verifies allocation behavior. Dataset allocations occur only during setup and are excluded from measured operations.
- BenchmarkSwitcher discovers every benchmark class in the assembly; the runner is not hard-coded to one suite.

## Run

Run from the repository root with an optimized build. Specify the target framework because the benchmark project is multi-targeted.

~~~powershell
dotnet run --project src/Tedd.MortonEncoding.Benchmarks -c Release -f net8.0 -- --filter '*'
dotnet run --project src/Tedd.MortonEncoding.Benchmarks -c Release -f net10.0 -- --filter '*'
~~~

Run one suite when iterating:

~~~powershell
dotnet run --project src/Tedd.MortonEncoding.Benchmarks -c Release -f net10.0 -- --filter '*UInt32Benchmarks*'
dotnet run --project src/Tedd.MortonEncoding.Benchmarks -c Release -f net10.0 -- --filter '*UInt64Benchmarks*'
~~~

List discovered benchmarks without executing them:

~~~powershell
dotnet run --project src/Tedd.MortonEncoding.Benchmarks -c Release -f net10.0 -- --list flat
~~~

To verify dispatcher fallback behavior on a BMI2-capable machine, disable managed hardware intrinsics before starting the benchmark process:

~~~powershell
$env:DOTNET_EnableHWIntrinsic = '0'
dotnet run --project src/Tedd.MortonEncoding.Benchmarks -c Release -f net10.0 -- --filter '*'
Remove-Item Env:DOTNET_EnableHWIntrinsic
~~~

BenchmarkDotNet's child processes inherit the setting, causing Bmi2.IsSupported and Bmi2.X64.IsSupported to be false. The setting disables all managed hardware intrinsics, not only BMI2; BMI2 is the only intrinsic used by this library. Treat enabled and disabled runs as separate experiments, and remove the variable before measuring hardware dispatch again.

Record the CPU model, BMI2 support state, operating system, power policy, SDK/runtime version, and commit with any published result. Use an idle machine with a stable power profile. Compare ratios only within the same category and run; absolute nanosecond values across machines are not directly comparable.
