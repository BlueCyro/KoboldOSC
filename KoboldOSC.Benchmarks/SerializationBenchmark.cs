using System.Buffers;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using KoboldOSC.Helpers;
using KoboldOSC.Messages;
using KoboldOSC.Structs;
using Rug.Osc;

namespace KoboldOSC.Benchmarks.Serialization;

// [DisassemblyDiagnoser]
[MemoryDiagnoser]
public class OSCBenchmarks
{
    public const string OSC_TEST_PATH = "/OSC/Test/Path";
    public const string TEST_OSC_STRING = "Howzaboutta nice long string for OSC to serialize. Yay! :)";

    [Benchmark]
    public int OSCHeapSerializeBenchmark()
    {

        var msg = new KOscMessage(OSC_TEST_PATH);
            msg.WriteInt(12);
            msg.WriteString(TEST_OSC_STRING);
            msg.WriteFloat(99f);
            msg.WriteTime(DateTime.Now.Ticks2Ntp());

        Span<byte> serialized = stackalloc byte[msg.ByteLength];
        msg.Serialize(serialized);

        return serialized.Length;
    }

    [Benchmark]
    public int RugOSCSerializeBenchmark()
    {
        var msg = new OscMessage(OSC_TEST_PATH,
            12,
            TEST_OSC_STRING,
            99f,
            OscTimeTag.FromDataTime(DateTime.Now));

        byte[] serialized = ArrayPool<byte>.Shared.Rent(msg.SizeInBytes);
        msg.Write(serialized);
        ArrayPool<byte>.Shared.Return(serialized);

        return serialized.Length;
    }

    [Benchmark]
    public int OSCStackSerializeBenchmark()
    {
        var msg = new KOscMessageS(OSC_TEST_PATH, [12, TEST_OSC_STRING, 99f, DateTime.Now]);

        Span<byte> serialized = stackalloc byte[msg.ByteLength];
        msg.Serialize(serialized);

        return serialized.Length;
    }

    [Benchmark]
    public int OSCHeapBundleSerializeBenchmark()
    {
        var msg1 = new KOscMessage(OSC_TEST_PATH);
            msg1.WriteInt(12);
            msg1.WriteString(TEST_OSC_STRING);
            msg1.WriteFloat(99f);
            msg1.WriteTime(DateTime.Now.Ticks2Ntp());
        
        var msg2 = new KOscMessage(OSC_TEST_PATH);
            msg2.WriteInt(12);
            msg2.WriteString(TEST_OSC_STRING);
            msg2.WriteFloat(99f);
            msg2.WriteTime(DateTime.Now.Ticks2Ntp());
        
        var msg3 = new KOscMessage(OSC_TEST_PATH);
            msg3.WriteInt(12);
            msg3.WriteString(TEST_OSC_STRING);
            msg3.WriteFloat(99f);
            msg3.WriteTime(DateTime.Now.Ticks2Ntp());


        var bundle = new KOscBundle(msg1, msg2, msg3);

        Span<byte> serialized = stackalloc byte[bundle.ByteLength];
        bundle.Serialize(serialized);

        return serialized.Length;
    }

    [Benchmark]
    public int RugOSCBundleSerializeBenchmark()
    {
        var msg1 = new OscMessage(OSC_TEST_PATH,
            12,
            TEST_OSC_STRING,
            99f,
            OscTimeTag.FromDataTime(DateTime.Now));
        
        var msg2 = new OscMessage(OSC_TEST_PATH,
            12,
            TEST_OSC_STRING,
            99f,
            OscTimeTag.FromDataTime(DateTime.Now));
        
        var msg3 = new OscMessage(OSC_TEST_PATH,
            12,
            TEST_OSC_STRING,
            99f,
            OscTimeTag.FromDataTime(DateTime.Now));
        

        OscBundle bundle = new(DateTime.Now, msg1, msg2, msg3);

        byte[] serialized = ArrayPool<byte>.Shared.Rent(bundle.SizeInBytes);
        bundle.Write(serialized);
        ArrayPool<byte>.Shared.Return(serialized);

        return serialized.Length;
    }



    [Benchmark]
    public int OSCStackBundleSerializeBenchmark()
    {
        var msg = new KOscMessageS(OSC_TEST_PATH, [TEST_OSC_STRING, 12, 99f, DateTime.Now]);
        
        var msg2 = new KOscMessageS(OSC_TEST_PATH, [TEST_OSC_STRING, 12, 99f, DateTime.Now]);


        var msg3 = new KOscMessageS(OSC_TEST_PATH, [TEST_OSC_STRING, 12, 99f, DateTime.Now]);


        var bundle = new KOscBundleS(msg, msg2, msg3);

        Span<byte> serialized = stackalloc byte[bundle.ByteLength];
        bundle.Serialize(serialized);

        return serialized.Length;
    }
}