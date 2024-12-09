using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using KoboldOSC.Helpers;
using KoboldOSC.Messages;

namespace KoboldOSC.Structs;

[StructLayout(LayoutKind.Sequential)]
public readonly unsafe ref struct LinkedString
{
    internal unsafe LinkedString(ref RefChain next, string value)
    {
        Value = value;
        Type = Value.GetOSCIdentifierS();
        Next = (RefChain*)Unsafe.AsPointer(ref next);
        ByteLength = Value.Length.Ensure4Byte();
        Totalbytes = next.TotalBytes + ByteLength;
        Length = next.Length + 1;
    }

    public readonly OscType Type;
    public readonly int Totalbytes;
    public readonly int ByteLength;
    public readonly int Length;
    public readonly RefChain* Next;
    public readonly InlineString512 Value;
}
