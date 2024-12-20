

using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;

namespace KoboldOSC.Benchmarks.Delegates;


[MemoryDiagnoser]
// [DisassemblyDiagnoser]
public class DelegateBenchmarks
{
    public static readonly DateTime TestTime = new(2024, 12, 14);


    [Benchmark]
    public long DelegateCallBenchmark()
    {
        return StaticHelper.Delegate(3f, 6, TestTime);
    }


    [Benchmark]
    public unsafe long FunctionPointerCallBenchmark()
    {
        return StaticHelper.FunctionPointer(3f, 6, TestTime);
    }


    [Benchmark]
    public long DirectCallBenchmark()
    {
        return StaticHelper.StaticFunction(3f, 6, TestTime);
    }
}



public static class StaticHelper
{

    unsafe static StaticHelper()
    {
        Delegate = StaticFunction;
        FunctionPointer = &StaticFunction;
    }

    public static readonly Func<float, int, DateTime, long> Delegate;
    public static unsafe readonly delegate* <float, int, DateTime, long> FunctionPointer;

    // Don't inline this method since the invocation is specifically being measured.
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static long StaticFunction(float f, int i, DateTime dt) => (long)(dt.Ticks + f + i);
}