using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace KoboldOSC.Structs;

[Obsolete]
[StructLayout(LayoutKind.Sequential)]
public readonly struct OSCInt(int value) : IOSCParameter
{
    public byte ID => Value.GetOSCIdentifier();
    public int AlignedLength => Length; // Implicitly aligned
    public int Length => Unsafe.SizeOf<int>();
    public readonly int Value = value;
    public readonly void CopyTo(Span<byte> bytes)
    {
        bytes[0] = (byte)(Value >> 24);
        bytes[1] = (byte)(Value >> 16);
        bytes[2] = (byte)(Value >>  8);
        bytes[3] = (byte) Value;
    }


    public static implicit operator int(OSCInt other) => other.Value;
    public static implicit operator OSCInt(int other) => new(other);
}
