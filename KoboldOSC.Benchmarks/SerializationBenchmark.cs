using System.Buffers;
using BenchmarkDotNet.Attributes;
using KoboldOSC.Benchmarks.Profiling;
using KoboldOSC.Helpers;
using KoboldOSC.Messages;
using Rug.Osc;

namespace KoboldOSC.Benchmarks.Serialization;

// [DisassemblyDiagnoser]
[MemoryDiagnoser]
public class OSCBenchmarks
{
    public const string OSC_TEST_PATH = "/OSC/Test/Path";
    public const string TEST_OSC_STRING = "This is a nice and long string for OSC to serialize.";

    [Benchmark]
    public int OSCHeapSerializeBenchmark()
    {

        var msg = new KOscMessage(OSC_TEST_PATH);
            msg.WriteInt(12);
            msg.WriteFloat(99f);
            msg.WriteTime(DateTime.Now.ToNtp());
            msg.WriteString(TEST_OSC_STRING);

        Span<byte> serialized = stackalloc byte[msg.ByteLength];
        msg.Serialize(serialized);

        return serialized.Length;
    }

    [Benchmark]
    public int RugOSCSerializeBenchmark()
    {
        var msg = new OscMessage(OSC_TEST_PATH,
            12,
            99f,
            OscTimeTag.FromDataTime(DateTime.Now),
            TEST_OSC_STRING);

        byte[] serialized = ArrayPool<byte>.Shared.Rent(msg.SizeInBytes);
        msg.Write(serialized);
        ArrayPool<byte>.Shared.Return(serialized);

        return serialized.Length;
    }

    [Benchmark]
    public int OSCStackSerializeBenchmark()
    {
        KOscMessageS msg = new(OSC_TEST_PATH);
        KOscMessageS.Start(out _)
            .WriteInt(12, out _)
            .WriteFloat(99f, out _)
            .WriteTimeTag(DateTime.Now, out _)
            .WriteString(TEST_OSC_STRING, out _)
            .End(ref msg);

        Span<byte> serialized = stackalloc byte[msg.ByteLength];
        msg.Serialize(serialized);

        return serialized.Length;
    }
}