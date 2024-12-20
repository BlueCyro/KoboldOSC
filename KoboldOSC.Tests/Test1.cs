using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using KoboldOSC;
using KoboldOSC.Helpers;
using KoboldOSC.Messages;
using KoboldOSC.Parameters;
using KoboldOSC.Structs;

namespace KoboldOSC.Tests;


// These suck and are 1000% not done.

[TestClass]
public sealed class SerializationTests
{
    public const string OSC_TEST_PATH = "/one/two/three";
    public const string TEST_OSC_STRING = "Howzaboutta nice long string for OSC to serialize. Yay! :)";
    public const string PNG_DATA = "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";


    public static readonly byte[] PngBytes = Encoding.UTF8.GetBytes(PNG_DATA);


    [TestMethod]
    public unsafe void TestMessageSerialize()
    {
        string path = "/test/osc/path/string";
        int pathByteLength = path.GetAlignedLength();

        string testString = "Howzaboutta nice long string for OSC to serialize. Yay! :)";
        int testStringByteLength = testString.GetAlignedLength();

        int oscInt = 12;
        float oscFloat = 99f;
        DateTime time = DateTime.Now;


        var msg = new KOscMessageS(path, [testString, 12, 99f, time, PngBytes]);
            // msg.WriteString(testString);
            // msg.WriteInt(12);
            // msg.WriteFloat(99f);
            // msg.WriteTimeTag(time);
            // msg.WriteBinary(PngBytes);
        
        Span<byte> serialized = stackalloc byte[msg.ByteLength];
        msg.Serialize(serialized);
        File.WriteAllBytes("./OSC_TEST_SERIALIZED.osc", serialized);


        int curOffset = 0;

        string typeTable = ",siftb";
        int typeTableByteLength = typeTable.GetAlignedLength();


        
        var pathDecoded = Encoding.UTF8.GetString(serialized[..pathByteLength]).TrimEnd('\0');
        Assert.AreEqual(pathDecoded, path);
        curOffset += pathByteLength;


        var typeTableDecoded = Encoding.UTF8.GetString(serialized.Slice(curOffset, typeTableByteLength)).TrimEnd('\0');
        Assert.AreEqual(typeTableDecoded, typeTable);
        curOffset += typeTableByteLength;


        var testStringDecoded = Encoding.UTF8.GetString(serialized.Slice(curOffset, testStringByteLength)).TrimEnd('\0');
        Assert.AreEqual(testStringDecoded, testString);
        curOffset += testStringByteLength;

        int intDecoded = BitConverter.ToInt32(serialized.Slice(curOffset, Unsafe.SizeOf<int>()));
        if (BitConverter.IsLittleEndian)
            intDecoded = BinaryPrimitives.ReverseEndianness(intDecoded);
        
        Assert.AreEqual(intDecoded, oscInt);

        curOffset += Unsafe.SizeOf<int>();


        uint floatDecodedRaw = BitConverter.ToUInt32(serialized.Slice(curOffset, Unsafe.SizeOf<float>()));
        if (BitConverter.IsLittleEndian)
            floatDecodedRaw = BinaryPrimitives.ReverseEndianness(floatDecodedRaw);
        
        float floatDecoded = Unsafe.As<uint, float>(ref floatDecodedRaw);
        Assert.AreEqual(floatDecoded, oscFloat);
        
        curOffset += Unsafe.SizeOf<float>();


        ulong timeDecoded = BitConverter.ToUInt64(serialized.Slice(curOffset, Unsafe.SizeOf<ulong>()));
        if (BitConverter.IsLittleEndian)
            timeDecoded = BinaryPrimitives.ReverseEndianness(timeDecoded);
        
        Assert.AreEqual(timeDecoded, time.Ticks2Ntp());





        Console.WriteLine($"Parameter count: {msg.ParamCount}");
        Console.WriteLine($"Byte Length: {msg.ByteLength}");

        // Write the packet out to a raw binary file for inspection
    }



    [TestMethod]
    public unsafe void TestBundleSerialize()
    {
        var msg = new KOscMessageS("/test/message/one", [12, "Test string from message #1", DateTime.Now]);
        
        var msg2 = new KOscMessageS("/test/message/two", [DateTime.Now, "Test string from message #2", 99f]);

        var msg3 = new KOscMessageS("/test/message/three", [12, 99f, "Test string from message #3", DateTime.Now]);


        var bundle = new KOscBundleS(msg, msg2, msg3);

        Span<byte> serialized = stackalloc byte[bundle.ByteLength];
        bundle.Serialize(serialized);

        File.WriteAllBytes("./OSC_BUNDLE_TEST.osc", serialized);
    }
}
