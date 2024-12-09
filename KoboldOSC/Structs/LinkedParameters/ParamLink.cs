using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using KoboldOSC.Messages;

namespace KoboldOSC.Structs;

[StructLayout(LayoutKind.Sequential)]
public readonly unsafe ref struct ParamLink<T>
    where T : unmanaged
{
    internal unsafe ParamLink(ref ParamChain next, T value)
    {
        Type = value.GetOSCIdentifierS();
        Next = (ParamChain*)Unsafe.AsPointer(ref next);
        Value = value;
        ByteLength = Unsafe.SizeOf<T>();
        TotalBytes = next.TotalBytes + ByteLength;
        Length = next.Length + 1;
    }

    public readonly OscType Type;
    public readonly int TotalBytes;
    public readonly int ByteLength;
    public readonly int Length;
    public readonly ParamChain* Next;
    public readonly T Value;
}
