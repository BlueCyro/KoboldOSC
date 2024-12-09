using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using KoboldOSC.Messages;

namespace KoboldOSC.Structs;

[StructLayout(LayoutKind.Sequential)]
public readonly ref struct ParamChain
{
    public readonly OscType Type;
    public readonly int TotalBytes;
    public readonly int ByteLength;
    public readonly int Length;
    public readonly unsafe ParamChain* Next;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly unsafe ref ParamChain GetNext() => ref Unsafe.AsRef<ParamChain>(Next);
}
