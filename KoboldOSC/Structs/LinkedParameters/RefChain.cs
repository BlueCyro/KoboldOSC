using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using KoboldOSC.Messages;

namespace KoboldOSC.Structs;

[StructLayout(LayoutKind.Sequential)]
public readonly ref struct RefChain
{
    public readonly OscType Type;
    public readonly int TotalBytes;
    public readonly int ByteLength;
    public readonly int Length;
    public readonly unsafe RefChain* Next;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly unsafe ref RefChain GetNext() => ref Unsafe.AsRef<RefChain>(Next);
}
