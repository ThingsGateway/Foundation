using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.NativeAot;

[AttributeUsage(AttributeTargets.Class)]
public sealed class CustomNativeAot10_0Attribute : JobConfigBaseAttribute
{
    public CustomNativeAot10_0Attribute() : base(CreateCustomJob()) { }

    private static Job CreateCustomJob()
    {
        var toolchain = NativeAotToolchain.CreateBuilder()
            .UseNuGet()
            .IlcInstructionSet(GetNativeInstructionSets())// 设置指令集
            .TargetFrameworkMoniker("net10.0")
            .ToToolchain();

        return Job.Default
            .WithRuntime(NativeAotRuntime.Net10_0) // AOT 运行时
            .WithToolchain(toolchain)
            .WithId("NativeAot 10.0");
    }
    static string GetNativeInstructionSets()
    {
        List<string> sets = new() { "base" };

        if (System.Runtime.Intrinsics.X86.Sse42.IsSupported)
            sets.Add("sse4.2");

        if (System.Runtime.Intrinsics.X86.Avx.IsSupported)
            sets.Add("avx");

        if (System.Runtime.Intrinsics.X86.Avx2.IsSupported)
            sets.Add("avx2");

        if (System.Runtime.Intrinsics.X86.Avx512F.IsSupported)
            sets.Add("avx512");

        return string.Join(',', sets);
    }

}

