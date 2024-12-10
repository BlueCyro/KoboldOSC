using System.Runtime.CompilerServices;
using KoboldOSC;
using KoboldOSC.Helpers;
using KoboldOSC.Messages;
using KoboldOSC.Structs;

namespace KoboldOSC.Tests;

[TestClass]
public sealed class SerializationTests
{
    public const string OSC_TEST_PATH = "/one/two/three";


#pragma warning disable CS0612 // Type or member is obsolete
    [TestMethod]
    public void TestNtpTime()
    {
        DateTime time = new(638692700035253476);

        ulong suspectA = time.ToNtp();
        ulong suspectB = time.Ticks2Ntp();

        Console.WriteLine($"Time: {time.Ticks}, ToNtp: {suspectA >> 32}.{(uint)suspectA}, Shorthand: {suspectB >> 32}.{(uint)suspectB}");
    }
#pragma warning restore CS0612 // Type or member is obsolete


    
    [TestMethod]
    public unsafe void TestMessageSerialize()
    {
        KOscMessageS msg = new(OSC_TEST_PATH);
        KOscMessageS.Start()
            .WriteInt(12)
            .WriteFloat(99f)
            .WriteTimeTag(DateTime.Now)
            .WriteString("This is a pretty cool OSC string I think. :)")
            .End(ref msg);

        Span<byte> serialized = stackalloc byte[msg.ByteLength];
        msg.Serialize(serialized);

        Console.WriteLine($"Parameter count: {msg.ItemCount}");
        Console.WriteLine($"Byte Length: {msg.ByteLength}");
        File.WriteAllBytes("./OSC_TEST_SERIALIZED.osc", serialized);
    }

    public static ref int TestRef(ref int test) => ref test;

    public static ref int TestOut(out int test)
    {
        test = 7;
        return ref Unsafe.AsRef(in test);
    }
}
