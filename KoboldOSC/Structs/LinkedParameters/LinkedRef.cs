using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using KoboldOSC.Messages;

namespace KoboldOSC.Structs;

[StructLayout(LayoutKind.Sequential)]
public readonly unsafe ref struct LinkedRef<T>
    where T : unmanaged
{
    internal unsafe LinkedRef(ref RefChain next, T value)
    {
        Type = value.GetOSCIdentifierS();
        Next = (RefChain*)Unsafe.AsPointer(ref next);
        Value = value;
        ByteLength = Unsafe.SizeOf<T>();
        TotalBytes = next.TotalBytes + ByteLength;
        Length = next.Length + 1;
    }

    public readonly OscType Type;
    public readonly int TotalBytes;
    public readonly int ByteLength;
    public readonly int Length;
    public readonly RefChain* Next;
    public readonly T Value;
}
